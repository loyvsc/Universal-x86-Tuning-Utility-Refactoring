using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
using Universal_x86_Tuning_Utility.Services.PresetServices;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class CustomPresetsViewModel : NotifyPropertyChangedBase
{
    public ICommand ApplyPresetCommand { get; }
    public ICommand SavePresetCommand { get; }
    public ICommand DeletePresetCommand { get; }
    public ICommand UndoCommand { get; }
    public ICommand ReloadPresetValuesCommand { get; }
    

    public bool NvidiaGpuSettingsAvailable
    {
        get => _nvidiaGpuSettingsAvailable;
        set => SetValue(ref _nvidiaGpuSettingsAvailable, value);
    }

    public bool RadeonGpuSettingsAvailable
    {
        get => _radeonGpuSettingsAvailable;
        set => SetValue(ref _radeonGpuSettingsAvailable, value);
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

    public bool IsAmdC0SettingAvailable
    {
        get => _isAmdC0SettingAvailable;
        set => SetValue(ref _isAmdC0SettingAvailable, value);
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

    public bool IsUndoActionAvailable
    {
        get => _isUndoActionAvailable;
        set => SetValue(ref _isUndoActionAvailable, value);
    }

    public bool IsAmdOc
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
        set => _selectedPreset;
    }

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
    private bool _isAmdC0SettingAvailable;
    private bool _isAmdPowerProfileSettingsAvailable;
    private bool _isAmdCCD1COSettingAvailable;
    private bool _isAmdCCD2COSettingAvailable;
    private bool _isAmdApuIGpuClockSettingAvailable;
    private bool _isAmdPboSettingAvailable;
    private bool _isAmdApuSettingsAvailable;
    private bool _isChangeRefreshRateAvailable;
    private bool _nvidiaGpuSettingsAvailable;
    private bool _radeonGpuSettingsAvailable;
    private bool _undoActionAvailable;

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

    public CustomPresetsViewModel(ILogger<CustomPresetsViewModel> logger,
        ISystemInfoService systemInfoService,
        INotificationManager notificationManager,
        IRyzenAdjService ryzenAdjService,
        IDisplayInfoService displayInfoService,
        IIntelManagementService intelManagementService,
        IASUSWmiService asusWmiService)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
        _notificationManager = notificationManager;
        _ryzenAdjService = ryzenAdjService;
        _displayInfoService = displayInfoService;
        _intelManagementService = intelManagementService;
        _asusWmiService = asusWmiService;

        _apuPresetService = PresetServiceFactory.GetPresetService(Settings.Default.Path + "apuPresets.json");
        _amdDesktopPresetService = PresetServiceFactory.GetPresetService(Settings.Default.Path + "amdDtCpuPresets.json");
        _intelPresetService = PresetServiceFactory.GetPresetService(Settings.Default.Path + "intelPresets.json");

        DeletePresetCommand = ReactiveCommand.CreateFromTask(DeleteCurrentPreset);
        UndoCommand = ReactiveCommand.CreateFromTask(Undo);

        Initialize();
    }

    private void Initialize()
    {
        // default values
        SelectedPreset = new Preset()
        {
            ApuSkinTemperature = 45,
            ApuTemperature = 95,
            
        }
        nudAPUSkinTemp.Value = 45;
        nudAPUTemp.Value = 95;
        nudSTAPMPow.Value = 28;
        nudFastPow.Value = 28;
        nudSlowPow.Value = 28;
        nudSlowTime.Value = 128;
        nudFastTime.Value = 64;
        nudCpuVrmTdc.Value = 64;
        nudCpuVrmEdc.Value = 64;
        nudGfxVrmTdc.Value = 64;
        nudGfxVrmEdc.Value = 64;
        nudSocVrmTdc.Value = 64;
        nudSocVrmEdc.Value = 64;
        nudAPUiGPUClk.Value = 1000;
        nudCPUTemp.Value = 85;
        nudPPT.Value = 140;
        nudEDC.Value = 160;
        nudTDC.Value = 160;
        nudIntelPL1.Value = 35;
        nudIntelPL2.Value = 65;
        nudAmdVID.Value = 1200;
        nudAmdCpuClk.Value = 3200;
        nudNVMaxCore.Value = 4000;

        nudIntelCpuBal.Value = 9;
        nudIntelGpuBal.Value = 13;

        RadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
        NvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;

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

            intelRatioControls = new NumberBox[]
            {
                nudIntelRatioC1,
                nudIntelRatioC2,
                nudIntelRatioC3,
                nudIntelRatioC4,
                nudIntelRatioC5,
                nudIntelRatioC6,
                nudIntelRatioC7,
                nudIntelRatioC8
            };

            if (clockRatio != null)
            {
                var core = 0;
                foreach (var clock in clockRatio)
                {
                    if (core < intelRatioControls.Length) intelRatioControls[core].Value = clock;

                    core++;
                }
            }

            // Get the names of all the stored presets
            var intelPresets = _intelPresetService.GetPresets();
            AvailablePresets = intelPresets.ToList();
        }
        else if (_systemInfoService.Cpu.Manufacturer == Manufacturer.AMD)
        {
            IsIntelSettingsAvailable = false;

            if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Apu)
            {
                IsAmdApuSettingsAvailable = _systemInfoService.Cpu.RyzenFamily == RyzenFamily.PhoenixPoint ||
                                            (_systemInfoService.Cpu.RyzenFamily == RyzenFamily.PhoenixPoint2 &&
                                             _systemInfoService.Cpu.RyzenFamily == RyzenFamily.Mendocino &&
                                             _systemInfoService.Cpu.RyzenFamily == RyzenFamily.Rembrandt &&
                                             _systemInfoService.Cpu.RyzenFamily == RyzenFamily.Lucienne &&
                                             _systemInfoService.Cpu.RyzenFamily == RyzenFamily.Renoir);

                IsAmdPboSettingAvailable = !_systemInfoService.Cpu.Name.Contains('U') &&
                                           _systemInfoService.Cpu.RyzenFamily < RyzenFamily.Renoir;
                IsAmdCpuTuneSettingAvailable = _systemInfoService.GetBatteryStatus() == BatteryStatus.NoSystemBattery;
                IsAmdSoftClockSettingAvailable = _systemInfoService.Cpu.RyzenFamily < RyzenFamily.Renoir;
                IsAmdC0SettingAvailable = _systemInfoService.Cpu.RyzenFamily > RyzenFamily.Renoir &&
                                          _systemInfoService.Cpu.RyzenFamily != RyzenFamily.Mendocino;

                if (_systemInfoService.Cpu.RyzenFamily < RyzenFamily.Renoir)
                {
                    IsAmdPowerProfileSettingsAvailable = false;
                    IsAmdC0SettingAvailable = false;
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
            else if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Desktop)
            {
                IsAmdC0SettingAvailable = _systemInfoService.Cpu.RyzenFamily >= RyzenFamily.Vermeer;
                IsAmdCCD1COSettingAvailable = IsAmdC0SettingAvailable;
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
                IsAsusMux = false;
            else if (mux > -1)
                IsAsusMux = true;
            else IsAsusMux = false;

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

    private async Task Undo()
    {
        IsAmdOc = false;
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

            RadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
            NvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;
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
            if cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null
            switch (_systemInfoService.Cpu.Manufacturer)
            {
                case Manufacturer.AMD:
                {
                    if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Apu)
                        _apuPresetService.DeletePreset(deletePresetName);
                    else if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Desktop)
                        _amdDesktopPresetService.DeletePreset(deletePresetName);

                    break;
                }
                case Manufacturer.Intel:
                {
                    _intelPresetService.DeletePreset(deletePresetName);
                    break;
                }
            }

            await _notificationManager.ShowTextNotification("Preset Deleted",
                $"Your preset {deletePresetName} has been deleted successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while deleting preset");
            await _notificationManager.ShowTextNotification("Preset not deleted",
                $"Exception occurred while deleting preset!");
        }
    }

    private Task ReloadPreset()
    {
        if (cbxPowerPreset.Text != null && cbxPowerPreset.Text != "")
            updateValues(cbxPowerPreset.SelectedItem.ToString());
    }

    private async Task SavePreset()
    {
        if (!SelectedPreset.Name.Contains("PM -"))
        {
            switch (_systemInfoService.Cpu.Manufacturer)
            {
                case Manufacturer.AMD:
                {
                    if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Apu)
                    {
                        // Save a preset
                        var preset = new Preset
                        {
                            CommandValue = GetCommandValues(),
                        
                            AsusPowerProfile = (int)SelectedAsusPowerProfile.PowerProfileMode, 
                        };

                        _apuPresetService.SavePreset(SelectedPreset.Name, preset);

                        RadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
                        NvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;
                    }
                    if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Desktop) 
                    { 
                        var preset = new Preset 
                        {
                        dtCpuTemp = (int)nudCPUTemp.Value,
                        dtCpuPPT = (int)nudPPT.Value,
                        dtCpuTDC = (int)nudTDC.Value,
                        dtCpuEDC = (int)nudEDC.Value,
                        pboScalar = (int)nudPBOScaler.Value,
                        coAllCore = (int)nudAllCO.Value,

                        boostProfile = (int)cbxBoost.SelectedIndex,

                        Rsr = (int)nudRSR.Value,
                        Boost = (int)nudBoost.Value,
                        ImageSharp = (int)nudImageSharp.Value,
                        IsRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                        IsRsr = (bool)cbRSR.IsChecked,
                        IsBoost = (bool)cbBoost.IsChecked,
                        IsAntiLag = (bool)cbAntiLag.IsChecked,
                        IsImageSharp = (bool)cbImageSharp.IsChecked,
                        IsSync = (bool)cbSync.IsChecked,

                        CommandValue = GetCommandValues(),


                        IsDtCpuTemp = (bool)cbCPUTemp.IsChecked,
                        IsDtCpuPpt = (bool)cbPPT.IsChecked,
                        IsDtCpuTdc = (bool)cbTDC.IsChecked,
                        IsDtCpuEdc = (bool)cbEDC.IsChecked,
                        IsPboScalar = (bool)cbPBOScaler.IsChecked,
                        IsCoAllCore = (bool)cbAllCO.IsChecked,

                        CoGfx = (int)nudGfxCO.Value,
                        IsCoGfx = (bool)cbGfxCO.IsChecked,

                        IsNvidia = (bool)tsNV.IsChecked,
                        NvMaxCoreClk = (int)nudNVMaxCore.Value,
                        NvCoreClk = (int)nudNVCore.Value,
                        NvMemClk = (int)nudNVMem.Value,

                        IsAmdOc = (bool)IsAmdOc,
                        AmdClock = (int)nudAmdCpuClk.Value,
                        AmdVid = (int)nudAmdVID.Value,

                        AsusGpuUlti = (bool)tsASUSUlti.IsChecked,
                        AsusIGpu = (bool)tsASUSEco.IsChecked,
                        AsusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                        DisplayHz = (int)cbxRefreshRate.SelectedIndex,

                        IsMag = (bool)tsUXTUSR.IsChecked,
                        IsVsync = (bool)cbVSync.IsChecked,
                        IsRecap = (bool)cbAutoCap.IsChecked,
                        Sharpness = (int)nudSharp.Value,
                        ResScaleIndex = (int)cbxResScale.SelectedIndex,

                        PowerMode = (int)cbxPowerMode.SelectedIndex
                    }; 
                        _amdDesktopPresetService.SavePreset(_selectedPreset.Name, preset); 
                    }
                    break;
                }
                case Manufacturer.Intel:
                {
                    var preset = new Preset
                    {
                        IntelPl1 = (int)nudIntelPL1.Value,
                        IntelPl2 = (int)nudIntelPL2.Value,
                        IntelVoltCPU = (int)nudIntelCoreUV.Value,
                        IntelVoltGPU = (int)nudIntelGfxUV.Value,
                        IntelVoltCache = (int)nudIntelCacheUV.Value,
                        IntelVoltSA = (int)nudIntelSAUV.Value,
                        IntelBalCpu = (int)nudIntelCpuBal.Value,
                        IntelBalGpu = (int)nudIntelGpuBal.Value,

                        IsApuGfxClk = (bool)cbAPUiGPUClk.IsChecked,
                        apuGfxClk = (int)nudAPUiGPUClk.Value,

                        Rsr = (int)nudRSR.Value,
                        Boost = (int)nudBoost.Value,
                        ImageSharp = (int)nudImageSharp.Value,
                        IsRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                        IsRsr = (bool)cbRSR.IsChecked,
                        IsBoost = (bool)cbBoost.IsChecked,
                        IsAntiLag = (bool)cbAntiLag.IsChecked,
                        IsImageSharp = (bool)cbImageSharp.IsChecked,
                        IsSync = (bool)cbSync.IsChecked,

                        CommandValue = GetCommandValues(),

                        IsIntelPl1 = (bool)cbIntelPL1.IsChecked,
                        IsIntelPl2 = (bool)cbIntelPL2.IsChecked,
                        IsIntelVolt = (bool)tsIntelUV.IsChecked,
                        IsIntelBal = (bool)tsIntelBal.IsChecked,

                        IsNvidia = (bool)tsNV.IsChecked,
                        NvMaxCoreClk = (int)nudNVMaxCore.Value,
                        NvCoreClk = (int)nudNVCore.Value,
                        NvMemClk = (int)nudNVMem.Value,

                        AsusGpuUlti = (bool)tsASUSUlti.IsChecked,
                        AsusiGpu = (bool)tsASUSEco.IsChecked,
                        AsusPowerProfile = (int)SelectedAsusPowerProfile.PowerProfileMode,

                        DisplayHz = (int)cbxRefreshRate.SelectedIndex,

                        IsMag = (bool)tsUXTUSR.IsChecked,
                        IsVsync = (bool)cbVSync.IsChecked,
                        IsRecap = (bool)cbAutoCap.IsChecked,
                        Sharpness = (int)nudSharp.Value,
                        ResScaleIndex = (int)cbxResScale.SelectedIndex,

                        PowerMode = (int)cbxPowerMode.SelectedIndex,

                        IsIntelClockRatio = (bool)tsIntelRatioCore.IsChecked,
                        IntelClockRatioC1 = (int)nudIntelRatioC1.Value,
                        IntelClockRatioC2 = (int)nudIntelRatioC2.Value,
                        IntelClockRatioC3 = (int)nudIntelRatioC3.Value,
                        IntelClockRatioC4 = (int)nudIntelRatioC4.Value,
                        IntelClockRatioC5 = (int)nudIntelRatioC5.Value,
                        IntelClockRatioC6 = (int)nudIntelRatioC6.Value,
                        IntelClockRatioC7 = (int)nudIntelRatioC7.Value,
                        IntelClockRatioC8 = (int)nudIntelRatioC8.Value
                    };
                    
                    _intelPresetService.SavePreset(_selectedPreset.Name, preset);
                    break;
                }
            }

            await _notificationManager.ShowTextNotification("Preset Saved",
                $"Your preset {_selectedPreset.Name} has been saved successfully!");
        }
    }

    private string GetCommandValues()
    {
        string commandValues = commandValues +
                               $"--UXTUSR={tsUXTUSR.IsChecked}-{cbVSync.IsChecked}-{nudSharp.Value / 100}-{cbxResScale.SelectedIndex}-{cbAutoCap.IsChecked} ";

        if (_systemInfoService.LaptopInfo?.IsAsus == true)
        {
            if (SelectedAsusPowerProfile.PowerProfileMode != AsusMode.AcControlled)
                commandValues = commandValues + $"--ASUS-Power={(int) SelectedAsusPowerProfile.PowerProfileMode} ";
            if (IsAsusEcoMode)
                commandValues = commandValues + $"--ASUS-Eco={IsAsusEcoMode} ";
            if (IsAsusGpuUltimateSettingsAvailable)
                commandValues = commandValues + $"--ASUS-MUX={IsAsusGpuUltimateSettingsAvailable} ";
        }

        if (sdRefreshRate.Visibility == Visibility.Visible && cbxRefreshRate.SelectedIndex > 0)
        {
            commandValues = commandValues +
                            $"--Refresh-Rate={WindowsDisplayInfoService.uniqueRefreshRates[cbxRefreshRate.SelectedIndex - 1]} ";
        }

        if (sdPowerMode.Visibility == Visibility.Visible && cbxPowerMode.SelectedIndex > 0)
            commandValues = commandValues + $"--Win-Power={cbxPowerMode.SelectedIndex - 1} ";

        if (Family.TYPE == Family.ProcessorType.Amd_Apu)
        {
            if (cbAPUTemp.IsChecked == true)
                commandValues = commandValues + $"--tctl-temp={nudAPUTemp.Value} --cHTC-temp={nudAPUTemp.Value} ";
            if (cbAPUSkinTemp.IsChecked == true)
                commandValues = commandValues + $"--apu-skin-temp={nudAPUSkinTemp.Value} ";
            if (cbSTAPMPow.IsChecked == true)
                commandValues = commandValues + $"--stapm-limit={nudSTAPMPow.Value * 1000}  ";
            if (cbFastPow.IsChecked == true) commandValues = commandValues + $"--fast-limit={nudFastPow.Value * 1000} ";
            if (cbFastTime.IsChecked == true) commandValues = commandValues + $"--stapm-time={nudFastTime.Value} ";
            if (cbSlowPow.IsChecked == true) commandValues = commandValues + $"--slow-limit={nudSlowPow.Value * 1000} ";
            if (cbSlowTime.IsChecked == true) commandValues = commandValues + $"--slow-time={nudSlowTime.Value} ";
            if (cbCpuVrmTdc.IsChecked == true)
                commandValues = commandValues + $"--vrm-current={nudCpuVrmTdc.Value * 1000} ";
            if (cbCpuVrmEdc.IsChecked == true)
                commandValues = commandValues + $"--vrmmax-current={nudCpuVrmEdc.Value * 1000} ";
            if (cbSocVrmTdc.IsChecked == true)
                commandValues = commandValues + $"--vrmsoc-current={nudSocVrmTdc.Value * 1000} ";
            if (cbSocVrmEdc.IsChecked == true)
                commandValues = commandValues + $"--vrmsocmax-current={nudSocVrmEdc.Value * 1000} ";
            if (cbGfxVrmTdc.IsChecked == true)
                commandValues = commandValues + $"--vrmgfx-current={nudGfxVrmTdc.Value * 1000} ";
            if (cbGfxVrmEdc.IsChecked == true)
                commandValues = commandValues + $"--vrmgfxmax-current={nudGfxVrmEdc.Value * 1000} ";
            if (cbAPUiGPUClk.IsChecked == true) commandValues = commandValues + $"--gfx-clk={nudAPUiGPUClk.Value} ";
            if (cbPBOScaler.IsChecked == true)
                commandValues = commandValues + $"--pbo-scalar={nudPBOScaler.Value * 100} ";

            if (cbAllCO.IsChecked == true)
            {
                if (nudAllCO.Value >= 0) commandValues = commandValues + $"--set-coall={nudAllCO.Value} ";
                if (nudAllCO.Value < 0)
                    commandValues = commandValues +
                                    $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudAllCO.Value))} ";
            }

            if (cbGfxCO.IsChecked == true)
            {
                if (nudGfxCO.Value >= 0) commandValues = commandValues + $"--set-cogfx={nudGfxCO.Value} ";
                if (nudGfxCO.Value < 0)
                    commandValues = commandValues +
                                    $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudGfxCO.Value))} ";
            }

            if (cbSoftMiniGPUClk.IsChecked == true)
                commandValues = commandValues + $"--min-gfxclk={nudSoftMiniGPUClk.Value} ";
            if (cbSoftMaxiGPUClk.IsChecked == true)
                commandValues = commandValues + $"--max-gfxclk={nudSoftMaxiGPUClk.Value} ";

            if (cbSoftMinCPUClk.IsChecked == true)
                commandValues = commandValues + $"--min-cpuclk={nudSoftMinCPUClk.Value} ";
            if (cbSoftMaxCPUClk.IsChecked == true)
                commandValues = commandValues + $"--max-cpuclk={nudSoftMaxCPUClk.Value} ";

            if (cbSoftMinDataClk.IsChecked == true)
                commandValues = commandValues + $"--min-lclk={nudSoftMinDataClk.Value} ";
            if (cbSoftMaxDataClk.IsChecked == true)
                commandValues = commandValues + $"--max-lclk={nudSoftMaxDataClk.Value} ";

            if (cbSoftMinVCNClk.IsChecked == true)
                commandValues = commandValues + $"--min-vcn={nudSoftMinVCNClk.Value} ";
            if (cbSoftMaxVCNClk.IsChecked == true)
                commandValues = commandValues + $"--max-vcn={nudSoftMaxVCNClk.Value} ";

            if (cbSoftMinFabClk.IsChecked == true)
                commandValues = commandValues + $"--min-fclk-frequency={nudSoftMinFabClk.Value} ";
            if (cbSoftMaxFabClk.IsChecked == true)
                commandValues = commandValues + $"--max-fclk-frequency={nudSoftMaxFabClk.Value} ";

            if (cbSoftMinSoCClk.IsChecked == true)
                commandValues = commandValues + $"--min-socclk-frequency={nudSoftMinSoCClk.Value} ";
            if (cbSoftMaxSoCClk.IsChecked == true)
                commandValues = commandValues + $"--max-socclk-frequency={nudSoftMaxSoCClk.Value} ";

            if (cbxBoost.SelectedIndex > 0)
            {
                if (cbxBoost.SelectedIndex == 1) commandValues = commandValues + $"--power-saving ";
                if (cbxBoost.SelectedIndex == 2) commandValues = commandValues + $"--max-performance ";
            }

            if (_systemInfoService.Cpu.RyzenFamily == RyzenFamily.DragonRange)
            {
                if (cbCCD1Core1.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((0 % 8) & 15)) << 20) | ((int)nudCCD1Core1.Value & 0xFFFF)} ";
                if (cbCCD1Core2.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((1 % 8) & 15)) << 20) | ((int)nudCCD1Core2.Value & 0xFFFF)} ";
                if (cbCCD1Core3.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((2 % 8) & 15)) << 20) | ((int)nudCCD1Core3.Value & 0xFFFF)} ";
                if (cbCCD1Core4.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((3 % 8) & 15)) << 20) | ((int)nudCCD1Core4.Value & 0xFFFF)} ";
                if (cbCCD1Core5.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((4 % 8) & 15)) << 20) | ((int)nudCCD1Core5.Value & 0xFFFF)} ";
                if (cbCCD1Core6.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((5 % 8) & 15)) << 20) | ((int)nudCCD1Core6.Value & 0xFFFF)} ";
                if (cbCCD1Core7.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((6 % 8) & 15)) << 20) | ((int)nudCCD1Core7.Value & 0xFFFF)} ";
                if (cbCCD1Core8.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((7 % 8) & 15)) << 20) | ((int)nudCCD1Core8.Value & 0xFFFF)} ";

                if (cbCCD2Core1.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((0 % 8) & 15)) << 20) | ((int)nudCCD2Core1.Value & 0xFFFF)} ";
                if (cbCCD2Core2.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((1 % 8) & 15)) << 20) | ((int)nudCCD2Core2.Value & 0xFFFF)} ";
                if (cbCCD2Core3.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((2 % 8) & 15)) << 20) | ((int)nudCCD2Core3.Value & 0xFFFF)} ";
                if (cbCCD2Core4.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((3 % 8) & 15)) << 20) | ((int)nudCCD2Core4.Value & 0xFFFF)} ";
                if (cbCCD2Core5.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((4 % 8) & 15)) << 20) | ((int)nudCCD2Core5.Value & 0xFFFF)} ";
                if (cbCCD2Core6.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((5 % 8) & 15)) << 20) | ((int)nudCCD2Core6.Value & 0xFFFF)} ";
                if (cbCCD2Core7.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((6 % 8) & 15)) << 20) | ((int)nudCCD2Core7.Value & 0xFFFF)} ";
                if (cbCCD2Core8.IsChecked == true)
                    commandValues = commandValues +
                                    $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((7 % 8) & 15)) << 20) | ((int)nudCCD2Core8.Value & 0xFFFF)} ";
            }
            else
            {
                if (cbCCD1Core1.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(0 << 20) | ((int)nudCCD1Core1.Value & 0xFFFF)} ";
                if (cbCCD1Core2.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(1 << 20) | ((int)nudCCD1Core2.Value & 0xFFFF)} ";
                if (cbCCD1Core3.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(2 << 20) | ((int)nudCCD1Core3.Value & 0xFFFF)} ";
                if (cbCCD1Core4.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(3 << 20) | ((int)nudCCD1Core4.Value & 0xFFFF)} ";
                if (cbCCD1Core5.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(4 << 20) | ((int)nudCCD1Core5.Value & 0xFFFF)} ";
                if (cbCCD1Core6.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(5 << 20) | ((int)nudCCD1Core6.Value & 0xFFFF)} ";
                if (cbCCD1Core7.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(6 << 20) | ((int)nudCCD1Core7.Value & 0xFFFF)} ";
                if (cbCCD1Core8.IsChecked == true)
                    commandValues = commandValues + $"--set-coper={(7 << 20) | ((int)nudCCD1Core8.Value & 0xFFFF)} ";
            }

            if (IsAmdOc == true)
            {
                double vid = 0;

                vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                commandValues = commandValues +
                                $"--oc-clk={(int)nudAmdCpuClk.Value} --oc-clk={(int)nudAmdCpuClk.Value} ";

                if (_systemInfoService.Cpu.RyzenFamily >= RyzenFamily.Rembrandt)
                {
                    vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                    commandValues = commandValues + $"--oc-volt={vid} --oc-volt={vid} ";
                }
                else
                {
                    vid = Math.Round((double)nudAmdVID.Value / 1000, 2);
                    commandValues = commandValues +
                                    $"--oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} --oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} ";
                }

                commandValues = commandValues + $"--enable-oc --enable-oc ";
            }
        }

        if (Family.TYPE == Family.ProcessorType.Amd_Desktop_Cpu)
        {
            if (cbCPUTemp.IsChecked == true) commandValues = commandValues + $"--tctl-limit={nudCPUTemp.Value * 1000} ";
            if (cbPPT.IsChecked == true) commandValues = commandValues + $"--ppt-limit={nudPPT.Value * 1000} ";
            if (cbTDC.IsChecked == true) commandValues = commandValues + $"--tdc-limit={nudTDC.Value * 1000} ";
            if (cbEDC.IsChecked == true) commandValues = commandValues + $"--edc-limit={nudEDC.Value * 1000} ";
            if (cbPBOScaler.IsChecked == true)
                commandValues = commandValues + $"--pbo-scalar={nudPBOScaler.Value * 100} ";

            if (cbAllCO.IsChecked == true)
            {
                if (nudAllCO.Value >= 0) commandValues = commandValues + $"--set-coall={nudAllCO.Value} ";
                if (nudAllCO.Value < 0)
                    commandValues = commandValues +
                                    $"--set-coall={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudAllCO.Value))} ";
            }

            if (cbGfxCO.IsChecked == true)
            {
                if (nudGfxCO.Value >= 0) commandValues = commandValues + $"--set-cogfx={nudGfxCO.Value} ";
                if (nudGfxCO.Value < 0)
                    commandValues = commandValues +
                                    $"--set-cogfx={Convert.ToUInt32(0x100000 - (uint)(-1 * (int)nudGfxCO.Value))} ";
            }

            if (cbCCD1Core1.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((0 % 8) & 15)) << 20) | ((int)nudCCD1Core1.Value & 0xFFFF)} ";
            if (cbCCD1Core2.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((1 % 8) & 15)) << 20) | ((int)nudCCD1Core2.Value & 0xFFFF)} ";
            if (cbCCD1Core3.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((2 % 8) & 15)) << 20) | ((int)nudCCD1Core3.Value & 0xFFFF)} ";
            if (cbCCD1Core4.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((3 % 8) & 15)) << 20) | ((int)nudCCD1Core4.Value & 0xFFFF)} ";
            if (cbCCD1Core5.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((4 % 8) & 15)) << 20) | ((int)nudCCD1Core5.Value & 0xFFFF)} ";
            if (cbCCD1Core6.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((5 % 8) & 15)) << 20) | ((int)nudCCD1Core6.Value & 0xFFFF)} ";
            if (cbCCD1Core7.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((6 % 8) & 15)) << 20) | ((int)nudCCD1Core7.Value & 0xFFFF)} ";
            if (cbCCD1Core8.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((0 << 4) | ((0 % 1) & 15)) << 4) | ((7 % 8) & 15)) << 20) | ((int)nudCCD1Core8.Value & 0xFFFF)} ";

            if (cbCCD2Core1.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((0 % 8) & 15)) << 20) | ((int)nudCCD2Core1.Value & 0xFFFF)} ";
            if (cbCCD2Core2.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((1 % 8) & 15)) << 20) | ((int)nudCCD2Core2.Value & 0xFFFF)} ";
            if (cbCCD2Core3.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((2 % 8) & 15)) << 20) | ((int)nudCCD2Core3.Value & 0xFFFF)} ";
            if (cbCCD2Core4.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((3 % 8) & 15)) << 20) | ((int)nudCCD2Core4.Value & 0xFFFF)} ";
            if (cbCCD2Core5.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((4 % 8) & 15)) << 20) | ((int)nudCCD2Core5.Value & 0xFFFF)} ";
            if (cbCCD2Core6.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((5 % 8) & 15)) << 20) | ((int)nudCCD2Core6.Value & 0xFFFF)} ";
            if (cbCCD2Core7.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((6 % 8) & 15)) << 20) | ((int)nudCCD2Core7.Value & 0xFFFF)} ";
            if (cbCCD2Core8.IsChecked == true)
                commandValues = commandValues +
                                $"--set-coper={(((((1 << 4) | ((0 % 1) & 15)) << 4) | ((7 % 8) & 15)) << 20) | ((int)nudCCD2Core8.Value & 0xFFFF)} ";

            if (IsAmdOc == true)
            {
                double vid = 0;

                vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                commandValues = commandValues +
                                $"--oc-clk={(int)nudAmdCpuClk.Value} --oc-clk={(int)nudAmdCpuClk.Value} ";

                if (_systemInfoService.Cpu.RyzenFamily >= RyzenFamily.Rembrandt)
                {
                    vid = ((double)nudAmdVID.Value - 1125) / 5 + 1200;
                    commandValues = commandValues + $"--oc-volt={vid} --oc-volt={vid} ";
                }
                else
                {
                    vid = Math.Round((double)nudAmdVID.Value / 1000, 2);
                    commandValues = commandValues +
                                    $"--oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} --oc-volt={Convert.ToUInt32((1.55 - vid) / 0.00625)} ";
                }

                commandValues = commandValues + $"--enable-oc --enable-oc ";
            }
        }

        if (Family.TYPE == Family.ProcessorType.Intel)
        {
            if (tsIntelRatioCore.IsChecked == true)
            {
                commandValues = commandValues + $"--intel-ratio=";
                var core = 0;
                foreach (int clock in clockRatio)
                {
                    if (core < intelRatioControls.Length)
                    {
                        if (core == clockRatio.Length - 1)
                            commandValues = commandValues + $"{intelRatioControls[core].Value} ";
                        else commandValues = commandValues + $"{intelRatioControls[core].Value}-";
                    }

                    core++;
                }
            }

            if (cbIntelPL1.IsChecked == true) commandValues = commandValues + $"--intel-pl={nudIntelPL1.Value} ";
            if (tsIntelUV.IsChecked == true)
                commandValues = commandValues +
                                $"--intel-volt-cpu={nudIntelCoreUV.Value} --intel-volt-gpu={nudIntelGfxUV.Value} --intel-volt-cache={nudIntelCacheUV.Value} --intel-volt-cpu={nudIntelSAUV.Value} ";
            if (tsIntelBal.IsChecked == true)
                commandValues = commandValues +
                                $"--intel-bal-cpu={nudIntelCpuBal.Value} --intel-bal-gpu={nudIntelGpuBal.Value} ";
            if (cbAPUiGPUClk.IsChecked == true) commandValues = commandValues + $"--intel-gpu={nudAPUiGPUClk.Value} ";
        }

        if (tsRadeonGraph.IsChecked == true)
        {
            if (cbAntiLag.IsChecked == true) commandValues = commandValues + $"--ADLX-Lag=0-true --ADLX-Lag=1-true ";
            else commandValues = commandValues + $"--ADLX-Lag=0-false --ADLX-Lag=1-false ";

            if (cbRSR.IsChecked == true) commandValues = commandValues + $"--ADLX-RSR=true-{(int)nudRSR.Value} ";
            else commandValues = commandValues + $"--ADLX-RSR=false-{(int)nudRSR.Value} ";

            if (cbBoost.IsChecked == true)
                commandValues = commandValues +
                                $"--ADLX-Boost=0-true-{(int)nudBoost.Value} --ADLX-Boost=1-true-{(int)nudBoost.Value} ";
            else
                commandValues = commandValues +
                                $"--ADLX-Boost=0-false-{(int)nudBoost.Value} --ADLX-Boost=1-false-{(int)nudBoost.Value} ";

            if (cbImageSharp.IsChecked == true)
                commandValues = commandValues +
                                $"--ADLX-ImageSharp=0-true-{(int)nudImageSharp.Value} --ADLX-ImageSharp=1-true-{(int)nudImageSharp.Value} ";
            else
                commandValues = commandValues +
                                $"--ADLX-ImageSharp=0-false-{(int)nudImageSharp.Value} --ADLX-ImageSharp=1-false-{(int)nudImageSharp.Value} ";

            if (cbSync.IsChecked == true) commandValues = commandValues + $"--ADLX-Sync=0-true --ADLX-Sync=1-true ";
            else commandValues = commandValues + $"--ADLX-Sync=0-false --ADLX-Sync=1-false ";
        }

        if (tsNV.IsChecked == true)
            commandValues = commandValues + $"--NVIDIA-Clocks={nudNVMaxCore.Value}-{nudNVCore.Value}-{nudNVMem.Value} ";


        return commandValues;
    }
}