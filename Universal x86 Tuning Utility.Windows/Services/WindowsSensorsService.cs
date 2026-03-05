using System;
using System.Linq;
using ApplicationCore.Interfaces;
using LibreHardwareMonitor.Hardware;
using SensorType = ApplicationCore.Enums.SensorType;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsSensorsService : ISensorsService, IDisposable
{
    private readonly Lazy<Computer> _thisPc;
    
    private DateTime _lastUpdate;
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1);

    public WindowsSensorsService()
    {
        _thisPc = new Lazy<Computer>(() =>
        {
            var computer = new Computer
            {
                IsCpuEnabled = true,
                IsGpuEnabled = true,
                IsMemoryEnabled = true
            };
            computer.Open();
            return computer;
        });
    }

    public void Start()
    {
        _thisPc.Value.Open();
    }

    public void Stop()
    {
        if (_thisPc.IsValueCreated)
        {
            _thisPc.Value.Close();
        }
    }

    private void UpdateAllHardware()
    {
        if (DateTime.UtcNow - _lastUpdate < _updateInterval)
        {
            return;
        }

        foreach (var hardware in _thisPc.Value.Hardware)
        {
            hardware.Update();
        }

        _lastUpdate = DateTime.UtcNow;
    }

    public float GetCPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAllHardware();
        var hardware = _thisPc.Value.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        if (hardware == null) return 0;
        
        return GetSensorValue(hardware, sensorType, sensorName);
    }

    public float GetAMDGPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAllHardware();
        var hardware = _thisPc.Value.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd);
        if (hardware == null) return 0;
        
        return GetSensorValue(hardware, sensorType, sensorName);
    }

    public float GetNvidiaGPUInfo(SensorType sensorType, string sensorName)
    {
        UpdateAllHardware();
        var hardware = _thisPc.Value.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia);
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
            SensorType.Power => LibreHardwareMonitor.Hardware.SensorType.Power,
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