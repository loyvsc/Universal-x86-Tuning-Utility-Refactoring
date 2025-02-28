using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore.Utilities;
using DesktopNotifications;
using ReactiveUI;
using Universal_x86_Tuning_Utility.Extensions;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services.GPUs.AMD.Apu;
using Universal_x86_Tuning_Utility.Services.RyzenAdj;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class AdaptiveViewModel : NotifyPropertyChangedBase
{
    public ICommand ToggleAdaptiveModeCommand { get; }
    public ICommand SavePresetCommand { get; }
    public ICommand ReloadGamesListCommand { get; }

    public bool IsAutoSwitchEnabled
    {
        get => _isAutoSwitchEnabled;
        set => SetValue(ref _isAutoSwitchEnabled, value, () =>
        {
            Settings.Default.autoSwitch = value;
        });
    }

    public bool IsStart
    {
        get => _isStart;
        set => SetValue(ref _isStart, value);
    }

    public AdaptivePreset CurrentPreset
    {
        get => _currentPreset;
        set => SetValue(ref _currentPreset, value);
    }

    public EnhancedObservableCollection<AdaptivePreset> AvailablePresets
    {
        get => _availablePresets;
        set => SetValue(ref _availablePresets, value);
    }

    public bool IsAsusPowerSettingsAvailable
    {
        get => _isAsusPowerSettingsAvailable;
        set => SetValue(ref _isAsusPowerSettingsAvailable, value);
    }

    public bool IsAmdApuTurboBoostOverdriveSettingsAvailable
    {
        get => _isAmdApuTurboBoostOverdriveSettingsAvailable;
        set => SetValue(ref _isAmdApuTurboBoostOverdriveSettingsAvailable, value);
    }

    public bool IsRadeonGraphicsOptionsAvailable
    {
        get => _isRadeonGraphicsOptionsAvailable;
        set => SetValue(ref _isRadeonGraphicsOptionsAvailable, value);
    }

    public bool IsNvidiaGraphicsOptionsAvailable
    {
        get => _isNvidiaGraphicsOptionsAvailable;
        set => SetValue(ref _isNvidiaGraphicsOptionsAvailable, value);
    }

    public bool IsCurveOptimizerOptionsAvailable
    {
        get => _isCurveOptimizerOptionsAvailable;
        set => SetValue(ref _isCurveOptimizerOptionsAvailable, value);
    }

    public double Polling
    {
        get => _polling;
        set => SetValue(ref _polling, value, () =>
        {
            Settings.Default.polling = value;
        });
    }

    private bool _isStart;
    private bool _isAsusPowerSettingsAvailable;
    private bool _isAmdApuTurboBoostOverdriveSettingsAvailable;
    private bool _isRadeonGraphicsOptionsAvailable;
    private bool _isNvidiaGraphicsOptionsAvailable;
    private bool _isCurveOptimizerOptionsAvailable;
    private AdaptivePreset _currentPreset;
    private EnhancedObservableCollection<AdaptivePreset> _availablePresets;
    
    private readonly ISystemInfoService _systemInfoService;
    private readonly IGameLauncherService _gameLauncherService;
    private readonly IAdaptivePresetService _adaptivePresetService;
    private readonly ISensorsService _sensorsService;
    private readonly INotificationManager _notificationManager;
    private readonly ICpuControlService _cpuControlService;
    private readonly IRtssService _rtssService;
    private readonly IAmdGpuService _amdGpuService;
    private readonly DispatcherTimer _adaptiveModeTimer;
    private readonly DispatcherTimer _sensorsTimer;

    public AdaptiveViewModel(ISystemInfoService systemInfoService,
                             IGameLauncherService gameLauncherService,
                             IAdaptivePresetService adaptivePresetService,
                             ISensorsService sensorsService,
                             INotificationManager notificationManager,
                             ICpuControlService cpuControlService,
                             IRtssService rtssService,
                             IAmdGpuService amdGpuService)
    {
        _systemInfoService = systemInfoService;
        _gameLauncherService = gameLauncherService;
        _adaptivePresetService = adaptivePresetService;
        _sensorsService = sensorsService;
        _notificationManager = notificationManager;
        _cpuControlService = cpuControlService;
        _rtssService = rtssService;
        _amdGpuService = amdGpuService;

        _adaptiveModeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _adaptiveModeTimer.Tick += AdaptiveModeTick;

        _sensorsTimer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(2)
        };
        _sensorsTimer.Tick += SensorsTimerTick;
        
        ToggleAdaptiveModeCommand = ReactiveCommand.CreateFromTask(ToggleAdaptiveMode);
        SavePresetCommand = ReactiveCommand.CreateFromTask(SavePreset);
        ReloadGamesListCommand = ReactiveCommand.Create(ReloadGamesList);
        
        Polling = Settings.Default.polling;

        Initialize();
    }

    private int CPUTemp, CPULoad, CPUClock, GPULoad, GPUClock, GPUMemClock;
    private bool _isAutoSwitchEnabled;
    private double _polling;

    private static int newMinCPUClock = 1440;
    private static int minCPUClock = 1440;
    
    private void ReloadGamesList()
    {
        AvailablePresets.Clear();
        var defaultPreset = _adaptivePresetService.GetPreset("Default");
        if (defaultPreset == null)
        {
            defaultPreset = new AdaptivePreset();
        
            if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Desktop ||
                _systemInfoService.Cpu.RyzenFamily == RyzenFamily.DragonRange)
            {
                defaultPreset.Power = 86;
            }
            else
            {
                defaultPreset.Power = 28;
            }

            defaultPreset.MaxGfx = 1900;
            defaultPreset.MinGgx = 400;
            defaultPreset.Temp = 95;
            defaultPreset.MinCpuClock = 1500;
            defaultPreset.NvMaxCoreClock = 4000;
            _adaptivePresetService.SavePreset("Default", defaultPreset);
        }
        AvailablePresets.Add(defaultPreset);
        
        _gameLauncherService.ReSearchGames(true);
        
        var installedGames = _gameLauncherService.InstalledGames.Value;
        foreach (var game in installedGames)
        {
            var adaptivePreset = _adaptivePresetService.GetPreset(game.GameName);
            if (adaptivePreset == null)
            {
                var newPreset = _adaptivePresetService.GetPreset("Default");
                _adaptivePresetService.SavePreset(game.GameName, newPreset);
                adaptivePreset = newPreset;
            }
            AvailablePresets.Add(adaptivePreset);
        }

        CurrentPreset = AvailablePresets[0];
    }

    private void SensorsTimerTick(object? sender, EventArgs e)
    {
        if (_systemInfoService.Cpu.Manufacturer == Manufacturer.Intel)
        {
            CPUTemp = (int)_sensorsService.GetCPUInfo(SensorType.Temperature, "Package");
        }
        else
        {
            CPUTemp = (int)_sensorsService.GetCPUInfo(SensorType.Temperature, "Core");
        }
        CPULoad = (int)_sensorsService.GetCPUInfo(SensorType.Load, "Total");

        for (int i = 0; i < _systemInfoService.Cpu.CoresCount; i++)
        {
            CPUClock += (int)_sensorsService.GetCPUInfo(SensorType.Clock, $"Core #{i}");
        }
        
        CPUClock /= _systemInfoService.Cpu.CoresCount;

        //CPUPower = (int)GetSensor.getCPUInfo(SensorType.Power, "Package");

        if (_systemInfoService.RadeonGpuCount <= 0)
        {
            GPULoad = _amdGpuService.GetGpuMetrics(0, AmdGpuSensor.GpuLoad);
            GPUClock = _amdGpuService.GetGpuMetrics(0, AmdGpuSensor.GpuClock);
            GPUMemClock = _amdGpuService.GetGpuMetrics(0, AmdGpuSensor.GpuMemClock);
        }

        IsGameRunning();

        if (CPULoad < 100 / _systemInfoService.Cpu.CoresCount + 5)
        {
            newMinCPUClock = minCPUClock + 500;
        }
        else
        {
            newMinCPUClock = minCPUClock;
        }

        if (AvailablePresets.Count > 0 && IsAutoSwitchEnabled)
        {
            if (selectedGameName != runningGameName)
            {
                int selectedIndex = 0; // index to select if the search fails

                foreach (var item in cbxPowerPreset.Items)
                {
                    if (item.ToString() == runningGameName)
                    {
                        cbxPowerPreset.SelectedItem = item;
                        return;
                    }
                }
            }
        }
    }
    
    private void IsGameRunning()
    {
        foreach (GameLauncherItem item in installedGames)
        {
            //var gamePath = game.Split("~");

            int i = 0;
            do
            {
                Process[] processes = Process.GetProcesses();

                foreach (Process process in processes)
                {
                    try
                    {
                        string executablePath = process.MainModule.FileName;
                        string executableDirectory = System.IO.Path.GetDirectoryName(executablePath);
                        string executableName = System.IO.Path.GetFileName(executablePath);

                        if (executablePath.Contains(item.path))
                        {
                            bool autoSwitch = true;
                            AdaptivePreset preset = _adaptivePresetService.GetPreset(item.gameName);
                            if (preset != null)
                            {
                                autoSwitch = preset.isAutoSwitch;
                            }

                            if (!autoSwitch)
                            {
                                continue;
                            }

                            runningGameName = item.gameName;
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }

                i++;
            } while (i < 2);
        }

        runningGameName = "Default";
    }

    private async Task AutoSwitch()
    {
        Settings.Default.autoSwitch = CurrentPreset.IsAutoSwitch;
        Settings.Default.Save();
    }

    private async Task SavePreset()
    {
        _adaptivePresetService.SavePreset(CurrentPreset.Name, CurrentPreset);
    }

    private void AdaptiveModeTick(object? sender, EventArgs e)
    {
        if (i < 2)
        {
            _cpuControlService.UpdatePowerLimit(CPUTemp, CPULoad, CurrentPreset.Power,
                CurrentPreset.Power - 5, CurrentPreset.Temp);
            _cpuControlService.UpdatePowerLimit(CPUTemp, CPULoad, CurrentPreset.Power,
                CurrentPreset.Power - 5, CurrentPreset.Temp);
            _cpuControlService.UpdatePowerLimit(CPUTemp, CPULoad, CurrentPreset.Power,
                CurrentPreset.Power - 5, CurrentPreset.Temp);
            i++;
        }
        else
        {
            _cpuControlService.UpdatePowerLimit(CPUTemp, CPULoad, CurrentPreset.Power, 8,
                CurrentPreset.Temp);

            if (CurrentPreset.IsCo)
                _cpuControlService.CurveOptimiserLimit(CPULoad, CurrentPreset.Co);

            if (CurrentPreset.IsGfx)
                AmdApuControlService.UpdateiGPUClock(CurrentPreset.MaxGfx, CurrentPreset.MinGgx,
                    (int)CurrentPreset.Temp, CPUPower, CPUTemp, GPUClock, GPULoad, GPUMemClock, CPUClock,
                    minCPUClock);

            string commandString = "";

            commandString = commandString +
                            $"--UXTUSR={CurrentPreset.IsMag}-{CurrentPreset.IsVsync}-{CurrentPreset.Sharpness / 100}-{CurrentPreset.ResScaleIndex}-{CurrentPreset.IsRecap} ";

            if (Settings.Default.isASUS)
            {
                if (CurrentPreset.AsusPowerProfile > 0)
                    commandString = commandString + $"--ASUS-Power={CurrentPreset.AsusPowerProfile} ";
            }

            _cpuControlService.UpdatePowerLimit();
            if (_cpuControlService.CpuCommand != lastCPU)
            {
                commandString += _cpuControlService.CpuCommand;
                lastCPU = _cpuControlService.CpuCommand;
            }

            if (_cpuControlService.CoCommand != null && _cpuControlService.CoCommand != "" &&
                CurrentPreset.IsCo && _cpuControlService.CoCommand != lastCO)
            {
                commandString += _cpuControlService.CoCommand;
                lastCO = _cpuControlService.CoCommand;
            }

            if (AmdApuControlService.Commmand != null && AmdApuControlService.Commmand != "" &&
                CurrentPreset.IsGfx && AmdApuControlService.Commmand != lastiGPU)
            {
                commandString += AmdApuControlService.Commmand;
                lastiGPU = AmdApuControlService.Commmand;
            }

            if (CurrentPreset.IsRadeonGraphics)
            {
                if (CurrentPreset.IsAntiLag)
                    commandString = commandString + $"--ADLX-Lag=0-true --ADLX-Lag=1-true ";
                else commandString = commandString + $"--ADLX-Lag=0-false --ADLX-Lag=1-false ";

                if (CurrentPreset.IsRsr)
                    commandString = commandString + $"--ADLX-RSR=true-{CurrentPreset.Rsr} ";
                else commandString = commandString + $"--ADLX-RSR=false-{CurrentPreset.Rsr} ";

                if (CurrentPreset.IsBoost)
                    commandString = commandString +
                                    $"--ADLX-Boost=0-true-{CurrentPreset.Boost} --ADLX-Boost=1-true-{CurrentPreset.Boost} ";
                else
                    commandString = commandString +
                                    $"--ADLX-Boost=0-false-{CurrentPreset.Boost} --ADLX-Boost=1-false-{CurrentPreset.Boost} ";

                if (CurrentPreset.IsImageSharp)
                    commandString = commandString +
                                    $"--ADLX-ImageSharp=0-true-{CurrentPreset.ImageSharp} --ADLX-ImageSharp=1-true-{CurrentPreset.ImageSharp} ";
                else
                    commandString = commandString +
                                    $"--ADLX-ImageSharp=0-false-{CurrentPreset.ImageSharp} --ADLX-ImageSharp=1-false-{CurrentPreset.ImageSharp} ";

                if (CurrentPreset.IsSync)
                    commandString = commandString + $"--ADLX-Sync=0-true --ADLX-Sync=1-true ";
                else commandString = commandString + $"--ADLX-Sync=0-false --ADLX-Sync=1-false ";
            }

            if (CurrentPreset.IsNvidia)
            {
                commandString = commandString +
                                $"--NVIDIA-Clocks={CurrentPreset.NvMaxCoreClock}-{CurrentPreset.NvCoreClock}-{CurrentPreset.NvMemClock} ";
            }

            if (commandString != null && commandString != "")
                await Task.Run(() => RyzenAdjService.Translate(commandString));
        }

        if (_rtssService.IsRTSSRunning() && tsRTSS.IsChecked == true)
            _rtssService.FpsLimit = nudRTSS.Value;


        //if (RTSS.RTSSRunning())
        //{
        //    int i = 0;
        //    bool found = false;
        //    do
        //    {
        //        AppFlags appFlag = RunningGames.appFlags[i];
        //        var appEntries = OSD.GetAppEntries(appFlag);
        //        foreach (var app in appEntries)
        //        {
        //            found = true;
        //            osd.Update($"{RunningGames.appFlags[i]} {app.InstantaneousFrames}FPS {app.InstantaneousFrameTime.Milliseconds}ms");
        //        }
        //        i++;
        //    } while (i < RunningGames.appFlags.Count && found == false);
        //}

        if (Settings.Default.polling != nudPolling.Value)
        {
            Settings.Default.polling = (double)nudPolling.Value;
            Settings.Default.Save();
        }

        if (adaptiveMode.Interval != TimeSpan.FromSeconds((double)nudPolling.Value))
        {
            adaptiveMode.Stop();
            adaptiveMode.Interval = TimeSpan.FromSeconds((double)nudPolling.Value);
            adaptiveMode.Start();
        }
        
        if (sensors.Interval != TimeSpan.FromSeconds((double)nudPolling.Value))
        {
            sensors.Stop();
            sensors.Interval = TimeSpan.FromSeconds((double)nudPolling.Value);
            sensors.Start();
        }
    }

    private void Initialize()
    {
        IsAsusPowerSettingsAvailable = _systemInfoService.LaptopInfo?.IsAsus == true;
        
        if (_systemInfoService.RadeonGpuCount == 0)
        {
            IsAmdApuTurboBoostOverdriveSettingsAvailable = false;
            IsRadeonGraphicsOptionsAvailable = false;
        }

        IsNvidiaGraphicsOptionsAvailable = _systemInfoService.NvidiaGpuCount != 0;
        
        _gameLauncherService.ReSearchGames(true);
        ReloadGamesList();
        
        if (_systemInfoService.Cpu.Manufacturer == Manufacturer.Intel)
        {
            IsCurveOptimizerOptionsAvailable = false;
            IsAmdApuTurboBoostOverdriveSettingsAvailable = false;
        }

        if (Settings.Default.isStartAdpative)
        {
            ToggleAdaptiveMode().GetAwaiter().GetResult();
        }
    }

    private async Task ToggleAdaptiveMode()
    {
        try
        {
            if (IsStart)
            {
                _adaptiveModeTimer.Stop();
                _sensorsService.Stop();
                Settings.Default.isAdaptiveModeRunning = false;
                IsStart = false;
            }
            else
            {
                _adaptiveModeTimer.Start();
                _sensorsService.Start();
                Settings.Default.isAdaptiveModeRunning = true;
                IsStart = true;
            }

            Settings.Default.Save();
        }
        catch (Exception ex)
        {
            await _notificationManager.ShowTextNotification("Error occurred", ex.Message,
                NotificationManagerExtensions.NotificationType.Error);
        }
    }
}