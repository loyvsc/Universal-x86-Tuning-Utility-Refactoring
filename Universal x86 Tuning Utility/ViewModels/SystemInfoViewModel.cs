using System;
using System.Collections.ObjectModel;
using System.Linq;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using Avalonia.Threading;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Helpers;
using Universal_x86_Tuning_Utility.Models;

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

    public ObservableCollection<BatteryModel> Batteries
    {
        get => _batteries;
        set => this.RaiseAndSetIfChanged(ref _batteries, value);
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
    private ObservableCollection<BatteryModel> _batteries = new ObservableCollection<BatteryModel>();

    public SystemInfoViewModel(ISystemInfoService systemInfoService, IBatteryInfoService batteryInfoService)
    {
        _systemInfoService = systemInfoService;
        _batteryInfoService = batteryInfoService;
        _batteryInfoTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        CpuInfo = _systemInfoService.Cpu;

        DeviceName = _systemInfoService.SystemName;
        DeviceManufacturer = _systemInfoService.Manufacturer;
        DeviceModel = _systemInfoService.Product;

        CpuCoresInfo = CpuInfo.BigLITTLEInfo ?? CpuInfo.CoresCount.ToString();
            
        CpuL1Cache = SizesConverter.ToString(_systemInfoService.Cpu.L1Size);
        CpuL2Cache = SizesConverter.ToString(_systemInfoService.Cpu.L2Size);
        CpuL3Cache = SizesConverter.ToString(_systemInfoService.Cpu.L3Size);
            
        CpuBaseClock = $"{_systemInfoService.Cpu.BaseClock} MHz";
        CpuInstructions = string.Join(", ", CpuInfo.SupportedInstructions);

        RamInfo = $"{_systemInfoService.Ram.Capacity} GB {_systemInfoService.Ram.Type.ToString()} @ {_systemInfoService.Ram.Speed} MT/s";
        
        if (_systemInfoService.Ram.Modules.Count > 1)
        {
            if (_systemInfoService.Ram.Modules
                .All(module => module.Producer == _systemInfoService.Ram.Modules.ElementAt(0).Producer))
            {
                RamProducer = _systemInfoService.Ram.Modules.ElementAt(0).Producer;
            }
            else
            {
                RamProducer = string.Join(" / ", _systemInfoService.Ram.Modules.Select(module => module.Producer));
            }
            
            if (_systemInfoService.Ram.Modules
                .All(module => module.Model == _systemInfoService.Ram.Modules.ElementAt(0).Model))
            {
                RamModel = _systemInfoService.Ram.Modules.ElementAt(0).Model;
            }
            else
            {
                RamModel = string.Join(" / ", _systemInfoService.Ram.Modules.Select(module => module.Model));
            }
        }
        else
        {
            RamProducer = _systemInfoService.Ram.Modules.ElementAt(0).Producer;
            RamModel = _systemInfoService.Ram.Modules.ElementAt(0).Model;
        }
        
        RamWidth = $"{_systemInfoService.Ram.Width} bit";
        RamTimings = _systemInfoService.Ram.Timings;

        int modulesCount = _systemInfoService.Ram.Modules.Count;
        RamSlots = $"{modulesCount} * {_systemInfoService.Ram.Width / modulesCount} bit";

        if (_batteryInfoService.GetBatteryStatus() != BatteryStatus.NoSystemBattery)
        {
            try
            {
                InitializeBatteryInfo();
                
                _batteryInfoTimer.Tick += OnBatteryInfoTimerTick;
                _batteryInfoTimer.Start();
                IsBatteryInfoAvailable = true;
                
                _batteryInfoService.BatteryCountChanged += OnBatteryCountChanged;
            }
            catch
            {
                IsBatteryInfoAvailable = false;
            }
        }
    }

    private void OnBatteryCountChanged()
    {
        Batteries.Clear();
        InitializeBatteryInfo();
    }

    private void InitializeBatteryInfo()
    {
        for (int i = 0; i < _batteryInfoService.Batteries.Count; i++)
        {
            var batteryInfo = _batteryInfoService.Batteries.ElementAt(i);
            var battery = new BatteryModel()
            {
                Index = i + 1,
                DeviceId = batteryInfo.DeviceId,
                BatteryHealth = batteryInfo.Health.Value.ToString("0.##%"),
                BatteryCycle = batteryInfo.CycleCount.Value.ToString(),
                BatteryCapacity = $"Full Charge: {batteryInfo.FullChargeCapacity.Value} mAh | Design: {batteryInfo.DesignCapacity.Value} mAh",
                BatteryChargeRate = (batteryInfo.Rate.Value / 1000).ToString("0.##W")
            };
            Batteries.Add(battery);
        }
    }

    private void OnBatteryInfoTimerTick(object? sender, EventArgs e)
    {
        foreach (var battery in Batteries)
        {
            battery.BatteryChargeRate = (_batteryInfoService.GetBatteryRate(battery.DeviceId) / 1000).ToString("0.##W");
            battery.BatteryHealth = _batteryInfoService.GetBatteryHealth(battery.DeviceId).ToString("0.##%");
            battery.BatteryCycle = _batteryInfoService.GetBatteryCycle(battery.DeviceId).ToString();

            var fullChargeCapacity = _batteryInfoService.GetFullChargeCapacity(battery.DeviceId);
            var designCapacity = _batteryInfoService.GetDesignCapacity(battery.DeviceId);
            battery.BatteryCapacity = $"Full Charge: {fullChargeCapacity} mAh | Design: {designCapacity} mAh";
        }
    }

    public void Dispose()
    {
        if (IsBatteryInfoAvailable)
        {
            _batteryInfoService.BatteryCountChanged -= OnBatteryCountChanged;
        }
        
        _batteryInfoTimer.Stop();
    }
}