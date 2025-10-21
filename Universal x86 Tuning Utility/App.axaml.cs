using System;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DAL.Services;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using MsBox.Avalonia;
using ReactiveUI;
using Serilog;
using Serilog.Events;
using Splat;
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
using INotificationManager = DesktopNotifications.INotificationManager;

namespace Universal_x86_Tuning_Utility;

public class App : Application
{
    private Serilog.ILogger _logger;
    private IClassicDesktopStyleApplicationLifetime _desktopApplicationLifetime;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(path: "./logs/log.txt", 
                outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj} {NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .Enrich.FromLogContext()
            .CreateLogger();

        Locator.CurrentMutable.RegisterConstant(Log.Logger);
        
        SplatRegistrations.RegisterLazySingleton<IPremadePresets, PremadePresets>();
        SplatRegistrations.RegisterLazySingleton<IPresetService, PresetService>();
        SplatRegistrations.RegisterLazySingleton<IPresetServiceFactory, PresetServiceFactory>();
        SplatRegistrations.RegisterLazySingleton<INavigationService, NavigationService>();
        SplatRegistrations.RegisterLazySingleton<IUpdateService, UpdateService>();
        SplatRegistrations.RegisterLazySingleton<IPlatformServiceAccessor, PlatformServiceAccessor>();
        SplatRegistrations.RegisterLazySingleton<IAmdApuControlService, AmdApuControlService>();
        SplatRegistrations.RegisterLazySingleton<IImageService, ImageService>();
        SplatRegistrations.RegisterLazySingleton<ICpuControlService, CpuControlService>();
        SplatRegistrations.RegisterLazySingleton<ILaptopInfoFactory, LaptopInfoFactory>();
       
        Locator.CurrentMutable.RegisterLazySingleton<IAdaptivePresetService>(() => new AdaptivePresetService("AdaptivePresets"));
        Locator.CurrentMutable.RegisterLazySingleton<IGameDataService>(() => new GameDataService(Settings.Default.Path + "gameData.json"));
        // Locator.CurrentMutable.RegisterLazySingleton<IDialogService>(() => new DialogService(new DialogManager(), type => Locator.Current.GetService(type)));
        Locator.CurrentMutable.RegisterLazySingleton(() => (IDialogService)new DialogService(
            new DialogManager(
                viewLocator: new ViewLocator(),
                dialogFactory: new DialogFactory().AddMessageBox()),
            viewModelFactory: x => Locator.Current.GetService(x)));

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

        SplatRegistrations.SetupIOC();
    }

    private class ViewLocator : ViewLocatorBase
    {
        /// <inheritdoc />
        protected override string GetViewName(object viewModel) => viewModel.GetType().FullName!.Replace("ViewModel", "");
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            Settings.Default.Path = AppContext.BaseDirectory;
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
    }

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        Log.CloseAndFlush();
        
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
        await MessageBoxManager.GetMessageBoxStandard("Error", ex.ToString())
            .ShowDialogAsync();
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private async void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        try
        {
            _logger = Locator.Current.GetService<Serilog.ILogger>()!;
            _logger.Information("Application started");
            if (Settings.Default.SettingsUpgradeRequired)
            {
                try
                {
                    Settings.Default.SettingsUpgradeRequired = false;
                    Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Failed to update settings on startup");
                }
            }
            
            if (UpdateHelper.IsInternetAvailable() && Settings.Default.UpdateCheck)
            {
                var updateManager = Locator.Current.GetService<IUpdateService>()!;
                var platformServiceAccessor = Locator.Current.GetService<IPlatformServiceAccessor>()!;
                var isUpdateAvailable = await updateManager.CheckIsUpdatesAvailableAsync(platformServiceAccessor.ProductVersion);

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
            _logger.Warning(ex, "Failed to build and start a host");
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