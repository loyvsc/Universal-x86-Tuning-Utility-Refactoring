using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface INvidiaGpuService
{
    public IReadOnlyCollection<BasicGpuInfo> Gpus { get; }

    public void RefreshGpusList();
    
    /// <exception cref="AggregateException">Throws when <paramref name="gpuId"/> incorrect or failed to set clocks</exception>
    public void SetClocks(uint gpuId, int core, int memory, int coreVoltage = 0);

    public Task SetMaxGpuClock(uint gpuId, int value);
    public int GetMaxGpuClock(uint gpuId);
}