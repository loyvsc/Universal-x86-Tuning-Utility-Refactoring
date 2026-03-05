using System;
using System.Text;
using ApplicationCore.Enums;
using ApplicationCore.Utilities;

namespace Universal_x86_Tuning_Utility.Helpers;

public class RyzenAdjCommandBuilder : IDisposable
{
    private bool _disposed;
    
    private readonly StringBuilder _sb;
    private const char CommandSeparator = ' ';

    public RyzenAdjCommandBuilder()
    {
        _sb = StringBuilderPool.Rent();
    }

    public void AddSuperResolution(bool isEnabled, bool isVsync, int sharpness, ResolutionScale scale,
        bool isRecap)
    {
        if (_disposed)
        {
            return;
        }
        
        _sb.Append("--UXTUSR=");
        _sb.Append(isEnabled);
        _sb.Append('-');
        _sb.Append(isVsync);
        _sb.Append('-');
        _sb.Append(sharpness);
        _sb.Append('-');
        _sb.Append((int)scale);
        _sb.Append('-');
        _sb.Append(isRecap);
        _sb.Append(CommandSeparator);
    }

    public void AddAsusPowerProfile(int asusPowerProfile)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ASUS-Power=");
        _sb.Append(asusPowerProfile);
        _sb.Append(CommandSeparator);
    }

    public void AddAsusEcoProfile(bool value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ASUS-Eco=");
        _sb.Append(value);
        _sb.Append(CommandSeparator);
    }

    public void AddAsusMuxProfile(bool value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ASUS-MUX=");
        _sb.Append(value);
        _sb.Append(CommandSeparator);
    }

    public void AddGfxClock(int gfxClock)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--gfx-clk=");
        _sb.Append(gfxClock);
        _sb.Append(CommandSeparator);
    }

    public void AddADLXLag(bool isEnabled)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ADLX-Lag=0-");
        _sb.Append(isEnabled.ToString());
        _sb.Append(CommandSeparator);
        _sb.Append("--ADLX-Lag=1-");
        _sb.Append(isEnabled.ToString());
        _sb.Append(CommandSeparator);
    }

    public void AddALDXRsr(bool isEnabled)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ADLX-RSR=0-");
        _sb.Append(isEnabled.ToString());
        _sb.Append(CommandSeparator);
        _sb.Append("--ADLX-RSR=1-");
        _sb.Append(isEnabled.ToString());
        _sb.Append(CommandSeparator);
    }

    public void AddADLXBoost(bool isEnabled, int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ADLX-Boost=0-");
        _sb.Append(isEnabled.ToString());
        _sb.Append('-');
        _sb.Append(value);
        _sb.Append(CommandSeparator);
        _sb.Append("--ADLX-Boost=1-");
        _sb.Append(isEnabled.ToString());
        _sb.Append('-');
        _sb.Append(value);
        _sb.Append(CommandSeparator);
    }

    public void AddADLXSync(bool isEnabled)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ADLX-Sync=0-");
        _sb.Append(isEnabled.ToString());
        _sb.Append(CommandSeparator);
        _sb.Append("--ADLX-Sync=1-");
        _sb.Append(isEnabled.ToString());
        _sb.Append(CommandSeparator);
    }

    public void AddImageSharp(bool isEnabled, int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--ADLX-ImageSharp=0-");
        _sb.Append(isEnabled.ToString());
        _sb.Append('-');
        _sb.Append(value);
        _sb.Append(CommandSeparator);
        _sb.Append("--ADLX-ImageSharp=1-");
        _sb.Append(isEnabled.ToString());
        _sb.Append('-');
        _sb.Append(value);
    }

    public void AddCustomCommand(string command)
    {
        if (_disposed)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(command))
        {
            _sb.Append(command);
            _sb.Append(CommandSeparator);
        }
    }

    public void AddNvidiaClocks(uint id, int maxCoreClock, int coreClock, int memClock)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--NVIDIA-Clocks=");
        _sb.Append(id);
        _sb.Append('-');
        _sb.Append(maxCoreClock);
        _sb.Append('-');
        _sb.Append(coreClock);
        _sb.Append('-');
        _sb.Append(memClock);
        _sb.Append('-');
        _sb.Append(CommandSeparator);
    }

    public void AddRefreshRate(string displayIdentifier, int displayHz)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--Refresh-Rate=");
        _sb.Append(displayIdentifier);
        _sb.Append(":::");
        _sb.Append(displayHz);
        _sb.Append(CommandSeparator);
    }

    public void AddPowerPlan(PowerPlan powerPlan)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--Win-Power=");
        _sb.Append(powerPlan);
        _sb.Append(CommandSeparator);
    }
    
    public void AddTctlTemp(int temp)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--tctl-temp=");
        _sb.Append(temp);
        _sb.Append(CommandSeparator);

        _sb.Append("--cHTC-temp=");
        _sb.Append(temp);
        _sb.Append(CommandSeparator);
    }

    public void AddApuSkinTemp(int temp)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--apu-skin-temp=");
        _sb.Append(temp);
        _sb.Append(CommandSeparator);
    }

    public void AddStapmLimit(int watts)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--stapm-limit=");
        _sb.Append(watts * 1000);
        _sb.Append(CommandSeparator);
    }

    public void AddFastLimit(int watts)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--fast-limit=");
        _sb.Append(watts * 1000);
        _sb.Append(CommandSeparator);
    }

    public void AddSlowLimit(int watts)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--slow-limit=");
        _sb.Append(watts * 1000);
        _sb.Append(CommandSeparator);
    }

    public void AddStapmTime(int seconds)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--stapm-time=");
        _sb.Append(seconds);
        _sb.Append(CommandSeparator);
    }

    public void AddSlowTime(int seconds)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--slow-time=");
        _sb.Append(seconds);
        _sb.Append(CommandSeparator);
    }

    public void AddCpuTdc(int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--vrm-current=");
        _sb.Append(value * 1000);
        _sb.Append(CommandSeparator);
    }

    public void AddCpuEdc(int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--vrmmax-current=");
        _sb.Append(value * 1000);
        _sb.Append(CommandSeparator);
    }

    public void AddSocTdc(int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--vrmsoc-current=");
        _sb.Append(value * 1000);
        _sb.Append(CommandSeparator);
    }

    public void AddSocEdc(int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--vrmsocmax-current=");
        _sb.Append(value * 1000);
        _sb.Append(CommandSeparator);
    }

    public void AddPboScalar(int scalar)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--pbo-scalar=");
        _sb.Append(scalar * 100);
        _sb.Append(CommandSeparator);
    }

    public void AddCoAll(int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--set-coall=");

        if (value >= 0)
        {
            _sb.Append(value);
        }
        else
        {
            _sb.Append(Convert.ToUInt32(0x100000 - (uint)(-value)));
        }

        _sb.Append(CommandSeparator);
    }

    public void AddCoGfx(int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--set-cogfx=");

        if (value >= 0)
        {
            _sb.Append(value);
        }
        else
        {
            _sb.Append(Convert.ToUInt32(0x100000 - (uint)(-value)));
        }

        _sb.Append(CommandSeparator);
    }

    public void AddCoPerCore(int ccd, int coreIndex, int coresCount, int value)
    {
        if (_disposed)
        {
            return;
        }

        uint encoded =
            (uint)((((((ccd << 4) | (0 & 15)) << 4) | ((coreIndex % coresCount) & 15)) << 20)
            | (value & 0xFFFF));

        _sb.Append("--set-coper=");
        _sb.Append(encoded);
        _sb.Append(CommandSeparator);
    }

    public void AddBoostProfile(AmdBoostProfile profile)
    {
        if (_disposed)
        {
            return;
        }

        switch (profile)
        {
            case AmdBoostProfile.PowerSave:
                _sb.Append("--power-saving ");
                break;

            case AmdBoostProfile.Performance:
                _sb.Append("--max-performance ");
                break;
        }

        _sb.Append(CommandSeparator);
    }

    public void AddAmdOc(int clock, double voltage)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--oc-clk=");
        _sb.Append(clock);
        _sb.Append(CommandSeparator);

        _sb.Append("--oc-clk=");
        _sb.Append(clock);
        _sb.Append(CommandSeparator);

        _sb.Append("--oc-volt=");
        _sb.Append(voltage);
        _sb.Append(CommandSeparator);

        _sb.Append("--oc-volt=");
        _sb.Append(voltage);
        _sb.Append(CommandSeparator);

        _sb.Append("--enable-oc ");
        _sb.Append(CommandSeparator);
    }

    public void AddLimit(string name, int value)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--");
        _sb.Append(name);
        _sb.Append('=');
        _sb.Append(value * 1000);
        _sb.Append(CommandSeparator);
    }

    public string Build()
    {
        if (_disposed)
        {
            return string.Empty;
        }

        return _sb.ToString();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            StringBuilderPool.Return(_sb);
            _disposed = true;
        }
    }
}