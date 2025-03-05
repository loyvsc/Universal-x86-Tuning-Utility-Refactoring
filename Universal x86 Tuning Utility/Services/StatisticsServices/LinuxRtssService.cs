using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.StatisticsServices;

public class LinuxRtssService : IRtssService
{
    public int FpsLimit { get; set; }
    
    public void Start()
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public bool IsRTSSRunning()
    {
        throw new System.NotImplementedException();
    }
}