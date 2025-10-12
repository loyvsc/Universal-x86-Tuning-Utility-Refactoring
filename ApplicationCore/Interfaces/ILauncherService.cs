using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGameLauncherService
{
    public Lazy<IReadOnlyCollection<GameLauncherItem>> InstalledGames { get; }
    public IReadOnlyCollection<GameLauncherItem> ReSearchGames();
    public Task LaunchGame(GameLauncherItem gameLauncherItem);
}