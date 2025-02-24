using System.Drawing;
using ApplicationCore.Enums;

namespace ApplicationCore.Interfaces;

public interface IASUSWmiService : IDisposable
{
    public void RunListener();
    public byte[] DeviceInit();
    public int DeviceSet(AsusDevice device, int newValue);
    public int DeviceSet(AsusDevice device, byte[] values);
    public int DeviceGet(AsusDevice deviceId);
    public AsusMode GetPerformanceMode();
    public void SetPerformanceMode(AsusMode newMode);
    public void SetGPUEco(bool eco);
    public int GetFan(AsusFan device);
    public void SetFanRange(AsusFan device, byte[] curve);
    public void SetFanCurve(AsusFan device, byte[] curve);
    public byte[] GetFanCurve(AsusFan device, int mode = 0);
    public bool IsInvalidCurve(byte[] curve);
    public byte[] FixFanCurve(byte[] curve);
    public bool IsXGConnected();
    public bool IsAllAmdPPT();
    public void ScanRange();
    public void TUFKeyboardBrightness(int brightness);
    public void TUFKeyboardRGB(int mode, Color color, int speed);
    public void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false);
    public void SubscribeToEvents(Action<object, EventArgs> eventHandler);
}