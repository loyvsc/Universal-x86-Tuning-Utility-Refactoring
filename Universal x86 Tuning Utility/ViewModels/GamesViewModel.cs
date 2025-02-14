using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using DesktopNotifications;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Services;
using Universal_x86_Tuning_Utility.Views.Windows;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class GamesViewModel : NotifyPropertyChangedBase, IDisposable
{
    public ICommand ReloadGamesListCommand { get; }
    public ICommand AddGameCommand { get; }
    public ICommand RunGameCommand { get; }

    public ObservableCollection<GameLauncherItem> Games
    {
        get => _games;
        set => SetValue(ref _games, value);
    }

    public bool GamesListUpdating
    {
        get => _gamesListUpdating;
        set => SetValue(ref _gamesListUpdating, value);
    }

    public bool IsActionsAvailable
    {
        get => _isActionsAvailable;
        set => SetValue(ref _isActionsAvailable, value);
    }

    private readonly ILogger<GamesViewModel> _logger;
    private readonly IGameDataService _gameDataService;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IDialogService _dialogService;
    private readonly INotificationManager _toastNotificationsService;
    private readonly IImageService _imageService;
    private readonly IGameLauncherService _gameLauncherService;
    private ObservableCollection<GameLauncherItem> _games;
    private readonly DispatcherTimer _updateFps;
    private bool _gamesListUpdating;
    private bool _isActionsAvailable;

    public GamesViewModel(ILogger<GamesViewModel> logger, 
                          IGameDataService gameDataService,
                          ISystemInfoService systemInfoService,
                          IDialogService dialogService,
                          INotificationManager toastNotificationsService,
                          IImageService imageService,
                          IGameLauncherService gameLauncherService)
    {
        _logger = logger;
        _gameDataService = gameDataService;
        _systemInfoService = systemInfoService;
        _dialogService = dialogService;
        _toastNotificationsService = toastNotificationsService;
        _imageService = imageService;
        _gameLauncherService = gameLauncherService;

        RunGameCommand = ReactiveCommand.CreateFromTask((GameLauncherItem gameToRun) => RunGame(gameToRun));
        
        _updateFps = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _updateFps.Tick += UpdateFPS_Tick;
        _updateFps.Start();
    }
    
    private void UpdateFPS_Tick(object? sender, EventArgs e)
    {
        try
        {
            if (!MainWindow.isMini && _systemInfoService.CpuInfo.Manufacturer == Manufacturer.Intel)
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when updating performance statistics");
        }
    }

    private async Task AddCustomGame()
    {
        var openFileDialogSettings = new OpenFileDialogSettings()
        {
            Title = "Select game",
            Filters = new List<FileFilter>()
            {
                new("Executable", ".Exe")
            }
        };

        var openFileDialogResult = await _dialogService.ShowOpenFileDialogAsync(this, openFileDialogSettings);
        
        if (openFileDialogResult != null)
        {
            // todo check path: expected - contains filename
            var filePath = openFileDialogResult.Path.ToString();
            var gameName = Path.GetFileNameWithoutExtension(filePath);
            
            var icon = Icon.ExtractAssociatedIcon(filePath);
            
            string gameImagesDirectory = @"\Assets\GameImages\";
            var iconPath = Path.Combine(gameImagesDirectory, gameName + ".ico");
            await using (var fileStream = new FileStream(iconPath, FileMode.Create))
            {
                icon.Save(fileStream);
                icon.Dispose();
            }
            
            var game = new GameLauncherItem
            {
                GameName = gameName,
                GameType = GameType.Custom,
                Path = Path.GetDirectoryName(filePath)!,
                Exe = filePath,
                IconPath = iconPath
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

            game.IconPath = await _imageService.GetIconImageUrl(game.GameName);

            return game;
        }));

        var filteredGamesList = games.ToList().OrderBy(item => item.GameName).Distinct().ToList();
        Games = new ObservableCollection<GameLauncherItem>(filteredGamesList);
        GamesListUpdating = false;
        IsActionsAvailable = true;
    }
    
    private async Task RunGame(GameLauncherItem gameToRun)
    {
        _gameLauncherService.LaunchGame(gameToRun);
        await _toastNotificationsService.ShowTextNotification($"Launching {gameToRun.GameName}", "This should only take a few moments!");
    }

    public void Dispose()
    {
        _updateFps.Stop();
    }
}