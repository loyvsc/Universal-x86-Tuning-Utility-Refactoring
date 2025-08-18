using System;
using System.Runtime.InteropServices;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.Win32;
using PowerModeChangedEventArgs = ApplicationCore.Events.PowerModeChangedEventArgs;
using PowerModeChangedEventHandler = ApplicationCore.Events.PowerModeChangedEventHandler;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsPowerPlanService : IPowerPlanService
{
    private readonly Serilog.ILogger _logger;
    private readonly IBatteryInfoService _batteryInfoService;
    public event PowerModeChangedEventHandler PowerModeChanged;
    
    public PowerPlan CurrentPowerPlan
    {
        get
        {
            if (PowerGetActualOverlayScheme(out var activePowerSchemeGuid))
            {
                if (activePowerSchemeGuid == _balancedPowerSchemeGuid)
                {
                    return PowerPlan.Balance;
                }
                if (activePowerSchemeGuid == _highPerformancePowerSchemeGuid)
                {
                    return PowerPlan.HighPerformance;
                }
                if (activePowerSchemeGuid == _powerSavePowerSchemeGuid)
                {
                    return PowerPlan.PowerSave;
                }
            }
            
            return PowerPlan.Unknown;
        }
    }

    [DllImport("powrprof.dll", EntryPoint = "PowerSetActiveOverlayScheme")]
    private static extern uint PowerSetActiveOverlayScheme(Guid OverlaySchemeGuid);
    
    [DllImport("powrprof.dll", EntryPoint = "PowerGetActualOverlayScheme")]
    private static extern bool PowerGetActualOverlayScheme(out Guid ActualOverlayGuid);
    
    private readonly Guid _balancedPowerSchemeGuid = new Guid("00000000-0000-0000-0000-000000000000");
    private readonly Guid _highPerformancePowerSchemeGuid = new Guid("DED574B5-45A0-4F42-8737-46345C09C238");
    private readonly Guid _powerSavePowerSchemeGuid = new Guid("961CC777-2547-4F9D-8174-7D86181b8A7A");

    public WindowsPowerPlanService(Serilog.ILogger logger, IBatteryInfoService batteryInfoService)
    {
        _logger = logger;
        _batteryInfoService = batteryInfoService;

        SystemEvents.PowerModeChanged += OnPowerModeChanged;
    }

    private void OnPowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
    {
        try
        {
            var batteryStatus = _batteryInfoService.GetBatteryStatus();
            var currentPowerMode = e.Mode switch
            {
                PowerModes.Resume => PowerMode.Resume,
                PowerModes.StatusChange => PowerMode.StatusChange,
                PowerModes.Suspend => PowerMode.Suspend
            };
            var powerModeChangedEventArgs = new PowerModeChangedEventArgs(batteryStatus, currentPowerMode);
            PowerModeChanged?.Invoke(powerModeChangedEventArgs);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when handling power mode changed");
        }
    }

    public void SetPowerPlan(PowerPlan powerPlan)
    {
        switch (powerPlan)
        {
            case PowerPlan.PowerSave:
            {
                _ = PowerSetActiveOverlayScheme(_powerSavePowerSchemeGuid);
                break;
            }
            case PowerPlan.Balance:
            {
                _ = PowerSetActiveOverlayScheme(_balancedPowerSchemeGuid);
                break;
            }
            case PowerPlan.HighPerformance:
            {
                _ = PowerSetActiveOverlayScheme(_highPerformancePowerSchemeGuid);
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
}