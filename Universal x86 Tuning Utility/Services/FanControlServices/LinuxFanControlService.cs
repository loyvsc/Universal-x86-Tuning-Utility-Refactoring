using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.FanControlServices;

public class LinuxFanControlService : IFanControlService
{
    public int MaxFanSpeed { get; }
    public int MinFanSpeed { get; }
    public int MinFanSpeedPercentage { get; }
    public double FanSpeed { get; }
    public bool IsFanControlEnabled { get; }
    public bool IsFanEnabled { get; }
    
    public void UpdateAddresses()
    {
        throw new System.NotImplementedException();
    }

    public void EnableFanControl()
    {
        throw new System.NotImplementedException();
    }

    public void DisableFanControl()
    {
        throw new System.NotImplementedException();
    }

    public void SetFanSpeed(int speedPercentage)
    {
        throw new System.NotImplementedException();
    }

    public void ReadFanSpeed()
    {
        throw new System.NotImplementedException();
    }
}