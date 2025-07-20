using System;
using System.IO;
using ApplicationCore.Interfaces;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using DAL.Services;
using DesktopNotifications;
using Microsoft.Extensions.Logging;
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
using INotificationManager = DesktopNotifications.INotificationManager;

namespace Universal_x86_Tuning_Utility;

public class App : Application
{
    private ILogger<App> _logger;
    private IClassicDesktopStyleApplicationLifetime _desktopApplicationLifetime;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        Locator.CurrentMutable.UseSerilogFullLogger();
        Locator.CurrentMutable.UseMicrosoftExtensionsLoggingWithWrappingFullLogger(new SerilogLoggerFactory());

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
        }

        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>
    /// Occurs when the application is loading.
    /// </summary>
    private async void OnStartup(object? sender, ControlledApplicationLifetimeStartupEventArgs e)
    {
        try
        {
            // todo: check logging service registration
            _logger = Locator.Current.GetService<ILogger<App>>()!;

            try
            {
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
            }
            catch (ConfigurationErrorsException ex)
            {
                string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;
                File.Delete(filename);
                Settings.Default.Reload();
            }

            // todo: refact updatehelper to https://learn.microsoft.com/en-us/windows/win32/api/wininet/nf-wininet-internetgetconnectedstate
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
        var mainWindow = _desktopApplicationLifetime.MainWindow!;
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