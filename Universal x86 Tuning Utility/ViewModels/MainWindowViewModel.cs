using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using Avalonia.Threading;
using DesktopNotifications;
using FluentAvalonia.FluentIcons;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using RTSSSharedMemoryNET;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Navigation;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services.PresetServices;
using Universal_x86_Tuning_Utility.Services.RyzenAdj;
using Universal_x86_Tuning_Utility.Services.SuperResolutionServices.Windows;
using PowerMode = ApplicationCore.Enums.PowerMode;
using PowerModeChangedEventArgs = ApplicationCore.Events.PowerModeChangedEventArgs;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class MainWindowViewModel : NotifyPropertyChangedBase
{
    private readonly ISystemInfoService _systemInfoService;
    private readonly IBatteryInfoService _batteryInfoService;
    private readonly INotificationManager _toastNotificationManager;
    private readonly IRyzenAdjService _ryzenAdjService;
    private readonly IRtssService _rtssService;
    private readonly IPowerPlanService _powerPlanService;
    private readonly IGameLauncherService _gameLauncherService;
    private readonly IImageService _imageService;
    private readonly IFanControlService _fanControlService;
    private readonly IPremadePresets _premadePresets;
    private readonly IGameDataService _gameDataService;
    private string _lastAppliedState = "";

    public ObservableCollection<NavigationViewModel> NavigationItems { get; set; }
    public INavigationPageFactory NavigationPageFactory { get; set; }

    public string Title
    {
        get => _title;
        set => SetValue(ref _title, value);
    }

    public string ProductManufacturer { get; }

    private readonly DispatcherTimer _miscTimer;
    private readonly DispatcherTimer _autoReapplyTimer;
    private readonly DispatcherTimer _autoRestoreTimer;

    private string _title;
    private bool _firstRun = true;
    private IReadOnlyCollection<GameLauncherItem> _gamesList;

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger,
        ISystemInfoService systemInfoService,
        IBatteryInfoService batteryInfoService,
        INotificationManager toastNotificationManager,
        IRyzenAdjService ryzenAdjService,
        IRtssService rtssService,
        IPowerPlanService powerPlanService,
        IGameLauncherService gameLauncherService,
        IImageService imageService,
        IFanControlService fanControlService,
        IPremadePresets premadePresets,
        IGameDataService gameDataService)
    {
        _systemInfoService = systemInfoService;
        _batteryInfoService = batteryInfoService;
        _toastNotificationManager = toastNotificationManager;
        _ryzenAdjService = ryzenAdjService;
        _rtssService = rtssService;
        _powerPlanService = powerPlanService;
        _gameLauncherService = gameLauncherService;
        _imageService = imageService;
        _fanControlService = fanControlService;
        _premadePresets = premadePresets;
        _gameDataService = gameDataService;
        _powerPlanService.PowerModeChanged += OnPowerModeChange;

        ProductManufacturer = _systemInfoService.Manufacturer.Value;

        _miscTimer = CreateTimer(1, (s, e) => HandleMiscellaneousTasks(s, e));
        _autoReapplyTimer = CreateTimer(Settings.Default.AutoReapplyTime, (s, e) => AutoReapplySettings(s, e));
        // _autoRestoreTimer = CreateTimer(1, (s, e) => WindowsSuperResolutionService.AutoRestore_Tick(s, e));

        InitializeViewModel();

        Title = $"Universal x86 Tuning Utility - {_systemInfoService.Cpu.Name}";

        NavigationPageFactory = new NavigationFactory();

        NavigateCommand = ReactiveCommand.Create<string>(tag => OnNavigate(tag));
    }

    private async void OnPowerModeChange(PowerModeChangedEventArgs e)
    {
        if (Settings.Default.isAdaptiveModeRunning == false)
        {
            if (e.BatteryStatus == BatteryStatus.Charging)
            {
                var batteryStatus = _batteryInfoService.GetBatteryStatus();

                if (batteryStatus == BatteryStatus.Charging)
                {
                    if (!string.IsNullOrEmpty(Settings.Default.acCommandString) &&
                        Settings.Default.acPreset != "None")
                    {
                        var preset = _premadePresets.PremadePresetsList.Find(x => x.Name == Settings.Default.acPreset);

                        if (preset != null)
                        {
                            Settings.Default.premadePreset = 0;
                            Settings.Default.acCommandString = preset.CommandValue;

                            Settings.Default.CommandString = Settings.Default.acCommandString;
                            Settings.Default.Save();

                            await _ryzenAdjService.Translate(Settings.Default.acCommandString);

                            if (_lastAppliedState != "ac")
                            {
                                await _toastNotificationManager.ShowTextNotification("Charge Preset Applied!",
                                    $"Your charge preset settings have been applied!");
                            }

                            _lastAppliedState = "ac";
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Settings.Default.dcCommandString) &&
                        Settings.Default.dcPreset != "None")
                    {
                        var preset = _premadePresets.PremadePresetsList.Find(x => x.Name == Settings.Default.dcPreset);

                        if (preset != null)
                        {
                            Settings.Default.premadePreset = 0;
                            Settings.Default.dcCommandString = preset.CommandValue;

                            Settings.Default.CommandString = Settings.Default.dcCommandString;
                            Settings.Default.Save();

                            await _ryzenAdjService.Translate(Settings.Default.dcCommandString);

                            if (_lastAppliedState != "dc")
                            {
                                await _toastNotificationManager.ShowTextNotification("Discharge Preset Applied!",
                                    "Your discharge preset settings have been applied!");
                            }

                            _lastAppliedState = "dc";
                        }
                    }
                }

                if (e.PowerMode == PowerMode.Resume)
                {
                    if (!string.IsNullOrEmpty(Settings.Default.resumeCommandString) &&
                        Settings.Default.resumePreset != "None")
                    {
                        var preset = _premadePresets.PremadePresetsList.Find(x => x.Name == Settings.Default.resumePreset);

                        if (preset != null)
                        {
                            Settings.Default.premadePreset = 0;
                            Settings.Default.resumeCommandString = preset.CommandValue;

                            Settings.Default.CommandString = Settings.Default.resumeCommandString;
                            Settings.Default.Save();

                            await _ryzenAdjService.Translate(Settings.Default.resumeCommandString);

                            if (_lastAppliedState != "resume")
                            {
                                await _toastNotificationManager.ShowTextNotification("Resume Preset Applied!",
                                    "Your resume preset settings have been applied!");
                            }

                            _lastAppliedState = "resume";
                        }
                    }
                }
            }
        }
    }

    private DispatcherTimer CreateTimer(int intervalInSeconds, EventHandler tickHandler)
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(intervalInSeconds)
        };
        timer.Tick += tickHandler;
        timer.Start();

        return timer;
    }

    private async void HandleMiscellaneousTasks(object sender, EventArgs e)
    {
        if (!File.Exists(Settings.Default.Path + "\\gameData.json") || !Settings.Default.isTrack) return;

        if (!_rtssService.IsRTSSRunning())
        {
            _rtssService.Start();
            return;
        }

        if (!_firstRun)
        {
            foreach (var game in _gamesList)
            {
                await ProcessGamePerformanceData(game);
            }
        }
        else
        {
            _miscTimer.Stop();
            _gamesList = _gameLauncherService.ReSearchGames();
            _firstRun = false;
            _miscTimer.Start();
        }
    }

    private async Task ApplyStartupSettings()
    {
        if (!Settings.Default.ApplyOnStart || string.IsNullOrWhiteSpace(Settings.Default.CommandString)) return;

        var statusCode = _batteryInfoService.GetBatteryStatus();

        var isCharging = statusCode == BatteryStatus.Charging;
        var commandString = isCharging ? Settings.Default.acCommandString : Settings.Default.dcCommandString;

        if (string.IsNullOrWhiteSpace(commandString))
        {
            commandString = Settings.Default.CommandString;
        }

        Settings.Default.CommandString = commandString;
        Settings.Default.Save();

        await _ryzenAdjService.Translate(commandString);

        var presetType = isCharging ? "Charge" : "Discharge";
        await _toastNotificationManager.ShowTextNotification($"{presetType} Preset Applied!",
            $"Your {presetType.ToLower()} preset settings have been applied!");
    }

    private void InitializeViewModel()
    {
        NavigationItems = new ObservableCollection<NavigationViewModel>
        {
            new NavigationViewModel()
            {
                Title = "Home",
                //Tag = "dashboard",    
                IconSymbol = FluentIconSymbol.Home20Regular,
                ViewModelType = typeof(DashboardViewModel)
            },
            // new NavigationViewItem()
            // {
            //     Content = "Premade",
            //     Tag = "premade",
            //     Icon = SymbolRegular.Predictions20,
            //     PageType = typeof(Views.Pages.PremadePage),
            //     IsVisible = _systemInfoService.Cpu.Manufacturer == Manufacturer.AMD
            // },
            // new NavigationViewItemBase()
            // {
            //     Content = "Custom",
            //     Tag = "custom",
            //     Icon = SymbolRegular.Book20,
            //     PageType = typeof(Views.Pages.CustomPresetsPage)
            // },
            // new NavigationViewItemBase()
            // {
            //     Content = "Adaptive",
            //     Tag = "adaptive",
            //     Icon = SymbolRegular.Radar20,
            //     PageType = typeof(Views.Pages.AdaptivePage)
            // },
            // new NavigationViewItemBase()
            // {
            //     Content = "Games",
            //     Tag = "games",
            //     Icon = SymbolRegular.Games20,
            //     PageType = typeof(Views.Pages.GamesPage)
            // },
            // new NavigationViewItemBase()
            // {
            //     Content = "Auto",
            //     Tag = "auto",
            //     Icon = SymbolRegular.Transmission20,
            //     PageType = typeof(Views.Pages.AutomationsPage)
            // },
            // // todo: remove this todos later
            // //new NavigationViewItemBase()
            // //{
            // //    Content = "Fan",
            // //    Tag = "fan",
            // //    Icon = SymbolRegular.WeatherDuststorm20,
            // //    PageType = typeof(Views.Pages.FanControl)
            // //},
            // // todo: remove later
            // // new NavigationViewItemBase()
            // //{
            // //    Content = "Magpie",
            // //    Tag = "magpie",
            // //    Icon = SymbolRegular.FullScreenMaximize20,
            // //    PageType = typeof(Views.Pages.DataPage)
            // //},
            // new NavigationViewItemBase()
            // {
            //     Content = "Info",
            //     Tag = "info",
            //     Icon = SymbolRegular.Info20,
            //     PageType = typeof(Views.Pages.SystemInfoPage)
            // }
        };

        // NavigationFooter = new ObservableCollection<INavigationControl>
        // {
        //     new NavigationViewItemBase()
        //     {
        //         Content = "Settings",
        //         Tag = "settings",
        //         Icon = SymbolRegular.Settings20,
        //         PageType = typeof(Views.Pages.SettingsPage)
        //     }
        // };
        //
        // TrayMenuItems = new ObservableCollection<MenuItem>
        // {
        //     new MenuItem
        //     {
        //         Header = "Home",
        //         Tag = "tray_home"
        //     }
        // };
    }

    public ICommand NavigateCommand { get; }

    private void OnNavigate(string parameter)
    {
        switch (parameter)
        {
            case "download":
                Process.Start(new ProcessStartInfo("https://github.com/JamesCJ60/Universal-x86-Tuning-Utility/releases")
                {
                    UseShellExecute = true
                });
                return;

            case "discord":
                Process.Start(new ProcessStartInfo("https://www.discord.gg/3EkYMZGJwq")
                {
                    UseShellExecute = true
                });
                return;

            case "support":
                Process.Start(new ProcessStartInfo("https://www.paypal.com/paypalme/JamesCJ60")
                {
                    UseShellExecute = true
                });
                Process.Start(new ProcessStartInfo("https://patreon.com/uxtusoftware")
                {
                    UseShellExecute = true
                });
                return;
        }
    }

    // private void SetupUI()
    // {
    //     WindowsSuperResolutionService.SetUpMagWindow(this);
    // }

    private async Task ProcessGamePerformanceData(GameLauncherItem game)
    {
        var appEntries = RTSSSharedMemoryNET.OSD.GetAppEntries()
            .Where(app => (app.Flags & AppFlags.MASK) != AppFlags.None).ToArray();

        foreach (var app in appEntries)
        {
            if (!IsGameMatched(game, app.Name)) continue;

            var gameDataManager = new GameDataService(Settings.Default.Path + "gameData.json");
            var gameData = gameDataManager.GetPreset(game.GameName);

            UpdateGamePerformanceData(app, gameData);
            gameDataManager.SavePreset(game.GameName, gameData);
        }
    }

    private bool IsGameMatched(GameLauncherItem game, string appName)
    {
        return !string.IsNullOrWhiteSpace(game.Path) && appName.Contains(game.Path, StringComparison.OrdinalIgnoreCase)
               || appName.Contains(_imageService.CleanFileName(game.GameName), StringComparison.OrdinalIgnoreCase)
               || !string.IsNullOrWhiteSpace(game.Executable) &&
               appName.Contains(game.Executable, StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateGamePerformanceData(AppEntry app, GameData gameData)
    {
        var fpsArray = ParseAndUpdateData(app.InstantaneousFrames, gameData.FpsAverageData, out var averageFps);
        var timeSpans = ParseAndUpdateData(app.InstantaneousFrameTime, gameData.MsAverageData, out var averageTimeSpan);

        gameData.FpsData = averageFps.ToString();
        gameData.FpsAverageData = fpsArray;
        gameData.MsData = averageTimeSpan.TotalMilliseconds.ToString("0.##");
        gameData.MsAverageData = timeSpans;
    }

    private string ParseAndUpdateData<T>(T newData, string existingData, out T average)
    {
        var dataList = existingData.Split(',').Select(s => (T)Convert.ChangeType(s, typeof(T))).ToList();
        dataList.Add(newData);

        if (dataList.Count > 100) dataList.RemoveAt(0);

        average = (T)Convert.ChangeType(dataList.Average(x => Convert.ToDouble(x)), typeof(T));
        return string.Join(",", dataList);
    }

    private async Task AutoReapplySettings(object sender, EventArgs e)
    {
        if (!Settings.Default.AutoReapply || Settings.Default.isAdaptiveModeRunning) return;

        if (!string.IsNullOrWhiteSpace(Settings.Default.CommandString))
        {
            await _ryzenAdjService.Translate(Settings.Default.CommandString);
        }

        UpdateTimerInterval(_autoReapplyTimer, Settings.Default.AutoReapplyTime);
    }

    private static void UpdateTimerInterval(DispatcherTimer timer, int newInterval)
    {
        if (timer.Interval == TimeSpan.FromSeconds(newInterval)) return;

        timer.Stop();
        timer.Interval = TimeSpan.FromSeconds(newInterval);
        timer.Start();
    }
    
    public void Dispose()
    {
        Settings.Default.isAdaptiveModeRunning = false;
        Settings.Default.Save();
        // WindowsSuperResolutionService.MagWindow?.Dispose();
    }
}