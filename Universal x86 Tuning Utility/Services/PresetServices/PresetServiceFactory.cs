using System.Collections.Generic;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.PresetServices;

public static class PresetServiceFactory
{
    private static readonly Dictionary<string, IPresetService> _presetServices = new();
    
    public static IPresetService GetPresetService(string presetsPath)
    {
        bool presetServiceAvailable = _presetServices.TryGetValue(presetsPath, out var presetService);
        if (!presetServiceAvailable)
        {
            presetService = new PresetService(presetsPath);
            _presetServices.Add(presetsPath, presetService);
        }
        return presetService;
    }
}