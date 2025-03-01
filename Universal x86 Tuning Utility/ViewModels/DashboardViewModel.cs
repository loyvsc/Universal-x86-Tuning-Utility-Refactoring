using System;
using System.Diagnostics;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using Avalonia.Threading;
using DesktopNotifications;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Services.GameLauncherServices;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class DashboardViewModel : NotifyPropertyChangedBase
{
    private readonly INotificationManager _notificationManager;
    public ICommand OpenWindowCommand { get; }
    public ICommand NavigateCommand { get; }
    
    public bool IsAmdSettingsAvailable
    {
        get => _isAmdSettingsAvailable;
        set => SetValue(ref _isAmdSettingsAvailable, value);
    }

    private readonly DispatcherTimer _autoAdaptive = new();
    private bool _isAmdSettingsAvailable;
    
    public DashboardViewModel(ISystemInfoService systemInfoService,
                             INotificationManager notificationManager)
    {
        _notificationManager = notificationManager;
        IsAmdSettingsAvailable = systemInfoService.Cpu.Manufacturer == Manufacturer.AMD;

        _autoAdaptive.Interval = TimeSpan.FromSeconds(1);
        _autoAdaptive.Tick += AutoAdaptive_Tick;
        _autoAdaptive.Start();

        OpenWindowCommand = ReactiveCommand.Create<string>(OnOpenWindow);
        NavigateCommand = ReactiveCommand.Create<string>(OnNavigate);
    }

    private void AutoAdaptive_Tick(object sender, EventArgs e)
    {
        if (Settings.Default.isStartAdpative)
        {
            _navigationService.Navigate(typeof(Views.Pages.AdaptivePage));
        }
        _autoAdaptive.Stop();
    }

    public void OnNavigatedTo()
    {
        System.Diagnostics.Debug.WriteLine($"INFO | {typeof(DashboardViewModel)} navigated", "Wpf.Ui.Demo");
    }

    public void OnNavigatedFrom()
    {
        System.Diagnostics.Debug.WriteLine($"INFO | {typeof(DashboardViewModel)} navigated", "Wpf.Ui.Demo");
    }

    private void OnNavigate(string parameter)
    {
        switch (parameter)
        {
            case "premade":
                _navigationService.Navigate(typeof(Views.Pages.PremadePage));
                return;

            case "custom":
                _navigationService.Navigate(typeof(Views.Pages.CustomPresetsPage));
                return;

            case "adaptive":
                _navigationService.Navigate(typeof(Views.Pages.AdaptivePage));
                return;

            case "auto":
                _navigationService.Navigate(typeof(Views.Pages.AutomationsPage));
                return;

            case "info":
                _navigationService.Navigate(typeof(Views.Pages.SystemInfoPage));
                return;

            case "help":
                Process.Start(new ProcessStartInfo("http://www.discord.gg/3EkYMZGJwq") { UseShellExecute = true });
                return;

            case "support":
                Process.Start(new ProcessStartInfo("https://www.paypal.com/paypalme/JamesCJ60") { UseShellExecute = true });
                Process.Start(new ProcessStartInfo("https://patreon.com/uxtusoftware") { UseShellExecute = true });
                return;
            case "games":
                _navigationService.Navigate(typeof(Views.Pages.GamesPage));
                return;
            default:
                string[] parts = parameter.Split('-');
                if (!parts[0].Contains("Microsoft Store")) WindowsGameLauncherService.LaunchGame(parts[2], parts[0], parts[1], parts[1]);
                else WindowsGameLauncherService.LaunchGame(parts[1], parts[0], parts[1], parts[1]);
                return;
        }
    }


    private void OnOpenWindow(string parameter)
    {
        switch (parameter)
        {
            //case "open_window_store":
            //    _testWindowService.Show<Views.Windows.StoreWindow>();
            //    return;

            //case "open_window_manager":
            //    _testWindowService.Show<Views.Windows.TaskManagerWindow>();
            //    return;

            //case "open_window_editor":
            //    _testWindowService.Show<Views.Windows.EditorWindow>();
            //    return;

            //case "open_window_settings":
            //    _testWindowService.Show<Views.Windows.SettingsWindow>();
            //    return;

            //case "open_window_experimental":
            //    _testWindowService.Show<Views.Windows.ExperimentalWindow>();
            //    return;
        }
    }
}