namespace ApplicationCore.Interfaces;

public interface IAmdApuControlService
{
    public int CurrentPowerLimit { get; set; }
    public string? Commmand { get; }

    public void UpdateiGPUClock(int maxClock, 
        int minClock, 
        int maxTemperature, 
        int powerDraw,
        int temperature, 
        int currentClock, 
        int gpuLoad,
        int memClock,
        int cpuClocks, 
        int minCpuClock, 
        int fps = 0,
        int fpsLimit = 0);
}