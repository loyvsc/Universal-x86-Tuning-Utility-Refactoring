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
        _sb.Append(scale);
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

    public void AddNvidiaClocks(int maxCoreClock, int coreClock, int memClock)
    {
        if (_disposed)
        {
            return;
        }

        _sb.Append("--NVIDIA-Clocks=");
        _sb.Append(maxCoreClock);
        _sb.Append('-');
        _sb.Append(coreClock);
        _sb.Append('-');
        _sb.Append(memClock);
        _sb.Append('-');
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