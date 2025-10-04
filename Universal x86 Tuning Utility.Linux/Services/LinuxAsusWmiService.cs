using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Serilog;
using Universal_x86_Tuning_Utility.Linux.Interfaces;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxAsusWmiService : IASUSWmiService
{
    private const string ASUS_ACPI_PATH = "/sys/devices/platform/asus-nb-wmi";
    private const string ASUS_FAN_PATH = "/sys/devices/platform/asus_fan";
    
    private readonly ILogger _logger;
    private readonly ISysFsEventService _sysFsEventService;
    private readonly List<IDisposable> _asusAtkWmiEventSubscriptions;
    
    public LinuxAsusWmiService(ILogger logger, ISysFsEventService sysFsEventService)
    {
        _logger = logger;
        _sysFsEventService = sysFsEventService;
        _asusAtkWmiEventSubscriptions = new List<IDisposable>();
    }

    public void RunListener()
    {
        try
        {
            _logger.Information("Starting ASUS sysfs listener...");

            _asusAtkWmiEventSubscriptions.Add(_sysFsEventService
                .SubscribeToPath(ASUS_ACPI_PATH)
                .Subscribe(args => _logger.Information("ACPI Event: {Event}", args)));

            _asusAtkWmiEventSubscriptions.Add(_sysFsEventService
                .SubscribeToPath(ASUS_FAN_PATH)
                .Subscribe(args => _logger.Information("FAN Event: {Event}", args)));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start RunListener()");
        }
    }

    public byte[] DeviceInit()
    {
        try
        {
            bool acpi = Directory.Exists(ASUS_ACPI_PATH);
            bool fan = Directory.Exists(ASUS_FAN_PATH);

            _logger.Information("ASUS WMI init check: ACPI={Acpi}, FAN={Fan}", acpi, fan);

            return new byte[] { (byte)(acpi ? 1 : 0), (byte)(fan ? 1 : 0) };
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "DeviceInit failed");
            return Array.Empty<byte>();
        }
    }

    public int DeviceSet(AsusDevice device, int newValue)
    {
        _logger.Information("Set value of {AsusDevice} to {NewValue} on Linux", device, newValue);
        
        try
        {
            string path = GetDevicePath(device);
            if (string.IsNullOrEmpty(path)) return -1;
            
            File.WriteAllText(path, newValue.ToString());
            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to DeviceSet {AsusDevice}", device);
            throw new Exception("Failed to DeviceSet. See inner exception to details.", ex);
        }
    }

    public int DeviceSet(AsusDevice device, byte[] values)
    {
        try
        {
            string path = GetDevicePath(device);
            if (string.IsNullOrEmpty(path)) return -1;

            if (!File.Exists(path))
            {
                _logger.Warning("DeviceSet path not found: {Path}", path);
                return -1;
            }

            File.WriteAllBytes(path, values);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "DeviceSet(byte[]) failed for {AsusDevice}", device);
            return -1;
        }
    }

    public int DeviceGet(AsusDevice device)
    {
        _logger.Information("Getting status of {AsusDevice} on Linux", device);
        
        try
        {
            string path = GetDevicePath(device);
            if (string.IsNullOrEmpty(path)) return -1;
            
            string value = File.ReadAllText(path).Trim();
            return int.Parse(value);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to DeviceGet {AsusDevice}", device);
            throw new Exception("Failed to DeviceGet. See inner exception to details.", ex);
        }
    }

    private string GetDevicePath(AsusDevice device)
    {
        return device switch
        {
            AsusDevice.CpuFan => $"{ASUS_FAN_PATH}/cpu_fan_speed",
            AsusDevice.GpuFan => $"{ASUS_FAN_PATH}/gpu_fan_speed",
            AsusDevice.SystemFan => $"{ASUS_FAN_PATH}/sys_fan_speed",
            AsusDevice.TufKeyboardState => $"{ASUS_ACPI_PATH}/kbd_backlight_state",
            AsusDevice.TufKeyboardBrightness => $"{ASUS_ACPI_PATH}/kbd_backlight",
            AsusDevice.GpuMux => $"{ASUS_ACPI_PATH}/gpu_mux",
            AsusDevice.GpuMuxVivo => $"{ASUS_ACPI_PATH}/gpu_mux_vivo",
            AsusDevice.GpuEco => $"{ASUS_ACPI_PATH}/gpu_eco_mode",
            AsusDevice.EGpu => $"{ASUS_ACPI_PATH}/dgpu_disable",
            AsusDevice.EGpuConnected => $"{ASUS_ACPI_PATH}/dgpu_connected",
            _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
        };
    }

    public void SetPerformanceMode(AsusMode newMode)
    {
        try
        {
            string path = $"{ASUS_ACPI_PATH}/throttle_thermal_policy";
            File.WriteAllText(path, ((int)newMode).ToString());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set performance mode");
        }
    }

    public AsusMode GetPerformanceMode()
    {
        try
        {
            string path = $"{ASUS_ACPI_PATH}/throttle_thermal_policy";
            string value = File.ReadAllText(path).Trim();
            return (AsusMode)int.Parse(value);
        }
        catch
        {
            return AsusMode.Balanced;
        }
    }

    public void SetGPUEco(bool eco)
    {
        try
        {
            string path = $"{ASUS_ACPI_PATH}/gpu_eco";
            File.WriteAllText(path, eco ? "1" : "0");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set GPU eco mode");
        }
    }

    public int GetFan(AsusFan device)
    {
        try
        {
            string path = device switch
            {
                AsusFan.CPU => $"{ASUS_FAN_PATH}/cpu_fan_speed",
                AsusFan.GPU => $"{ASUS_FAN_PATH}/gpu_fan_speed",
                AsusFan.Mid => $"{ASUS_FAN_PATH}/sys_fan_speed",
                _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
            };

            if (!File.Exists(path)) return -1;

            var val = File.ReadAllText(path).Trim();
            return int.TryParse(val, out int rpm) ? rpm : -1;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to GetFan {AsusFan}", device);
            return -1;
        }
    }

    public void SetFanRange(AsusFan device, byte[] curve)
    {
        try
        {
            string fanName = device switch
            {
                AsusFan.CPU => "cpu",
                AsusFan.GPU => "gpu", 
                AsusFan.Mid => "sys",
                _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
            };

            string path = $"{ASUS_FAN_PATH}/{fanName}_fan_curve_range";

            if (!File.Exists(path))
            {
                _logger.Warning("SetFanRange path not found: {Path}", path);
                return;
            }

            File.WriteAllBytes(path, curve);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to SetFanRange for {AsusFan}", device);
        }
    }

    public void SetFanCurve(AsusFan device, byte[] curve)
    {
        try
        {
            if (curve.Length != 16)
                throw new ArgumentException("Curve must be 16 bytes");

            string path = $"{ASUS_FAN_PATH}/{device.ToString().ToLower()}_fan_curve";
            if (!File.Exists(path))
            {
                _logger.Warning("Fan curve path not found: {Path}", path);
                return;
            }

            File.WriteAllBytes(path, curve);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to SetFanCurve {AsusFan}", device);
        }
    }

    public byte[] GetFanCurve(AsusFan device, int mode = 0)
    {
        try
        {
            string dev = device switch
            {
                AsusFan.CPU => "cpu",
                AsusFan.GPU => "gpu",
                AsusFan.Mid => "sys",
                _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
            };

            string path = mode == 0
                ? $"{ASUS_FAN_PATH}/{dev}_fan_curve"
                : $"{ASUS_FAN_PATH}/{dev}_fan_curve{mode}";

            if (!File.Exists(path))
            {
                _logger.Warning("Fan curve file not found: {Path}", path);
                return Array.Empty<byte>();
            }

            return File.ReadAllBytes(path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to GetFanCurve {AsusFan}", device);
            return Array.Empty<byte>();
        }
    }

    public bool IsInvalidCurve(byte[] curve)
    {
        return curve.Length != 16 || IsEmptyCurve(curve);
    }

    private bool IsEmptyCurve(byte[] curve)
    {
        return curve.All(singleByte => singleByte == 0);
    }

    public byte[] FixFanCurve(byte[] curve)
    {
        // Та же реализация, что и в Windows
        if (curve.Length != 16) throw new ArgumentException("Incorrect curve");

        var points = new Dictionary<byte, byte>();
        byte old = 0;

        for (int i = 0; i < 8; i++)
        {
            if (curve[i] == old) curve[i]++;
            points[curve[i]] = curve[i + 8];
            old = curve[i];
        }

        var pointsFixed = new Dictionary<byte, byte>();
        bool fix = false;

        int count = 0;
        foreach (var pair in points.OrderBy(x => x.Key))
        {
            if (count == 0 && pair.Key >= 40)
            {
                fix = true;
                pointsFixed.Add(30, 0);
            }

            if (count != 3 || !fix)
                pointsFixed.Add(pair.Key, pair.Value);
            count++;
        }

        count = 0;
        foreach (var pair in pointsFixed.OrderBy(x => x.Key))
        {
            curve[count] = pair.Key;
            curve[count + 8] = pair.Value;
            count++;
        }

        return curve;
    }

    public bool IsXGConnected()
    {
        try
        {
            string path = $"{ASUS_ACPI_PATH}/dgpu_connected";
            return File.Exists(path) && File.ReadAllText(path).Trim() == "1";
        }
        catch
        {
            return false;
        }
    }

    public void ScanRange()
    {
        throw new NotImplementedException();
    }

    public void TUFKeyboardBrightness(int brightness)
    {
        try
        {
            string path = $"{ASUS_ACPI_PATH}/kbd_backlight";
            if (File.Exists(path))
                File.WriteAllText(path, brightness.ToString());
            else
                _logger.Warning("Keyboard brightness path not found: {Path}", path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set TUF keyboard brightness");
        }
    }

    public void TUFKeyboardRGB(int mode, Color color, int speed)
    {
        try
        {
            string path = $"{ASUS_ACPI_PATH}/kbd_rgb";
            if (!File.Exists(path))
            {
                _logger.Warning("Keyboard RGB path not found: {Path}", path);
                return;
            }

            string val = $"{mode} {color.R} {color.G} {color.B} {speed}";
            File.WriteAllText(path, val);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set TUF keyboard RGB");
        }
    }

    public void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false)
    {
        try
        {
            string path = $"{ASUS_ACPI_PATH}/kbd_power";

            if (!File.Exists(path))
            {
                _logger.Warning("Keyboard power path not found: {Path}", path);
                return;
            }

            // Простейший вариант: записываем флаги в одну строку
            string value = $"{(awake ? 1 : 0)} {(boot ? 1 : 0)} {(sleep ? 1 : 0)} {(shutdown ? 1 : 0)}";
            File.WriteAllText(path, value);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set TUF keyboard power state");
        }
    }

    public void SubscribeToEvents(Action<EventArgs> eventHandler)
    {
        _asusAtkWmiEventSubscriptions.Add(_sysFsEventService.SubscribeToPath(ASUS_ACPI_PATH).Subscribe(eventHandler));
    }

    public void Dispose()
    {
        foreach (var subscription in _asusAtkWmiEventSubscriptions)
        {
            subscription.Dispose();
        }
    }
}