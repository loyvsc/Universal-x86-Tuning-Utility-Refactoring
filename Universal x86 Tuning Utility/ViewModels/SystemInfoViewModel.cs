using System;
using System.Linq;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Avalonia.Threading;
using ReactiveUI;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class SystemInfoViewModel : ReactiveObject, IDisposable
{
    public CpuInfo CpuInfo
    {
        get => _cpuInfoInfo;
        set => this.RaiseAndSetIfChanged(ref _cpuInfoInfo, value);
    }

    public string DeviceName
    {
        get => _deviceName;
        set => this.RaiseAndSetIfChanged(ref _deviceName, value);
    }

    public string DeviceManufacturer
    {
        get => _deviceManufacturer;
        set => this.RaiseAndSetIfChanged(ref _deviceManufacturer, value);
    }

    public string DeviceModel
    {
        get => _deviceModel;
        set => this.RaiseAndSetIfChanged(ref _deviceModel, value);
    }

    public string RamInfo
    {
        get => _ramInfo;
        set => this.RaiseAndSetIfChanged(ref _ramInfo, value);
    }

    public string RamProducer
    {
        get => _ramProducer;
        set => this.RaiseAndSetIfChanged(ref _ramProducer, value);
    }

    public string RamModel
    {
        get => _ramModel;
        set => this.RaiseAndSetIfChanged(ref _ramModel, value);
    }

    public string RamWidth
    {
        get => _ramWidth;
        set => this.RaiseAndSetIfChanged(ref _ramWidth, value);
    }

    public string RamSlots
    {
        get => _ramSlots;
        set => this.RaiseAndSetIfChanged(ref _ramSlots, value);
    }

    public MemoryTimings RamTimings
    {
        get => _ramTimings;
        set => this.RaiseAndSetIfChanged(ref _ramTimings, value);
    }

    public bool IsBatteryInfoAvailable
    {
        get => _isBatteryInfoAvailable;
        set => this.RaiseAndSetIfChanged(ref _isBatteryInfoAvailable, value);
    }

    public string BatteryHealth
    {
        get => _batteryHealth;
        set => this.RaiseAndSetIfChanged(ref _batteryHealth, value);
    }

    public string BatteryCycle
    {
        get => _batteryCycle;
        set => this.RaiseAndSetIfChanged(ref _batteryCycle, value);
    }

    public string BatteryCapacity
    {
        get => _batteryCapacity;
        set => this.RaiseAndSetIfChanged(ref _batteryCapacity, value);
    }

    public string BatteryChargeRate
    {
        get => _batteryChargeRate;
        set => this.RaiseAndSetIfChanged(ref _batteryChargeRate, value);
    }

    public string CpuCoresInfo
    {
        get => _cpuCoresInfo;
        set => this.RaiseAndSetIfChanged(ref _cpuCoresInfo, value);
    }

    public string CpuBaseClock
    {
        get => _cpuBaseClock;
        set => this.RaiseAndSetIfChanged(ref _cpuBaseClock, value);
    }

    public string CpuInstructions
    {
        get => _cpuInstructions;
        set => this.RaiseAndSetIfChanged(ref _cpuInstructions, value);
    }

    public string CpuL1Cache
    {
        get => _cpuL1Cache;
        set => this.RaiseAndSetIfChanged(ref _cpuL1Cache, value);
    }

    public string CpuL2Cache
    {
        get => _cpuL2Cache;
        set => this.RaiseAndSetIfChanged(ref _cpuL2Cache, value);
    }

    public string CpuL3Cache
    {
        get => _cpuL3Cache;
        set => this.RaiseAndSetIfChanged(ref _cpuL3Cache, value);
    }

    public string CpuL4Cache
    {
        get => _cpuL4Cache;
        set => this.RaiseAndSetIfChanged(ref _cpuL4Cache, value);
    }
    
    private CpuInfo _cpuInfoInfo;
    private string _deviceName;
    private string _deviceManufacturer;
    private string _deviceModel;
    private string _ramInfo;
    private string _ramProducer;
    private string _ramModel;
    private string _ramWidth;
    private MemoryTimings _ramTimings;
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
    private readonly IBatteryInfoService _batteryInfoService;
    private readonly DispatcherTimer _batteryInfoTimer;
    
    public SystemInfoViewModel(ISystemInfoService systemInfoService, IBatteryInfoService batteryInfoService)
    {
        _systemInfoService = systemInfoService;
        _batteryInfoService = batteryInfoService;
        _batteryInfoTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        CpuInfo = _systemInfoService.Cpu;

        CpuCoresInfo = CpuInfo.BigLITTLEInfo ?? CpuInfo.CoresCount.ToString();
            
        var l1Size = _systemInfoService.Cpu.L1Size / 1024;
        CpuL1Cache = $"{l1Size.ToString("0.##")} MB";
        
        var l2Size = _systemInfoService.Cpu.L2Size / 1024;
        CpuL2Cache = $"{l2Size.ToString("0.##")} MB";

        var l3Size = _systemInfoService.Cpu.L3Size / 1024;
        CpuL3Cache = $"{l3Size:0.##} MB";
            
        CpuBaseClock = $"{_systemInfoService.Cpu.BaseClock} MHz";
        CpuInstructions = string.Join(", ", CpuInfo.SupportedInstructions);

        var ramCapacityAsGigabytes = _systemInfoService.Ram.Capacity / 1024 / 1024 / 1024;
        RamInfo = $"{ramCapacityAsGigabytes} GB {_systemInfoService.Ram.Type.ToString()} @ {_systemInfoService.Ram.Speed} MT/s";
        
        if (_systemInfoService.Ram.Modules.Count > 1 &&
            _systemInfoService.Ram.Modules
                .All(module => module.Producer == _systemInfoService.Ram.Modules.ElementAt(0).Producer))
        {
            RamProducer = _systemInfoService.Ram.Modules.ElementAt(0).Producer;
        }
        else
        {
            RamProducer = string.Join('/', _systemInfoService.Ram.Modules.Select(module => module.Producer));
        }

        RamModel = string.Join('/', _systemInfoService.Ram.Modules.Select(module => module.Model));
        RamWidth = $"{_systemInfoService.Ram.Width} bit";

        int modulesCount = _systemInfoService.Ram.Modules.Count;
        RamSlots = $"{modulesCount} * {_systemInfoService.Ram.Width / modulesCount} bit";

        if (_batteryInfoService.GetBatteryStatus() != BatteryStatus.NoSystemBattery)
        {
            try
            {
                BatteryHealth = _batteryInfoService.GetBatteryHealth().ToString("0.##%");
                BatteryCycle = _batteryInfoService.GetBatteryCycle().ToString();

                var fullChargeCapacity = _batteryInfoService.ReadFullChargeCapacity();
                var designCapacity = _batteryInfoService.ReadDesignCapacity();
                BatteryCapacity = $"Full Charge: {fullChargeCapacity} mAh | Design: {designCapacity} mAh";

                BatteryChargeRate = (_batteryInfoService.GetBatteryRate() / 1000).ToString("0.##W");
                _batteryInfoTimer.Tick += OnBatteryInfoTimerTick;
                _batteryInfoTimer.Start();
                IsBatteryInfoAvailable = true;
            }
            catch
            {
                IsBatteryInfoAvailable = false;
            }
        }
    }

    private void OnBatteryInfoTimerTick(object? sender, EventArgs e)
    {
        var batteryRate = _batteryInfoService.GetBatteryRate() / 1000;
        BatteryChargeRate = batteryRate.ToString("0.##W");
    }

    public void Dispose()
    {
        _batteryInfoTimer.Stop();
    }
}