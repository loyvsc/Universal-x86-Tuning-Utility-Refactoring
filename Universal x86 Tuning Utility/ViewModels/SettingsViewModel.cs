using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using DesktopNotifications;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Helpers;
using Universal_x86_Tuning_Utility.Properties;
using Task = System.Threading.Tasks.Task;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class SettingsViewModel : NotifyPropertyChangedBase
{
    public ICommand CheckUpdateCommand { get; }
    public ICommand DownloadUpdateCommand { get; }
    public ICommand StartStressTestCommand { get; }
    public ICommand StartOnSystemBootCommand { get; }

    public int ReapplySecond
    {
        get => _reapplySecond;
        set => SetValue(ref _reapplySecond, value);
    }
    
    public string ApplicationVersion
    {
        get => _appVersion;
        set => SetValue(ref _appVersion, value);
    }

    public bool IsAutoStartEnabled
    {
        get => _isAutoStartEnabled;
        set => SetValue(ref _isAutoStartEnabled, value);
    }

    public bool IsStartMinimizedEnabled
    {
        get => _isStartMinimizedEnabled;
        set => SetValue(ref _isStartMinimizedEnabled, value);
    }

    public bool IsMinimizeOnClose
    {
        get => _isMinimizeOnClose;
        set => SetValue(ref _isMinimizeOnClose, value);
    }

    public bool IsReapplyOnStart
    {
        get => _isReapplyOnStart;
        set => SetValue(ref _isReapplyOnStart, value);
    }

    public bool IsAutoReapply
    {
        get => _isIsAutoReapply;
        set => SetValue(ref _isIsAutoReapply, value);
    }

    public bool IsAutoCheckUpdates
    {
        get => _isAutoCheckUpdates;
        set => SetValue(ref _isAutoCheckUpdates, value);
    }

    public bool IsAutoStartAdaptiveMode
    {
        get => _isAutoStartAdaptiveMode;
        set => SetValue(ref _isAutoStartAdaptiveMode, value);
    }

    public bool IsAutoTrackGames
    {
        get => _isAutoTrackGames;
        set => SetValue(ref _isAutoTrackGames, value);
    }

    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set => SetValue(ref _isUpdateAvailable, value);
    }

    public string UpdatesMessage
    {
        get => _updatesMessage;
        set => SetValue(ref _updatesMessage, value);
    }
    
    private readonly ILogger<SettingsViewModel> _logger;
    private readonly IUpdateService _updateService;
    private readonly IUpdateInstallerService _updateInstallerService;
    private readonly INotificationManager _toastNotificationsManager;
    private readonly ISystemBootService _systemBootService;
    private string _appVersion = string.Empty;
    private string _updatesMessage;
    private int _reapplySecond;
    private bool _isUpdateAvailable;
    private bool _isReapplyOnStart;
    private bool _isAutoStartAdaptiveMode;
    private bool _isAutoTrackGames;
    private bool _isIsAutoReapply;
    private bool _isAutoStartEnabled;
    private bool _isStartMinimizedEnabled;
    private bool _isMinimizeOnClose;
    private bool _isAutoCheckUpdates;

    public SettingsViewModel(ILogger<SettingsViewModel> logger,
                             IUpdateService updateService,
                             IUpdateInstallerService updateInstallerService,
                             INotificationManager toastNotificationsManager,
                             ISystemBootService systemBootService)
    {
        _logger = logger;
        _updateService = updateService;
        _updateInstallerService = updateInstallerService;
        _toastNotificationsManager = toastNotificationsManager;
        _systemBootService = systemBootService;

        CheckUpdateCommand = ReactiveCommand.CreateFromTask(CheckUpdate);
        StartStressTestCommand = ReactiveCommand.CreateFromTask(StartStressTest);
        DownloadUpdateCommand = ReactiveCommand.CreateFromTask(DownloadUpdate);
        StartOnSystemBootCommand = ReactiveCommand.CreateFromTask(StartOnSystemBoot);

        IsAutoStartEnabled = Settings.Default.StartOnBoot;
        IsStartMinimizedEnabled = Settings.Default.StartMini;
        IsMinimizeOnClose = Settings.Default.MinimizeClose;
        IsReapplyOnStart = Settings.Default.ApplyOnStart;
        IsAutoReapply = Settings.Default.AutoReapply;
        ReapplySecond = Settings.Default.AutoReapplyTime;
        IsAutoCheckUpdates = Settings.Default.UpdateCheck;
        IsAutoStartAdaptiveMode = Settings.Default.isStartAdpative;
        IsAutoTrackGames = Settings.Default.isTrack;

        ApplicationVersion = App.Version;
    }

    private async Task StartOnSystemBoot()
    {
        if (IsAutoStartEnabled)
        {
            _systemBootService.CreateTask("UXTU", Path.Combine(App.RootDirectory, App.ExecutableFileName), taskDescription: "Start UXTU");
        }
        else
        {
            _systemBootService.DeleteTask("UXTU");
        }
    }

    private async Task StartStressTest()
    {
        // todo: create new service for run stress tests??
        if (File.Exists(Settings.Default.Path + @"\Assets\Stress-Test\AVX2 Stress Test.exe"))
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = @".\Assets\Stress-Test\AVX2 Stress Test.exe";
                process.Start();
            }
        }
    }

    private async Task DownloadUpdate()
    {
        await _toastNotificationsManager.ShowTextNotification(
            title: "Update available!",
            text: "Universal x86 Tuning Utility will close and the installer will open when the download is complete");

        try
        {
            await _updateInstallerService.DownloadAndInstallNewestPackage();
        }
        catch (Exception ex)
        {
            // log error or display error message to user
            _logger.LogError(ex, "Error occurred at downloading or installing update");
            await _toastNotificationsManager.ShowTextNotification(title: "Error",
                text: "Error occurred at downloading or installing update",
                notificationType: NotificationManagerExtensions.NotificationType.Error);
        }
    }

    private async Task CheckUpdate()
    {
        if (UpdateHelper.IsInternetAvailable())
        {
            var isUpdateAvailable = await _updateService.IsUpdatesAvailable(App.Version);

            if (isUpdateAvailable)
            {
                UpdatesMessage = "An update for Universal x86 Tuning Utility has been found!";
                IsUpdateAvailable = true;
            }
            else
            {
                UpdatesMessage = "Universal x86 Tuning Utility is up to date!";
            }
        }
        else
        {
            UpdatesMessage = "No internet connection!";
        }
    }
}