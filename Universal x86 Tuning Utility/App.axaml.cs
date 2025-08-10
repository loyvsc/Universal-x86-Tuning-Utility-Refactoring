using System;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CastelloBranco.AvaloniaMessageBox;
using DAL.Services;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Serilog.Extensions.Logging;
using Splat;
using Splat.Microsoft.Extensions.Logging;
using Splat.Serilog;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Helpers;
using Universal_x86_Tuning_Utility.Interfaces;
using Universal_x86_Tuning_Utility.Navigation;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services;
using Universal_x86_Tuning_Utility.Services.GPUs;
using Universal_x86_Tuning_Utility.Services.PresetServices;
using Universal_x86_Tuning_Utility.ViewModels;
using Universal_x86_Tuning_Utility.Views.Windows;
using Application = Avalonia.Application;
using ILogger = Splat.ILogger;
using INotificationManager = DesktopNotifications.INotificationManager;

namespace Universal_x86_Tuning_Utility;

public class App : Application
{
    private ILogger<App> _logger;
    private IClassicDesktopStyleApplicationLifetime _desktopApplicationLifetime;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        //Viewmodels
        SplatRegistrations.RegisterLazySingleton<AdaptiveViewModel>();
        SplatRegistrations.RegisterLazySingleton<AutomationsViewModel>();
        SplatRegistrations.RegisterLazySingleton<CustomPresetsViewModel>();
        SplatRegistrations.RegisterLazySingleton<DashboardViewModel>();
        SplatRegistrations.RegisterLazySingleton<DataViewModel>();
        SplatRegistrations.RegisterLazySingleton<FanControlViewModel>();
        SplatRegistrations.RegisterLazySingleton<GamesViewModel>();
        SplatRegistrations.RegisterLazySingleton<MainWindowViewModel>();
        SplatRegistrations.RegisterLazySingleton<PremadePresetsViewModel>();
        SplatRegistrations.RegisterLazySingleton<SettingsViewModel>();
        SplatRegistrations.RegisterLazySingleton<SystemInfoViewModel>();

        SplatRegistrations.RegisterLazySingleton<IAdaptivePresetService, AdaptivePresetService>();
        SplatRegistrations.RegisterLazySingleton<IGameDataService, GameDataService>();
        SplatRegistrations.RegisterLazySingleton<IPremadePresets, PremadePresets>();
        SplatRegistrations.RegisterLazySingleton<IPresetService, PresetService>();
        SplatRegistrations.RegisterLazySingleton<IPresetServiceFactory, PresetServiceFactory>();
        SplatRegistrations.RegisterLazySingleton<INavigationService, NavigationService>();
        SplatRegistrations.RegisterLazySingleton<IUpdateService, UpdateService>();
        SplatRegistrations.RegisterLazySingleton<IPlatformServiceAccessor, PlatformServiceAccessor>();
        SplatRegistrations.RegisterLazySingleton<IAmdApuControlService, AmdApuControlService>();

        SplatRegistrations.SetupIOC();

        Locator.CurrentMutable.UseMicrosoftExtensionsLoggingWithWrappingFullLogger(new SerilogLoggerFactory());
        Locator.CurrentMutable.UseSerilogFullLogger();
        Locator.CurrentMutable.RegisterLazySingleton<IAdaptivePresetService>(() => new AdaptivePresetService("/AdaptivePresets"));
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _desktopApplicationLifetime = desktop;
            _desktopApplicationLifetime.MainWindow = new MainWindow()
            {
                DataContext = Locator.Current.GetService<MainWindowViewModel>()
            };
            _desktopApplicationLifetime.Startup += OnStartup;
            _desktopApplicationLifetime.Exit += OnExit;
            
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;

            RxApp.DefaultExceptionHandler = new RxAppObservableExceptionHandler();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        TaskScheduler.UnobservedTaskException -= TaskSchedulerOnUnobservedTaskException;
        AppDomain.CurrentDomain.UnhandledException -= CurrentDomainOnUnhandledException;
    }

    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
    {
        var ex = (Exception) args.ExceptionObject;
        
        HandeUnhandledException(ex);
    }

    private void TaskSchedulerOnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs args)
    {
        args.SetObserved();

        HandeUnhandledException(args.Exception);
    }

    private async void HandeUnhandledException(Exception ex)
    {
        await ExceptionMessageBox.ShowExceptionDialogAsync(null, ex);
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private async void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        try
        {
            _logger = Locator.Current.GetService<ILogger<App>>()!;

            if (Settings.Default.SettingsUpgradeRequired)
            {
                try
                {
                    Settings.Default.SettingsUpgradeRequired = false;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update settings on startup");
                }
            }
            
            if (UpdateHelper.IsInternetAvailable() && Settings.Default.UpdateCheck)
            {
                var updateManager = Locator.Current.GetService<IUpdateService>()!;
                var platformServiceAccessor = Locator.Current.GetService<IPlatformServiceAccessor>()!;
                var isUpdateAvailable = await updateManager.IsUpdatesAvailable(platformServiceAccessor.ProductVersion);

                if (isUpdateAvailable)
                {
                    var notificationService = Locator.Current.GetService<INotificationManager>()!;
                    await notificationService.ShowTextNotification(
                        title: "New Update Available!",
                        text:
                        "Head to the settings menu to easily download the new Universal x86 Tuning Utility update!");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to build and start a host");
        }
    }

    private void OnCloseMenuItemClicked(object? sender, EventArgs e)
    {
        _desktopApplicationLifetime.Shutdown();
    }

    private void OnTrayIconClicked(object? sender, EventArgs e)
    {
        var mainWindow = _desktopApplicationLifetime.MainWindow;
        if (mainWindow != null)
        {
            if (mainWindow.WindowState != WindowState.Minimized)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            else
            {
                mainWindow.Show();
                mainWindow.WindowState = WindowState.Normal;
            }
        }
    }
}