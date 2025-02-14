using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.GPUs.NVIDIA;

public class LinuxNvidiaGpuService : INvidiaGpuService
{
    public void SetClocks(int core, int memory, int coreVoltage = 0)
    {
        throw new System.NotImplementedException();
    }

    public int MaxGpuClock { get; set; }
}