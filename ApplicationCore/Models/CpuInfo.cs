using ApplicationCore.Enums;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class CpuInfo : NotifyPropertyChangedBase
{
    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public int Family
    {
        get => _family;
        set => SetValue(ref _family, value);
    }

    public int Model
    {
        get => _model;
        set => SetValue(ref _model, value);
    }

    public int Stepping
    {
        get => _stepping;
        set => SetValue(ref _stepping, value);
    }

    public Manufacturer Manufacturer
    {
        get => _manufacturer;
        set => SetValue(ref _manufacturer, value);
    }

    public AmdProcessorType AmdProcessorType { get; set; } = AmdProcessorType.Unknown;
    public RyzenFamily RyzenFamily { get; set; } = RyzenFamily.Unknown;
    public RyzenGenerations RyzenGeneration { get; set; } = RyzenGenerations.Unknown;

    private string _name;
    private int _family;
    private int _model;
    private int _stepping;
    private Manufacturer _manufacturer;
}