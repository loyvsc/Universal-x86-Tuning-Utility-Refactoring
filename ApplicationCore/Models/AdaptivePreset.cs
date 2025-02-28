using System.ComponentModel;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class AdaptivePreset : NotifyPropertyChangedBase
{
    private int _temp;
    private int _power;
    private int _co;
    private int _minGgx;
    private int _maxGfx;
    private int _minCpuClock;
    private bool _isCo;
    private bool _isGfx;
    private int _rsr;
    private int _boost;
    private int _imageSharp;
    private bool _isRadeonGraphics;
    private bool _isAntiLag;
    private bool _isRsr;
    private bool _isBoost;
    private bool _isImageSharp;
    private bool _isSync;
    private bool _isNvidia;
    private int _nvMaxCoreClock = 4000;
    private int _nvCoreClock;
    private int _nvMemClock;
    private int _asusPowerProfile;
    private bool _isMag;
    private bool _isVsync;
    private bool _isRecap;
    private int _sharpness;
    private int _resScaleIndex;
    private bool _isAutoSwitch;

    public int Temp
    {
        get => _temp;
        set => SetValue(ref _temp, value);
    }

    public int Power
    {
        get => _power;
        set => SetValue(ref _power, value);
    }

    public int Co
    {
        get => _co;
        set => SetValue(ref _co, value);
    }

    public int MinGgx
    {
        get => _minGgx;
        set => SetValue(ref _minGgx, value);
    }

    public int MaxGfx
    {
        get => _maxGfx;
        set => SetValue(ref _maxGfx, value);
    }

    public int MinCpuClock
    {
        get => _minCpuClock;
        set => SetValue(ref _minCpuClock, value);
    }

    public bool IsCo
    {
        get => _isCo;
        set => SetValue(ref _isCo, value);
    }

    public bool IsGfx
    {
        get => _isGfx;
        set => SetValue(ref _isGfx, value);
    }

    public int Rsr
    {
        get => _rsr;
        set => SetValue(ref _rsr, value);
    }

    public int Boost
    {
        get => _boost;
        set => SetValue(ref _boost, value);
    }

    public int ImageSharp
    {
        get => _imageSharp;
        set => SetValue(ref _imageSharp, value);
    }

    public bool IsRadeonGraphics
    {
        get => _isRadeonGraphics;
        set => SetValue(ref _isRadeonGraphics, value);
    }

    public bool IsAntiLag
    {
        get => _isAntiLag;
        set => SetValue(ref _isAntiLag, value, () =>
        {
            if (value)
            {
                IsBoost = false;
            }
        });
    }

    public bool IsRsr
    {
        get => _isRsr;
        set => SetValue(ref _isRsr, value, () =>
        {
            if (value)
            {
                IsBoost = false;
                IsImageSharp = false;
            }
        });
    }

    public bool IsBoost
    {
        get => _isBoost;
        set => SetValue(ref _isBoost, value, () =>
        {
            if (value)
            {
                IsRsr = false;
                IsAntiLag = false;
            }
        });
    }

    public bool IsImageSharp
    {
        get => _isImageSharp;
        set => SetValue(ref _isImageSharp, value, () =>
        {
            if (value)
            {
                IsRsr = false;
            }
        });
    }

    public bool IsSync
    {
        get => _isSync;
        set => SetValue(ref _isSync, value);
    }

    public bool IsNvidia
    {
        get => _isNvidia;
        set => SetValue(ref _isNvidia, value);
    }

    public int NvMaxCoreClock
    {
        get => _nvMaxCoreClock;
        set => SetValue(ref _nvMaxCoreClock, value);
    }

    public int NvCoreClock
    {
        get => _nvCoreClock;
        set => SetValue(ref _nvCoreClock, value);
    }

    public int NvMemClock
    {
        get => _nvMemClock;
        set => SetValue(ref _nvMemClock, value);
    }

    public int AsusPowerProfile
    {
        get => _asusPowerProfile;
        set => SetValue(ref _asusPowerProfile, value);
    }

    public bool IsMag
    {
        get => _isMag;
        set => SetValue(ref _isMag, value);
    }

    public bool IsVsync
    {
        get => _isVsync;
        set => SetValue(ref _isVsync, value);
    }

    public bool IsRecap
    {
        get => _isRecap;
        set => SetValue(ref _isRecap, value);
    }

    public int Sharpness
    {
        get => _sharpness;
        set => SetValue(ref _sharpness, value);
    }

    public int ResScaleIndex
    {
        get => _resScaleIndex;
        set => SetValue(ref _resScaleIndex, value);
    }

    [DefaultValue(true)]
    public bool IsAutoSwitch
    {
        get => _isAutoSwitch;
        set => SetValue(ref _isAutoSwitch, value);
    }
}