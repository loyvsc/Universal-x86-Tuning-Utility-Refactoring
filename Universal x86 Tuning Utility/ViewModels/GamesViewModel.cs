using System;
using System.Collections.Generic;
using ApplicationCore.Utilities;
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
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.ViewModels.Dialogs;
using ILogger = Serilog.ILogger;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class GamesViewModel : ReactiveObject, IDisposable
{
    public ICommand ReloadGamesListCommand { get; }
    public ICommand ChangeGameIconCommand { get; }
    public ICommand AddGameCommand { get; }
    public ICommand RunGameCommand { get; }

    public EnhancedObservableCollection<GameLauncherItem> Games
    {
        get => _games;
        set => this.RaiseAndSetIfChanged(ref _games, value);
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
    private EnhancedObservableCollection<GameLauncherItem> _games = new();
    private IDisposable? _updateFpsTimer;
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
        ReloadGamesListCommand = ReactiveCommand.CreateRunInBackground(ReloadGamesList);
        ChangeGameIconCommand = ReactiveCommand.CreateFromTask((GameLauncherItem gameToRun) => ChangeGameIcon(gameToRun));
        
        ThreadPool.QueueUserWorkItem(async _ =>
        {
            await ReloadGamesList();
            _updateFpsTimer = DispatcherTimer.Run(UpdateFPS_Tick, TimeSpan.FromSeconds(2), DispatcherPriority.Normal);
        }, null);
    }
    
    private bool UpdateFPS_Tick()
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
                    
                    gameToUpdate.SetAverageFps(gameData.FpsData);
                    gameToUpdate.SetAverageMs(gameData.MsData);

                    _gameDataService.SavePreset(name, gameData);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception occurred when updating performance statistics");
        }

        return true;
    }

    private async Task AddCustomGame()
    {
        _logger.Information("Adding custom game");
        var openFileDialogSettings = new OpenFileDialogSettings()
        {
            Title = "Select game",
            Filters = new List<FileFilter>()
            {
                new("Executable", ".exe"),
                new("Batch script", ".bat"),
                new("CMD file", ".cmd"),
                new("All files", ".*"),
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
                Executable = filePath
            };
            var icon = await _imageService.GetIconImageUrl(gameName);
            if (string.IsNullOrWhiteSpace(icon))
            {
                var lastFolderNames = filePath.Split('\\').SkipLast(1).TakeLast(3);
                
                foreach (var pathPart in lastFolderNames)
                {
                    var potentialIcon = await _imageService.GetIconImageUrl(pathPart);
                    if (!string.IsNullOrWhiteSpace(potentialIcon))
                    {
                        icon = potentialIcon;
                        game.GameName = pathPart;
                        break;
                    }
                }

                if (string.IsNullOrWhiteSpace(icon))
                {
                    icon = await _iconExtractor.ExtractIcon(filePath, GameImagesDirectory);
                }
            }
            game.IconPath = icon;
            
            var preset = new GameData
            {
                GameName = game.GameName
            };
            _gameDataService.SavePreset(game.GameName, preset);
            
            Games.Add(game);
            _logger.Information("Custom game added (path: {0})", filePath);
        }
    }

    private async Task ChangeGameIcon(GameLauncherItem? gameToChange)
    {
        if (gameToChange == null) return;
        
        var openFileDialogSettings = new OpenFileDialogSettings()
        {
            Title = "Select new icon",
            Filters = new List<FileFilter>()
            {
                new("Portable Network Graphic", ".png"),
                new("JPEG", ".jpg"),
                new("ICO", ".ico")
            }
        };

        var openFileDialogResult = await _dialogService.ShowOpenFileDialogAsync(openFileDialogSettings);

        if (openFileDialogResult != null)
        {
            var newIconPath = openFileDialogResult.LocalPath;
            
            File.Copy(newIconPath, gameToChange.IconPath, true);
            
            gameToChange.RaiseIconChanged();
        }
    }

    private async Task ReloadGamesList()
    {
        _logger.Information("Games list reloading");
        
        IsActionsAvailable = false;
        _dialogService.Show<ReloadingGamesDialogViewModel>(this, _reloadingGamesDialogViewModel);

        var installedGames = _gameLauncherService.ReSearchGames();

        var games = await Task.WhenAll(installedGames.Select(async game =>
        {
            var gameData = _gameDataService.GetPreset(game.GameName);

            if (gameData == null)
            {
                gameData = new GameData();
            }
            else
            {
                game.SetAverageMs(gameData.MsData);
                game.SetAverageFps(gameData.FpsData);
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
        Games.Reset(filteredGamesList);
        _dialogService.Close(_reloadingGamesDialogViewModel);
        IsActionsAvailable = true;
        
        _logger.Information("Games list reloaded");
    }
    
    private async Task RunGame(GameLauncherItem gameToRun)
    {
        _logger.Information("Running game {gameName} ({launcherType})", gameToRun.GameName, gameToRun.GameType.ToString());
        await _toastNotificationsService.ShowTextNotification($"Launching {gameToRun.GameName}", "This should only take a few moments!");
        await _gameLauncherService.LaunchGame(gameToRun);
    }

    public void Dispose()
    {
        _updateFpsTimer?.Dispose();
    }
}