using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IPresetService
{
    public IEnumerable<string> GetPresetNames();
    public IEnumerable<Preset> GetPresets();
    public Preset? GetPreset(string presetName);
    public void SavePreset(string name, Preset preset);
    public void DeletePreset(string name);
}