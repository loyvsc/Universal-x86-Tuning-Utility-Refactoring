namespace ApplicationCore.Interfaces;

public interface IDisplayInfoService
{
    public List<string> UniqueTargetScreenResolutions  { get; }
    public List<int> UniqueTargetRefreshRates { get; }
    
    public void ApplySettings(int newHz);
}