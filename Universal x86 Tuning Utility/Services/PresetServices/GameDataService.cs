using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Services.PresetServices;

public class GameDataService : IGameDataService
{
    private readonly string _filePath;
    private readonly Lazy<Dictionary<string, GameData>> _presets;

    public GameDataService(string filePath)
    {
        _filePath = filePath;
        _presets = new Lazy<Dictionary<string, GameData>>(() =>
        {
            if (!File.Exists(_filePath))
            {
                return [];
            }
            
            var serializedPresets = File.ReadAllText(_filePath);
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, GameData>>(serializedPresets) ?? [];
            }
            catch
            {
                return [];
            }
        });
    }

    public IEnumerable<string> GetPresetNames()
    {
        return _presets.Value.Keys;
    }

    public GameData? GetPreset(string presetName)
    {
        return _presets.Value.GetValueOrDefault(presetName);
    }

    public void SavePreset(string name, GameData preset)
    {
        _presets.Value[name] = preset;
        SavePresets();
    }

    public void DeletePreset(string name)
    {
        _presets.Value.Remove(name);
        SavePresets();
    }

    private void SavePresets()
    {
        var serializedPresets = JsonSerializer.Serialize(_presets.Value);
        File.WriteAllText(_filePath, serializedPresets);
    }
}