using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.PresetServices;

public class PresetServiceFactory : IPresetServiceFactory
{
    private readonly Dictionary<string, IPresetService> _presetServices = new();
    
    public IPresetService GetPresetService(string presetsPath)
    {
        var presetService =
            CollectionsMarshal.GetValueRefOrAddDefault(_presetServices, presetsPath, out bool exists);
        if (!exists)
        {
            presetService = new PresetService(presetsPath);
        }

        return presetService;
    }
}