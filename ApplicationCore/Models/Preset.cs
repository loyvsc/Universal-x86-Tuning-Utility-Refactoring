using System.ComponentModel;
using ApplicationCore.Utilities;

namespace ApplicationCore.Models;

public class Preset : NotifyPropertyChangedBase
{
    private int _apuTemperature;
    private int _apuSkinTemperature;
    private int _apuStapmPower;
    private int _apuStapmTime;
    private int _apuFastPower;
    private int _apuSlowPower;
    private int _apuSlowTime;
    private int _apuCpuTdc;
    private int _apuCpuEdc;
    private int _apuSocTdc;
    private int _apuSocEdc;
    private int _apuGfxTdc;
    private int _apuGfxEdc;
    private int _apuGfxClock;
    private int _pboScalar;
    private int _coAllCore;
    private int _coGfx;

    private int _dtCpuTemperature;
    private int _dtCpuPpt;
    private int _dtCpuTdc;
    private int _dtCpuEdc;

    private int _boostProfile;

    private int _intelPl1;
    private int _intelPl2;

    private bool _isIntelVoltages;
    private int _intelVoltageCpu;
    private int _intelVoltageGpu;
    private int _intelVoltageCache;
    private int _intelVoltageSa;

    private bool _isIntelBal;
    private int _intelBalCpu = 9;
    private int _intelBalGpu = 13;

    private bool _isIntelClockRatio;

    private int _intelClockRatioC1 = 20;
    private int _intelClockRatioC2 = 20;
    private int _intelClockRatioC3 = 20;
    private int _intelClockRatioC4 = 20;
    private int _intelClockRatioC5 = 20;
    private int _intelClockRatioC6 = 20;
    private int _intelClockRatioC7 = 20;
    private int _intelClockRatioC8 = 20;
    private int _rsr;
    private int _boost;
    private int _imageSharp;

    private int _ccd1Core1;
    private int _ccd1Core2;
    private int _ccd1Core3;
    private int _ccd1Core4;
    private int _ccd1Core5;
    private int _ccd1Core6;
    private int _ccd1Core7;
    private int _ccd1Core8;

    private int _ccd2Core1;
    private int _ccd2Core2;
    private int _ccd2Core3;
    private int _ccd2Core4;
    private int _ccd2Core5;
    private int _ccd2Core6;
    private int _ccd2Core7;
    private int _ccd2Core8;

    private int _nvMaxCoreClk = 4000;
    private int _nvCoreClk;
    private int _nvMemClk;

    private int _amdClock;
    private int _amdVid;

    private int _softMiniGpuClk;
    private int _softMaxiGpuClk;
    private int _softMinCpuClk;
    private int _softMaxCpuClk;
    private int _softMinDataClk;
    private int _softMaxDataClk;
    private int _softMinFabClk;
    private int _softMaxFabClk;
    private int _softMinVcnClk;
    private int _softMaxVcnClk;
    private int _softMinSoCClk;
    private int _softMaxSoCClk;
    private string _commandValue;

    private bool _isApuTemp;
    private bool _isApuSkinTemp;
    private bool _isApuStapmPow;
    private bool _isApuStapmTime;
    private bool _isApuFastPow;
    private bool _isApuSlowPow;
    private bool _isApuSlowTime;
    private bool _isApuCpuTdc;
    private bool _isApuCpuEdc;
    private bool _isApuSocTdc;
    private bool _isApuSocEdc;
    private bool _isApuGfxTdc;
    private bool _isApuGfxEdc;
    private bool _isApuGfxClk;
    private bool _isPboScalar;
    private bool _isCoAllCore;
    private bool _isCoGfx;

    private bool _isDtCpuTemp;
    private bool _isDtCpuPpt;
    private bool _isDtCpuTdc;
    private bool _isDtCpuEdc;

    private bool _isIntelPl1;
    private bool _isIntelPl2;

    private bool _isRadeonGraphics;
    private bool _isAntiLag;
    private bool _isRsr;
    private bool _isBoost;
    private bool _isImageSharp;
    private bool _isSync;
    private bool _isNvidia;
    private bool _isCcd1Core1;
    private bool _isCcd1Core2;
    private bool _isCcd1Core3;
    private bool _isCcd1Core4;
    private bool _isCcd1Core5;
    private bool _isCcd1Core6;
    private bool _isCcd1Core7;
    private bool _isCcd1Core8;

    private bool _isCcd2Core1;
    private bool _isCcd2Core2;
    private bool _isCcd2Core3;
    private bool _isCcd2Core4;
    private bool _isCcd2Core5;
    private bool _isCcd2Core6;
    private bool _isCcd2Core7;
    private bool _isCcd2Core8;

    private bool _isAmdOc;

    private bool _isSoftMiniGpuClk;
    private bool _isSoftMaxiGpuClk;
    private bool _isSoftMinCpuClk;
    private bool _isSoftMaxCpuClk;
    private bool _isSoftMinDataClk;
    private bool _isSoftMaxDataClk;
    private bool _isSoftMinFabClk;
    private bool _isSoftMaxFabClk;
    private bool _isSoftMinVcnClk;
    private bool _isSoftMaxVcnClk;
    private bool _isSoftMinSoCClk;
    private bool _isSoftMaxSoCClk;

    private int _asusPowerProfile;
    private bool _asusGpuUlti;
    private bool _asusIGpu;

    private int _displayHz;

    private int _powerMode;

    private bool _isMag;
    private bool _isVsync;
    private bool _isRecap;
    private int _sharpness;
    private int _resScaleIndex;
    
    private string _name;

    public string Name
    {
        get => _name;
        set => SetValue(ref _name, value);
    }

    public int ApuTemperature
    {
        get => _apuTemperature;
        set => SetValue(ref _apuTemperature, value);
    }

    public int ApuSkinTemperature
    {
        get => _apuSkinTemperature;
        set => SetValue(ref _apuSkinTemperature, value);
    }

    public int ApuStapmPower
    {
        get => _apuStapmPower;
        set => SetValue(ref _apuStapmPower, value);
    }

    public int ApuStapmTime
    {
        get => _apuStapmTime;
        set => SetValue(ref _apuStapmTime, value);
    }

    public int ApuFastPower
    {
        get => _apuFastPower;
        set => SetValue(ref _apuFastPower, value);
    }

    public int ApuSlowPower
    {
        get => _apuSlowPower;
        set => SetValue(ref _apuSlowPower, value);
    }

    public int ApuSlowTime
    {
        get => _apuSlowTime;
        set => SetValue(ref _apuSlowTime, value);
    }

    public int ApuCpuTdc
    {
        get => _apuCpuTdc;
        set => SetValue(ref _apuCpuTdc, value);
    }

    public int ApuCpuEdc
    {
        get => _apuCpuEdc;
        set => SetValue(ref _apuCpuEdc, value);
    }

    public int ApuSocTdc
    {
        get => _apuSocTdc;
        set => SetValue(ref _apuSocTdc, value);
    }

    public int ApuSocEdc
    {
        get => _apuSocEdc;
        set => SetValue(ref _apuSocEdc, value);
    }

    public int ApuGfxTdc
    {
        get => _apuGfxTdc;
        set => SetValue(ref _apuGfxTdc, value);
    }

    public int ApuGfxEdc
    {
        get => _apuGfxEdc;
        set => SetValue(ref _apuGfxEdc, value);
    }

    public int ApuGfxClock
    {
        get => _apuGfxClock;
        set => SetValue(ref _apuGfxClock, value);
    }

    public int PboScalar
    {
        get => _pboScalar;
        set => SetValue(ref _pboScalar, value);
    }

    public int CoAllCore
    {
        get => _coAllCore;
        set => SetValue(ref _coAllCore, value);
    }

    public int CoGfx
    {
        get => _coGfx;
        set => SetValue(ref _coGfx, value);
    }

    public int DtCpuTemperature
    {
        get => _dtCpuTemperature;
        set => SetValue(ref _dtCpuTemperature, value);
    }

    public int DtCpuPpt
    {
        get => _dtCpuPpt;
        set => SetValue(ref _dtCpuPpt, value);
    }

    public int DtCpuTdc
    {
        get => _dtCpuTdc;
        set => SetValue(ref _dtCpuTdc, value);
    }

    public int DtCpuEdc
    {
        get => _dtCpuEdc;
        set => SetValue(ref _dtCpuEdc, value);
    }

    // todo: refactor this. create enum for exmpl
    public int BoostProfile
    {
        get => _boostProfile;
        set => SetValue(ref _boostProfile, value);
    }

    public int IntelPl1
    {
        get => _intelPl1;
        set => SetValue(ref _intelPl1, value);
    }

    public int IntelPl2
    {
        get => _intelPl2;
        set => SetValue(ref _intelPl2, value);
    }

    public bool IsIntelVoltages
    {
        get => _isIntelVoltages;
        set => SetValue(ref _isIntelVoltages, value);
    }

    public int IntelVoltageCpu
    {
        get => _intelVoltageCpu;
        set => SetValue(ref _intelVoltageCpu, value);
    }

    public int IntelVoltageGpu
    {
        get => _intelVoltageGpu;
        set => SetValue(ref _intelVoltageGpu, value);
    }

    public int IntelVoltageCache
    {
        get => _intelVoltageCache;
        set => SetValue(ref _intelVoltageCache, value);
    }

    public int IntelVoltageSa
    {
        get => _intelVoltageSa;
        set => SetValue(ref _intelVoltageSa, value);
    }

    public bool IsIntelBal
    {
        get => _isIntelBal;
        set => SetValue(ref _isIntelBal, value);
    }

    public int IntelBalCpu
    {
        get => _intelBalCpu;
        set => SetValue(ref _intelBalCpu, value);
    }

    public int IntelBalGpu
    {
        get => _intelBalGpu;
        set => SetValue(ref _intelBalGpu, value);
    }

    public bool IsIntelClockRatio
    {
        get => _isIntelClockRatio;
        set => SetValue(ref _isIntelClockRatio, value);
    }

    public int IntelClockRatioC1
    {
        get => _intelClockRatioC1;
        set => SetValue(ref _intelClockRatioC1, value);
    }

    public int IntelClockRatioC2
    {
        get => _intelClockRatioC2;
        set => SetValue(ref _intelClockRatioC2, value);
    }

    public int IntelClockRatioC3
    {
        get => _intelClockRatioC3;
        set => SetValue(ref _intelClockRatioC3, value);
    }

    public int IntelClockRatioC4
    {
        get => _intelClockRatioC4;
        set => SetValue(ref _intelClockRatioC4, value);
    }

    public int IntelClockRatioC5
    {
        get => _intelClockRatioC5;
        set => SetValue(ref _intelClockRatioC5, value);
    }

    public int IntelClockRatioC6
    {
        get => _intelClockRatioC6;
        set => SetValue(ref _intelClockRatioC6, value);
    }

    public int IntelClockRatioC7
    {
        get => _intelClockRatioC7;
        set => SetValue(ref _intelClockRatioC7, value);
    }

    public int IntelClockRatioC8
    {
        get => _intelClockRatioC8;
        set => SetValue(ref _intelClockRatioC8, value);
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

    public List<AmdCcdCoreState> Ccd1States
    {
        get => _ccd1States;
        set => SetValue(ref _ccd1States, value);
    }

    private List<AmdCcdCoreState> _ccd1States;

    public List<AmdCcdCoreState> Ccd2States
    {
        get => _ccd2States;
        set => SetValue(ref _ccd2States, value);
    }

    private List<AmdCcdCoreState> _ccd2States;

    public int NvMaxCoreClk
    {
        get => _nvMaxCoreClk;
        set => SetValue(ref _nvMaxCoreClk, value);
    }

    public int NvCoreClk
    {
        get => _nvCoreClk;
        set => SetValue(ref _nvCoreClk, value);
    }

    public int NvMemClk
    {
        get => _nvMemClk;
        set => SetValue(ref _nvMemClk, value);
    }

    public int AmdClock
    {
        get => _amdClock;
        set => SetValue(ref _amdClock, value);
    }

    public int AmdVid
    {
        get => _amdVid;
        set => SetValue(ref _amdVid, value);
    }

    public int SoftMiniGpuClk
    {
        get => _softMiniGpuClk;
        set => SetValue(ref _softMiniGpuClk, value);
    }

    public int SoftMaxiGpuClk
    {
        get => _softMaxiGpuClk;
        set => SetValue(ref _softMaxiGpuClk, value);
    }

    public int SoftMinCpuClk
    {
        get => _softMinCpuClk;
        set => SetValue(ref _softMinCpuClk, value);
    }

    public int SoftMaxCpuClk
    {
        get => _softMaxCpuClk;
        set => SetValue(ref _softMaxCpuClk, value);
    }

    public int SoftMinDataClk
    {
        get => _softMinDataClk;
        set => SetValue(ref _softMinDataClk, value);
    }

    public int SoftMaxDataClk
    {
        get => _softMaxDataClk;
        set => SetValue(ref _softMaxDataClk, value);
    }

    public int SoftMinFabClk
    {
        get => _softMinFabClk;
        set => SetValue(ref _softMinFabClk, value);
    }

    public int SoftMaxFabClk
    {
        get => _softMaxFabClk;
        set => SetValue(ref _softMaxFabClk, value);
    }

    public int SoftMinVcnClk
    {
        get => _softMinVcnClk;
        set => SetValue(ref _softMinVcnClk, value);
    }

    public int SoftMaxVcnClk
    {
        get => _softMaxVcnClk;
        set => SetValue(ref _softMaxVcnClk, value);
    }

    public int SoftMinSoCClk
    {
        get => _softMinSoCClk;
        set => SetValue(ref _softMinSoCClk, value);
    }

    public int SoftMaxSoCClk
    {
        get => _softMaxSoCClk;
        set => SetValue(ref _softMaxSoCClk, value);
    }

    public string CommandValue
    {
        get => _commandValue;
        set => SetValue(ref _commandValue, value);
    }

    public bool IsApuTemp
    {
        get => _isApuTemp;
        set => SetValue(ref _isApuTemp, value);
    }

    public bool IsApuSkinTemp
    {
        get => _isApuSkinTemp;
        set => SetValue(ref _isApuSkinTemp, value);
    }

    public bool IsApuStapmPow
    {
        get => _isApuStapmPow;
        set => SetValue(ref _isApuStapmPow, value);
    }

    public bool IsApuStapmTime
    {
        get => _isApuStapmTime;
        set => SetValue(ref _isApuStapmTime, value);
    }

    public bool IsApuFastPow
    {
        get => _isApuFastPow;
        set => SetValue(ref _isApuFastPow, value);
    }

    public bool IsApuSlowPow
    {
        get => _isApuSlowPow;
        set => SetValue(ref _isApuSlowPow, value);
    }

    public bool IsApuSlowTime
    {
        get => _isApuSlowTime;
        set => SetValue(ref _isApuSlowTime, value);
    }

    public bool IsApuCpuTdc
    {
        get => _isApuCpuTdc;
        set => SetValue(ref _isApuCpuTdc, value);
    }

    public bool IsApuCpuEdc
    {
        get => _isApuCpuEdc;
        set => SetValue(ref _isApuCpuEdc, value);
    }

    public bool IsApuSocTdc
    {
        get => _isApuSocTdc;
        set => SetValue(ref _isApuSocTdc, value);
    }

    public bool IsApuSocEdc
    {
        get => _isApuSocEdc;
        set => SetValue(ref _isApuSocEdc, value);
    }

    public bool IsApuGfxTdc
    {
        get => _isApuGfxTdc;
        set => SetValue(ref _isApuGfxTdc, value);
    }

    public bool IsApuGfxEdc
    {
        get => _isApuGfxEdc;
        set => SetValue(ref _isApuGfxEdc, value);
    }

    public bool IsApuGfxClk
    {
        get => _isApuGfxClk;
        set => SetValue(ref _isApuGfxClk, value);
    }

    public bool IsPboScalar
    {
        get => _isPboScalar;
        set => SetValue(ref _isPboScalar, value);
    }

    public bool IsCoAllCore
    {
        get => _isCoAllCore;
        set => SetValue(ref _isCoAllCore, value);
    }

    public bool IsCoGfx
    {
        get => _isCoGfx;
        set => SetValue(ref _isCoGfx, value);
    }

    public bool IsDtCpuTemp
    {
        get => _isDtCpuTemp;
        set => SetValue(ref _isDtCpuTemp, value);
    }

    public bool IsDtCpuPpt
    {
        get => _isDtCpuPpt;
        set => SetValue(ref _isDtCpuPpt, value);
    }

    public bool IsDtCpuTdc
    {
        get => _isDtCpuTdc;
        set => SetValue(ref _isDtCpuTdc, value);
    }

    public bool IsDtCpuEdc
    {
        get => _isDtCpuEdc;
        set => SetValue(ref _isDtCpuEdc, value);
    }

    public bool IsIntelPl1
    {
        get => _isIntelPl1;
        set => SetValue(ref _isIntelPl1, value);
    }

    public bool IsIntelPl2
    {
        get => _isIntelPl2;
        set => SetValue(ref _isIntelPl2, value);
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
            IsBoost = false;
            IsImageSharp = false;
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

    public bool IsAmdOc
    {
        get => _isAmdOc;
        set => SetValue(ref _isAmdOc, value);
    }

    public bool IsSoftMiniGpuClk
    {
        get => _isSoftMiniGpuClk;
        set => SetValue(ref _isSoftMiniGpuClk, value);
    }

    public bool IsSoftMaxiGpuClk
    {
        get => _isSoftMaxiGpuClk;
        set => SetValue(ref _isSoftMaxiGpuClk, value);
    }

    public bool IsSoftMinCpuClk
    {
        get => _isSoftMinCpuClk;
        set => SetValue(ref _isSoftMinCpuClk, value);
    }

    public bool IsSoftMaxCpuClk
    {
        get => _isSoftMaxCpuClk;
        set => SetValue(ref _isSoftMaxCpuClk, value);
    }

    public bool IsSoftMinDataClk
    {
        get => _isSoftMinDataClk;
        set => SetValue(ref _isSoftMinDataClk, value);
    }

    public bool IsSoftMaxDataClk
    {
        get => _isSoftMaxDataClk;
        set => SetValue(ref _isSoftMaxDataClk, value);
    }

    public bool IsSoftMinFabClk
    {
        get => _isSoftMinFabClk;
        set => SetValue(ref _isSoftMinFabClk, value);
    }

    public bool IsSoftMaxFabClk
    {
        get => _isSoftMaxFabClk;
        set => SetValue(ref _isSoftMaxFabClk, value);
    }

    public bool IsSoftMinVcnClk
    {
        get => _isSoftMinVcnClk;
        set => SetValue(ref _isSoftMinVcnClk, value);
    }

    public bool IsSoftMaxVcnClk
    {
        get => _isSoftMaxVcnClk;
        set => SetValue(ref _isSoftMaxVcnClk, value);
    }

    public bool IsSoftMinSoCClk
    {
        get => _isSoftMinSoCClk;
        set => SetValue(ref _isSoftMinSoCClk, value);
    }

    public bool IsSoftMaxSoCClk
    {
        get => _isSoftMaxSoCClk;
        set => SetValue(ref _isSoftMaxSoCClk, value);
    }

    public int AsusPowerProfile
    {
        get => _asusPowerProfile;
        set => SetValue(ref _asusPowerProfile, value);
    }

    public bool AsusGpuUlti
    {
        get => _asusGpuUlti;
        set => SetValue(ref _asusGpuUlti, value);
    }

    public bool AsusIGpu
    {
        get => _asusIGpu;
        set => SetValue(ref _asusIGpu, value);
    }

    public int DisplayHz
    {
        get => _displayHz;
        set => SetValue(ref _displayHz, value);
    }

    public int PowerMode
    {
        get => _powerMode;
        set => SetValue(ref _powerMode, value);
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
}