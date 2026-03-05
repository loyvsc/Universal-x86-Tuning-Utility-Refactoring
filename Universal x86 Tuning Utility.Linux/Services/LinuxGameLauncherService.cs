using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxGameLauncherService : IGameLauncherService
{
    public Lazy<IReadOnlyCollection<GameLauncherItem>> InstalledGames { get; }
    
    public IReadOnlyCollection<GameLauncherItem> ReSearchGames()
    {
        throw new NotImplementedException();
    }

    public Task LaunchGame(GameLauncherItem gameLauncherItem)
    {
        throw new NotImplementedException();
    }

    private Task RunGame(string executableFilePath)
    {
        throw new NotImplementedException();
    }
}