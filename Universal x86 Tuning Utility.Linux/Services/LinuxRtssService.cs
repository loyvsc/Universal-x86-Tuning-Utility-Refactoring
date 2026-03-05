using System.Collections.Generic;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Linux.Services;

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

    public IEnumerable<ApplicationRenderInfo> GetApplicationRenderInfo()
    {
        throw new System.NotImplementedException();
    }
}