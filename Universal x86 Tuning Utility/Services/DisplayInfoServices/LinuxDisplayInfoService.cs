using System;
using System.Collections.Generic;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Services.DisplayInfoServices;

public class LinuxDisplayInfoService : IDisplayInfoService
{
    public event DisplayAttachedEventHandler? DisplayAttached;
    public event DisplayRemovedEventHandler? DisplayRemoved;
    public Lazy<IReadOnlyCollection<Display>> Displays { get; }
    public void ApplySettings(Display targetDisplay, DisplayResolution targetDisplayResolution, int targetHz)
    {
        throw new NotImplementedException();
    }

    public void ApplySettings(string targetDisplayIdentifier, int targetHz)
    {
        throw new NotImplementedException();
    }
}