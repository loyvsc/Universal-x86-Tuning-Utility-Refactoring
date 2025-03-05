using System;
using System.Runtime.InteropServices;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.GPUs.AMD;

public class WindowsAmdGpuService : IAmdGpuService
{
    private const string PerfMetrics = "ADLX_PerformanceMetrics.dll";
    private const string GraphSettings = "ADLX_3DSettings.dll";

    public bool IsRsrEnabled
    {
        get => GetRSRState() == 1;
        set
        {
            var isRsrEnabled = GetRSRState() == 1;
            if (isRsrEnabled != value)
            {
                _ = SetRSR(value);
            }
        }
    }

    public int GetFpsData()
    {
        return GetFPSData();
    }

    public int GetGpuMetrics(int gpuId, AmdGpuSensor gpuSensor)
    {
        int sensorId = gpuSensor switch
        {
            AmdGpuSensor.GpuLoad => 7,
            AmdGpuSensor.GpuClock => 0,
            AmdGpuSensor.GpuMemClock => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(gpuSensor), gpuSensor, "Gpu sensor is not supported")
        };

        return GetGPUMetrics(gpuId, sensorId);
    }

    public void SetFpsLimit(int gpuId, int fpsLimit, bool isEnabled)
    {
        _ = SetFPSLimit(gpuId, isEnabled, fpsLimit);
    }

    public void SetAntilag(int gpuId, bool isEnabled)
    {
        _ = SetAntiLag(gpuId, isEnabled);
    }

    public void SetBoost(int gpuId, int percent, bool isEnabled)
    {
        _ = SetBoost(gpuId, isEnabled, percent);
    }

    public int RsrSharpness
    {
        get => GetRSRSharpness();
        set => SetRSRSharpness(value);
    }

    public void SetChill(int gpuId, int maxFps, int minFps, bool isEnabled)
    {
        _ = SetChill(gpuId, isEnabled, maxFps, minFps);
    }

    public void SetImageSharpening(int gpuId, int percent, bool isEnabled)
    {
        _ = SetImageSharpning(gpuId, isEnabled, percent);
    }

    public void SetEnhancedSynchronization(int gpuId, bool isEnabled)
    {
        _ = SetEnhancedSync(gpuId, isEnabled);
    }
    
    [DllImport(PerfMetrics, CallingConvention = CallingConvention.Cdecl)] private static extern int GetFPSData();
    [DllImport(PerfMetrics, CallingConvention = CallingConvention.Cdecl)] private static extern int GetGPUMetrics(int GPU, int Sensor);

    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int SetFPSLimit(int GPU, bool isEnabled, int FPS);
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int SetAntiLag(int GPU, bool isEnabled);
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int SetBoost(int GPU, bool isEnabled, int percent);
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int SetRSR(bool isEnabled);
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int GetRSRState();
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern bool SetRSRSharpness(int sharpness);
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int GetRSRSharpness();
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int SetChill(int GPU, bool isEnabled, int maxFPS, int minFPS);
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int SetImageSharpning(int GPU, bool isEnabled, int percent);
    [DllImport(GraphSettings, CallingConvention = CallingConvention.Cdecl)] private static extern int SetEnhancedSync(int GPU, bool isEnabled);
}