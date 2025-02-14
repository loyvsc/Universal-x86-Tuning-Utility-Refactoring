namespace ApplicationCore.Models;

public class XgMobileStatusEventArgs
{
    public bool Connected { get; init; }
    public bool Detected { get; init; }
    public bool DetectedChanged { get; init; }
    public bool ConnectedChanged { get; init; }
}