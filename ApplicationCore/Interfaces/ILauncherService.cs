using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IGameLauncherService
{
    public IReadOnlyCollection<GameLauncherItem> ReSearchGames();
    public Task LaunchGame(GameLauncherItem gameLauncherItem);
}