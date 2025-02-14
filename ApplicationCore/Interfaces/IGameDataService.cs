using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGameDataService
{
    public IEnumerable<string> GetPresetNames();
    public GameData? GetPreset(string presetName);
    public void SavePreset(string name, GameData preset);
    public void DeletePreset(string name);
}