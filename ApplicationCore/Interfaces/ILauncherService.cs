using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGameLauncherService
{
    public Lazy<IReadOnlyCollection<GameLauncherItem>> InstalledGames { get; }
    public IReadOnlyCollection<GameLauncherItem> ReSearchGames(bool isAdaptive = false);
    public Task LaunchGame(GameLauncherItem gameLauncherItem);
    public Task RunGame(string executableFilePath);
}