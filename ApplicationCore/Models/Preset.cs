namespace ApplicationCore.Models;

public class Preset
{
    public int ApuTemperature { get; set; }
    public int ApuSkinTemperature { get; set; }
    public int ApuSTAPMPower { get; set; }
    public int ApuSTAPMTime { get; set; }
    public int ApuFastPower { get; set; }
    public int ApuSlowPower { get; set; }
    public int ApuSlowTime { get; set; }
    public int ApuCpuTdc { get; set; }
    public int ApuCpuEdc { get; set; }
    public int ApuSocTdc { get; set; }
    public int ApuSocEdc { get; set; }
    public int ApuGfxTdc { get; set; }
    public int ApuGfxEdc { get; set; }
    public int ApuGfxClock { get; set; }
    public int PboScalar { get; set; }
    public int CoAllCore { get; set; }
    public int CoGfx { get; set; }

    public int DtCpuTemperature { get; set; }
    public int DtCpuPPT { get; set; }
    public int DtCpuTDC { get; set; }
    public int DtCpuEDC { get; set; }

    public int BoostProfile { get; set; }

    public int IntelPL1 { get; set; }
    public int IntelPL2 { get; set; }

    public bool IsIntelVoltages { get; set; }
    public int IntelVoltageCPU { get; set; }
    public int IntelVoltageGPU { get; set; }
    public int IntelVoltageCache { get; set; }
    public int IntelVoltageSA { get; set; }

    public bool IsIntelBal { get; set; }
    public int IntelBalCPU { get; set; } = 9;
    public int IntelBalGPU { get; set; } = 13;

    public bool isIntelClockRatio { get; set; }

    public int intelClockRatioC1 { get; set; } = 20;
    public int intelClockRatioC2 { get; set; } = 20;

    public int IntelClockRatioC3 { get; set; } = 20;
    public int IntelClockRatioC4 { get; set; } = 20;
    public int intelClockRatioC5 { get; set; } = 20;
    public int intelClockRatioC6 { get; set; } = 20;
    public int intelClockRatioC7 { get; set; } = 20;
    public int intelClockRatioC8 { get; set; } = 20;
    public int rsr { get; set; }
    public int boost { get; set; }
    public int imageSharp { get; set; }

    public int ccd1Core1 { get; set; }
    public int ccd1Core2 { get; set; }
    public int ccd1Core3 { get; set; }
    public int ccd1Core4 { get; set; }
    public int ccd1Core5 { get; set; }
    public int ccd1Core6 { get; set; }
    public int ccd1Core7 { get; set; }
    public int ccd1Core8 { get; set; }

    public int ccd2Core1 { get; set; }
    public int ccd2Core2 { get; set; }
    public int ccd2Core3 { get; set; }
    public int ccd2Core4 { get; set; }
    public int ccd2Core5 { get; set; }
    public int ccd2Core6 { get; set; }
    public int ccd2Core7 { get; set; }
    public int ccd2Core8 { get; set; }

    public int nvMaxCoreClk { get; set; } = 4000;
    public int nvCoreClk { get; set; }
    public int nvMemClk { get; set; }

    public int amdClock { get; set; }
    public int amdVID { get; set; }

    public int softMiniGPUClk { get; set; }
    public int softMaxiGPUClk { get; set; }
    public int softMinCPUClk { get; set; }
    public int softMaxCPUClk { get; set; }
    public int softMinDataClk { get; set; }
    public int softMaxDataClk { get; set; }
    public int softMinFabClk { get; set; }
    public int softMaxFabClk { get; set; }
    public int softMinVCNClk { get; set; }
    public int softMaxVCNClk { get; set; }
    public int softMinSoCClk { get; set; }
    public int softMaxSoCClk { get; set; }
    public string commandValue { get; set; }

    public bool isApuTemp { get; set; }
    public bool isApuSkinTemp { get; set; }
    public bool isApuSTAPMPow { get; set; }
    public bool isApuSTAPMTime { get; set; }
    public bool isApuFastPow { get; set; }
    public bool isApuSlowPow { get; set; }
    public bool isApuSlowTime { get; set; }
    public bool isApuCpuTdc { get; set; }
    public bool isApuCpuEdc { get; set; }
    public bool isApuSocTdc { get; set; }
    public bool isApuSocEdc { get; set; }
    public bool isApuGfxTdc { get; set; }
    public bool isApuGfxEdc { get; set; }
    public bool isApuGfxClk { get; set; }
    public bool isPboScalar { get; set; }
    public bool isCoAllCore { get; set; }
    public bool isCoGfx { get; set; }

    public bool isDtCpuTemp { get; set; }
    public bool isDtCpuPPT { get; set; }
    public bool isDtCpuTDC { get; set; }
    public bool isDtCpuEDC { get; set; }

    public bool isIntelPL1 { get; set; }
    public bool isIntelPL2 { get; set; }

    public bool isRadeonGraphics { get; set; }
    public bool isAntiLag { get; set; }
    public bool isRSR { get; set; }
    public bool isBoost { get; set; }
    public bool isImageSharp { get; set; }
    public bool isSync { get; set; }
    public bool isNVIDIA { get; set; }
    public bool IsCCD1Core1 { get; set; }
    public bool IsCCD1Core2 { get; set; }
    public bool IsCCD1Core3 { get; set; }
    public bool IsCCD1Core4 { get; set; }
    public bool IsCCD1Core5 { get; set; }
    public bool IsCCD1Core6 { get; set; }
    public bool IsCCD1Core7 { get; set; }
    public bool IsCCD1Core8 { get; set; }

    public bool IsCCD2Core1 { get; set; }
    public bool IsCCD2Core2 { get; set; }
    public bool IsCCD2Core3 { get; set; }
    public bool IsCCD2Core4 { get; set; }
    public bool IsCCD2Core5 { get; set; }
    public bool IsCCD2Core6 { get; set; }
    public bool IsCCD2Core7 { get; set; }
    public bool IsCCD2Core8 { get; set; }

    public bool IsAmdOC { get; set; }

    public bool isSoftMiniGPUClk { get; set; }
    public bool isSoftMaxiGPUClk { get; set; }
    public bool isSoftMinCPUClk { get; set; }
    public bool isSoftMaxCPUClk { get; set; }
    public bool isSoftMinDataClk { get; set; }
    public bool isSoftMaxDataClk { get; set; }
    public bool isSoftMinFabClk { get; set; }
    public bool isSoftMaxFabClk { get; set; }
    public bool isSoftMinVCNClk { get; set; }
    public bool isSoftMaxVCNClk { get; set; }
    public bool isSoftMinSoCClk { get; set; }
    public bool isSoftMaxSoCClk { get; set; }

    public int asusPowerProfile { get; set; }
    public bool asusGPUUlti { get; set; }
    public bool asusiGPU { get; set; }

    public int displayHz { get; set; }

    public int powerMode { get; set; }

    public bool isMag { get; set; }
    public bool isVsync { get; set; }
    public bool isRecap { get; set; }
    public int Sharpness { get; set; }
    public int ResScaleIndex { get; set; }
}