using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Serilog;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxSensorsService : ISensorsService
{
    private readonly ILogger _logger;
    private DateTime _lastUpdate;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);
    private readonly Timer _timer;

    private float _cachedCpuTemp;
    private float _cachedCpuFreq;
    private float _cachedGpuTemp;
    private float _cachedGpuLoad;
    private float _cachedCpuLoad;
    private float _cachedCpuPower;
    private float _cachedGpuPower;

    public LinuxSensorsService(ILogger logger)
    {
        _logger = logger;

        _timer = new Timer(_updateInterval.TotalMilliseconds);
    }

    public void Start()
    {
        _timer.Start();
        _logger.Information("LinuxSensorsService started");
    }

    public void Stop()
    {
        _timer.Stop();
        _logger.Information("LinuxSensorsService stopped");
    }

    private void UpdateAll()
    {
        if (DateTime.UtcNow - _lastUpdate < _updateInterval)
            return;

        _cachedCpuTemp = ReadCpuTemp();
        _cachedCpuFreq = ReadCpuFreq();
        _cachedCpuLoad = ReadCpuLoad();
        _cachedCpuPower = ReadCpuPower();
        _cachedGpuTemp = ReadGpuTemp();
        _cachedGpuLoad = ReadGpuLoad();
        _cachedGpuPower = ReadGpuPower();

        _lastUpdate = DateTime.UtcNow;
    }


    public float GetCPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAll();
        return sensorType switch
        {
            SensorType.Temperature => _cachedCpuTemp,
            SensorType.Clock => _cachedCpuFreq,
            SensorType.Load => _cachedCpuLoad,
            SensorType.Power => _cachedCpuPower,
            _ => 0
        };
    }


    public float GetAMDGPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAll();
        return sensorType switch
        {
            SensorType.Temperature => _cachedGpuTemp,
            SensorType.Load => _cachedGpuLoad,
            SensorType.Power => _cachedGpuPower,
            _ => 0
        };
    }

    public float GetNvidiaGPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAll();
        return sensorType switch
        {
            SensorType.Temperature => GetNvidiaTemperature(),
            SensorType.Load => GetNvidiaLoad(),
            SensorType.Power => GetNvidiaPower(),
            _ => 0
        };
    }

    private float ReadCpuLoad()
    {
        try
        {
            var cpuStat = File.ReadAllLines("/proc/stat").FirstOrDefault(l => l.StartsWith("cpu "));
            if (cpuStat == null) return 0;

            var parts = cpuStat.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Skip(1).Select(float.Parse).ToArray();
            float idle = parts[3];
            float total = parts.Sum();

            float load = 100 * (1 - idle / total);
            return load;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read CPU load");
        }

        return 0;
    }

    private float ReadCpuPower()
    {
        try
        {
            var energyFile = Directory.GetFiles("/sys/class/powercap/", "energy_uj", SearchOption.AllDirectories)
                .FirstOrDefault(f => f.Contains("package"));
            if (energyFile != null)
            {
                var energy = File.ReadAllText(energyFile).Trim();
                return float.Parse(energy, CultureInfo.InvariantCulture) / 1_000_000f; // микроджоули → миллиджоули
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read CPU power");
        }

        return 0;
    }

    private float ReadGpuPower()
    {
        try
        {
            var hwmon = Directory.GetDirectories("/sys/class/hwmon")
                .FirstOrDefault(d => Directory.GetFiles(d, "name").Any(f => File.ReadAllText(f).Contains("amdgpu")));
            if (hwmon != null)
            {
                var powerFile = Path.Combine(hwmon, "power1_average");
                if (File.Exists(powerFile))
                {
                    var raw = File.ReadAllText(powerFile).Trim();
                    return float.Parse(raw, CultureInfo.InvariantCulture) / 1_000_000f; // микроватты → Ватты
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read GPU power");
        }

        return 0;
    }

    private float GetNvidiaPower()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=power.draw --format=csv,noheader,nounits",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var output = Process.Start(psi)?.StandardOutput.ReadToEnd()?.Trim();
            return float.TryParse(output, out float watts) ? watts : 0;
        }
        catch
        {
            return 0;
        }
    }

    private float ReadCpuTemp()
    {
        try
        {
            var path = "/sys/class/thermal/thermal_zone0/temp";
            if (File.Exists(path))
            {
                var raw = File.ReadAllText(path).Trim();
                return float.Parse(raw, CultureInfo.InvariantCulture) / 1000f;
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read CPU temp");
        }

        return 0;
    }

    private float ReadCpuFreq()
    {
        try
        {
            var path = "/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq";
            if (File.Exists(path))
            {
                var raw = File.ReadAllText(path).Trim();
                return float.Parse(raw, CultureInfo.InvariantCulture) / 1000f; // kHz → MHz
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read CPU freq");
        }

        return 0;
    }

    private float ReadGpuTemp()
    {
        try
        {
            var hwmon = Directory.GetDirectories("/sys/class/hwmon")
                .FirstOrDefault(d => Directory.GetFiles(d, "name").Any(f => File.ReadAllText(f).Contains("amdgpu")));

            if (hwmon != null)
            {
                var tempFile = Path.Combine(hwmon, "temp1_input");
                if (File.Exists(tempFile))
                {
                    var raw = File.ReadAllText(tempFile).Trim();
                    return float.Parse(raw, CultureInfo.InvariantCulture) / 1000f;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read AMD GPU temp");
        }

        return 0;
    }

    private float ReadGpuLoad()
    {
        try
        {
            var busyFile = "/sys/class/drm/card0/device/gpu_busy_percent";
            if (File.Exists(busyFile))
            {
                var raw = File.ReadAllText(busyFile).Trim();
                return float.Parse(raw, CultureInfo.InvariantCulture);
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read GPU load");
        }

        return 0;
    }

    private float GetNvidiaTemperature()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=temperature.gpu --format=csv,noheader,nounits",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var output = Process.Start(psi)?.StandardOutput.ReadToEnd()?.Trim();
            return float.TryParse(output, out float temp) ? temp : 0;
        }
        catch
        {
            return 0;
        }
    }

    private float GetNvidiaLoad()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "nvidia-smi",
                Arguments = "--query-gpu=utilization.gpu --format=csv,noheader,nounits",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var output = Process.Start(psi)?.StandardOutput.ReadToEnd()?.Trim();
            return float.TryParse(output, out float load) ? load : 0;
        }
        catch
        {
            return 0;
        }
    }

    public void Dispose()
    {
        Stop();
    }
}