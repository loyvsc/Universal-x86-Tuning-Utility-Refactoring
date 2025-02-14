namespace ApplicationCore.Interfaces;

public interface INvidiaGpuService
{
    /// <exception cref="AggregateException">Throws when no nvidia gpu installed or failed to set clocks</exception>
    void SetClocks(int core, int memory, int coreVoltage = 0);

    int MaxGpuClock { get; set; }
}