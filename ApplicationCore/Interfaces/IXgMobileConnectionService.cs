using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IXgMobileConnectionService
{
    public bool Connected { get; }
    public bool Detected { get; }
    public event EventHandler<XgMobileStatusEventArgs>? XgMobileStatusChanged;
    public bool IsEGPUConnected();
    public void EnableXgMobileLight();
    public void DisableXgMobileLight();
    public bool SetXgMobileFan(List<AsusCurvePoint> points);
    public bool ResetXgMobileFan();
}