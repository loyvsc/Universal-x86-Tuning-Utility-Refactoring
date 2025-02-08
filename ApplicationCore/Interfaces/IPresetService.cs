using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IPresetService
{
    IEnumerable<string> GetPresetNames();
    Preset GetPreset(string presetName);
    void SavePreset(string name, Preset preset);
    void DeletePreset(string name);
}