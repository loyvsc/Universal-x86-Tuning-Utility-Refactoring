using ApplicationCore.Enums;
using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IAmdGpuService
{
    public bool IsRsrEnabled { get; set; }
    public int RsrSharpness { get; set; }
    
    public int GetGpuMetrics(int gpuId, AmdGpuSensor gpuSensor);
    public void SetAntilag(int gpuId, bool isEnabled);
    public void SetBoost(int gpuId, int percent, bool isEnabled);
    public void SetChill(int gpuId, int maxFps, int minFps, bool isEnabled);
    public void SetImageSharpening(int gpuId, int percent, bool isEnabled);
    public void SetEnhancedSynchronization(int gpuId, bool isEnabled);
}