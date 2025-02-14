using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Services.PresetServices;

public class GameDataService : IGameDataService
{
    private readonly string _filePath;
    private Dictionary<string, GameData> _presets;

    public GameDataService(string filePath)
    {
        _filePath = filePath;
        _presets = new Dictionary<string, GameData>();
        
        LoadPresets();
    }

    public IEnumerable<string> GetPresetNames()
    {
        return _presets.Keys;
    }

    public GameData? GetPreset(string presetName)
    {
        return _presets.GetValueOrDefault(presetName);
    }

    public void SavePreset(string name, GameData preset)
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
            var readPresets = JsonSerializer.Deserialize<Dictionary<string, GameData>>(serializedPresets);
            
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