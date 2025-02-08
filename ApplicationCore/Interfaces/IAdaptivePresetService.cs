using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IAdaptivePresetService
{
    IEnumerable<string> GetPresetNames();
    AdaptivePreset GetPreset(string presetName);
    void SavePreset(string name, AdaptivePreset preset);
    void DeletePreset(string name);
}