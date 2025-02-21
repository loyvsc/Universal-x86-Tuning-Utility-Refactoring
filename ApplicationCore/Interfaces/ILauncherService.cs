using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGameLauncherService
{
    public Lazy<List<GameLauncherItem>> InstalledGames { get; }
    public List<GameLauncherItem> ReSearchGames(bool isAdaptive = false);
    public Task LaunchGame(GameLauncherItem gameLauncherItem);
    public Task RunGame(string executableFilePath);
}