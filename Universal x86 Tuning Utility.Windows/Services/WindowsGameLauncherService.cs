using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using GameLib;
using GameLib.Plugin.BattleNet;
using GameLib.Plugin.Epic;
using GameLib.Plugin.Gog;
using GameLib.Plugin.Origin;
using GameLib.Plugin.Steam;
using GameLib.Plugin.Steam.Model;
using Microsoft.Extensions.Logging;
using Universal_x86_Tuning_Utility.Interfaces;
using WinRT;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsGameLauncherService : IGameLauncherService
{
    public Lazy<IReadOnlyCollection<GameLauncherItem>> InstalledGames { get; }
    
    private readonly Serilog.ILogger _logger;
    private readonly ICliService _cliService;
    private readonly IIconExtracter _iconExtracter;

    public WindowsGameLauncherService(Serilog.ILogger logger,
                                      ICliService cliService,
                                      IIconExtracter iconExtracter)
    {
        _logger = logger;
        _cliService = cliService;
        _iconExtracter = iconExtracter;

        InstalledGames = new Lazy<IReadOnlyCollection<GameLauncherItem>>(() => ReSearchGames());
    }

    public IReadOnlyCollection<GameLauncherItem> ReSearchGames(bool isAdaptive = false)
    {
        var list = new List<GameLauncherItem>();
        
        var gameLauncherOptions = new GameLib.Core.LauncherOptions
        {
            QueryOnlineData = false 
        };
        var gameLauncher = new LauncherManager(gameLauncherOptions);
            
        foreach (var launcher in gameLauncher.GetLaunchers())
        {
            switch (launcher)
            {
                case SteamLauncher:
                {
                    foreach (var game in launcher.Games)
                    {
                        if (!game.Name.Contains("Steamworks")
                            && !game.Name.Contains("SteamVR") 
                            && !game.Name.Contains("Google Earth")
                            && !game.Name.Contains("Wallpaper Engine") 
                            && !game.Name.Contains("ModLoader") 
                            && !game.Name.Contains("Soundtrack"))
                        {
                            var launcherItem = new GameLauncherItem
                            {
                                GameName = game.Name,
                                GameId = game.Id,
                                GameType = GameType.Steam,
                                Path = game.InstallDir
                            };
                            // launcherItem.iconPath = game.ExecutableIcon;

                            if (game.Executables.Count() == 1)
                            {
                                launcherItem.Path = game.InstallDir;
                                launcherItem.Executable = Path.GetFileNameWithoutExtension(game.Executables.First());
                            }
                            else
                            {
                                string[] array = launcherItem.GameName.Split(' ');
                                foreach (var exe in game.Executables)
                                {
                                    string exeName = Path.GetFileNameWithoutExtension(exe);
                                    if (game.Name.Contains("Call of duty", StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (exeName.Contains("cod", StringComparison.OrdinalIgnoreCase))
                                        {
                                            launcherItem.Path = game.InstallDir;
                                            launcherItem.Executable = exeName;
                                            break;
                                        }
                                    }
                                    foreach (string arr in array)
                                    {
                                        if (exeName.Contains(arr, StringComparison.OrdinalIgnoreCase))
                                        {
                                            launcherItem.Path = game.InstallDir;
                                            launcherItem.Executable = exeName;
                                            break;
                                        }
                                    }

                                    if (launcherItem.Path != null)
                                    {
                                        break;
                                    }
                                }
                            }
                            if (launcherItem.Path == "" || launcherItem.Executable == "")
                            {
                                launcherItem.Path = Path.GetFileNameWithoutExtension(game.Executables.Last());
                                launcherItem.Executable = Path.GetFileNameWithoutExtension(game.Executables.Last());
                            }
                                    
                            list.Add(launcherItem);
                        }
                    }
                    break;
                }
                case BattleNetLauncher:
                {
                    foreach (var game in launcher.Games)
                    {
                        var launcherItem = new GameLauncherItem
                        {
                            GameName = game.Name,
                            GameId = game.Id,
                            GameType = GameType.BattleNet
                        };
                            
                        switch (game.Name)
                        {
                            case "Call of Duty Black Ops Cold War":
                            {
                                launcherItem.Path = game.InstallDir;
                                launcherItem.Executable = "BlackOpsColdWar";
                                break;
                            }

                            default:
                            {
                                launcherItem.Path = game.InstallDir;
                                launcherItem.Executable = Path.GetFileNameWithoutExtension(launcherItem.Path);
                                break;
                            }
                        }
                            
                        list.Add(launcherItem);
                    }
                    break;
                }
                default:
                {
                    var gameType = launcher switch
                    {
                        EpicLauncher => GameType.EpicGamesStore,
                        OriginLauncher => GameType.Origin,
                        GogLauncher => GameType.Gog,
                        _ => GameType.Custom
                    };
                    foreach (var game in launcher.Games)
                    {
                        var launcherItem = new GameLauncherItem()
                        {
                            GameName = game.Name,
                            GameId = game.Id,
                            Path = game.InstallDir,
                            Executable = Path.GetFileNameWithoutExtension(game.InstallDir),
                            GameType = gameType
                        };
                        list.Add(launcherItem);
                    }
                    break;
                }
            }
        }

        //microsoft store apps below
        var packageManager = new PackageManager();
        IEnumerable<Package> packages = packageManager.FindPackages();

        foreach (var driveInfo in DriveInfo.GetDrives())
        {
            try
            {
                string xboxGameDirectory = Path.Combine(driveInfo.Name, "XboxGames");
                if (Directory.Exists(xboxGameDirectory))
                {
                    var filesInDirectory = Directory.GetDirectories(xboxGameDirectory);

                    if (filesInDirectory.Length > 0)
                    {
                        var files = filesInDirectory.Select(x => Path.GetFileName(x)).ToArray();

                        if (files.Length > 0)
                        {
                            foreach (var package in packages)
                            {
                                string install = package.InstalledLocation.Path;
                                string sig = package.SignatureKind.ToString();

                                if (install.Contains("WindowsApps") && sig == "Store" &&
                                    package.IsFramework == false)
                                {
                                    if (files.Contains(package.DisplayName))
                                    {
                                        var launcherItem = new GameLauncherItem()
                                        {
                                            GameType = GameType.EpicGamesStore,
                                            GameName = package.DisplayName,
                                            GameId = package.Id.FullName,
                                            Path = package.InstalledPath,
                                            ImageLocation = package.Logo.AbsolutePath
                                        };
                                        list.Add(launcherItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occurred when checking for Microsoft Store games");
            }
        }
        
        return list.Distinct(new GameLauncherItemEqualityComparer()).ToList();
    }
    
    public async Task LaunchGame(GameLauncherItem gameLauncherItem)
    {
        if (gameLauncherItem.GameType == GameType.Custom)
        {
            if (File.Exists(gameLauncherItem.Path))
            {
                await RunGame(gameLauncherItem.Path);
            }
        }
        else
        {
            if (gameLauncherItem.GameId != string.Empty)
            {
                switch (gameLauncherItem.GameType)
                {
                    case GameType.EpicGamesStore:
                    {
                        await _cliService.RunProcess(gameLauncherItem.Executable);
                        break;
                    }
                    case GameType.Steam:
                    {
                        await _cliService.RunProcess(gameLauncherItem.Executable);
                        break;
                    }
                    case GameType.BattleNet:
                    {
                        string battlenetfile = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), "Battle.net\\Battle.net.exe");
                        if (BattleNetRunning())
                        {
                            await _cliService.RunProcess(battlenetfile, " --exec=\"launch " + gameLauncherItem.GameId.ToUpper() + "\"");
                        }
                        else
                        {
                            await _cliService.RunProcess(battlenetfile);
                            Thread.Sleep(15000);
                            await _cliService.RunProcess(battlenetfile, " --exec=\"launch " + gameLauncherItem.GameId.ToUpper() + "\"");
                        }
                        break;
                    }
                    case GameType.Gog:
                    {
                        await _cliService.RunProcess("cmd.exe", " /command=runGame /gameId=" + gameLauncherItem.GameId); 
                        Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), "GOG Galaxy", "GalaxyClient.exe");
                        break;
                    }
                    case GameType.MicrosoftStore:
                    {
                        var packageManager = new PackageManager();
                        packageManager.FindPackage(gameLauncherItem.GameId).GetAppListEntries().First().LaunchAsync();
                        break;
                    }
                }
            }
        }
    }

    public async Task RunGame(string executableFilePath)
    {
        if (File.Exists(executableFilePath))
        {
            await _cliService.RunProcess(executableFilePath);
        }
    }

    private bool BattleNetRunning()
    {
        var processesByName = Process.GetProcessesByName("Battle.net.exe");
        return processesByName.Length != 0;
    }

    private class GameLauncherItemEqualityComparer : IEqualityComparer<GameLauncherItem>
    {
        public bool Equals(GameLauncherItem? x, GameLauncherItem? y)
        {
            // Check if two GameLauncherItems are equal based on gameID, gameName, and appType.
            return x?.GameId == y?.GameId && 
                   x?.GameName == y?.GameName &&
                   x?.GameType == y?.GameType;
        }

        public int GetHashCode(GameLauncherItem obj)
        {
            // Generate a hash code based on gameID, gameName, and appType.
            return (obj.GameId + obj.GameName + obj.GameType).GetHashCode();
        }
    }
}