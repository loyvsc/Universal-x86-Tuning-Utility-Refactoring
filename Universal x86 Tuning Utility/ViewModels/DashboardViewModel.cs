using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using Universal_x86_Tuning_Utility.Services.GameLauncherServices;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class DashboardViewModel : NotifyPropertyChangedBase
{
    private readonly ISystemInfoService _systemInfoService;


    private ICommand _openWindowCommand;


    public ICommand OpenWindowCommand => _openWindowCommand ??= new RelayCommand<string>(OnOpenWindow);

    public DispatcherTimer autoAdaptive = new DispatcherTimer();
    public DashboardViewModel(ISystemInfoService systemInfoService)
    {
        _navigationService = navigationService;
        _systemInfoService = systemInfoService;

        if (Family.TYPE == Family.ProcessorType.Intel)
        {
            caPremade.IsEnabled = false;
            btnPremade.IsEnabled = false;
        }

        autoAdaptive.Interval = TimeSpan.FromSeconds(1);
        autoAdaptive.Tick += AutoAdaptive_Tick;
        autoAdaptive.Start();
    }

    public void AutoAdaptive_Tick(object sender, EventArgs e)
    {
        if(Settings.Default.isStartAdpative) _navigationService.Navigate(typeof(Views.Pages.AdaptivePage));
        autoAdaptive.Stop();
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
                toastNotiifcation.Show("Worked!");
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