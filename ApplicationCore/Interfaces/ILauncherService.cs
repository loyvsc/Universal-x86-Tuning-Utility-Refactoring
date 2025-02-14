using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGameLauncherService
{
    public Lazy<List<GameLauncherItem>> InstalledGames { get; }
    public List<GameLauncherItem> ReSearchGames(bool isAdaptive = false);
    public void LaunchGame(GameLauncherItem gameLauncherItem);
    public void RunGame(string executableFilePath);
}