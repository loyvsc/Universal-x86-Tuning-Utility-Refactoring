using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGameDataManager
{
    IEnumerable<string> GetPresetNames();
    GameData GetPreset(string presetName);
    void SavePreset(string name, GameData preset);
    void DeletePreset(string name);
}