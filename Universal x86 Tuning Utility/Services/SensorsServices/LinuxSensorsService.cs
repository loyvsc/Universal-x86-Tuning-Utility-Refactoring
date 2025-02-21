using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.SensorsServices;

public class LinuxSensorsService : ISensorsService
{
    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public float GetCPUInfo(SensorType sensorType, string sensorName)
    {
        throw new System.NotImplementedException();
    }

    public float GetAMDGPUInfo(SensorType sensorType, string sensorName)
    {
        throw new System.NotImplementedException();
    }

    public float GetNvidiaGPUInfo(SensorType sensorType, string sensorName)
    {
        throw new System.NotImplementedException();
    }
}