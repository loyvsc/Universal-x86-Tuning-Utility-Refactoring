using System;
using System.Runtime.InteropServices;
using ApplicationCore.Enums;
using ApplicationCore.Events;
using ApplicationCore.Interfaces;
using Microsoft.Win32;
using PowerModeChangedEventArgs = ApplicationCore.Events.PowerModeChangedEventArgs;
using PowerModeChangedEventHandler = ApplicationCore.Events.PowerModeChangedEventHandler;

namespace Universal_x86_Tuning_Utility.Services.PowerPlanServices;

public class WindowsPowerPlanService : IPowerPlanService
{
    private readonly ISystemInfoService _systemInfoService;
    public event PowerModeChangedEventHandler PowerModeChanged;
    
    [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveOverlayScheme")]
    private static extern uint PowerSetActiveOverlayScheme(Guid OverlaySchemeGuid);

    private const string BalancedPowerScheme = "00000000-0000-0000-0000-000000000000";
    private const string HighPerformancePowerScheme = "DED574B5-45A0-4F42-8737-46345C09C238";
    private const string PowerSaverPowerScheme = "961CC777-2547-4F9D-8174-7D86181b8A7A";

    public WindowsPowerPlanService(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
        
        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
    {
        var batteryStatus = _systemInfoService.GetBatteryStatus();
        var currentPowerMode = e.Mode switch
        {
            PowerModes.Resume => PowerMode.Resume,
            PowerModes.StatusChange => PowerMode.StatusChange,
            PowerModes.Suspend => PowerMode.Suspend
        };
        var powerModeChangedEventArgs = new PowerModeChangedEventArgs(batteryStatus, currentPowerMode);
        PowerModeChanged?.Invoke(powerModeChangedEventArgs);
    }

    public void SetPowerPlan(PowerPlan powerPlan)
    {
        switch (powerPlan)
        {
            case PowerPlan.PowerSave:
            {
                _ = PowerSetActiveOverlayScheme(new Guid(PowerSaverPowerScheme.ToLower()));
                break;
            }
            case PowerPlan.Balance:
            {
                _ = PowerSetActiveOverlayScheme(new Guid(BalancedPowerScheme.ToLower()));
                break;
            }
            case PowerPlan.HighPerformance:
            {
                _ = PowerSetActiveOverlayScheme(new Guid(HighPerformancePowerScheme.ToLower()));
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(powerPlan),
                    powerPlan, "Invalid PowerPlan scheme");
        }
    }

    public void Dispose()
    {
        SystemEvents.PowerModeChanged -= OnPowerModeChanged;
    }
    
    // public async Task HideAttribute(string subGroup, string attribute)
    // {
    //     await Task.Run(() =>
    //     {
    //         // Execute the "powercfg -attributes" command to hide the attribute
    //         var processStartInfo = new ProcessStartInfo
    //         {
    //             FileName = "powercfg",
    //             Arguments = $"-attributes {subGroup} {attribute} -ATTRIB_HIDE",
    //             UseShellExecute = false,
    //             CreateNoWindow = true,
    //         };
    //         
    //         using(var process = new Process())
    //         {
    //             process.StartInfo = processStartInfo;
    //             process.Start();
    //             await process.WaitForExitAsync();
    //         }
    //     });
    // }
    
    // public async Task SetPowerValue(string scheme, string subGroup, string powerSetting, uint value, bool isAc)
    // {
    //     // Execute the "powercfg /setacvalueindex" or "powercfg /setdcvalueindex" command to set the power value
    //     var processStartInfo = new ProcessStartInfo
    //     {
    //         FileName = "powercfg",
    //         Arguments = $"/set{(isAC ? "ac" : "dc")}valueindex {scheme} {subGroup} {powerSetting} {value}",
    //         UseShellExecute = false,
    //         CreateNoWindow = true,
    //     };
    //         
    //     using (var process = new Process())
    //     {
    //         process.StartInfo = processStartInfo;
    //         process.Start();
    //         await process.WaitForExitAsync();
    //     }
    // }
    //
    // public async Task SetActiveScheme(string scheme)
    // {
    //     // Execute the "powercfg /setactive" command to activate the power scheme
    //     var processStartInfo = new ProcessStartInfo
    //     {
    //         FileName = "powercfg",
    //         Arguments = $"/setactive {scheme}",
    //         UseShellExecute = false,
    //         CreateNoWindow = true,
    //     };
    //         
    //     using (var process = new Process())
    //     {
    //         process.StartInfo = processStartInfo;
    //         process.Start();
    //         await process.WaitForExitAsync();
    //     }
    // }
}