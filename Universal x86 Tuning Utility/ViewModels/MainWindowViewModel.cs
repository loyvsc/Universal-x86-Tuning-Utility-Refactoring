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
using ApplicationCore.Models.LaptopInfo;
using Avalonia.Threading;
using DesktopNotifications;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Navigation;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services.PresetServices;
using PowerMode = ApplicationCore.Enums.PowerMode;
using PowerModeChangedEventArgs = ApplicationCore.Events.PowerModeChangedEventArgs;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly Serilog.ILogger _logger;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IBatteryInfoService _batteryInfoService;
    private readonly INotificationManager _toastNotificationManager;
    private readonly IRyzenAdjService _ryzenAdjService;
    private readonly IRtssService _rtssService;
    private readonly IPowerPlanService _powerPlanService;
    private readonly IGameLauncherService _gameLauncherService;
    private readonly IImageService _imageService;
    private readonly IPremadePresets _premadePresets;
    private string _lastAppliedState = "";

    public ObservableCollection<NavigationViewModel> NavigationItems { get; set; }

    public NavigationViewModel SelectedNavigationItem
    {
        get => _selectedNavigationItem;
        set => this.RaiseAndSetIfChanged(ref _selectedNavigationItem, value);
    }

    public INavigationPageFactory NavigationPageFactory { get; set; }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }
    
    public bool IsPortableConsole { get; }

    private readonly DispatcherTimer _miscTimer;
    private readonly DispatcherTimer _autoReapplyTimer;
    private readonly DispatcherTimer _autoRestoreTimer;

    private string _title;
    private bool _firstRun = true;
    private IReadOnlyCollection<GameLauncherItem> _gamesList;
    private NavigationViewModel _selectedNavigationItem;

    public MainWindowViewModel(Serilog.ILogger logger,
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
        _logger = logger;
        _systemInfoService = systemInfoService;
        _batteryInfoService = batteryInfoService;
        _toastNotificationManager = toastNotificationManager;
        _ryzenAdjService = ryzenAdjService;
        _rtssService = rtssService;
        _powerPlanService = powerPlanService;
        _gameLauncherService = gameLauncherService;
        _imageService = imageService;
        _premadePresets = premadePresets;
        _powerPlanService.PowerModeChanged += OnPowerModeChange;

        IsPortableConsole = _systemInfoService.LaptopInfo is PortableConsoleInfo;

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
                // Tag = "dashboard",    
                IconSymbol = Icon.Home,
                ViewModelType = typeof(DashboardViewModel)
            },
            new NavigationViewModel()
            {
                Title = "Premade",
                // Tag = "premade",
                IconSymbol = Icon.Predictions,
                ViewModelType = typeof(PremadePresetsViewModel),
                // IsVisible = _systemInfoService.Cpu.Manufacturer == Manufacturer.AMD
            },
            new NavigationViewModel()
            {
                Title = "Custom",
                // Tag = "custom",
                IconSymbol = Icon.Book,
                ViewModelType = typeof(CustomPresetsViewModel)
            },
            new NavigationViewModel()
            {
                Title = "Adaptive",
                // Tag = "adaptive",
                IconSymbol = Icon.Radar,
                ViewModelType = typeof(AdaptiveViewModel)
            },
            new NavigationViewModel()
            {
                Title = "Games",
                // Tag = "games",
                IconSymbol = Icon.Games,
                ViewModelType = typeof(GamesViewModel)
            },
            new NavigationViewModel()
            {
                Title = "Auto",
                // Tag = "auto",
                IconSymbol = Icon.Transmission,
                ViewModelType = typeof(AutomationsViewModel)
            },
            // // todo: remove this todos later
            // //new NavigationViewModel()
            // //{
            // //    Title = "Fan",
            // //    // Tag = "fan",
            // //    IconSymbol = Icon.WeatherDuststorm20,
            // //    ViewModelType = typeof(Views.Pages.FanControl)
            // //},
            // // todo: remove later
            // // new NavigationViewModel()
            // //{
            // //    Title = "Magpie",
            // //    // Tag = "magpie",
            // //    IconSymbol = Icon.FullScreenMaximize20,
            // //    ViewModelType = typeof(Views.Pages.DataPage)
            // //},
            new NavigationViewModel()
            {
                Title = "Info",
                // Tag = "info",
                IconSymbol = Icon.Info,
                ViewModelType = typeof(SystemInfoViewModel)
            }
        };

        // NavigationFooter = new ObservableCollection<INavigationControl>
        // {
        //     new NavigationViewModel()
        //     {
        //         Title = "Settings",
        //         // Tag = "settings",
        //         IconSymbol = Icon.Settings20,
        //         ViewModelType = typeof(Views.Pages.SettingsPage)
        //     }
        // };
        //
        // TrayMenuItems = new ObservableCollection<MenuItem>
        // {
        //     new MenuItem
        //     {
        //         Header = "Home",
        //         // Tag = "tray_home"
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
        foreach (var app in _rtssService.GetApplicationRenderInfo())
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

    private void UpdateGamePerformanceData(ApplicationRenderInfo app, GameData gameData)
    {
        gameData.FpsData.Add(app.InstantaneousFrames);
        gameData.MsData.Add(app.InstantaneousFrameTime.TotalMilliseconds);
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