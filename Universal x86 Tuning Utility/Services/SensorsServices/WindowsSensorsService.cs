using System;
using System.Linq;
using ApplicationCore.Interfaces;
using LibreHardwareMonitor.Hardware;
using SensorType = ApplicationCore.Enums.SensorType;

namespace Universal_x86_Tuning_Utility.Services.SensorsServices;

public class WindowsSensorsService : ISensorsService, IDisposable
{
    private readonly Computer _thisPc = new Computer
    {
        IsCpuEnabled = true,
        IsGpuEnabled = true,
        IsMemoryEnabled = true
    };

    private DateTime _lastUpdate;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

    public void Start()
    {
        _thisPc.Open();
    }

    public void Stop()
    {
        _thisPc.Close();
    }

    private void UpdateAllHardware()
    {
        if (DateTime.UtcNow - _lastUpdate < _updateInterval)
        {
            return;
        }

        foreach (var hardware in _thisPc.Hardware)
        {
            hardware.Update();
        }

        _lastUpdate = DateTime.UtcNow;
    }

    public float GetCPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAllHardware();
        var hardware = _thisPc.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        if (hardware == null) return 0;
        
        return GetSensorValue(hardware, sensorType, sensorName);
    }

    public float GetAMDGPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAllHardware();
        var hardware = _thisPc.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd);
        if (hardware == null) return 0;
        
        return GetSensorValue(hardware, sensorType, sensorName);
    }

    public float GetNvidiaGPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAllHardware();
        var hardware = _thisPc.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia);
        if (hardware == null) return 0;
        
        return GetSensorValue(hardware, sensorType, sensorName);
    }

    private float GetSensorValue(IHardware hardware, SensorType sensorType, string sensorName)
    {
        var libreSensorType = sensorType switch
        {
            SensorType.Load => LibreHardwareMonitor.Hardware.SensorType.Load,
            SensorType.Clock => LibreHardwareMonitor.Hardware.SensorType.Clock,
            SensorType.Temperature => LibreHardwareMonitor.Hardware.SensorType.Temperature,
            _ => throw new ArgumentOutOfRangeException(nameof(sensorType))
        };
        
        var sensor = hardware.Sensors.FirstOrDefault(s => s.SensorType == libreSensorType && s.Name.Contains(sensorName));
        return sensor?.Value ?? 0;
    }

    public void Dispose()
    {
        Stop();
    }
}