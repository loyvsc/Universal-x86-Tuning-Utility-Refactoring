using Avalonia;
using System;
using ApplicationCore.Interfaces;
using Avalonia.Controls.ApplicationLifetimes;
using DAL.Services;
using DesktopNotifications.FreeDesktop;
using Splat;
using Universal_x86_Tuning_Utility.Interfaces;
using Universal_x86_Tuning_Utility.Linux.Services;
using Universal_x86_Tuning_Utility.Linux.Services.GPUs;

namespace Universal_x86_Tuning_Utility.Linux;

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
            .UseX11()
            .UseSkia()
            .AfterSetup(builder =>
            {
                var context = FreeDesktopApplicationContext.FromCurrentProcess();
                var manager = new FreeDesktopNotificationManager(context);
                manager.Initialize().GetAwaiter().GetResult();

                builder.AfterSetup(b =>
                {
                    if (b.Instance?.ApplicationLifetime is IControlledApplicationLifetime lifetime)
                    {
                        lifetime.Exit += (s, e) => { manager.Dispose(); };
                    }
                });
                
                SplatRegistrations.RegisterConstant(manager!);
                SplatRegistrations.RegisterLazySingleton<IASUSWmiService, LinuxAsusWmiService>();
                SplatRegistrations.RegisterLazySingleton<ICliService, LinuxCliService>();
                SplatRegistrations.RegisterLazySingleton<ICpuControlService, LinuxCpuControlService>();
                SplatRegistrations.RegisterLazySingleton<IDisplayInfoService, LinuxDisplayInfoService>();
                SplatRegistrations.RegisterLazySingleton<IFanControlService, LinuxFanControlService>();
                SplatRegistrations.RegisterLazySingleton<IGameLauncherService, LinuxGameLauncherService>();
                SplatRegistrations.RegisterLazySingleton<IAmdGpuService, LinuxAmdGpuService>();
                SplatRegistrations.RegisterLazySingleton<INvidiaGpuService, LinuxNvidiaGpuService>();
                SplatRegistrations.RegisterLazySingleton<IIntelManagementService, LinuxIntelManagementService>();
                SplatRegistrations.RegisterLazySingleton<IPowerPlanService, LinuxPowerPlanService>();
                SplatRegistrations.RegisterLazySingleton<ISensorsService, LinuxSensorsService>();
                SplatRegistrations.RegisterLazySingleton<IRtssService, LinuxRtssService>();
                SplatRegistrations.RegisterLazySingleton<IStressTestService, LinuxStressTestService>();
                SplatRegistrations.RegisterLazySingleton<ISystemBootService, LinuxSystemBootService>();
                SplatRegistrations.RegisterLazySingleton<ISystemInfoService, LinuxSystemInfoService>();
                SplatRegistrations.RegisterLazySingleton<IUpdateService, UpdateService>();
                SplatRegistrations.RegisterLazySingleton<IUpdateInstallerService, LinuxUpdateInstallerService>();
                SplatRegistrations.RegisterLazySingleton<IBatteryInfoService, LinuxBatteryInfoService>();
                SplatRegistrations.RegisterLazySingleton<IIconExtracter, LinuxIconExtracter>();
        
                SplatRegistrations.SetupIOC();
            })
            .LogToTrace();
}