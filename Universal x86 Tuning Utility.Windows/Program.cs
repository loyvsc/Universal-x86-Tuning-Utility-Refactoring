using Avalonia;
using System;
using ApplicationCore.Interfaces;
using Avalonia.ReactiveUI;
using DesktopNotifications.Avalonia;
using Splat;
using Universal_x86_Tuning_Utility.Interfaces;
using Universal_x86_Tuning_Utility.Windows.Interfaces;
using Universal_x86_Tuning_Utility.Windows.Services;
using Universal_x86_Tuning_Utility.Windows.Services.Asus;
using Universal_x86_Tuning_Utility.Windows.Services.GPUs;
using Universal_x86_Tuning_Utility.Windows.Services.SystemInfoServices;

namespace Universal_x86_Tuning_Utility.Windows;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .With(() => new SkiaOptions()
            {
                MaxGpuResourceSizeBytes = 256_000_000
            })
            .With(() => new Win32PlatformOptions()
            {
                CompositionMode = new [] { Win32CompositionMode.DirectComposition },
                RenderingMode = new [] { Win32RenderingMode.AngleEgl }
            })
            .UsePlatformDetect()
            .WithInterFont()
            .SetupDesktopNotifications(out var notificationManager)
            .AfterPlatformServicesSetup(_ =>
            {
                SplatRegistrations.RegisterConstant(notificationManager!);
                SplatRegistrations.RegisterLazySingleton<IASUSWmiService, WindowsAsusWmiService>(); 
                SplatRegistrations.RegisterLazySingleton<ICliService, WindowsCliService>();
                SplatRegistrations.RegisterLazySingleton<IDisplayInfoService, WindowsDisplayInfoService>();
                SplatRegistrations.RegisterLazySingleton<IFanControlService, WindowsFanControlService>();
                SplatRegistrations.RegisterLazySingleton<IGameLauncherService, WindowsGameLauncherService>();
                SplatRegistrations.RegisterLazySingleton<IAmdGpuService, WindowsAmdGpuService>();
                SplatRegistrations.RegisterLazySingleton<INvidiaGpuService, WindowsNvidiaGpuService>();
                SplatRegistrations.RegisterLazySingleton<IIntelManagementService, WindowsIntelManagementService>();
                SplatRegistrations.RegisterLazySingleton<IPowerPlanService, WindowsPowerPlanService>();
                SplatRegistrations.RegisterLazySingleton<IRyzenAdjService, WindowsRyzenAdjService>();
                SplatRegistrations.RegisterLazySingleton<ISensorsService, WindowsSensorsService>();
                SplatRegistrations.RegisterLazySingleton<IRtssService, WindowsRtssService>();
                SplatRegistrations.RegisterLazySingleton<IStressTestService, WindowsStressTestService>();
                SplatRegistrations.RegisterLazySingleton<ISystemBootService, WindowsSystemBootService>();
                SplatRegistrations.RegisterLazySingleton<ISystemInfoService, WindowsSystemInfoService>();
                SplatRegistrations.RegisterLazySingleton<IUpdateInstallerService, WindowsUpdateInstallerService>();
                SplatRegistrations.RegisterLazySingleton<IBatteryInfoService, WindowsBatteryInfoService>();
                SplatRegistrations.RegisterLazySingleton<IIconExtractor, WindowsIconExtractor>();
                SplatRegistrations.RegisterLazySingleton<IXgMobileConnectionService, XgMobileConnectionService>();
                SplatRegistrations.RegisterLazySingleton<IManagementEventService, ManagementEventService>();
        
                SplatRegistrations.SetupIOC();
            })
            .LogToTrace()
            .UseReactiveUI();
}