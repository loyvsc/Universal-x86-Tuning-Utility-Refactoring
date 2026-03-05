using System.Collections.Generic;
using System.Collections.Specialized;

namespace Universal_x86_Tuning_Utility.Linux.Services.Display.Wayland.Compositors;

public class KdeCompositor : IWaylandCompositor
{
    public void ApplyDisplaySettings(string identifier, int width, int height, int hz)
    {
        throw new System.NotImplementedException();
    }

    public IReadOnlyList<ApplicationCore.Models.Display> GetDisplayInfo()
    {
        throw new System.NotImplementedException();
    }

    public event NotifyCollectionChangedEventHandler? DisplayInfoChanged;
}