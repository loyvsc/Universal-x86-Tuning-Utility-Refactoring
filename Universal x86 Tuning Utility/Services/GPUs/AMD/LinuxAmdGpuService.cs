using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.GPUs.AMD;

public class LinuxAmdGpuService : IAmdGpuService
{
    public bool IsRsrEnabled { get; set; }
    public int RsrSharpness { get; set; }
    
    public int GetFpsData()
    {
        throw new System.NotImplementedException();
    }

    public int GetGpuMetrics(int gpuId, AmdGpuSensorEnum gpuSensor)
    {
        throw new System.NotImplementedException();
    }

    public void SetFpsLimit(int gpuId, int fpsLimit, bool isEnabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetAntilag(int gpuId, bool isEnabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetBoost(int gpuId, int percent, bool isEnabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetChill(int gpuId, int maxFps, int minFps, bool isEnabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetImageSharpening(int gpuId, int percent, bool isEnabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetEnhancedSynchronization(int gpuId, bool isEnabled)
    {
        throw new System.NotImplementedException();
    }
}