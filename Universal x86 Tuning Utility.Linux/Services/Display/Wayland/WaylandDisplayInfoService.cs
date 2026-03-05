using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Serilog;
using Universal_x86_Tuning_Utility.Linux.Services.Display.Wayland.Compositors;

namespace Universal_x86_Tuning_Utility.Linux.Services.Display.Wayland;

public class WaylandDisplayInfoService : IDisplayInfoService
{
    private IWaylandCompositor? _compositor;
    private string? _currentCompositorIdentifier;
    
    private readonly ILogger _logger;
    private readonly List<ApplicationCore.Models.Display> _displays = new List<ApplicationCore.Models.Display>();
    
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public event DisplayAttachedEventHandler? DisplayAttached;
    public event DisplayRemovedEventHandler? DisplayRemoved;

    public IReadOnlyCollection<ApplicationCore.Models.Display> Displays => _displays;

    public WaylandDisplayInfoService(ILogger logger)
    {
        _logger = logger;

        DetermineCompositor();
    }

    public void ApplySettings(ApplicationCore.Models.Display targetDisplay, DisplayResolution targetDisplayResolution, int targetHz)
    {
        if (_compositor == null)
            throw new Exception("Service not initialized");
        
        _compositor.ApplyDisplaySettings(targetDisplay.Identifier, targetDisplayResolution.Width, targetDisplayResolution.Height, targetHz);

        targetDisplay.UpdateCurrentResolution(targetDisplayResolution, targetHz);
        
        _logger.Information("Display settings applied");
    }

    public void ApplySettings(string targetDisplayIdentifier, int targetHz)
    {
        var display = _displays.FirstOrDefault(x => x.Identifier == targetDisplayIdentifier);
        if (display == null)
            throw new ArgumentException("Display not found", nameof(targetDisplayIdentifier));
        
        ApplySettings(display, display.CurrentResolution, targetHz);
    }

    private void DetermineCompositor()
    {
        string compositor = Environment.GetEnvironmentVariable("XDG_CURRENT_DESKTOP") ?? "";

        if (compositor == _currentCompositorIdentifier)
            return;

        if (_compositor != null)
        {
            _compositor.DisplayInfoChanged -= CompositorOnDisplayInfoChanged;
        }

        if (compositor.Contains("GNOME", StringComparison.OrdinalIgnoreCase))
        {
            _compositor = new GnomeCompositor();
        }
        else if (compositor.Contains("KDE", StringComparison.OrdinalIgnoreCase))
        {
            _compositor = new KdeCompositor();
        }
        else
        {
            _compositor = new WlrRandrCompositor();
        }
        
        _currentCompositorIdentifier = compositor;
        
        _compositor.DisplayInfoChanged += CompositorOnDisplayInfoChanged;
        
        _displays.Clear();
        _displays.AddRange(_compositor.GetDisplayInfo());
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, Displays));
    }

    private void CompositorOnDisplayInfoChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            var newDisplays = e.NewItems?.OfType<ApplicationCore.Models.Display>();

            if (newDisplays != null)
            {
                foreach (var newDisplay in newDisplays)
                {
                    _displays.Add(newDisplay);
                    DisplayAttached?.Invoke(newDisplay);
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            var removedDisplays = e.NewItems?.OfType<ApplicationCore.Models.Display>();

            if (removedDisplays != null)
            {
                foreach (var removedDisplay in removedDisplays)
                {
                    _displays.Remove(removedDisplay);
                    DisplayRemoved?.Invoke(removedDisplay);
                }
            }
        }
    }
}