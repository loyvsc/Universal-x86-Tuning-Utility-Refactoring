namespace ApplicationCore.Interfaces;

public interface ICpuControlService
{
    /// <summary>
    /// Current power limit in watts
    /// </summary>
    public int CurrentPowerLimit { get; }

    public string CpuCommand { get; }
    public string CoCommand { get; }

    public void UpdatePowerLimit(int temperature,
        int cpuLoad,
        int maxPowerLimit,
        int minPowerLimit,
        int maxTemperature);

    public void CurveOptimiserLimit(int cpuLoad, int maxCurveOptimiser);
}