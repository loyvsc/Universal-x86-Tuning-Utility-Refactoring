namespace ApplicationCore.Interfaces;

public interface ICpuAffinityService : IDisposable
{
    public void SetGlobalAffinity(int mode);
    public uint GetActiveProcessorsCount(uint groupNumber);
    public uint GetAllGroupsActiveProcessorsCount();
    public void Stop();
}