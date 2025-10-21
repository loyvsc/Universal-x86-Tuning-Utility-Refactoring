using System.Collections.Generic;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.PresetServices;

public class PresetServiceFactory : IPresetServiceFactory
{
    private readonly Dictionary<string, IPresetService> _presetServices = new();
    
    public IPresetService GetPresetService(string presetsPath)
    {
        if (!_presetServices.TryGetValue(presetsPath, out var presetService))
        {
            _presetServices.Add(presetsPath, new PresetService(presetsPath));
            presetService = _presetServices[presetsPath];
        }

        return presetService;
    }
}