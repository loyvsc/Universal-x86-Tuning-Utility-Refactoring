using System;
using System.Drawing;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.Asus;

public class LinuxAsusWmiService : IASUSWmiService
{
    public void RunListener()
    {
        throw new NotImplementedException();
    }

    public byte[] DeviceInit()
    {
        throw new NotImplementedException();
    }

    public int DeviceSet(AsusDevice device, int newValue)
    {
        throw new NotImplementedException();
    }

    public int DeviceSet(AsusDevice device, byte[] values)
    {
        throw new NotImplementedException();
    }

    public int DeviceGet(AsusDevice deviceId)
    {
        throw new NotImplementedException();
    }

    public AsusMode GetPerformanceMode()
    {
        throw new NotImplementedException();
    }

    public void SetPerformanceMode(AsusMode newMode)
    {
        throw new NotImplementedException();
    }

    public void SetGPUEco(bool eco)
    {
        throw new NotImplementedException();
    }

    public int GetFan(AsusFan device)
    {
        throw new NotImplementedException();
    }

    public void SetFanRange(AsusFan device, byte[] curve)
    {
        throw new NotImplementedException();
    }

    public void SetFanCurve(AsusFan device, byte[] curve)
    {
        throw new NotImplementedException();
    }

    public byte[] GetFanCurve(AsusFan device, int mode = 0)
    {
        throw new NotImplementedException();
    }

    public bool IsInvalidCurve(byte[] curve)
    {
        throw new NotImplementedException();
    }

    public byte[] FixFanCurve(byte[] curve)
    {
        throw new NotImplementedException();
    }

    public bool IsXGConnected()
    {
        throw new NotImplementedException();
    }

    public bool IsAllAmdPPT()
    {
        throw new NotImplementedException();
    }

    public void ScanRange()
    {
        throw new NotImplementedException();
    }

    public void TUFKeyboardBrightness(int brightness)
    {
        throw new NotImplementedException();
    }

    public void TUFKeyboardRGB(int mode, Color color, int speed)
    {
        throw new NotImplementedException();
    }

    public void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false)
    {
        throw new NotImplementedException();
    }

    public void SubscribeToEvents(Action<object, EventArgs> eventHandler)
    {
        throw new NotImplementedException();
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }
}