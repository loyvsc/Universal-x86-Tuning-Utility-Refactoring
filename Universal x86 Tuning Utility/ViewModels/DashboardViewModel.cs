using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using Avalonia.Threading;
using DesktopNotifications;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Settings = Universal_x86_Tuning_Utility.Properties.Settings;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class DashboardViewModel : ReactiveObject
{
    private readonly ISystemInfoService _systemInfoService;
    private readonly INotificationManager _notificationManager;
    private readonly IGpuOriginalityService _gpuOriginalityService;
    private readonly INavigationService _navigationService;
    public ICommand OpenWindowCommand { get; }
    public ICommand NavigateCommand { get; }
    
    public bool IsAmdSettingsAvailable
    {
        get => _isAmdSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdSettingsAvailable, value);
    }

    private readonly DispatcherTimer _autoAdaptive = new();
    private bool _isAmdSettingsAvailable;
    
    public DashboardViewModel(ISystemInfoService systemInfoService,
                             INotificationManager notificationManager,
                             IGpuOriginalityService gpuOriginalityService,
                             INavigationService navigationService)
    {
        _systemInfoService = systemInfoService;
        _notificationManager = notificationManager;
        _gpuOriginalityService = gpuOriginalityService;
        _navigationService = navigationService;
        IsAmdSettingsAvailable = systemInfoService.Cpu.Manufacturer == Manufacturer.AMD;

        _autoAdaptive.Interval = TimeSpan.FromSeconds(1);
        _autoAdaptive.Tick += AutoAdaptive_Tick;
        _autoAdaptive.Start();

        OpenWindowCommand = ReactiveCommand.Create<string>(OnOpenWindow);
        NavigateCommand = ReactiveCommand.Create<string>(OnNavigate);
    }

    private void AutoAdaptive_Tick(object? sender, EventArgs e)
    {
        _autoAdaptive.Stop();
        var checkResults = _gpuOriginalityService.CheckIsGpusOriginal();
        foreach (var checkResult in checkResults.results)
        {
            if (!checkResult.IsGpuOriginal)
            {
                var sb = StringBuilderPool.Rent();
                sb.Append($"Possible fake or modified GPU detected on {checkResult.GpuName}");
                if (checkResults.results.Count() > 1)
                {
                    sb.Append($" (№{checkResult.GpuNumber})");
                }
                
                _notificationManager.ShowTextNotification("GPU Warning", sb.ToString());
                
                StringBuilderPool.Return(sb);
            }
        }
        
        if (checkResults.notFoundNames.Any())
        {
            _notificationManager.ShowTextNotification("GPU Warning",
                $"GPU specification not found in reference database for {string.Join(", ", checkResults.notFoundNames)}"
            );
        }
        
        if (Settings.Default.isStartAdpative)
        {
            _navigationService.Navigate(typeof(Views.Pages.AdaptivePage));
        }
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
                _navigationService.Navigate(typeof(PremadePresetsViewModel));
                break;
            case "custom":
                _navigationService.Navigate(typeof(CustomPresetsViewModel));
                break;
            case "adaptive":
                _navigationService.Navigate(typeof(AdaptiveViewModel));
                break;
            case "auto":
                _navigationService.Navigate(typeof(AutomationsViewModel));
                break;
            case "info":
                _navigationService.Navigate(typeof(SystemInfoViewModel));
                break;
            case "help":
                Process.Start(new ProcessStartInfo("http://www.discord.gg/3EkYMZGJwq") { UseShellExecute = true });
                break;
            case "support":
                Process.Start(new ProcessStartInfo("https://www.paypal.com/paypalme/JamesCJ60") { UseShellExecute = true });
                Process.Start(new ProcessStartInfo("https://patreon.com/uxtusoftware") { UseShellExecute = true });
                break;
            case "games":
                _navigationService.Navigate(typeof(GamesViewModel));
                break;
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