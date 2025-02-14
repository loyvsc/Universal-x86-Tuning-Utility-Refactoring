using System;
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
using Universal_x86_Tuning_Utility.Scripts;
using Universal_x86_Tuning_Utility.Services.RyzenAdj;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class CustomPresetsViewModel : NotifyPropertyChangedBase
{
    private readonly IPresetService _apuPresetService;
    private readonly IPresetService _amdDtCpuPresetService;
    private readonly IPresetService _intelPresetService;
    private readonly ILogger<CustomPresetsViewModel> _logger;
    private readonly ISystemInfoService _systemInfoService;
    private readonly INotificationManager _notificationManager;
    private readonly IRyzenAdjService _ryzenAdjService;

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
    
    private Preset _currentPreset;
    private bool _nvidiaGpuSettingsAvailable;
    private bool _radeonGpuSettingsAvailable;
    private bool _undoActionAvailable;

    public CustomPresetsViewModel(ILogger<CustomPresetsViewModel> logger,
                                  ISystemInfoService systemInfoService,
                                  INotificationManager notificationManager,
                                  IRyzenAdjService ryzenAdjService)
    {
        _logger = logger;
        _systemInfoService = systemInfoService;
        _notificationManager = notificationManager;
        _ryzenAdjService = ryzenAdjService;

        DeletePresetCommand = ReactiveCommand.CreateFromTask(DeleteCurrentPreset);
    }

    private async Task ApplyPreset()
    {
        try
        {
            string commandValues = "";
            
            commandValues = getCommandValues();

            if (!string.IsNullOrEmpty(commandValues))
            {
                await _ryzenAdjService.Translate(commandValues);
                await _notificationManager.ShowTextNotification("Preset Applied", "Your custom preset settings have been applied!");
            }

            RadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
            NvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when applying preset");
            await _notificationManager.ShowTextNotification("Preset not applied", "Error occurred when applying preset!", NotificationManagerExtensions.NotificationType.Error);
        }
    }

    private Task DeleteCurrentPreset()
    {
        try
        {
            if cbxPowerPreset.Text != "" && cbxPowerPreset.Text != null
            switch (_systemInfoService.CpuInfo.Manufacturer)
            {
                case Manufacturer.AMD:
                {
                    if (_systemInfoService.CpuInfo.AmdProcessorType == AmdProcessorType.Apu)
                    {
                        _apuPresetService.DeletePreset(deletePresetName);
                    }
                    else if (_systemInfoService.CpuInfo.AmdProcessorType == AmdProcessorType.Desktop)
                    {
                        _amdDtCpuPresetService.DeletePreset(deletePresetName);
                    }

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
        }
    }

    private Task ReloadPreset()
    {
        apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");
        amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");
        intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");
        if (cbxPowerPreset.Text != null && cbxPowerPreset.Text != "") updateValues(cbxPowerPreset.SelectedItem.ToString());
    }

    private async Task SavePreset()
    {
        if (!tbxPresetName.Text.Contains("PM -"))
        {
            if (Family.TYPE == Family.ProcessorType.Amd_Apu)
            {
                if (tbxPresetName.Text != "" && tbxPresetName.Text != null)
                {
                    // Save a preset
                    Preset preset = new Preset
                    {
                        apuTemp = (int)nudAPUTemp.Value,
                        apuSkinTemp = (int)nudAPUSkinTemp.Value,
                        apuSTAPMPow = (int)nudSTAPMPow.Value,
                        apuSTAPMTime = (int)nudFastTime.Value,
                        apuFastPow = (int)nudFastPow.Value,
                        apuSlowPow = (int)nudSlowPow.Value,
                        apuSlowTime = (int)nudSlowTime.Value,

                        apuCpuTdc = (int)nudCpuVrmTdc.Value,
                        apuCpuEdc = (int)nudCpuVrmEdc.Value,
                        apuSocTdc = (int)nudSocVrmTdc.Value,
                        apuSocEdc = (int)nudSocVrmEdc.Value,
                        apuGfxTdc = (int)nudGfxVrmTdc.Value,
                        apuGfxEdc = (int)nudGfxVrmEdc.Value,

                        apuGfxClk = (int)nudAPUiGPUClk.Value,

                        pboScalar = (int)nudPBOScaler.Value,
                        coAllCore = (int)nudAllCO.Value,

                        coGfx = (int)nudGfxCO.Value,
                        isCoGfx = (bool)cbGfxCO.IsChecked,

                        boostProfile = (int)cbxBoost.SelectedIndex,

                        rsr = (int)nudRSR.Value,
                        boost = (int)nudBoost.Value,
                        imageSharp = (int)nudImageSharp.Value,
                        isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                        isRSR = (bool)cbRSR.IsChecked,
                        isBoost = (bool)cbBoost.IsChecked,
                        isAntiLag = (bool)cbAntiLag.IsChecked,
                        isImageSharp = (bool)cbImageSharp.IsChecked,
                        isSync = (bool)cbSync.IsChecked,

                        ccd1Core1 = (int)nudCCD1Core1.Value,
                        ccd1Core2 = (int)nudCCD1Core2.Value,
                        ccd1Core3 = (int)nudCCD1Core3.Value,
                        ccd1Core4 = (int)nudCCD1Core4.Value,
                        ccd1Core5 = (int)nudCCD1Core5.Value,
                        ccd1Core6 = (int)nudCCD1Core6.Value,
                        ccd1Core7 = (int)nudCCD1Core7.Value,
                        ccd1Core8 = (int)nudCCD1Core8.Value,

                        ccd2Core1 = (int)nudCCD2Core1.Value,
                        ccd2Core2 = (int)nudCCD2Core2.Value,
                        ccd2Core3 = (int)nudCCD2Core3.Value,
                        ccd2Core4 = (int)nudCCD2Core4.Value,
                        ccd2Core5 = (int)nudCCD2Core5.Value,
                        ccd2Core6 = (int)nudCCD2Core6.Value,
                        ccd2Core7 = (int)nudCCD2Core7.Value,
                        ccd2Core8 = (int)nudCCD2Core8.Value,

                        commandValue = getCommandValues(),

                        isApuTemp = (bool)cbAPUTemp.IsChecked,
                        isApuSkinTemp = (bool)cbAPUSkinTemp.IsChecked,
                        isApuSTAPMPow = (bool)cbSTAPMPow.IsChecked,
                        isApuSlowPow = (bool)cbSlowPow.IsChecked,
                        isApuSlowTime = (bool)cbSlowTime.IsChecked,
                        isApuFastPow = (bool)cbFastPow.IsChecked,
                        isApuSTAPMTime = (bool)cbFastTime.IsChecked,

                        isApuCpuTdc = (bool)cbCpuVrmTdc.IsChecked,
                        isApuCpuEdc = (bool)cbCpuVrmEdc.IsChecked,
                        isApuSocTdc = (bool)cbSocVrmTdc.IsChecked,
                        isApuSocEdc = (bool)cbSocVrmEdc.IsChecked,
                        isApuGfxTdc = (bool)cbGfxVrmTdc.IsChecked,
                        isApuGfxEdc = (bool)cbGfxVrmEdc.IsChecked,

                        isApuGfxClk = (bool)cbAPUiGPUClk.IsChecked,

                        isPboScalar = (bool)cbPBOScaler.IsChecked,
                        isCoAllCore = (bool)cbAllCO.IsChecked,

                        IsCCD1Core1 = (bool)cbCCD1Core1.IsChecked,
                        IsCCD1Core2 = (bool)cbCCD1Core2.IsChecked,
                        IsCCD1Core3 = (bool)cbCCD1Core3.IsChecked,
                        IsCCD1Core4 = (bool)cbCCD1Core4.IsChecked,
                        IsCCD1Core5 = (bool)cbCCD1Core5.IsChecked,
                        IsCCD1Core6 = (bool)cbCCD1Core6.IsChecked,
                        IsCCD1Core7 = (bool)cbCCD1Core7.IsChecked,
                        IsCCD1Core8 = (bool)cbCCD1Core8.IsChecked,

                        IsCCD2Core1 = (bool)cbCCD2Core1.IsChecked,
                        IsCCD2Core2 = (bool)cbCCD2Core2.IsChecked,
                        IsCCD2Core3 = (bool)cbCCD2Core3.IsChecked,
                        IsCCD2Core4 = (bool)cbCCD2Core4.IsChecked,
                        IsCCD2Core5 = (bool)cbCCD2Core5.IsChecked,
                        IsCCD2Core6 = (bool)cbCCD2Core6.IsChecked,
                        IsCCD2Core7 = (bool)cbCCD2Core7.IsChecked,
                        IsCCD2Core8 = (bool)cbCCD2Core8.IsChecked,

                        isNVIDIA = (bool)tsNV.IsChecked,
                        nvMaxCoreClk = (int)nudNVMaxCore.Value,
                        nvCoreClk = (int)nudNVCore.Value,
                        nvMemClk = (int)nudNVMem.Value,

                        IsAmdOC = (bool)tsAmdOC.IsChecked,
                        amdClock = (int)nudAmdCpuClk.Value,
                        amdVID = (int)nudAmdVID.Value,

                        softMiniGPUClk = (int)nudSoftMiniGPUClk.Value,
                        softMinCPUClk = (int)nudSoftMinCPUClk.Value,
                        softMinFabClk = (int)nudSoftMinFabClk.Value,
                        softMinDataClk = (int)nudSoftMinDataClk.Value,
                        softMinSoCClk = (int)nudSoftMinSoCClk.Value,
                        softMinVCNClk = (int)nudSoftMinVCNClk.Value,

                        softMaxiGPUClk = (int)nudSoftMaxiGPUClk.Value,
                        softMaxCPUClk = (int)nudSoftMaxCPUClk.Value,
                        softMaxFabClk = (int)nudSoftMaxFabClk.Value,
                        softMaxDataClk = (int)nudSoftMaxDataClk.Value,
                        softMaxSoCClk = (int)nudSoftMaxSoCClk.Value,
                        softMaxVCNClk = (int)nudSoftMaxVCNClk.Value,

                        isSoftMiniGPUClk = (bool)cbSoftMiniGPUClk.IsChecked,
                        isSoftMinCPUClk = (bool)cbSoftMinCPUClk.IsChecked,
                        isSoftMinFabClk = (bool)cbSoftMinFabClk.IsChecked,
                        isSoftMinDataClk = (bool)cbSoftMinDataClk.IsChecked,
                        isSoftMinSoCClk = (bool)cbSoftMinSoCClk.IsChecked,
                        isSoftMinVCNClk = (bool)cbSoftMinVCNClk.IsChecked,

                        isSoftMaxiGPUClk = (bool)cbSoftMaxiGPUClk.IsChecked,
                        isSoftMaxCPUClk = (bool)cbSoftMaxCPUClk.IsChecked,
                        isSoftMaxFabClk = (bool)cbSoftMaxFabClk.IsChecked,
                        isSoftMaxDataClk = (bool)cbSoftMaxDataClk.IsChecked,
                        isSoftMaxSoCClk = (bool)cbSoftMaxSoCClk.IsChecked,
                        isSoftMaxVCNClk = (bool)cbSoftMaxVCNClk.IsChecked,

                        asusGPUUlti = (bool)tsASUSUlti.IsChecked,
                        asusiGPU = (bool)tsASUSEco.IsChecked,
                        asusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                        displayHz = (int)cbxRefreshRate.SelectedIndex,

                        isMag = (bool)tsUXTUSR.IsChecked,
                        isVsync = (bool)cbVSync.IsChecked,
                        isRecap = (bool)cbAutoCap.IsChecked,
                        Sharpness = (int)nudSharp.Value,
                        ResScaleIndex = (int)cbxResScale.SelectedIndex,

                        powerMode = (int)cbxPowerMode.SelectedIndex,

                    };
                    
                    apuPresetManager.SavePreset(tbxPresetName.Text, preset);

                    apuPresetManager = new PresetManager(Settings.Default.Path + "apuPresets.json");

                    // Get the names of all the stored presets
                    IEnumerable<string> presetNames = apuPresetManager.GetPresetNames();

                    cbxPowerPreset.Items.Clear();

                    // Populate a combo box with the preset names
                    foreach (string presetName in presetNames)
                    {
                        cbxPowerPreset.Items.Add(presetName);
                    }

                    await _notificationManager.ShowTextNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");

                    RadeonGpuSettingsAvailable = _systemInfoService.RadeonGpuCount > 0;
                    NvidiaGpuSettingsAvailable = _systemInfoService.NvidiaGpuCount > 0;
                }
            }

            if (_systemInfoService.CpuInfo.AmdProcessorType == AmdProcessorType.Desktop)
            {
                if (tbxPresetName.Text != "" && tbxPresetName.Text != null)
                {
                    // Save a preset
                    Preset preset = new Preset
                    {
                        dtCpuTemp = (int)nudCPUTemp.Value,
                        dtCpuPPT = (int)nudPPT.Value,
                        dtCpuTDC = (int)nudTDC.Value,
                        dtCpuEDC = (int)nudEDC.Value,
                        pboScalar = (int)nudPBOScaler.Value,
                        coAllCore = (int)nudAllCO.Value,

                        boostProfile = (int)cbxBoost.SelectedIndex,

                        rsr = (int)nudRSR.Value,
                        boost = (int)nudBoost.Value,
                        imageSharp = (int)nudImageSharp.Value,
                        isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                        isRSR = (bool)cbRSR.IsChecked,
                        isBoost = (bool)cbBoost.IsChecked,
                        isAntiLag = (bool)cbAntiLag.IsChecked,
                        isImageSharp = (bool)cbImageSharp.IsChecked,
                        isSync = (bool)cbSync.IsChecked,

                        commandValue = getCommandValues(),


                        isDtCpuTemp = (bool)cbCPUTemp.IsChecked,
                        isDtCpuPPT = (bool)cbPPT.IsChecked,
                        isDtCpuTDC = (bool)cbTDC.IsChecked,
                        isDtCpuEDC = (bool)cbEDC.IsChecked,
                        isPboScalar = (bool)cbPBOScaler.IsChecked,
                        isCoAllCore = (bool)cbAllCO.IsChecked,

                        coGfx = (int)nudGfxCO.Value,
                        isCoGfx = (bool)cbGfxCO.IsChecked,

                        isNVIDIA = (bool)tsNV.IsChecked,
                        nvMaxCoreClk = (int)nudNVMaxCore.Value,
                        nvCoreClk = (int)nudNVCore.Value,
                        nvMemClk = (int)nudNVMem.Value,

                        ccd1Core1 = (int)nudCCD1Core1.Value,
                        ccd1Core2 = (int)nudCCD1Core2.Value,
                        ccd1Core3 = (int)nudCCD1Core3.Value,
                        ccd1Core4 = (int)nudCCD1Core4.Value,
                        ccd1Core5 = (int)nudCCD1Core5.Value,
                        ccd1Core6 = (int)nudCCD1Core6.Value,
                        ccd1Core7 = (int)nudCCD1Core7.Value,
                        ccd1Core8 = (int)nudCCD1Core8.Value,

                        ccd2Core1 = (int)nudCCD2Core1.Value,
                        ccd2Core2 = (int)nudCCD2Core2.Value,
                        ccd2Core3 = (int)nudCCD2Core3.Value,
                        ccd2Core4 = (int)nudCCD2Core4.Value,
                        ccd2Core5 = (int)nudCCD2Core5.Value,
                        ccd2Core6 = (int)nudCCD2Core6.Value,
                        ccd2Core7 = (int)nudCCD2Core7.Value,
                        ccd2Core8 = (int)nudCCD2Core8.Value,

                        IsCCD1Core1 = (bool)cbCCD1Core1.IsChecked,
                        IsCCD1Core2 = (bool)cbCCD1Core2.IsChecked,
                        IsCCD1Core3 = (bool)cbCCD1Core3.IsChecked,
                        IsCCD1Core4 = (bool)cbCCD1Core4.IsChecked,
                        IsCCD1Core5 = (bool)cbCCD1Core5.IsChecked,
                        IsCCD1Core6 = (bool)cbCCD1Core6.IsChecked,
                        IsCCD1Core7 = (bool)cbCCD1Core7.IsChecked,
                        IsCCD1Core8 = (bool)cbCCD1Core8.IsChecked,

                        IsCCD2Core1 = (bool)cbCCD2Core1.IsChecked,
                        IsCCD2Core2 = (bool)cbCCD2Core2.IsChecked,
                        IsCCD2Core3 = (bool)cbCCD2Core3.IsChecked,
                        IsCCD2Core4 = (bool)cbCCD2Core4.IsChecked,
                        IsCCD2Core5 = (bool)cbCCD2Core5.IsChecked,
                        IsCCD2Core6 = (bool)cbCCD2Core6.IsChecked,
                        IsCCD2Core7 = (bool)cbCCD2Core7.IsChecked,
                        IsCCD2Core8 = (bool)cbCCD2Core8.IsChecked,

                        IsAmdOC = (bool)tsAmdOC.IsChecked,
                        amdClock = (int)nudAmdCpuClk.Value,
                        amdVID = (int)nudAmdVID.Value,

                        asusGPUUlti = (bool)tsASUSUlti.IsChecked,
                        asusiGPU = (bool)tsASUSEco.IsChecked,
                        asusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                        displayHz = (int)cbxRefreshRate.SelectedIndex,

                        isMag = (bool)tsUXTUSR.IsChecked,
                        isVsync = (bool)cbVSync.IsChecked,
                        isRecap = (bool)cbAutoCap.IsChecked,
                        Sharpness = (int)nudSharp.Value,
                        ResScaleIndex = (int)cbxResScale.SelectedIndex,

                        powerMode = (int)cbxPowerMode.SelectedIndex,
                    };
                    amdDtCpuPresetManager.SavePreset(tbxPresetName.Text, preset);

                    amdDtCpuPresetManager = new PresetManager(Settings.Default.Path + "amdDtCpuPresets.json");

                    // Get the names of all the stored presets
                    IEnumerable<string> presetNames = amdDtCpuPresetManager.GetPresetNames();

                    cbxPowerPreset.Items.Clear();

                    // Populate a combo box with the preset names
                    foreach (string presetName in presetNames)
                    {
                        cbxPowerPreset.Items.Add(presetName);
                    }

                    await _notificationManager.ShowTextNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");
                }
            }

            if (_systemInfoService.CpuInfo.Manufacturer == Manufacturer.Intel)
            {
                if (tbxPresetName.Text != "" && tbxPresetName.Text != null)
                {
                    // Save a preset
                    Preset preset = new Preset
                    {
                        IntelPL1 = (int)nudIntelPL1.Value,
                        IntelPL2 = (int)nudIntelPL2.Value,
                        IntelVoltCPU = (int)nudIntelCoreUV.Value,
                        IntelVoltGPU = (int)nudIntelGfxUV.Value,
                        IntelVoltCache = (int)nudIntelCacheUV.Value,
                        IntelVoltSA = (int)nudIntelSAUV.Value,
                        IntelBalCPU = (int)nudIntelCpuBal.Value,
                        IntelBalGPU = (int)nudIntelGpuBal.Value,

                        isApuGfxClk = (bool)cbAPUiGPUClk.IsChecked,
                        apuGfxClk = (int)nudAPUiGPUClk.Value,

                        rsr = (int)nudRSR.Value,
                        boost = (int)nudBoost.Value,
                        imageSharp = (int)nudImageSharp.Value,
                        isRadeonGraphics = (bool)tsRadeonGraph.IsChecked,
                        isRSR = (bool)cbRSR.IsChecked,
                        isBoost = (bool)cbBoost.IsChecked,
                        isAntiLag = (bool)cbAntiLag.IsChecked,
                        isImageSharp = (bool)cbImageSharp.IsChecked,
                        isSync = (bool)cbSync.IsChecked,

                        commandValue = getCommandValues(),

                        isIntelPL1 = (bool)cbIntelPL1.IsChecked,
                        isIntelPL2 = (bool)cbIntelPL2.IsChecked,
                        IsIntelVolt = (bool)tsIntelUV.IsChecked,
                        IsIntelBal = (bool)tsIntelBal.IsChecked,

                        isNVIDIA = (bool)tsNV.IsChecked,
                        nvMaxCoreClk = (int)nudNVMaxCore.Value,
                        nvCoreClk = (int)nudNVCore.Value,
                        nvMemClk = (int)nudNVMem.Value,

                        asusGPUUlti = (bool)tsASUSUlti.IsChecked,
                        asusiGPU = (bool)tsASUSEco.IsChecked,
                        asusPowerProfile = (int)cbxAsusPower.SelectedIndex,

                        displayHz = (int)cbxRefreshRate.SelectedIndex,

                        isMag = (bool)tsUXTUSR.IsChecked,
                        isVsync = (bool)cbVSync.IsChecked,
                        isRecap = (bool)cbAutoCap.IsChecked,
                        Sharpness = (int)nudSharp.Value,
                        ResScaleIndex = (int)cbxResScale.SelectedIndex,

                        powerMode = (int)cbxPowerMode.SelectedIndex,

                        isIntelClockRatio = (bool)tsIntelRatioCore.IsChecked,
                        intelClockRatioC1 = (int)nudIntelRatioC1.Value,
                        intelClockRatioC2 = (int)nudIntelRatioC2.Value,
                        intelClockRatioC3 = (int)nudIntelRatioC3.Value,
                        intelClockRatioC4 = (int)nudIntelRatioC4.Value,
                        intelClockRatioC5 = (int)nudIntelRatioC5.Value,
                        intelClockRatioC6 = (int)nudIntelRatioC6.Value,
                        intelClockRatioC7 = (int)nudIntelRatioC7.Value,
                        intelClockRatioC8 = (int)nudIntelRatioC8.Value,
                    };
                    _intelPresetService.SavePreset(tbxPresetName.Text, preset);

                    intelPresetManager = new PresetManager(Settings.Default.Path + "intelPresets.json");

                    // Get the names of all the stored presets
                    IEnumerable<string> presetNames = _intelPresetService.GetPresetNames();

                    cbxPowerPreset.Items.Clear();

                    // Populate a combo box with the preset names
                    foreach (string presetName in presetNames)
                    {
                        cbxPowerPreset.Items.Add(presetName);
                    }

                    await _notificationManager.ShowTextNotification("Preset Saved", $"Your preset {tbxPresetName.Text} has been saved successfully!");
                }
            }
        }
    }
}