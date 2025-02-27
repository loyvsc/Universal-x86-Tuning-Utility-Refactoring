using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class AutomationsViewModel : NotifyPropertyChangedBase
{
    public ICommand ReloadPResetsCommand { get; }

    public Preset SelectedAcPreset
    {
        get => _selectedAcPreset;
        set => SetValue(ref _selectedAcPreset, value, () =>
        {
            Settings.Default.acPreset = value.Name;
            Settings.Default.acCommandString = value.CommandValue;
        });
    }

    public Preset SelectedDcPreset
    {
        get => _selectedDcPreset;
        set => SetValue(ref _selectedDcPreset, value, () =>
        {
            Settings.Default.dcPreset = value.Name;
            Settings.Default.dcCommandString = value.CommandValue;
        });
    }

    public Preset SelectedResumePreset
    {
        get => _selectedResumePreset;
        set => SetValue(ref _selectedResumePreset, value, () =>
        {
            Settings.Default.resumePreset = value.Name;
            Settings.Default.resumeCommandString = value.CommandValue;
        });
    }

    public List<Preset> Presets
    {
        get => _presets;
        set => SetValue(ref _presets, value);
    }
    
    private List<Preset> _presets;
    private Preset _selectedAcPreset;
    private Preset _selectedResumePreset;
    private Preset _selectedDcPreset;
    
    private readonly ISystemInfoService _systemInfoService;
    private readonly IPremadePresets _premadePresets;

    public AutomationsViewModel(IPresetServiceFactory presetServiceFactory,
                                ISystemInfoService systemInfoService,
                                IPremadePresets premadePresets)
    {
        _systemInfoService = systemInfoService;
        _premadePresets = premadePresets;

        var presetService = _systemInfoService.Cpu.Manufacturer == Manufacturer.AMD
            ? _systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Apu
                ? presetServiceFactory.GetAmdApuPresetService()
                : presetServiceFactory.GetAmdDesktopPresetService()
            : presetServiceFactory.GetAmdDesktopPresetService();
        
        ReloadPResetsCommand = ReactiveCommand.CreateFromTask(ReloadPresets);
        
        Presets = new List<Preset>();
        Presets.Add(Preset.Empty);
        if (_systemInfoService.Cpu.Manufacturer == Manufacturer.AMD)
        {
            Presets.AddRange(_premadePresets.PremadePresetsList);
        }
        Presets.AddRange(presetService.GetPresets());
        SetUsingPresets();
    }

    private void SetUsingPresets()
    {
        if (Settings.Default.acPreset != "")
        {
            SelectedAcPreset = Presets.FirstOrDefault(x => x.Name == Settings.Default.acPreset) ?? Presets[0];
        }

        if (Settings.Default.dcPreset != "")
        {
            SelectedDcPreset = Presets.FirstOrDefault(x => x.Name == Settings.Default.dcPreset) ?? Presets[0]; 
        }        
        
        if (Settings.Default.resumePreset != "")
        {
            SelectedResumePreset = Presets.FirstOrDefault(x => x.Name == Settings.Default.resumePreset) ?? Presets[0];
        }
    }

    private async Task ReloadPresets()
    {
        Presets.Clear();
        Presets.Add(Preset.Empty);
        if (_systemInfoService.Cpu.Manufacturer == Manufacturer.AMD)
        {
            Presets.AddRange(_premadePresets.PremadePresetsList);
        }

        SetUsingPresets();
    }
}