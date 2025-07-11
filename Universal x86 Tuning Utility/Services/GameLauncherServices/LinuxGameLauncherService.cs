using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Services.GameLauncherServices;

public class LinuxGameLauncherService : IGameLauncherService
{
    public Lazy<IReadOnlyCollection<GameLauncherItem>> InstalledGames { get; }
    
    public IReadOnlyCollection<GameLauncherItem> ReSearchGames(bool isAdaptive = false)
    {
        throw new NotImplementedException();
    }

    public Task LaunchGame(GameLauncherItem gameLauncherItem)
    {
        throw new NotImplementedException();
    }

    public Task RunGame(string executableFilePath)
    {
        throw new NotImplementedException();
    }
}