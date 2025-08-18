using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Enums.Display;
using ApplicationCore.Enums.Laptop;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Models.LaptopInfo;
using ApplicationCore.Utilities;
using DesktopNotifications;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Models;
using Universal_x86_Tuning_Utility.Properties;
using AmdPowerProfile = ApplicationCore.Models.AmdPowerProfile;
using ILogger = Serilog.ILogger;
using PowerMode = ApplicationCore.Models.PowerMode;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class CustomPresetsViewModel : ReactiveObject, IDisposable
{
    public ICommand ApplyPresetCommand { get; }
    public ICommand SavePresetCommand { get; }
    public ICommand DeletePresetCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ReloadPresetValuesCommand { get; }
    public ICommand AsusGpuUltimateModSwitchedCommand { get; }
    public ICommand IdentificateMonitorsCommand { get; }

    #region Properties

    public bool IsNvidiaGpuSettingsAvailable
    {
        get => _isNvidiaGpuSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isNvidiaGpuSettingsAvailable, value);
    }

    public bool IsRadeonGpuSettingsAvailable
    {
        get => _isIsRadeonGpuSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isIsRadeonGpuSettingsAvailable, value);
    }

    public List<Preset> AvailablePresets
    {
        get => _availablePresets;
        set => this.RaiseAndSetIfChanged(ref _availablePresets, value);
    }

    public Preset CurrentPreset
    {
        get => _currentPreset;
        set => this.RaiseAndSetIfChanged(ref _currentPreset, value);
    }

    public bool UndoActionAvailable
    {
        get => _undoActionAvailable;
        set => this.RaiseAndSetIfChanged(ref _undoActionAvailable, value);
    }

    public bool IsChangeRefreshRateAvailable
    {
        get => _isChangeRefreshRateAvailable;
        set => this.RaiseAndSetIfChanged(ref _isChangeRefreshRateAvailable, value);
    }

    public EnhancedObservableCollection<DisplayModel> AvailableDisplays
    {
        get => _availableDisplays;
        set => this.RaiseAndSetIfChanged(ref _availableDisplays, value);
    }

    public DisplayModel SelectedDisplay
    {
        get => _selectedDisplay;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedDisplay, value);
            if (SelectedPreset != null)
            {
                SelectedPreset.DisplayIdentifier = value.Identifier;
            }
        }
    }

    public bool IsIntelSettingsAvailable
    {
        get => _isIntelSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isIntelSettingsAvailable, value);
    }

    public bool IsAmdApuSettingsAvailable
    {
        get => _isAmdApuSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdApuSettingsAvailable, value);
    }

    public bool IsAmdCpuSettingsAvailable
    {
        get => _isAmdCpuSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdCpuSettingsAvailable, value);
    }

    public bool IsAmdPboSettingAvailable
    {
        get => _isAmdPboSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdPboSettingAvailable, value);
    }

    public bool IsAmdCpuTuneSettingAvailable
    {
        get => _isAmdCpuTuneSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdCpuTuneSettingAvailable, value);
    }

    public bool IsAmdSoftClockSettingAvailable
    {
        get => _isAmdSoftClockSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdSoftClockSettingAvailable, value);
    }

    public bool IsAmdCOSettingAvailable
    {
        get => _isAmdCoSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdCoSettingAvailable, value);
    }

    public bool IsAmdPowerProfileSettingsAvailable
    {
        get => _isAmdPowerProfileSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdPowerProfileSettingsAvailable, value);
    }

    public bool IsAmdCCD1COSettingAvailable
    {
        get => _isAmdCCD1COSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdCCD1COSettingAvailable, value);
    }

    public bool IsAmdCCD2COSettingAvailable
    {
        get => _isAmdCCD2COSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdCCD2COSettingAvailable, value);
    }

    public bool IsAmdApuIGpuClockSettingAvailable
    {
        get => _isAmdApuIGpuClockSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdApuIGpuClockSettingAvailable, value);
    }

    public bool IsAmdApuVrmSettingAvailable
    {
        get => _isAmdApuVrmSettingAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAmdApuVrmSettingAvailable, value);
    }

    public bool IsUndoActionAvailable
    {
        get => _isUndoActionAvailable;
        set => this.RaiseAndSetIfChanged(ref _isUndoActionAvailable, value);
    }

    public bool IsAsusEcoMode
    {
        get => _isAsusEcoMode;
        set => this.RaiseAndSetIfChanged(ref _isAsusEcoMode, value);
    }

    public bool IsAsusEcoModeAvailable
    {
        get => _isAsusEcoModeAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAsusEcoModeAvailable, value);
    }

    public bool IsAsusPowerSettingsAvailable
    {
        get => _isAsusPowerSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAsusPowerSettingsAvailable, value);
    }

    public bool IsAsusGpuUltimateSettingsAvailable
    {
        get => _isAsusGpuUltimateSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAsusGpuUltimateSettingsAvailable, value);
    }

    public bool IsAsusGpuEcoModeSettingsAvailable
    {
        get => _isAsusGpuEcoModeSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAsusGpuEcoModeSettingsAvailable, value);
    }

    public bool IsAmdCpuThermalSettingsAvailable
    {
        get => _isAsusGpuEcoModeSettingsAvailable;
        set => this.RaiseAndSetIfChanged(ref _isAsusGpuEcoModeSettingsAvailable, value);
    }

    public List<AsusPowerProfile> AsusPowerProfiles
    {
        get => _asusPowerProfiles;
        set => this.RaiseAndSetIfChanged(ref _asusPowerProfiles, value);
    }

    public AsusPowerProfile SelectedAsusPowerProfile
    {
        get => _selectedAsusPowerProfile;
        set => this.RaiseAndSetIfChanged(ref _selectedAsusPowerProfile, value);
    }

    public Preset? SelectedPreset
    {
        get => _selectedPreset;
        set => this.RaiseAndSetIfChanged(ref _selectedPreset, value);
    }

    public List<PowerMode> PowerModes
    {
        get => _powerModes;
        set => this.RaiseAndSetIfChanged(ref _powerModes, value);
    }

    public List<AmdPowerProfile> AmdPowerProfiles
    {
        get => _amdPowerProfiles;
        set => this.RaiseAndSetIfChanged(ref _amdPowerProfiles, value);
    }

    public List<UXTUSuperResolutionScale> UXTUSuperResolutionScales
    {
        get => _uxtuSuperResolutionScales;
        set => this.RaiseAndSetIfChanged(ref _uxtuSuperResolutionScales, value);
    }

    #endregion

    #region Backing fields

    private List<UXTUSuperResolutionScale> _uxtuSuperResolutionScales;
    private Preset? _selectedPreset;
    private bool _isAsusEcoMode;
    private bool _isAsusEcoModeAvailable;
    private bool _isAsusPowerSettingsAvailable;
    private bool _isAsusGpuUltimateSettingsAvailable;
    private bool _isAsusGpuEcoModeSettingsAvailable;
    private List<AsusPowerProfile> _asusPowerProfiles;
    private AsusPowerProfile _selectedAsusPowerProfile;
    private Preset _currentPreset;
    private List<Preset> _availablePresets;
    private bool _isUndoActionAvailable;
    private bool _isIntelSettingsAvailable;
    private bool _isAmdCpuTuneSettingAvailable;
    private bool _isAmdSoftClockSettingAvailable;
    private bool _isAmdCoSettingAvailable;
    private bool _isAmdPowerProfileSettingsAvailable;
    private bool _isAmdCCD1COSettingAvailable;
    private bool _isAmdCCD2COSettingAvailable;
    private bool _isAmdApuIGpuClockSettingAvailable;
    private bool _isAmdPboSettingAvailable;
    private bool _isAmdApuSettingsAvailable;
    private bool _isChangeRefreshRateAvailable;
    private bool _isNvidiaGpuSettingsAvailable;
    private bool _isIsRadeonGpuSettingsAvailable;
    private bool _undoActionAvailable;
    private bool _isAmdCpuSettingsAvailable;
    private bool _isAmdApuVrmSettingAvailable;
    private List<AmdPowerProfile> _amdPowerProfiles;
    private List<PowerMode> _powerModes;

    #endregion

    #region Services

    private readonly IPresetService _apuPresetService;
    private readonly IPresetService _amdDesktopPresetService;
    private readonly IPresetService _intelPresetService;
    private readonly ILogger _logger;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IBatteryInfoService _batteryInfoService;
    private readonly INotificationManager _notificationManager;
    private readonly IRyzenAdjService _ryzenAdjService;
    private readonly IDisplayInfoService _displayInfoService;
    private readonly IIntelManagementService _intelManagementService;
    private readonly IASUSWmiService _asusWmiService;
    private EnhancedObservableCollection<DisplayModel> _availableDisplays;
    private DisplayModel _selectedDisplay;

    #endregion

    public CustomPresetsViewModel(ILogger logger,
        ISystemInfoService systemInfoService,
        IBatteryInfoService batteryInfoService,
        INotificationManager notificationManager,
        IRyzenAdjService ryzenAdjService,
        IDisplayInfoService displayInfoService,
        IIntelManagementService intelManagementService,
        IASUSWmiService asusWmiService,
        IPresetServiceFactory presetServiceFactory)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
        _batteryInfoService = batteryInfoService;
        _notificationManager = notificationManager;
        _ryzenAdjService = ryzenAdjService;
        _displayInfoService = displayInfoService;
        _intelManagementService = intelManagementService;
        _asusWmiService = asusWmiService;

        _apuPresetService = presetServiceFactory.GetPresetService(Settings.Default.Path + "apuPresets.json");
        _amdDesktopPresetService = presetServiceFactory.GetPresetService(Settings.Default.Path + "amdDtCpuPresets.json");
        _intelPresetService = presetServiceFactory.GetPresetService(Settings.Default.Path + "intelPresets.json");

        ApplyPresetCommand = ReactiveCommand.CreateFromTask(ApplyPreset);
        ReloadPresetValuesCommand = ReactiveCommand.CreateFromTask(RestorePresetValues);
        DeletePresetCommand = ReactiveCommand.CreateFromTask(DeleteCurrentPreset);
        UndoCommand = ReactiveCommand.CreateFromTask(Undo);
        SavePresetCommand = ReactiveCommand.CreateFromTask(SavePreset);
        IdentificateMonitorsCommand = ReactiveCommand.CreateFromTask(IdentificateMonitors);

        PowerModes = new List<PowerMode>
        {
            new PowerMode(PowerPlan.SystemControlled, "System Controlled"),
            new PowerMode(PowerPlan.SystemControlled, "Best Power Efficiency"),
            new PowerMode(PowerPlan.SystemControlled, "Balanced"),
            new PowerMode(PowerPlan.SystemControlled, "Best Performance")
        };
        
        AmdPowerProfiles = new List<AmdPowerProfile>
        {
            new AmdPowerProfile(AmdBoostProfile.Auto, "Auto"),
            new AmdPowerProfile(AmdBoostProfile.PowerSave, "Power Saving"),
            new AmdPowerProfile(AmdBoostProfile.Performance, "Performance")
        };
        
        UXTUSuperResolutionScales = new List<UXTUSuperResolutionScale>()
        {
            new UXTUSuperResolutionScale(ResolutionScale.ApplicationControlled, "Application Controlled"),
            new UXTUSuperResolutionScale(ResolutionScale.UltraQuality, "Ultra Quality (77%)"),
            new UXTUSuperResolutionScale(ResolutionScale.Quality, "Quality (67%)"),
            new UXTUSuperResolutionScale(ResolutionScale.Balanced, "Balanced (59%)"),
            new UXTUSuperResolutionScale(ResolutionScale.Performance, "Performance (50%)"),
            new UXTUSuperResolutionScale(ResolutionScale.UltraPerformance, "Ultra Performance (33%)"),
        };

        Initialize();
        
        _displayInfoService.DisplayAttached += DisplayInfoServiceOnDisplayAttached;
        _displayInfoService.DisplayRemoved += DisplayInfoServiceOnDisplayRemoved;
    }

    private void DisplayInfoServiceOnDisplayRemoved(Display display)
    {
        var displayToRemove = AvailableDisplays.FirstOrDefault(x => x.Identifier == display.Identifier);
        if (displayToRemove != null)
        {
            AvailableDisplays.Remove(displayToRemove);
        }
    }

    private void DisplayInfoServiceOnDisplayAttached(Display display)
    {
        AvailableDisplays.Add(new DisplayModel(display));
    }

    private void Initialize()
    {
        // default values
        SelectedPreset = new Preset()
        {
            ApuSkinTemperature = 45,
            ApuTemperature = 95,
            ApuStapmPower = 28,
            ApuFastPower = 28,
            ApuSlowPower = 28,
            ApuSlowTime = 128,
            ApuStapmTime = 64,
            ApuCpuTdc = 64,
            ApuCpuEdc = 64,
            ApuGfxTdc = 64,
            ApuGfxEdc = 64,
            ApuSocTdc = 64,
            ApuSocEdc = 64,
            ApuGfxClock = 1000,
            
            AmdClock = 3200,
            AmdVid = 1200,
            NvMaxCoreClk = 4000,
            
            DtCpuTemperature = 85,
            DtCpuPpt = 140,
            DtCpuEdc = 160,
            DtCpuTdc = 160,
            
            IntelPl1 = 35,
            IntelPl2 = 65,
            IntelBalCpu = 9,
            IntelBalGpu = 13,
        };

        IsRadeonGpuSettingsAvailable = _systemInfoService.Gpus.Count(x => x.Manufacturer == GpuManufacturer.AMD) != 0;
        IsNvidiaGpuSettingsAvailable = _systemInfoService.Gpus.Count(x => x.Manufacturer == GpuManufacturer.Nvidia) != 0;
        
        if (_displayInfoService.Displays.Value.Any(x => x.SupportedRefreshRates.Count > 1))
        {
            IsChangeRefreshRateAvailable = true;
            
            AvailableDisplays = new EnhancedObservableCollection<DisplayModel>(_displayInfoService.Displays.Value.Select(x => new DisplayModel(x)));
            SelectedDisplay =
                AvailableDisplays.FirstOrDefault(x =>
                    x.SupportedOutputTechnology == DisplayOutputTechnology.Internal) ?? AvailableDisplays[0];
        }
        else
        {
            IsChangeRefreshRateAvailable = false;
        }

        if (_systemInfoService.Cpu.Manufacturer == Manufacturer.Intel)
        {
            IsIntelSettingsAvailable = true;

            var clockRatio = _intelManagementService.ReadClockRatios().Result;
            SelectedPreset.IntelClockRatios = new List<IntelClockRatio>(clockRatio.Length+1);
            
            for (var i = 0; i < clockRatio.Length; i++)
            {
                SelectedPreset.IntelClockRatios[i] = new IntelClockRatio()
                {
                    CoreGroupIndex = i,
                    Ratio = clockRatio[i]
                };
            }

            // Get the names of all the stored presets
            var intelPresets = _intelPresetService.GetPresets();
            AvailablePresets = intelPresets.ToList();
        }
        else if (_systemInfoService.Cpu is RyzenCpuInfo ryzenCpuInfo)
        {
            IsAmdCpuSettingsAvailable = true;

            if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Apu)
            {
                IsAmdApuSettingsAvailable = ryzenCpuInfo.RyzenFamily is 
                                            RyzenFamily.PhoenixPoint or
                                            RyzenFamily.PhoenixPoint2 or
                                            RyzenFamily.Mendocino or
                                            RyzenFamily.Rembrandt or
                                            RyzenFamily.Lucienne or
                                            RyzenFamily.Renoir;

                IsAmdApuVrmSettingAvailable = true;
                IsAmdPboSettingAvailable = !_systemInfoService.Cpu.Name.Contains('U') &&
                                           ryzenCpuInfo.RyzenFamily < RyzenFamily.Renoir;
                IsAmdCpuTuneSettingAvailable = _batteryInfoService.GetBatteryStatus() == BatteryStatus.NoSystemBattery;
                IsAmdSoftClockSettingAvailable = ryzenCpuInfo.RyzenFamily < RyzenFamily.Renoir;
                IsAmdCOSettingAvailable = ryzenCpuInfo.RyzenFamily > RyzenFamily.Renoir &&
                                          ryzenCpuInfo.RyzenFamily != RyzenFamily.Mendocino;

                if (ryzenCpuInfo.RyzenFamily < RyzenFamily.Renoir)
                {
                    IsAmdPowerProfileSettingsAvailable = false;
                    IsAmdCOSettingAvailable = false;
                }

                IsAmdApuIGpuClockSettingAvailable = ryzenCpuInfo.RyzenFamily is
                    RyzenFamily.Renoir or
                    RyzenFamily.Lucienne or
                    RyzenFamily.Mendocino or
                    RyzenFamily.Rembrandt or
                    RyzenFamily.PhoenixPoint or
                    RyzenFamily.PhoenixPoint2 or
                    RyzenFamily.HawkPoint;

                IsAmdCCD2COSettingAvailable = ryzenCpuInfo.RyzenFamily == RyzenFamily.DragonRange &&
                                              ryzenCpuInfo.RyzenSeries == RyzenSeries.Ryzen9;

                var apuPresets = _apuPresetService.GetPresets();
                AvailablePresets = apuPresets.ToList();
            }
            else if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Desktop)
            {
                IsAmdCpuThermalSettingsAvailable = true;
                IsAmdCOSettingAvailable = ryzenCpuInfo.RyzenFamily >= RyzenFamily.Vermeer;
                IsAmdCCD1COSettingAvailable = IsAmdCOSettingAvailable;
                IsAmdCCD2COSettingAvailable = ryzenCpuInfo.RyzenSeries == RyzenSeries.Ryzen9;

                var desktopPresets = _amdDesktopPresetService.GetPresets();
                AvailablePresets = desktopPresets.ToList();
            }
        }

        if (_systemInfoService.LaptopInfo is AsusLaptopInfo asusLaptopInfo)
        {
            IsAsusPowerSettingsAvailable = true;
            IsAsusGpuUltimateSettingsAvailable = true;
            IsAsusGpuEcoModeSettingsAvailable = true;

            var isGamingLaptop = asusLaptopInfo.LaptopSeries is AsusLaptopSeries.ROG or AsusLaptopSeries.TUF;

            var device = isGamingLaptop ? AsusDevice.GpuMux : AsusDevice.GpuMuxVivo;

            var mux = _asusWmiService.DeviceGet(device);

            if (mux > 0)
                SelectedPreset.IsAsusGpuUlti = false;
            else if (mux > -1)
                SelectedPreset.IsAsusGpuUlti = true;
            else SelectedPreset.IsAsusGpuUlti = false;

            device = AsusDevice.GpuEco;
            var eco = _asusWmiService.DeviceGet(device);

            IsAsusEcoModeAvailable = true;
            if (eco is > -1 and < 1)
                IsAsusEcoMode = false;
            else if (eco > 0)
                IsAsusEcoMode = true;
            else IsAsusEcoModeAvailable = false;

            AsusPowerProfiles = new List<AsusPowerProfile>()
            {
                new("AC Controlled", AsusMode.AcControlled),
                new("Silent", AsusMode.Silent),
                new("Performance", AsusMode.Balanced),
                new("Turbo", AsusMode.Turbo)
            };

            var currentPerformanceMode = _asusWmiService.GetPerformanceMode();
            var powerModeIndex = AsusPowerProfiles.FindIndex(x => x.PowerProfileMode == currentPerformanceMode);
            SelectedAsusPowerProfile = AsusPowerProfiles[powerModeIndex];
        }
        else
        {
            IsAsusPowerSettingsAvailable = false;
            IsAsusGpuUltimateSettingsAvailable = false;
            IsAsusGpuEcoModeSettingsAvailable = false;
        }
    }

    private async Task AsusGpuUltimateModSwitched()
    {
        if (!_selectedPreset.IsAsusGpuUlti)
        {
            await _notificationManager.ShowTextNotification("GPU Ultimate Mode", "Disabling GPU Ultimate Mode requires a restart to take\naffect!");
        }
    }
    
    private async Task RestorePresetValues()
    {
        var presetService = _systemInfoService.Cpu.Manufacturer switch
        {
            Manufacturer.AMD => _systemInfoService.Cpu.ProcessorType == ProcessorType.Apu
                ? _apuPresetService
                : _amdDesktopPresetService,
            Manufacturer.Intel => _intelPresetService
        };
        if (presetService.GetPresetNames().Contains(SelectedPreset.Name))
        {
            SelectedPreset = presetService.GetPreset(SelectedPreset.Name)!;
        }
    }

    private async Task IdentificateMonitors()
    {
        ScreenIdentification.Show();
    }

    private async Task Undo()
    {
        SelectedPreset.IsAmdOc = false;
        await _ryzenAdjService.Translate("--disable-oc ");
        await _ryzenAdjService.Translate(GetCommandValues());
        Settings.Default.CommandString = GetCommandValues();
        Settings.Default.Save();
        IsUndoActionAvailable = false;
        await _ryzenAdjService.Translate("--disable-oc ");
    }

    private async Task ApplyPreset()
    {
        try
        {
            var commandValues = GetCommandValues();

            if (!string.IsNullOrEmpty(commandValues))
            {
                await _ryzenAdjService.Translate(commandValues);
                await _notificationManager.ShowTextNotification("Preset Applied",
                    "Your custom preset settings have been applied!");
            }

            IsRadeonGpuSettingsAvailable = _systemInfoService.Gpus.Count(x => x.Manufacturer == GpuManufacturer.AMD) != 0;
            IsNvidiaGpuSettingsAvailable = _systemInfoService.Gpus.Count(x => x.Manufacturer == GpuManufacturer.Nvidia) != 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception occurred when applying preset");
            await _notificationManager.ShowTextNotification(
                title:"Preset not applied",
                text :"Error occurred when applying preset!",
                notificationType: NotificationManagerExtensions.NotificationType.Error);
        }
    }

    private async Task DeleteCurrentPreset()
    {
        try
        {
            switch (_systemInfoService.Cpu.Manufacturer)
            {
                case Manufacturer.AMD:
                {
                    if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Apu)
                        _apuPresetService.DeletePreset(SelectedPreset.Name);
                    else if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Desktop)
                        _amdDesktopPresetService.DeletePreset(SelectedPreset.Name);
                    break;
                }
                case Manufacturer.Intel:
                {
                    _intelPresetService.DeletePreset(SelectedPreset.Name);
                    break;
                }
            }

            await _notificationManager.ShowTextNotification("Preset Deleted",
                $"Your preset {SelectedPreset.Name} has been deleted successfully!");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Exception occurred while deleting preset");
            await _notificationManager.ShowTextNotification("Preset not deleted",
                $"Exception occurred while deleting preset!");
        }
    }

    private async Task SavePreset()
    {
        if (!SelectedPreset.Name.Contains("PM -"))
        {
            SelectedPreset.CommandValue = GetCommandValues();
            switch (_systemInfoService.Cpu.Manufacturer)
            {
                case Manufacturer.AMD:
                {
                    if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Apu)
                    {
                        // Save a preset
                        SelectedPreset.AsusPowerProfile = (int)SelectedAsusPowerProfile.PowerProfileMode;

                        _apuPresetService.SavePreset(SelectedPreset.Name, SelectedPreset);

                        IsRadeonGpuSettingsAvailable = _systemInfoService.Gpus.Count(x => x.Manufacturer == GpuManufacturer.AMD) != 0;
                        IsNvidiaGpuSettingsAvailable = _systemInfoService.Gpus.Count(x => x.Manufacturer == GpuManufacturer.Nvidia) != 0;
                    }
                    else if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Desktop)
                    {
                        _amdDesktopPresetService.SavePreset(SelectedPreset.Name, SelectedPreset); 
                    }
                    break;
                }
                case Manufacturer.Intel:
                {
                    _intelPresetService.SavePreset(_selectedPreset.Name, _selectedPreset);
                    break;
                }
            }

            await _notificationManager.ShowTextNotification("Preset Saved",
                $"Your preset {_selectedPreset.Name} has been saved successfully!");
        }
    }

    private string GetCommandValues()
    {
        var sb = StringBuilderPool.Rent();

        sb.Append("--UXTUSR=")
               .Append(SelectedPreset.IsMag).Append('-')
               .Append(SelectedPreset.IsVsync).Append('-')
               .Append(SelectedPreset.Sharpness / 100).Append('-')
               .Append(SelectedPreset.ResolutionScale.ResolutionScale).Append('-')
               .Append(SelectedPreset.IsRecap).Append(' ');

        if (_systemInfoService.LaptopInfo?.Brand == LaptopBrand.ASUS)
        {
            if (SelectedAsusPowerProfile.PowerProfileMode != AsusMode.AcControlled)
                sb.Append("--ASUS-Power=").Append((int)SelectedAsusPowerProfile.PowerProfileMode).Append(' ');
            
            if (IsAsusEcoMode)
                sb.Append("--ASUS-Eco=").Append(IsAsusEcoMode).Append(' ');
            
            if (IsAsusGpuUltimateSettingsAvailable)
                sb.Append("--ASUS-MUX=").Append(IsAsusGpuUltimateSettingsAvailable).Append(' ');
        }

        if (IsChangeRefreshRateAvailable && SelectedPreset.DisplayHz > 0)
        {
            sb.Append("--Refresh-Rate=")
                   .Append(SelectedPreset.DisplayIdentifier)
                   .Append(":::")
                   .Append(SelectedPreset.DisplayHz)
                   .Append(' ');
        }

        if (SelectedPreset.PowerMode.PowerPlan != PowerPlan.SystemControlled)
            sb.Append("--Win-Power=").Append(SelectedPreset.PowerMode.PowerPlan).Append(' ');

        if (_systemInfoService.Cpu.Manufacturer == Manufacturer.AMD)
        {
            if (_systemInfoService.Cpu is RyzenCpuInfo ryzenCpuInfo)
            {
                if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Apu)
                {
                    if (SelectedPreset.IsApuTemp)
                    {
                        sb.Append("--tctl-temp=").Append(SelectedPreset.ApuTemperature).Append(' ')
                               .Append("--cHTC-temp=").Append(SelectedPreset.ApuTemperature).Append(' ');
                    }
                    
                    if (SelectedPreset.IsApuSkinTemp)
                        sb.Append("--apu-skin-temp=").Append(SelectedPreset.ApuSkinTemperature).Append(' ');
                    
                    if (SelectedPreset.IsApuStapmPow)
                        sb.Append("--stapm-limit=").Append(SelectedPreset.ApuStapmPower * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuFastPow)
                        sb.Append("--fast-limit=").Append(SelectedPreset.ApuFastPower * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuStapmTime)
                        sb.Append("--stapm-time=").Append(SelectedPreset.ApuStapmPower).Append(' ');
                    
                    if (SelectedPreset.IsApuSlowPow)
                        sb.Append("--slow-limit=").Append(SelectedPreset.ApuSlowPower * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuSlowTime)
                        sb.Append("--slow-time=").Append(SelectedPreset.ApuSlowTime).Append(' ');
                    
                    if (SelectedPreset.IsApuCpuTdc)
                        sb.Append("--vrm-current=").Append(SelectedPreset.ApuCpuTdc * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuCpuEdc)
                        sb.Append("--vrmmax-current=").Append(SelectedPreset.ApuCpuEdc * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuSocTdc)
                        sb.Append("--vrmsoc-current=").Append(SelectedPreset.ApuSocTdc * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuSocEdc)
                        sb.Append("--vrmsocmax-current=").Append(SelectedPreset.ApuSocEdc * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuGfxClk)
                        sb.Append("--vrmgfx-current=").Append(SelectedPreset.ApuGfxClock * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuGfxEdc)
                        sb.Append("--vrmgfxmax-current=").Append(SelectedPreset.ApuGfxEdc * 1000).Append(' ');
                    
                    if (SelectedPreset.IsApuGfxClk)
                        sb.Append("--gfx-clk=").Append(SelectedPreset.ApuGfxClock).Append(' ');
                    
                    if (SelectedPreset.IsPboScalar)
                        sb.Append("--pbo-scalar=").Append(SelectedPreset.PboScalar * 100).Append(' ');

                    if (SelectedPreset.IsCoAllCore)
                    {
                        if (SelectedPreset.CoAllCore >= 0)
                        {
                            sb.Append("--set-coall=").Append(SelectedPreset.CoAllCore).Append(' ');
                        }
                        else if (SelectedPreset.CoAllCore < 0)
                        {
                            sb.Append("--set-coall=")
                                   .Append(Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoAllCore)))
                                   .Append(' ');
                        }
                    }

                    if (SelectedPreset.IsCoGfx)
                    {
                        if (SelectedPreset.CoGfx >= 0)
                        {
                            sb.Append("--set-cogfx=").Append(SelectedPreset.CoGfx).Append(' ');
                        }
                        else if (SelectedPreset.CoGfx < 0)
                        {
                            sb.Append("--set-cogfx=")
                                   .Append(Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoGfx)))
                                   .Append(' ');
                        }
                    }

                    if (SelectedPreset.IsSoftMiniGpuClk)
                        sb.Append("--min-gfxclk=").Append(SelectedPreset.SoftMiniGpuClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMaxiGpuClk)
                        sb.Append("--max-gfxclk=").Append(SelectedPreset.SoftMaxiGpuClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMinCpuClk)
                        sb.Append("--min-cpuclk=").Append(SelectedPreset.SoftMinCpuClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMaxCpuClk)
                        sb.Append("--max-cpuclk=").Append(SelectedPreset.SoftMaxCpuClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMinDataClk)
                        sb.Append("--min-lclk=").Append(SelectedPreset.SoftMinDataClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMaxDataClk)
                        sb.Append("--max-lclk=").Append(SelectedPreset.SoftMaxDataClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMinVcnClk)
                        sb.Append("--min-vcn=").Append(SelectedPreset.SoftMinVcnClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMaxVcnClk)
                        sb.Append("--max-vcn=").Append(SelectedPreset.SoftMaxVcnClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMinFabClk)
                        sb.Append("--min-fclk-frequency=").Append(SelectedPreset.SoftMinFabClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMaxFabClk)
                        sb.Append("--max-fclk-frequency=").Append(SelectedPreset.SoftMaxFabClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMinSoCClk)
                        sb.Append("--min-socclk-frequency=").Append(SelectedPreset.SoftMinSoCClk).Append(' ');
                    
                    if (SelectedPreset.IsSoftMaxSoCClk)
                        sb.Append("--max-socclk-frequency=").Append(SelectedPreset.SoftMaxSoCClk).Append(' ');

                    switch (SelectedPreset.BoostProfile.BoostPlan)
                    {
                        case AmdBoostProfile.PowerSave:
                            sb.Append("--power-saving ");
                            break;
                        case AmdBoostProfile.Performance:
                            sb.Append("--max-performance ");
                            break;
                    }

                    int coresCount = SelectedPreset.Ccd1States.Count;
                    if (ryzenCpuInfo.RyzenFamily == RyzenFamily.DragonRange)
                    {
                        foreach (var state in SelectedPreset.Ccd1States)
                        {
                            if (state.IsEnabled)
                            {
                                sb.Append("--set-coper=")
                                       .Append((((((0 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % coresCount) & 15)) << 20) | (state.Value & 0xFFFF))
                                       .Append(' ');
                            }
                        }

                        foreach (var state in SelectedPreset.Ccd2States)
                        {
                            if (state.IsEnabled)
                            {
                                sb.Append("--set-coper=")
                                       .Append((((((1 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % 8) & 15)) << 20) | (state.Value & 0xFFFF))
                                       .Append(' ');
                            }
                        }
                    }
                    else
                    {
                        foreach (var state in SelectedPreset.Ccd1States)
                        {
                            if (state.IsEnabled)
                            {
                                sb.Append("--set-coper=")
                                       .Append((state.CoreIndex << 20) | (state.Value & 0xFFFF))
                                       .Append(' ');
                            }
                        }
                    }

                    if (SelectedPreset.IsAmdOc)
                    {
                        sb.Append("--oc-clk=").Append(SelectedPreset.AmdClock).Append(' ')
                               .Append("--oc-clk=").Append(SelectedPreset.AmdClock).Append(' ');

                        double vid;
                        if (ryzenCpuInfo.RyzenFamily >= RyzenFamily.Rembrandt)
                        {
                            vid = ((double)SelectedPreset.AmdVid - 1125) / 5 + 1200;
                            sb.Append("--oc-volt=").Append(vid).Append(' ')
                                   .Append("--oc-volt=").Append(vid).Append(' ');
                        }
                        else
                        {
                            vid = Math.Round((double)SelectedPreset.AmdVid / 1000, 2);
                            uint voltValue = Convert.ToUInt32((1.55 - vid) / 0.00625);
                            sb.Append("--oc-volt=").Append(voltValue).Append(' ')
                                   .Append("--oc-volt=").Append(voltValue).Append(' ');
                        }

                        sb.Append("--enable-oc ");
                    }
                }
            }
        }

        if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Desktop)
        {
            if (SelectedPreset.IsDtCpuTemp)
                sb.Append("--tctl-limit=").Append(SelectedPreset.DtCpuTemperature * 1000).Append(' ');
            
            if (SelectedPreset.IsDtCpuPpt)
                sb.Append("--ppt-limit=").Append(SelectedPreset.DtCpuPpt * 1000).Append(' ');
            
            if (SelectedPreset.IsDtCpuTdc)
                sb.Append("--tdc-limit=").Append(SelectedPreset.DtCpuTdc * 1000).Append(' ');
            
            if (SelectedPreset.IsDtCpuEdc)
                sb.Append("--edc-limit=").Append(SelectedPreset.DtCpuEdc * 1000).Append(' ');
            
            if (SelectedPreset.IsPboScalar)
                sb.Append("--pbo-scalar=").Append(SelectedPreset.PboScalar * 100).Append(' ');

            if (SelectedPreset.IsCoAllCore)
            {
                if (SelectedPreset.CoAllCore >= 0)
                {
                    sb.Append("--set-coall=").Append(SelectedPreset.CoAllCore).Append(' ');
                }
                else if (SelectedPreset.CoAllCore < 0)
                {
                    sb.Append("--set-coall=")
                           .Append(Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoAllCore)))
                           .Append(' ');
                }

                if (SelectedPreset.IsCoGfx)
                {
                    if (SelectedPreset.CoGfx >= 0)
                    {
                        sb.Append("--set-cogfx=").Append(SelectedPreset.CoGfx).Append(' ');
                    }
                    else if (SelectedPreset.CoGfx < 0)
                    {
                        sb.Append("--set-cogfx=")
                               .Append(Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoGfx)))
                               .Append(' ');
                    }
                }

                int coresCount = SelectedPreset.Ccd1States.Count;
                foreach (var state in SelectedPreset.Ccd1States)
                {
                    if (state.IsEnabled)
                    {
                        sb.Append("--set-coper=")
                               .Append((((((0 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % coresCount) & 15)) << 20) | (state.Value & 0xFFFF))
                               .Append(' ');
                    }
                }

                foreach (var state in SelectedPreset.Ccd2States)
                {
                    if (state.IsEnabled)
                    {
                        sb.Append("--set-coper=")
                               .Append((((((1 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % coresCount) & 15)) << 20) | (state.Value & 0xFFFF))
                               .Append(' ');
                    }
                }

                if (SelectedPreset.IsAmdOc && _systemInfoService.Cpu is RyzenCpuInfo ryzenCpuInfo)
                {
                    sb.Append("--oc-clk=").Append(SelectedPreset.AmdClock).Append(' ')
                           .Append("--oc-clk=").Append(SelectedPreset.AmdClock).Append(' ');

                    double vid;
                    if (ryzenCpuInfo.RyzenFamily >= RyzenFamily.Rembrandt)
                    {
                        vid = ((double)SelectedPreset.AmdVid - 1125) / 5 + 1200;
                        sb.Append("--oc-volt=").Append(vid).Append(' ')
                               .Append("--oc-volt=").Append(vid).Append(' ');
                    }
                    else
                    {
                        vid = Math.Round((double)SelectedPreset.AmdVid / 1000, 2);
                        uint voltValue = Convert.ToUInt32((1.55 - vid) / 0.00625);
                        sb.Append("--oc-volt=").Append(voltValue).Append(' ')
                               .Append("--oc-volt=").Append(voltValue).Append(' ');
                    }

                    sb.Append("--enable-oc ");
                }
            }

            if (_systemInfoService.Cpu.Manufacturer == Manufacturer.Intel)
            {
                if (SelectedPreset.IsIntelClockRatio)
                {
                    sb.Append("--intel-ratio=")
                           .AppendJoin('-', SelectedPreset.IntelClockRatios.Select(x => x.Ratio))
                           .Append(' ');
                }

                if (SelectedPreset.IsIntelPl1)
                    sb.Append("--intel-pl=").Append(SelectedPreset.IntelPl1).Append(' ');
                
                if (SelectedPreset.IsIntelVoltages)
                {
                    sb.Append("--intel-volt-cpu=").Append(SelectedPreset.IntelVoltageCpu).Append(' ')
                           .Append("--intel-volt-gpu=").Append(SelectedPreset.IntelVoltageGpu).Append(' ')
                           .Append("--intel-volt-cache=").Append(SelectedPreset.IntelVoltageCache).Append(' ')
                           .Append("--intel-volt-cpu=").Append(SelectedPreset.IntelVoltageSa).Append(' ');
                }
                
                if (SelectedPreset.IsIntelBal)
                {
                    sb.Append("--intel-bal-cpu=").Append(SelectedPreset.IntelBalCpu).Append(' ')
                           .Append("--intel-bal-gpu=").Append(SelectedPreset.IntelBalGpu).Append(' ');
                }
                
                if (SelectedPreset.IsApuGfxClk)
                    sb.Append("--intel-gpu=").Append(SelectedPreset.ApuGfxClock).Append(' ');
            }

            if (SelectedPreset.IsRadeonGraphics)
            {
                sb.Append(SelectedPreset.IsAntiLag
                    ? "--ADLX-Lag=0-true --ADLX-Lag=1-true "
                    : "--ADLX-Lag=0-false --ADLX-Lag=1-false ");

                sb.Append(SelectedPreset.IsRsr
                    ? $"--ADLX-RSR=true-{SelectedPreset.Rsr} "
                    : $"--ADLX-RSR=false-{SelectedPreset.Rsr} ");

                sb.Append(
                    SelectedPreset.IsBoost
                        ? $"--ADLX-Boost=0-true-{SelectedPreset.Boost} --ADLX-Boost=1-true-{SelectedPreset.Boost} "
                        : $"--ADLX-Boost=0-false-{SelectedPreset.Boost} --ADLX-Boost=1-false-{SelectedPreset.Boost} ");

                sb.Append(
                    SelectedPreset.IsImageSharp
                        ? $"--ADLX-ImageSharp=0-true-{SelectedPreset.ImageSharp} --ADLX-ImageSharp=1-true-{SelectedPreset.ImageSharp} "
                        : $"--ADLX-ImageSharp=0-false-{SelectedPreset.ImageSharp} --ADLX-ImageSharp=1-false-{SelectedPreset.ImageSharp} ");

                sb.Append(SelectedPreset.IsSync
                    ? "--ADLX-Sync=0-true --ADLX-Sync=1-true "
                    : "--ADLX-Sync=0-false --ADLX-Sync=1-false ");
            }

            if (SelectedPreset.IsNvidia)
            {
                sb.Append("--NVIDIA-Clocks=")
                       .Append(SelectedPreset.NvMaxCoreClk).Append('-')
                       .Append(SelectedPreset.NvCoreClk).Append('-')
                       .Append(SelectedPreset.NvMemClk).Append(' ');
            }
        }
        
        var commandStr = sb.ToString().TrimEnd();
        StringBuilderPool.Return(sb);
        return commandStr;
    }

    public void Dispose()
    {
        _displayInfoService.DisplayAttached -= DisplayInfoServiceOnDisplayAttached;
        _displayInfoService.DisplayRemoved -= DisplayInfoServiceOnDisplayRemoved;
    }
}