using System.Collections.ObjectModel;
using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface INvidiaGpuService
{
    /// <exception cref="AggregateException">Throws when no nvidia gpu installed or failed to set clocks</exception>
    public void SetClocks(int core, int memory, int coreVoltage = 0);

    public int MaxGpuClock { get; set; }
    
    public IReadOnlyCollection<CheckIsGpuOriginalResult> CheckIsGpusOriginal(); 
}