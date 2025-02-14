using ApplicationCore.Enums;

namespace ApplicationCore.Interfaces;

public interface ISensorsService
{
    public void Start();
    public void Stop();
    public float GetCPUInfo(SensorType sensorType, string sensorName);
    public float GetAMDGPUInfo(SensorType sensorType, string sensorName);
    public float GetNvidiaGPUInfo(SensorType sensorType, string sensorName);
}