using System.Drawing;
using ApplicationCore.Enums;

namespace ApplicationCore.Interfaces;

public interface IASUSWmiService : IDisposable
{
    public void RunListener();
    public byte[] DeviceInit();
    public int DeviceSet(uint deviceId, int status, string logName);
    public int DeviceSet(uint deviceId, byte[] Params, string logName);
    public int DeviceGet(uint deviceId);
    public void SetGPUEco(int eco);
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