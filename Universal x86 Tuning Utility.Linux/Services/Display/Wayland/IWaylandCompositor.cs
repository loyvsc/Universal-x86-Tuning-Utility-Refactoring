using System.Collections.Generic;
using System.Collections.Specialized;

namespace Universal_x86_Tuning_Utility.Linux.Services.Display.Wayland;

public interface IWaylandCompositor
{
    public void ApplyDisplaySettings(string identifier, int width, int height, int hz);

    public IReadOnlyList<ApplicationCore.Models.Display> GetDisplayInfo();
    
    public event NotifyCollectionChangedEventHandler? DisplayInfoChanged;
}