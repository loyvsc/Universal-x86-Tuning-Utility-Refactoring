using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public delegate void DisplayAttachedEventHandler(Display display);
public delegate void DisplayRemovedEventHandler(Display display);

public interface IDisplayInfoService
{
    public event DisplayAttachedEventHandler? DisplayAttached;
    public event DisplayRemovedEventHandler? DisplayRemoved;
    
    public Lazy<IReadOnlyCollection<Display>> Displays { get; }
    
    public void ApplySettings(Display targetDisplay, DisplayResolution targetDisplayResolution, int targetHz);
    public void ApplySettings(string targetDisplayIdentifier, int targetHz);
}