namespace ApplicationCore.Models;

public class AsusCurvePoint
{
    public int Temperature
    {
        get => _temperature;
        set => _temperature = Math.Max(Math.Min(value, 110), 0);
    }
    
    public int Fan
    {
        get => _fan;
        set => _fan = Math.Max(Math.Min(value, 100), 0);
    }
    
    private int _temperature;
    private int _fan;
    
}