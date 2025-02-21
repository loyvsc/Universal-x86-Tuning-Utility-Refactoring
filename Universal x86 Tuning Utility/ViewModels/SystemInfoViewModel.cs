using System;
using System.Linq;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using Avalonia.Threading;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class SystemInfoViewModel : NotifyPropertyChangedBase, IDisposable
{
    public CpuInfo CpuInfo
    {
        get => _cpuInfoInfo;
        set => SetValue(ref _cpuInfoInfo, value);
    }

    public string DeviceName
    {
        get => _deviceName;
        set => SetValue(ref _deviceName, value);
    }

    public string DeviceManufacturer
    {
        get => _deviceManufacturer;
        set => SetValue(ref _deviceManufacturer, value);
    }

    public string DeviceModel
    {
        get => _deviceModel;
        set => SetValue(ref _deviceModel, value);
    }

    public string RamInfo
    {
        get => _ramInfo;
        set => SetValue(ref _ramInfo, value);
    }

    public string RamProducer
    {
        get => _ramProducer;
        set => SetValue(ref _ramProducer, value);
    }

    public string RamModel
    {
        get => _ramModel;
        set => SetValue(ref _ramModel, value);
    }

    public string RamWidth
    {
        get => _ramWidth;
        set => SetValue(ref _ramWidth, value);
    }

    public string RamSlots
    {
        get => _ramSlots;
        set => SetValue(ref _ramSlots, value);
    }

    public bool IsBatteryInfoAvailable
    {
        get => _isBatteryInfoAvailable;
        set => SetValue(ref _isBatteryInfoAvailable, value);
    }

    public string BatteryHealth
    {
        get => _batteryHealth;
        set => SetValue(ref _batteryHealth, value);
    }

    public string BatteryCycle
    {
        get => _batteryCycle;
        set => SetValue(ref _batteryCycle, value);
    }

    public string BatteryCapacity
    {
        get => _batteryCapacity;
        set => SetValue(ref _batteryCapacity, value);
    }

    public string BatteryChargeRate
    {
        get => _batteryChargeRate;
        set => SetValue(ref _batteryChargeRate, value);
    }

    public string CpuCoresInfo
    {
        get => _cpuCoresInfo;
        set => SetValue(ref _cpuCoresInfo, value);
    }

    public string CpuBaseClock
    {
        get => _cpuBaseClock;
        set => SetValue(ref _cpuBaseClock, value);
    }

    public string CpuInstructions
    {
        get => _cpuInstructions;
        set => SetValue(ref _cpuInstructions, value);
    }

    public string CpuL1Cache
    {
        get => _cpuL1Cache;
        set => SetValue(ref _cpuL1Cache, value);
    }

    public string CpuL2Cache
    {
        get => _cpuL2Cache;
        set => SetValue(ref _cpuL2Cache, value);
    }

    public string CpuL3Cache
    {
        get => _cpuL3Cache;
        set => SetValue(ref _cpuL3Cache, value);
    }

    public string CpuL4Cache
    {
        get => _cpuL4Cache;
        set => SetValue(ref _cpuL4Cache, value);
    }
    
    private CpuInfo _cpuInfoInfo;
    private string _deviceName;
    private string _deviceManufacturer;
    private string _deviceModel;
    private string _ramInfo;
    private string _ramProducer;
    private string _ramModel;
    private string _ramWidth;
    private string _ramSlots;
    private string _batteryHealth;
    private string _batteryCycle;
    private string _batteryCapacity;
    private string _batteryChargeRate;
    private string _cpuCoresInfo;
    private string _cpuBaseClock;
    private string _cpuInstructions;
    private string _cpuL1Cache;
    private string _cpuL2Cache;
    private string _cpuL3Cache;
    private string _cpuL4Cache;
    private bool _isBatteryInfoAvailable;
    
    private readonly ISystemInfoService _systemInfoService;
    private readonly DispatcherTimer _batteryInfoTimer;
    
    public SystemInfoViewModel(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
        _batteryInfoTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        CpuInfo = _systemInfoService.Cpu;
        
        CpuCoresInfo = CpuInfo.LogicalCoresCount == CpuInfo.CoresCount 
            ? CpuInfo.CoresCount.ToString() 
            : _systemInfoService.GetBigLITTLE(CpuInfo.CoresCount, CpuInfo.L2Size);
            
        var l1Size = _systemInfoService.Cpu.L1Size / 1024;
        CpuL1Cache = $"{l1Size.ToString("0.##")} MB";
        
        var l2Size = _systemInfoService.Cpu.L2Size/ 1024;
        CpuL2Cache = $"{l2Size.ToString("0.##")} MB";

        var l3Size = _systemInfoService.Cpu.L3Size/ 1024;
        CpuL3Cache = $"{l3Size:0.##} MB";
            
        CpuBaseClock = $"{_systemInfoService.Cpu.BaseClock} MHz";
        CpuInstructions = string.Join(", ", CpuInfo.SupportedInstructions);

        var ramCapacityAsGigabytes = _systemInfoService.Ram.Capacity / 1024 / 1024 / 1024;
        RamInfo = $"{ramCapacityAsGigabytes} GB {_systemInfoService.Ram.Type.ToString()} @ {_systemInfoService.Ram.Speed} MT/s";
        
        if (_systemInfoService.Ram.Modules.Length > 1 &&
            _systemInfoService.Ram.Modules
                .All(module => module.Producer == _systemInfoService.Ram.Modules[0].Producer))
        {
            RamProducer = _systemInfoService.Ram.Modules[0].Producer;
        }
        else
        {
            RamProducer = string.Join('/', _systemInfoService.Ram.Modules.Select(module => module.Producer));
        }

        RamModel = string.Join('/', _systemInfoService.Ram.Modules.Select(module => module.Model));
        RamWidth = $"{_systemInfoService.Ram.Width} bit";

        int modulesCount = _systemInfoService.Ram.Modules.Length;
        RamSlots = $"{modulesCount} * {_systemInfoService.Ram.Width / modulesCount} bit";

        if (_systemInfoService.GetBatteryStatus() != BatteryStatus.NoSystemBattery)
        {
            IsBatteryInfoAvailable = true;
            try
            {
                BatteryHealth = _systemInfoService.GetBatteryHealth().ToString("0.##%");
                BatteryCycle = _systemInfoService.GetBatteryCycle().ToString();

                var fullChargeCapacity = _systemInfoService.ReadFullChargeCapacity();
                var designCapacity = _systemInfoService.ReadDesignCapacity();
                BatteryCapacity = $"Full Charge: {fullChargeCapacity} mAh | Design: {designCapacity} mAh";

                BatteryChargeRate = (_systemInfoService.GetBatteryRate() / 1000).ToString("0.##W");
                _batteryInfoTimer.Tick += OnBatteryInfoTimerTick;
                _batteryInfoTimer.Start();
            }
            catch
            {
                IsBatteryInfoAvailable = false;
            }
        }
    }

    private void OnBatteryInfoTimerTick(object? sender, EventArgs e)
    {
        var batteryRate = _systemInfoService.GetBatteryRate() / 1000;
        BatteryChargeRate = batteryRate.ToString("0.##W");
    }

    public void Dispose()
    {
        _batteryInfoTimer.Stop();
    }
}