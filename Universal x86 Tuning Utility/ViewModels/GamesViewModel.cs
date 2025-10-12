using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Accord.Math.Distances;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Avalonia.Threading;
using DesktopNotifications;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Interfaces;
using ILogger = Serilog.ILogger;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class GamesViewModel : ReactiveObject, IDisposable
{
    public ICommand ReloadGamesListCommand { get; }
    public ICommand AddGameCommand { get; }
    public ICommand RunGameCommand { get; }

    public ObservableCollection<GameLauncherItem> Games
    {
        get => _games;
        set => this.RaiseAndSetIfChanged(ref _games, value);
    }

    public bool GamesListUpdating
    {
        get => _gamesListUpdating;
        set => this.RaiseAndSetIfChanged(ref _gamesListUpdating, value);
    }

    public bool IsActionsAvailable
    {
        get => _isActionsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isActionsAvailable, value);
    }
    
    private const string GameImagesDirectory = @"Assets\GameImages\";

    private readonly ILogger _logger;
    private readonly IGameDataService _gameDataService;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _toastNotificationsService;
    private readonly IImageService _imageService;
    private readonly IGameLauncherService _gameLauncherService;
    private readonly IIconExtractor _iconExtractor;
    private ObservableCollection<GameLauncherItem> _games;
    private readonly DispatcherTimer _updateFps;
    private bool _gamesListUpdating;
    private bool _isActionsAvailable;

    public GamesViewModel(ILogger logger, 
                          IGameDataService gameDataService,
                          IDialogService dialogService,
                          INotificationManager toastNotificationsService,
                          IImageService imageService,
                          IGameLauncherService gameLauncherService,
                          IIconExtractor iconExtractor)
    {
        _logger = logger;
        _gameDataService = gameDataService;
        _dialogService = dialogService;
        _toastNotificationsService = toastNotificationsService;
        _imageService = imageService;
        _gameLauncherService = gameLauncherService;
        _iconExtractor = iconExtractor;

        RunGameCommand = ReactiveCommand.CreateFromTask((GameLauncherItem gameToRun) => RunGame(gameToRun));
        AddGameCommand = ReactiveCommand.CreateFromTask(AddCustomGame);
        ReloadGamesListCommand = ReactiveCommand.CreateFromTask(ReloadGamesList);
        
        _updateFps = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _updateFps.Tick += UpdateFPS_Tick;
        
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            await ReloadGamesList();
            if (!_updateFps.IsEnabled)
            {
                _updateFps.Start();
            }
        }, null);
    }
    
    private async void UpdateFPS_Tick(object? sender, EventArgs e)
    {
        try
        {
            var presetNames = _gameDataService.GetPresetNames();
            foreach (var name in presetNames)
            {
                var gameToUpdate = Games.FirstOrDefault(item => item.GameName == name);
                if (gameToUpdate != null)
                {
                    var gameData = _gameDataService.GetPreset(name)!;
                        
                    if (gameData.FpsData != "No Data")
                    {
                        gameToUpdate.FpsData = $"{gameData.FpsData} FPS";
                    }
                    if (gameData.MsData != "No Data")
                    {
                        gameToUpdate.MillisecondData = $"{gameData.MsData} ms";
                    }

                    _gameDataService.SavePreset(name, gameData);
                }
            }
        }
        catch (Exception ex)
        {
            // _logger.Error(ex, "Exception occurred when updating performance statistics");
        }
    }

    private async Task AddCustomGame()
    {
        var openFileDialogSettings = new OpenFileDialogSettings()
        {
            Title = "Select game",
            Filters = new List<FileFilter>()
            {
                new("Executable", ".exe")
            }
        };

        var openFileDialogResult = await _dialogService.ShowOpenFileDialogAsync(openFileDialogSettings);
        
        if (openFileDialogResult != null)
        {
            var filePath = openFileDialogResult.LocalPath;
            var gameName = Path.GetFileNameWithoutExtension(filePath);
            
            var game = new GameLauncherItem
            {
                GameName = gameName,
                GameType = GameType.Custom,
                Path = Path.GetDirectoryName(filePath)!,
                Executable = filePath,
                IconPath = await _iconExtractor.ExtractIcon(filePath, GameImagesDirectory)
            };
            
            var preset = new GameData
            {
                GameName = game.GameName,
                FpsData = "No Data"
            };
            _gameDataService.SavePreset(game.GameName, preset);
            
            Games.Add(game);
        }
    }

    private async Task ReloadGamesList()
    {
        IsActionsAvailable = false;
        GamesListUpdating = true;

        var installedGames = _gameLauncherService.InstalledGames.Value;

        var games = await Task.WhenAll(installedGames.Select(async game =>
        {
            var gameData = _gameDataService.GetPreset(game.GameName);

            if (gameData == null)
            {
                gameData = new GameData
                {
                    FpsData = "No Data"
                };
            }
            else
            {
                gameData.FpsData = $"{gameData.FpsData} FPS";
            }

            _gameDataService.SavePreset(game.GameName, gameData);
            
            var iconPath = await _imageService.GetIconImageUrl(game.GameName);
            if (string.IsNullOrWhiteSpace(iconPath))
            {
                var otherGames = installedGames.ToList();
                otherGames.Remove(game);
                
                var levenshtein = new Levenshtein();
                var sameGame = otherGames.FirstOrDefault(x => levenshtein.Distance(game.GameName, x.GameName) <= 20);
                if (sameGame != null)
                {
                    iconPath = string.IsNullOrWhiteSpace(sameGame.IconPath) ? await _imageService.GetIconImageUrl(sameGame.GameName) : sameGame.IconPath;
                }
            }

            game.IconPath = iconPath;

            return game;
        }));

        var filteredGamesList = games.ToList().OrderBy(item => item.GameName).Distinct().ToList();
        Games = new ObservableCollection<GameLauncherItem>(filteredGamesList);
        GamesListUpdating = false;
        IsActionsAvailable = true;
    }
    
    private async Task RunGame(GameLauncherItem gameToRun)
    {
        await _toastNotificationsService.ShowTextNotification($"Launching {gameToRun.GameName}", "This should only take a few moments!");
        await _gameLauncherService.LaunchGame(gameToRun);
    }

    public void Dispose()
    {
        _updateFps.Stop();
    }
}