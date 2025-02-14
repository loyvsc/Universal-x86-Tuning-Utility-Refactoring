using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using GameLib;
using Microsoft.Extensions.Logging;
using Universal_x86_Tuning_Utility.Services.Intel;

namespace Universal_x86_Tuning_Utility.Services.GameLauncherServices;

public class WindowsGameLauncherService : IGameLauncherService
{
    public Lazy<List<GameLauncherItem>> InstalledGames { get; }
    
    private readonly ILogger<WindowsGameLauncherService> _logger;
    private List<GameLauncherItem>? _installedGames;
    
    public WindowsGameLauncherService(ILogger<WindowsGameLauncherService> logger)
    {
        _logger = logger;
        
        InstalledGames = new Lazy<List<GameLauncherItem>>(() => ReSearchGames());
    }

    public List<GameLauncherItem> ReSearchGames(bool isAdaptive = false)
    {
        var list = new List<GameLauncherItem>();
        
            var gameLauncherOptions = new GameLib.Core.LauncherOptions
            {
                QueryOnlineData = false 
            };
            var gameLauncher = new LauncherManager(gameLauncherOptions);
            
            // todo: refactor this
            foreach (var launcher in gameLauncher.GetLaunchers())
            {
                switch (launcher.Name)
                {
                    case "Steam":
                        foreach (var game in launcher.Games)
                        {
                            if (!game.Name.Contains("Steamworks") && !game.Name.Contains("SteamVR") && !game.Name.Contains("Google Earth") && !game.Name.Contains("Wallpaper Engine") && !game.Name.Contains("tModLoader") && !game.Name.Contains("- Original Soundtrack"));
                            {
                                if (game.Id != "228980")
                                {
                                    var launcherItem = new GameLauncherItem
                                    {
                                        GameName = game.Name,
                                        GameId = game.Id
                                    };
                                    //launcherItem.iconPath = game.ExecutableIcon;

                                    if (game.Executables.Count() == 1)
                                    {
                                        launcherItem.Path = game.InstallDir;
                                        launcherItem.Exe = Path.GetFileNameWithoutExtension(game.Executables.First());
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
                                                    launcherItem.Exe = exeName;
                                                    break;
                                                }
                                            }
                                            foreach (string arr in array)
                                            {
                                                if (exeName.Contains(arr, StringComparison.OrdinalIgnoreCase))
                                                {
                                                    launcherItem.Path = game.InstallDir;
                                                    launcherItem.Exe = exeName;
                                                    break;
                                                }
                                            }

                                            if (launcherItem.Path != null)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    if (launcherItem.Path == "" || launcherItem.Exe == "")
                                    {
                                        launcherItem.Path = Path.GetFileNameWithoutExtension(game.Executables.Last());
                                        launcherItem.Exe = Path.GetFileNameWithoutExtension(game.Executables.Last());
                                    }
                                    
                                    list.Add(launcherItem);
                                }

                            }
                        }
                        break;
                    case "Battle.net":
                        foreach (var game in launcher.Games)
                        {
                            var launcherItem = new GameLauncherItem
                            {
                                GameName = game.Name,
                                GameId = game.Id
                            };
                            
                            switch (game.Name)
                            {
                                case "Call of Duty Black Ops Cold War":
                                    launcherItem.Path = game.InstallDir;
                                    launcherItem.Exe = "BlackOpsColdWar";
                                    break;

                                default:
                                    launcherItem.Path = game.InstallDir;
                                    launcherItem.Exe = Path.GetFileNameWithoutExtension(launcherItem.Path);
                                    break;
                            }
                            
                            list.Add(launcherItem);
                        }
                        break;
                    case "Epic Games":
                        foreach (var game in launcher.Games)
                        {
                            GameLauncherItem launcherItem = new GameLauncherItem
                            {
                                GameName = game.Name,
                                GameId = game.Id,
                                Path = game.InstallDir,
                                Exe = Path.GetFileNameWithoutExtension(game.InstallDir),
                                GameType = GameType.EpicGamesStore
                            };
                            list.Add(launcherItem);
                        }
                        break;

                    default:
                        foreach (var game in launcher.Games)
                        {
                            var launcherItem = new GameLauncherItem()
                            {
                                GameName = game.Name,
                                GameId = game.Id,
                                Path = game.InstallDir,
                                Exe = Path.GetFileNameWithoutExtension(game.InstallDir)
                            };
                            list.Add(launcherItem);
                        }
                        break;

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
                    string[] filesInDirectory;
                    if (Directory.Exists(xboxGameDirectory))
                    {
                        filesInDirectory = Directory.GetDirectories(xboxGameDirectory);

                        if (filesInDirectory.Length > 0)
                        {
                            var strings = filesInDirectory.Select(x => Path.GetFileName(x)).ToArray();

                            if (strings.Length > 0)
                            {
                                foreach (var package in packages)
                                {
                                    string install = package.InstalledLocation.Path;
                                    string sig = package.SignatureKind.ToString();

                                    if (install.Contains("WindowsApps") && sig == "Store" &&
                                        package.IsFramework == false)
                                    {
                                        if (strings.Contains(package.DisplayName))
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
                    _logger.LogError(ex, "Exception occurred when checking for Microsoft Store games");
                }
            }

            list = list.OrderBy(item => item.GameName).ToList();

            // if (isAdaptive)
            // {
            //     extraApps = new GameLauncherItem();
            //     extraApps.GameName = "Yuzu";
            //     extraApps.Path = "yuzu.exe";
            //     list.Add(extraApps);
            //
            //     extraApps = new GameLauncherItem();
            //     extraApps.GameName = "RPCS3";
            //     extraApps.Path = "rpcs3.exe";
            //     list.Add(extraApps);
            //
            //     extraApps = new GameLauncherItem();
            //     extraApps.GameName = "Cemu";
            //     extraApps.Path = "cemu.exe";
            //     list.Add(extraApps);
            //
            //     extraApps = new GameLauncherItem();
            //     extraApps.GameName = "Dolphin";
            //     extraApps.Path = "Dolphin.exe";
            //     list.Add(extraApps);
            //
            //     extraApps = new GameLauncherItem();
            //     extraApps.GameName = "Citra";
            //     extraApps.Path = "Citra.exe";
            //     list.Add(extraApps);
            // }
            
            return list.Distinct(new GameLauncherItemEqualityComparer()).ToList();
    }
    
    public void LaunchGame(GameLauncherItem gameLauncherItem)
    {
        if (gameLauncherItem.GameType == GameType.Custom)
        {
            if (File.Exists(gameLauncherItem.Path))
            {
                RunGame(gameLauncherItem.Path);
            }
        }
        else
        {
            if (gameLauncherItem.GameId != "")
            {
                switch (gameLauncherItem.GameType)
                {
                    case GameType.EpicGamesStore:
                        RunLaunchString(gameLauncherItem);
                        break;
                    case GameType.Steam:
                        RunLaunchString(launchcommand);
                        break;
                    case GameType.BattleNet:
                        string battlenetfile = Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), "Battle.net\\Battle.net.exe");
                        if (BattleNetRunning())
                        {
                            RunCli.RunCommand(" --exec=\"launch " + gameLauncherItem.GameId.ToUpper() + "\"", false, battlenetfile, 3000, true);
                        }
                        else
                        {
                            RunGame(battlenetfile);
                            Thread.Sleep(15000);
                            RunCli.RunCommand(" --exec=\"launch " + gameLauncherItem.GameId.ToUpper() + "\"", false, battlenetfile, 3000, true);
                        }
                        break;
                    case GameType.Gog:
                        RunCli.RunCommand(" /command=runGame /gameId=" + gameLauncherItem.GameId, false, 
                            Path.Combine(Environment.GetEnvironmentVariable("ProgramFiles(x86)"), "GOG Galaxy", "GalaxyClient.exe"));
                        break;
                    case GameType.MicrosoftStore:
                        var pm = new PackageManager();
                        pm.FindPackage(gameLauncherItem.GameId).GetAppListEntries().First().LaunchAsync();
                        break;
                    default: break;
                }
            }
        }
    }

    public void RunGame(string executableFilePath)
    {
        if (File.Exists(executableFilePath))
        {
            Process.Start(new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = Path.GetFileName(executableFilePath),
                WorkingDirectory = Path.GetDirectoryName(executableFilePath)
            });
        }
    }

    private static bool BattleNetRunning()
    {
        var processesByName = Process.GetProcessesByName("Battle.net.exe");
        return processesByName.Length != 0;
    }

    private class GameLauncherItemEqualityComparer : IEqualityComparer<GameLauncherItem>
    {
        public bool Equals(GameLauncherItem x, GameLauncherItem y)
        {
            // Check if two GameLauncherItems are equal based on gameID, gameName, and appType.
            return x.GameId == y.GameId && x.GameName == y.GameName && x.GameType == y.GameType;
        }

        public int GetHashCode(GameLauncherItem obj)
        {
            // Generate a hash code based on gameID, gameName, and appType.
            return (obj.GameId + obj.GameName + obj.GameType).GetHashCode();
        }
    }

}