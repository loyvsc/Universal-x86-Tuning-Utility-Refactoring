using System.Diagnostics;
using System;
using System.IO;
using Universal_x86_Tuning_Utility.Properties;
using System.Configuration;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DesktopNotifications;
using Microsoft.Extensions.Logging;
using Splat;
using Splat.Serilog;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Helpers;
using Universal_x86_Tuning_Utility.Services.Amd;
using Universal_x86_Tuning_Utility.Services.SystemInfoServices;
using Universal_x86_Tuning_Utility.ViewModels;
using Universal_x86_Tuning_Utility.Views.Pages;
using Universal_x86_Tuning_Utility.Views.Windows;
using Application = Avalonia.Application;

namespace Universal_x86_Tuning_Utility;

public class App : Application
{
    public static readonly string SCALE_MODELS_JSON_PATH = @".\ScaleModels.json";

    public static readonly string Version = typeof(AvaloniaObject).Assembly.GetName().Version.ToString(); // todo: test this
    public static readonly string RootDirectory = AppDomain.CurrentDomain.BaseDirectory;
    public static readonly string ExecutableFileName = Process.GetCurrentProcess().MainModule.FileName;

    private ILogger<App> _logger;
    private IClassicDesktopStyleApplicationLifetime _desktopApplicationLifetime;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
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
            Locator.CurrentMutable.UseSerilogFullLogger();
            
            //Services
            SplatRegistrationsExtensions.RegisterPlatformSpecificService<ISystemInfoService, WindowsSystemInfoService, LinuxSystemInfoService>();
            
            //Viewmodels
            SplatRegistrations.Register<MainWindowViewModel>();
            SplatRegistrations.Register<DashboardViewModel>();
            SplatRegistrations.Register<GamesViewModel>();
            SplatRegistrations.Register<CustomPresetsViewModel>();
            SplatRegistrations.Register<SettingsViewModel>();
            SplatRegistrations.Register<DataViewModel>();
            
            //Pages
            SplatRegistrations.Register<DashboardPage>();
            SplatRegistrations.Register<CustomPresetsPage>();
            SplatRegistrations.Register<PremadePage>();
            SplatRegistrations.Register<AdaptivePage>();
            SplatRegistrations.Register<AutomationsPage>();
            SplatRegistrations.Register<FanControlPage>();
            SplatRegistrations.Register<SystemInfoPage>();
            SplatRegistrations.Register<GamesPage>();
            SplatRegistrations.Register<DataPage>();
            SplatRegistrations.Register<SettingsPage>();

            SplatRegistrations.SetupIOC();

            _logger = Locator.Current.GetService<ILogger<App>>()!;

            try
            {
                if (Settings.Default.SettingsUpgradeRequired)
                {
                    try
                    {
                        Settings.Default.Upgrade();
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
                // todo: check inner exception
                string filename = ((ConfigurationErrorsException)ex.InnerException).Filename;
                File.Delete(filename);
                Settings.Default.Reload();
            }
            
            if (UpdateHelper.IsInternetAvailable() && Settings.Default.UpdateCheck)
            {
                var updateManager = Locator.Current.GetService<IUpdateService>()!; 
                var isUpdateAvailable = await updateManager.IsUpdatesAvailable(Version);

                if (isUpdateAvailable)
                {
                    var notificationService = Locator.Current.GetService<INotificationManager>()!;
                    await notificationService.ShowTextNotification(
                        title: "New Update Available!", 
                        text: "Head to the settings menu to easily download the new Universal x86 Tuning Utility update!");
                }
            }
        } 
        catch (Exception ex) 
        { 
            _logger.LogCritical(ex, "Failed to build and start a host");
        }
    }

    private void OnCloseMenuItemClick(object? sender, EventArgs e)
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