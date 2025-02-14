using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Services.PresetServices;

public class AdaptivePresetService : IAdaptivePresetService
{
    private readonly string _filePath;
    private Dictionary<string, AdaptivePreset> _presets;

    public AdaptivePresetService(string filePath)
    {
        _filePath = filePath;
        _presets = new Dictionary<string, AdaptivePreset>();
        
        LoadPresets();
    }

    public IEnumerable<string> GetPresetNames()
    {
        return _presets.Keys;
    }

    public AdaptivePreset? GetPreset(string presetName)
    {
        return _presets.GetValueOrDefault(presetName);
    }

    public void SavePreset(string name, AdaptivePreset preset)
    {
        _presets[name] = preset;
        SavePresets();
    }

    public void DeletePreset(string name)
    {
        _presets.Remove(name);
        SavePresets();
    }

    private void LoadPresets()
    {
        if (File.Exists(_filePath))
        {
            var serializedPresets = File.ReadAllText(_filePath);
            var readPresets = JsonSerializer.Deserialize<Dictionary<string, AdaptivePreset>>(serializedPresets);
            if (readPresets != null)
            {
                _presets = readPresets;
            }
        }
        else
        {
            _presets.Clear();
        }
    }


    private void SavePresets()
    {
        var serializedPresets = JsonSerializer.Serialize(_presets);
        File.WriteAllText(_filePath, serializedPresets);
    }
}