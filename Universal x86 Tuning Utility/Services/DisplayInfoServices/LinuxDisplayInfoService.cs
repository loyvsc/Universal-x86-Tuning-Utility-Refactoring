using System.Collections.Generic;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.DisplayInfoServices;

public class LinuxDisplayInfoService : IDisplayInfoService
{
    public List<string> UniqueTargetScreenResolutions { get; }
    public List<int> UniqueTargetRefreshRates { get; }
    
    public void ApplySettings(int newHz)
    {
        throw new System.NotImplementedException();
    }
}