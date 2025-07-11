namespace ApplicationCore.Interfaces;

public interface IAmdApuControlService
{
    public int PowerLimit { get; set; }
    
    public bool IsAvailable { get; }
    
    public int MinClock { get; }
    public int MaxClock { get; }
    public int Clock { get; }
    
    public int MaxTemperature { get; }
    public int Temperature { get; }
    
    public int MemeryClock { get; }
    
    public int iGpuLoad { get; }
    
    public int FpsLimit { get; }
    

    /// <exception cref="AggregateException">Throws </exception>
    public void UpdateiGPUClock(int maxClock, 
        int minClock, 
        int maxTemperature, 
        int temperature, 
        int currentClock, 
        int gpuLoad,
        int memClock,
        int cpuClocks, 
        int minCpuClock, 
        int fps = 0,
        int fpsLimit = 0);
}