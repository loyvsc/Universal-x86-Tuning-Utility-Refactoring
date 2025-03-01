using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using DesktopNotifications;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Properties;
using AmdPowerProfile = ApplicationCore.Models.AmdPowerProfile;
using PowerMode = ApplicationCore.Models.PowerMode;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class CustomPresetsViewModel : NotifyPropertyChangedBase
{
    public ICommand ApplyPresetCommand { get; }
    public ICommand SavePresetCommand { get; }
    public ICommand DeletePresetCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ReloadPresetValuesCommand { get; }

    #region Properties

    public bool IsNvidiaGpuSettingsAvailable
    {
        get => _isNvidiaGpuSettingsAvailable;
        set => SetValue(ref _isNvidiaGpuSettingsAvailable, value);
    }

    public bool IsRadeonGpuSettingsAvailable
    {
        get => _isIsRadeonGpuSettingsAvailable;
        set => SetValue(ref _isIsRadeonGpuSettingsAvailable, value);
    }

    public List<Preset> AvailablePresets
    {
        get => _availablePresets;
        set => SetValue(ref _availablePresets, value);
    }

    public Preset CurrentPreset
    {
        get => _currentPreset;
        set => SetValue(ref _currentPreset, value);
    }

    public bool UndoActionAvailable
    {
        get => _undoActionAvailable;
        set => SetValue(ref _undoActionAvailable, value);
    }

    public bool IsChangeRefreshRateAvailable
    {
        get => _isChangeRefreshRateAvailable;
        set => SetValue(ref _isChangeRefreshRateAvailable, value);
    }

    public List<string> RefreshRates
    {
        get => _refreshRates;
        set => SetValue(ref _refreshRates, value);
    }

    public bool IsIntelSettingsAvailable
    {
        get => _isIntelSettingsAvailable;
        set => SetValue(ref _isIntelSettingsAvailable, value);
    }

    public bool IsAmdApuSettingsAvailable
    {
        get => _isAmdApuSettingsAvailable;
        set => SetValue(ref _isAmdApuSettingsAvailable, value);
    }

    public bool IsAmdCpuSettingsAvailable
    {
        get => _isAmdCpuSettingsAvailable;
        set => SetValue(ref _isAmdCpuSettingsAvailable, value);
    }

    public bool IsAmdPboSettingAvailable
    {
        get => _isAmdPboSettingAvailable;
        set => SetValue(ref _isAmdPboSettingAvailable, value);
    }

    public bool IsAmdCpuTuneSettingAvailable
    {
        get => _isAmdCpuTuneSettingAvailable;
        set => SetValue(ref _isAmdCpuTuneSettingAvailable, value);
    }

    public bool IsAmdSoftClockSettingAvailable
    {
        get => _isAmdSoftClockSettingAvailable;
        set => SetValue(ref _isAmdSoftClockSettingAvailable, value);
    }

    public bool IsAmdCOSettingAvailable
    {
        get => _isAmdCoSettingAvailable;
        set => SetValue(ref _isAmdCoSettingAvailable, value);
    }

    public bool IsAmdPowerProfileSettingsAvailable
    {
        get => _isAmdPowerProfileSettingsAvailable;
        set => SetValue(ref _isAmdPowerProfileSettingsAvailable, value);
    }

    public bool IsAmdCCD1COSettingAvailable
    {
        get => _isAmdCCD1COSettingAvailable;
        set => SetValue(ref _isAmdCCD1COSettingAvailable, value);
    }

    public bool IsAmdCCD2COSettingAvailable
    {
        get => _isAmdCCD2COSettingAvailable;
        set => SetValue(ref _isAmdCCD2COSettingAvailable, value);
    }

    public bool IsAmdApuIGpuClockSettingAvailable
    {
        get => _isAmdApuIGpuClockSettingAvailable;
        set => SetValue(ref _isAmdApuIGpuClockSettingAvailable, value);
    }

    public bool IsAmdApuVrmSettingAvailable
    {
        get => _isAmdApuVrmSettingAvailable;
        set => SetValue(ref _isAmdApuVrmSettingAvailable, value);
    }

    public bool IsUndoActionAvailable
    {
        get => _isUndoActionAvailable;
        set => SetValue(ref _isUndoActionAvailable, value);
    }

    public bool IsAsusEcoMode
    {
        get => _isAsusEcoMode;
        set => SetValue(ref _isAsusEcoMode, value);
    }

    public bool IsAsusEcoModeAvailable
    {
        get => _isAsusEcoModeAvailable;
        set => SetValue(ref _isAsusEcoModeAvailable, value);
    }

    public bool IsAsusPowerSettingsAvailable
    {
        get => _isAsusPowerSettingsAvailable;
        set => SetValue(ref _isAsusPowerSettingsAvailable, value);
    }

    public bool IsAsusGpuUltimateSettingsAvailable
    {
        get => _isAsusGpuUltimateSettingsAvailable;
        set => SetValue(ref _isAsusGpuUltimateSettingsAvailable, value);
    }

    public bool IsAsusGpuEcoModeSettingsAvailable
    {
        get => _isAsusGpuEcoModeSettingsAvailable;
        set => SetValue(ref _isAsusGpuEcoModeSettingsAvailable, value);
    }

    public bool IsAmdCpuThermalSettingsAvailable
    {
        get => _isAsusGpuEcoModeSettingsAvailable;
        set => SetValue(ref _isAsusGpuEcoModeSettingsAvailable, value);
    }

    public List<AsusPowerProfile> AsusPowerProfiles
    {
        get => _asusPowerProfiles;
        set => SetValue(ref _asusPowerProfiles, value);
    }

    public AsusPowerProfile SelectedAsusPowerProfile
    {
        get => _selectedAsusPowerProfile;
        set => SetValue(ref _selectedAsusPowerProfile, value);
    }

    public Preset SelectedPreset
    {
        get => _selectedPreset;
        set => SetValue(ref _selectedPreset, value);
    }

    public List<PowerMode> PowerModes
    {
        get => _powerModes;
        set => SetValue(ref _powerModes, value);
    }

    public List<AmdPowerProfile> AmdPowerProfiles
    {
        get => _amdPowerProfiles;
        set => SetValue(ref _amdPowerProfiles, value);
    }

    public List<UXTUSuperResolutionScale> UXTUSuperResolutionScales
    {
        get => _uxtuSuperResolutionScales;
        set => SetValue(ref _uxtuSuperResolutionScales, value);
    }

    #endregion

    #region Backing fields

    private List<UXTUSuperResolutionScale> _uxtuSuperResolutionScales;
    private Preset _selectedPreset;
    private bool _isAsusMux;
    private bool _isAsusEcoMode;
    private bool _isAsusEcoModeAvailable;
    private bool _isAsusPowerSettingsAvailable;
    private bool _isAsusGpuUltimateSettingsAvailable;
    private bool _isAsusGpuEcoModeSettingsAvailable;
    private List<AsusPowerProfile> _asusPowerProfiles;
    private AsusPowerProfile _selectedAsusPowerProfile;
    private Preset _currentPreset;
    private List<Preset> _availablePresets;
    private List<string> _refreshRates;
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
    private readonly ILogger<CustomPresetsViewModel> _logger;
    private readonly ISystemInfoService _systemInfoService;
    private readonly INotificationManager _notificationManager;
    private readonly IRyzenAdjService _ryzenAdjService;
    private readonly IDisplayInfoService _displayInfoService;
    private readonly IIntelManagementService _intelManagementService;
    private readonly IASUSWmiService _asusWmiService;

    #endregion

    public CustomPresetsViewModel(ILogger<CustomPresetsViewModel> logger,
        ISystemInfoService systemInfoService,
        INotificationManager notificationManager,
        IRyzenAdjService ryzenAdjService,
        IDisplayInfoService displayInfoService,
        IIntelManagementService intelManagementService,
        IASUSWmiService asusWmiService,
        IPresetServiceFactory presetServiceFactory)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
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

        IsRadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
        IsNvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;

        if (_displayInfoService.UniqueTargetRefreshRates.Count > 1)
        {
            IsChangeRefreshRateAvailable = true;

            RefreshRates.Add("System Controlled");
            foreach (var refreshRate in _displayInfoService.UniqueTargetRefreshRates)
                RefreshRates.Add($"{refreshRate} Hz");
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
        else if (_systemInfoService.Cpu.Manufacturer == Manufacturer.AMD)
        {
            IsAmdCpuSettingsAvailable = true;

            if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Apu)
            {
                IsAmdApuSettingsAvailable = _systemInfoService.Cpu.RyzenFamily is 
                                            RyzenFamily.PhoenixPoint or
                                            RyzenFamily.PhoenixPoint2 or
                                            RyzenFamily.Mendocino or
                                            RyzenFamily.Rembrandt or
                                            RyzenFamily.Lucienne or
                                            RyzenFamily.Renoir;

                IsAmdApuVrmSettingAvailable = true;
                IsAmdPboSettingAvailable = !_systemInfoService.Cpu.Name.Contains('U') &&
                                           _systemInfoService.Cpu.RyzenFamily < RyzenFamily.Renoir;
                IsAmdCpuTuneSettingAvailable = _systemInfoService.GetBatteryStatus() == BatteryStatus.NoSystemBattery;
                IsAmdSoftClockSettingAvailable = _systemInfoService.Cpu.RyzenFamily < RyzenFamily.Renoir;
                IsAmdCOSettingAvailable = _systemInfoService.Cpu.RyzenFamily > RyzenFamily.Renoir &&
                                          _systemInfoService.Cpu.RyzenFamily != RyzenFamily.Mendocino;

                if (_systemInfoService.Cpu.RyzenFamily < RyzenFamily.Renoir)
                {
                    IsAmdPowerProfileSettingsAvailable = false;
                    IsAmdCOSettingAvailable = false;
                }

                IsAmdApuIGpuClockSettingAvailable = _systemInfoService.Cpu.RyzenFamily is
                    RyzenFamily.Renoir or
                    RyzenFamily.Lucienne or
                    RyzenFamily.Mendocino or
                    RyzenFamily.Rembrandt or
                    RyzenFamily.PhoenixPoint or
                    RyzenFamily.PhoenixPoint2 or
                    RyzenFamily.HawkPoint;

                IsAmdCCD2COSettingAvailable = _systemInfoService.Cpu.RyzenFamily == RyzenFamily.DragonRange &&
                                              _systemInfoService.Cpu.Name.Contains("Ryzen 9");

                var apuPresets = _apuPresetService.GetPresets();
                AvailablePresets = apuPresets.ToList();
            }
            else if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Desktop)
            {
                IsAmdCpuThermalSettingsAvailable = true;
                IsAmdCOSettingAvailable = _systemInfoService.Cpu.RyzenFamily >= RyzenFamily.Vermeer;
                IsAmdCCD1COSettingAvailable = IsAmdCOSettingAvailable;
                IsAmdCCD2COSettingAvailable = _systemInfoService.Cpu.Name.Contains("Ryzen 9");

                var desktopPresets = _amdDesktopPresetService.GetPresets();
                AvailablePresets = desktopPresets.ToList();
            }
        }

        if (_systemInfoService.LaptopInfo?.IsAsus == true)
        {
            IsAsusPowerSettingsAvailable = true;
            IsAsusGpuUltimateSettingsAvailable = true;
            IsAsusGpuEcoModeSettingsAvailable = true;

            var isGamingLaptop = _systemInfoService.Product.Contains("ROG")
                                 || _systemInfoService.Product.Contains("TUF");

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

            IsRadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
            IsNvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when applying preset");
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
            _logger.LogError(ex, "Exception occurred while deleting preset");
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

                        IsRadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
                        IsNvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;
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
        var commands = new List<string>(32);

        commands.Add(
            $"--UXTUSR={SelectedPreset.IsMag}-{SelectedPreset.IsVsync}-{SelectedPreset.Sharpness / 100}" +
            $"-{SelectedPreset.ResolutionScale.ResolutionScale}-{SelectedPreset.IsRecap}");

        if (_systemInfoService.LaptopInfo?.IsAsus == true)
        {
            if (SelectedAsusPowerProfile.PowerProfileMode != AsusMode.AcControlled)
                commands.Add($"--ASUS-Power={(int)SelectedAsusPowerProfile.PowerProfileMode}");
            if (IsAsusEcoMode)
                commands.Add($"--ASUS-Eco={IsAsusEcoMode}");
            if (IsAsusGpuUltimateSettingsAvailable)
                commands.Add($"--ASUS-MUX={IsAsusGpuUltimateSettingsAvailable}");
        }

        if (IsChangeRefreshRateAvailable && SelectedPreset.DisplayHz > 0)
        {
            commands.Add($"--Refresh-Rate={RefreshRates[SelectedPreset.DisplayHz - 1]}");
        }

        if (SelectedPreset.PowerMode.PowerPlan != PowerPlan.SystemControlled)
            commands.Add($"--Win-Power={SelectedPreset.PowerMode.PowerPlan}");

        if (_systemInfoService.Cpu.Manufacturer == Manufacturer.AMD && 
            _systemInfoService.Cpu.ProcessorType == ProcessorType.Apu)
        {
            if (SelectedPreset.IsApuTemp)
                commands.Add(
                    $"--tctl-temp={SelectedPreset.ApuTemperature} --cHTC-temp={SelectedPreset.ApuTemperature}");
            if (SelectedPreset.IsApuSkinTemp)
                commands.Add($"--apu-skin-temp={SelectedPreset.ApuSkinTemperature}");
            if (SelectedPreset.IsApuStapmPow)
                commands.Add($"--stapm-limit={SelectedPreset.ApuStapmPower * 1000}");
            if (SelectedPreset.IsApuFastPow)
                commands.Add($"--fast-limit={SelectedPreset.ApuFastPower * 1000}");
            if (SelectedPreset.IsApuStapmTime)  
                commands.Add($"--stapm-time={SelectedPreset.ApuStapmPower} ");
            if (SelectedPreset.IsApuSlowPow)
                commands.Add($"--slow-limit={SelectedPreset.ApuSlowPower * 1000}");
            if (SelectedPreset.IsApuSlowTime)
                commands.Add($"--slow-time={SelectedPreset.ApuSlowTime}");
            if (SelectedPreset.IsApuCpuTdc)
                commands.Add($"--vrm-current={SelectedPreset.ApuCpuTdc * 1000}");
            if (SelectedPreset.IsApuCpuEdc)
                commands.Add($"--vrmmax-current={SelectedPreset.ApuCpuEdc * 1000}");
            if (SelectedPreset.IsApuSocTdc)
                commands.Add($"--vrmsoc-current={SelectedPreset.ApuSocTdc * 1000}");
            if (SelectedPreset.IsApuSocEdc)
                commands.Add($"--vrmsocmax-current={SelectedPreset.ApuSocEdc * 1000}");
            if (SelectedPreset.IsApuGfxClk)
                commands.Add($"--vrmgfx-current={SelectedPreset.ApuGfxClock * 1000}");
            if (SelectedPreset.IsApuGfxEdc)
                commands.Add($"--vrmgfxmax-current={SelectedPreset.ApuGfxEdc * 1000}");
            if (SelectedPreset.IsApuGfxClk)
                commands.Add($"--gfx-clk={SelectedPreset.ApuGfxClock}");
            if (SelectedPreset.IsPboScalar)
                commands.Add($"--pbo-scalar={SelectedPreset.PboScalar * 100}");

            if (SelectedPreset.IsCoAllCore)
            {
                if (SelectedPreset.CoAllCore >= 0)
                {
                    commands.Add($"--set-coall={SelectedPreset.CoAllCore}");
                }
                else if (SelectedPreset.CoAllCore < 0)
                {
                    commands.Add(
                        $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoAllCore))} ");
                }
            }

            if (SelectedPreset.IsCoGfx)
            {
                if (SelectedPreset.CoGfx >= 0)
                {
                    commands.Add($"--set-cogfx={SelectedPreset.CoGfx}");
                }
                else if (SelectedPreset.CoGfx < 0)
                {
                    commands.Add($"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoGfx))}");
                }
            }

            if (SelectedPreset.IsSoftMiniGpuClk)
                commands.Add($"--min-gfxclk={SelectedPreset.SoftMiniGpuClk}");
            if (SelectedPreset.IsSoftMaxiGpuClk)
                commands.Add($"--max-gfxclk={SelectedPreset.SoftMaxiGpuClk}");

            if (SelectedPreset.IsSoftMinCpuClk)
                commands.Add($"--min-cpuclk={SelectedPreset.SoftMinCpuClk}");
            if (SelectedPreset.IsSoftMaxCpuClk)
                commands.Add($"--max-cpuclk={SelectedPreset.SoftMaxCpuClk}");

            if (SelectedPreset.IsSoftMinDataClk)
                commands.Add($"--min-lclk={SelectedPreset.SoftMinDataClk}");
            if (SelectedPreset.IsSoftMaxDataClk)
                commands.Add($"--max-lclk={SelectedPreset.SoftMaxDataClk}");

            if (SelectedPreset.IsSoftMinVcnClk)
                commands.Add($"--min-vcn={SelectedPreset.SoftMinVcnClk}");
            if (SelectedPreset.IsSoftMaxVcnClk)
                commands.Add($"--max-vcn={SelectedPreset.SoftMaxVcnClk}");

            if (SelectedPreset.IsSoftMinFabClk)
                commands.Add($"--min-fclk-frequency={SelectedPreset.SoftMinFabClk}");
            if (SelectedPreset.IsSoftMaxFabClk)
                commands.Add($"--max-fclk-frequency={SelectedPreset.SoftMaxFabClk}");

            if (SelectedPreset.IsSoftMinSoCClk)
                commands.Add($"--min-socclk-frequency={SelectedPreset.SoftMinSoCClk}");
            if (SelectedPreset.IsSoftMaxSoCClk)
                commands.Add($"--max-socclk-frequency={SelectedPreset.SoftMaxSoCClk}");

            switch (SelectedPreset.BoostProfile.BoostPlan)
            {
                case AmdBoostProfile.PowerSave:
                {
                    commands.Add("--power-saving");
                    break;
                }
                case AmdBoostProfile.Performance:
                {
                    commands.Add("--max-performance");
                    break;
                }
            }

            int coresCount = SelectedPreset.Ccd1States.Count;
            if (_systemInfoService.Cpu.RyzenFamily == RyzenFamily.DragonRange)
            {
                foreach (var state in SelectedPreset.Ccd1States)
                {
                    if (state.IsEnabled)
                    {
                        commands.Add(
                            $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % coresCount) & 15)) << 20) | (state.Value & 0xFFFF)}");
                    }
                }

                foreach (var state in SelectedPreset.Ccd2States)
                {
                    if (state.IsEnabled)
                    {
                        commands.Add(
                            $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % 8) & 15)) << 20) | (state.Value & 0xFFFF)}");
                    }
                }

            }
            else
            {
                foreach (var state in SelectedPreset.Ccd1States)
                {
                    if (state.IsEnabled)
                    {
                        commands.Add($"--set-coper={(state.CoreIndex << 20) | (state.Value & 0xFFFF)} ");
                    }
                }
            }

            if (SelectedPreset.IsAmdOc)
            {
                commands.Add($"--oc-clk={SelectedPreset.AmdClock} --oc-clk={SelectedPreset.AmdClock}");

                double vid;
                if (_systemInfoService.Cpu.RyzenFamily >= RyzenFamily.Rembrandt)
                {
                    vid = ((double)SelectedPreset.AmdVid - 1125) / 5 + 1200;
                    commands.Add($"--oc-volt={vid} --oc-volt={vid}");
                }
                else
                {
                    vid = Math.Round((double)SelectedPreset.AmdVid / 1000, 2);
                    commands.Add(
                        $"--oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} --oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)}");
                }

                commands.Add("--enable-oc");
            }
        }

        if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Desktop)
        {
            if (SelectedPreset.IsDtCpuTemp)
                commands.Add($"--tctl-limit={SelectedPreset.DtCpuTemperature * 1000}");
            if (SelectedPreset.IsDtCpuPpt)
                commands.Add($"--ppt-limit={SelectedPreset.DtCpuPpt * 1000}");
            if (SelectedPreset.IsDtCpuTdc)
                commands.Add($"--tdc-limit={SelectedPreset.DtCpuTdc * 1000}");
            if (SelectedPreset.IsDtCpuEdc)
                commands.Add($"--edc-limit={SelectedPreset.DtCpuEdc * 1000}");
            if (SelectedPreset.IsPboScalar)
                commands.Add($"--pbo-scalar={SelectedPreset.PboScalar * 100} ");

            if (SelectedPreset.IsCoAllCore)
            {
                if (SelectedPreset.CoAllCore >= 0)
                {
                    commands.Add($"--set-coall={SelectedPreset.CoAllCore}");
                }
                else if (SelectedPreset.CoAllCore < 0)
                {
                    commands.Add(
                        $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoAllCore))}");
                }

                if (SelectedPreset.IsCoGfx)
                {
                    if (SelectedPreset.CoGfx >= 0)
                    {
                        commands.Add($"--set-cogfx={SelectedPreset.CoGfx}");
                    }
                    else if (SelectedPreset.CoGfx < 0)
                    {
                        commands.Add(
                            $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * SelectedPreset.CoGfx))}");
                    }
                }

                int coresCount = SelectedPreset.Ccd1States.Count;
                foreach (var state in SelectedPreset.Ccd1States)
                {
                    if (state.IsEnabled)
                    {
                        commands.Add(
                            $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % coresCount) & 15)) << 20) | (state.Value & 0xFFFF)}");
                    }
                }

                foreach (var state in SelectedPreset.Ccd2States)
                {
                    if (state.IsEnabled)
                    {
                        commands.Add(
                            $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((state.CoreIndex % coresCount) & 15)) << 20) | (state.Value & 0xFFFF)} ");
                    }
                }

                if (SelectedPreset.IsAmdOc)
                {
                    commands.Add($"--oc-clk={SelectedPreset.AmdClock} --oc-clk={SelectedPreset.AmdClock}");

                    double vid;
                    if (_systemInfoService.Cpu.RyzenFamily >= RyzenFamily.Rembrandt)
                    {
                        vid = ((double)SelectedPreset.AmdVid - 1125) / 5 + 1200;
                        commands.Add($"--oc-volt={vid} --oc-volt={vid}");
                    }
                    else
                    {
                        vid = Math.Round((double)SelectedPreset.AmdVid / 1000, 2);
                        commands.Add(
                            $"--oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} --oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)}");
                    }

                    commands.Add("--enable-oc");
                }
            }

            if (_systemInfoService.Cpu.Manufacturer == Manufacturer.Intel)
            {
                if (SelectedPreset.IsIntelClockRatio)
                {
                    var clockRatioValue = string.Join('-', SelectedPreset.IntelClockRatios.Select(x => x.Ratio));
                    commands.Add($"--intel-ratio={clockRatioValue}");
                }

                if (SelectedPreset.IsIntelPl1)
                    commands.Add($"--intel-pl={SelectedPreset.IntelPl1}");
                if (SelectedPreset.IsIntelVoltages)
                    commands.Add(
                        $"--intel-volt-cpu={SelectedPreset.IntelVoltageCpu} --intel-volt-gpu={SelectedPreset.IntelVoltageGpu} --intel-volt-cache={SelectedPreset.IntelVoltageCache} --intel-volt-cpu={SelectedPreset.IntelVoltageSa}");
                if (SelectedPreset.IsIntelBal)
                    commands.Add(
                        $"--intel-bal-cpu={SelectedPreset.IntelBalCpu} --intel-bal-gpu={SelectedPreset.IntelBalGpu}");
                if (SelectedPreset.IsApuGfxClk)
                    commands.Add($"--intel-gpu={SelectedPreset.ApuGfxClock}");
            }

            if (SelectedPreset.IsRadeonGraphics)
            {
                commands.Add(SelectedPreset.IsAntiLag
                    ? "--ADLX-Lag=0-true --ADLX-Lag=1-true"
                    : "--ADLX-Lag=0-false --ADLX-Lag=1-false");

                commands.Add(SelectedPreset.IsRsr
                    ? $"--ADLX-RSR=true-{SelectedPreset.Rsr}"
                    : $"--ADLX-RSR=false-{SelectedPreset.Rsr}");

                commands.Add(
                    SelectedPreset.IsBoost
                        ? $"--ADLX-Boost=0-true-{SelectedPreset.Boost} --ADLX-Boost=1-true-{SelectedPreset.Boost}"
                        : $"--ADLX-Boost=0-false-{SelectedPreset.Boost} --ADLX-Boost=1-false-{SelectedPreset.Boost}");

                commands.Add(
                    SelectedPreset.IsImageSharp
                        ? $"--ADLX-ImageSharp=0-true-{SelectedPreset.ImageSharp} --ADLX-ImageSharp=1-true-{SelectedPreset.ImageSharp}"
                        : $"--ADLX-ImageSharp=0-false-{SelectedPreset.ImageSharp} --ADLX-ImageSharp=1-false-{SelectedPreset.ImageSharp}");

                commands.Add(SelectedPreset.IsSync
                    ? "--ADLX-Sync=0-true --ADLX-Sync=1-true"
                    : "--ADLX-Sync=0-false --ADLX-Sync=1-false");
            }

            if (SelectedPreset.IsNvidia)
            {
                commands.Add(
                    $"--NVIDIA-Clocks={SelectedPreset.NvMaxCoreClk}-{SelectedPreset.NvCoreClk}-{SelectedPreset.NvMemClk} ");
            }
        }
        
        return string.Join(' ', commands);
    }
}