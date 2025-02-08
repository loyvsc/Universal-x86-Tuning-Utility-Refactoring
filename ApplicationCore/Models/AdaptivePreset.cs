using System.ComponentModel;

namespace ApplicationCore.Models;

public class AdaptivePreset
{
    public int Temp { get; set; }
    public int Power { get; set; }
    public int CO { get; set; }
    public int MinGFX { get; set; }
    public int MaxGFX { get; set; }
    public int MinCPU { get; set; }
    public bool IsCO { get; set; }
    public bool IsGFX { get; set; }

    public int Rsr { get; set; }
    public int Boost { get; set; }
    public int ImageSharp { get; set; }

    public bool IsRadeonGraphics { get; set; }
    public bool IsAntiLag { get; set; }
    public bool IsRSR { get; set; }
    public bool IsBoost { get; set; }
    public bool IsImageSharp { get; set; }
    public bool IsSync { get; set; }

    public bool IsNVIDIA { get; set; }
    public int NvMaxCoreClock { get; set; } = 4000;
    public int NvCoreClock { get; set; }
    public int NvMemClock { get; set; }

    public int AsusPowerProfile { get; set; }

    public bool IsMag { get; set; }
    public bool IsVsync { get; set; }
    public bool IsRecap { get; set; }
    public int Sharpness { get; set; }
    public int ResScaleIndex { get; set; }

    [DefaultValue(true)]
    public bool IsAutoSwitch { get; set; }
}