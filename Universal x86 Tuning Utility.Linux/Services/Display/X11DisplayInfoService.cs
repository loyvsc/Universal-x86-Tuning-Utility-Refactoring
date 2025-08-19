using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Enums.Display;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Serilog;
using X11;

namespace Universal_x86_Tuning_Utility.Linux.Services.Display;

public class X11DisplayInfoService : IDisplayInfoService
{
    public event DisplayAttachedEventHandler? DisplayAttached;
    public event DisplayRemovedEventHandler? DisplayRemoved;
    public Lazy<IReadOnlyCollection<ApplicationCore.Models.Display>> Displays { get; }

    private readonly ILogger _logger;
    
    public X11DisplayInfoService(ILogger logger)
    {
        _logger = logger;
        Displays = new Lazy<IReadOnlyCollection<ApplicationCore.Models.Display>>(() => GetDisplays().AsReadOnly());
    }
    
    private IList<ApplicationCore.Models.Display> GetDisplays()
    {
        var displays = new List<ApplicationCore.Models.Display>();

        var x11DisplayHandle = Xlib.XOpenDisplay(null);
        int screenCount = Xlib.XScreenCount(x11DisplayHandle);

        for (int i = 0; i < screenCount; i++)
        {
            ref var screen = ref Xlib.XScreenOfDisplay(x11DisplayHandle, i);
            
            // var identifier = displayDevice.DevicePath;
            // var supportedResolutions = new List<DisplayResolution>();
            // var supportedRefreshRates = new HashSet<int>();
            //         
            // if (displayDevice.IsAvailable)
            // {
            //     foreach (var possibleSetting in displayDevice.GetPossibleSettings())
            //     {
            //         var width = possibleSetting.Resolution.Width;
            //         var height = possibleSetting.Resolution.Height;
            //         var frequency = possibleSetting.Frequency;
            //         
            //         if (supportedResolutions.FirstOrDefault(x => x.Width == width && x.Height == height) == null)
            //         {
            //             supportedResolutions.Add(new DisplayResolution(width, height));
            //         }
            //         supportedRefreshRates.Add(frequency);
            //     }
            //             
            //     if (supportedResolutions.Count != 0)
            //     {
            //         var connectionInfo = pathInfo.TargetsInfo.FirstOrDefault(x => x.DisplayTarget.DevicePath == displayDevice.DevicePath);
            //                 
            //         if (connectionInfo != null)
            //         {
            //             var currentWidth = connectionInfo.SignalInfo.ActiveSize.Width;
            //             var currentHeight = connectionInfo.SignalInfo.ActiveSize.Height;
            //             var currentRefreshRate = (int) connectionInfo.SignalInfo.VerticalSyncFrequencyInMillihertz / 1000;
            //             var outputTechnology = connectionInfo.OutputTechnology switch
            //             {
            //                 DisplayConfigVideoOutputTechnology.HD15 => DisplayOutputTechnology.VGA,
            //                 DisplayConfigVideoOutputTechnology.SVideo => DisplayOutputTechnology.SVideo,
            //                 DisplayConfigVideoOutputTechnology.CompositeVideo => DisplayOutputTechnology.Composite,
            //                 DisplayConfigVideoOutputTechnology.ComponentVideo => DisplayOutputTechnology.Component,
            //                 DisplayConfigVideoOutputTechnology.DVI => DisplayOutputTechnology.DVI,
            //                 DisplayConfigVideoOutputTechnology.HDMI => DisplayOutputTechnology.HDMI,
            //                 DisplayConfigVideoOutputTechnology.LVDS => DisplayOutputTechnology.LVDS,
            //                 DisplayConfigVideoOutputTechnology.DJPN => DisplayOutputTechnology.D_JPN,
            //                 DisplayConfigVideoOutputTechnology.SDI => DisplayOutputTechnology.SDI,
            //                 DisplayConfigVideoOutputTechnology.DisplayPortExternal => DisplayOutputTechnology.DisplayPort,
            //                 DisplayConfigVideoOutputTechnology.DisplayPortEmbedded => DisplayOutputTechnology.DisplayPort,
            //                 DisplayConfigVideoOutputTechnology.UDIExternal => DisplayOutputTechnology.UDI,
            //                 DisplayConfigVideoOutputTechnology.UDIEmbedded => DisplayOutputTechnology.UDI,
            //                 DisplayConfigVideoOutputTechnology.SDTVDongle => DisplayOutputTechnology.SDTV,
            //                 DisplayConfigVideoOutputTechnology.Miracast => DisplayOutputTechnology.Miracast,
            //                 DisplayConfigVideoOutputTechnology.Internal => DisplayOutputTechnology.Internal,
            //                 _ => DisplayOutputTechnology.Unknown
            //             };
            //         
            //             displays.Add(new ApplicationCore.Models.Display(identifier, 
            //                 name: displayDevice.DisplayName,
            //                 supportedResolutions, 
            //                 currentResolution: supportedResolutions.First(x => x.Width == currentWidth && x.Height == currentHeight), 
            //                 supportedRefreshRates, 
            //                 currentRefreshRate: currentRefreshRate + 1,
            //                 outputTechnology: outputTechnology));
            //         }
            //     }
            //             
            // }
            
            
        }
        
        return displays;
    }
    
    public void ApplySettings(ApplicationCore.Models.Display targetDisplay, DisplayResolution targetDisplayResolution, int targetHz)
    {
        throw new NotImplementedException();
    }

    public void ApplySettings(string targetDisplayIdentifier, int targetHz)
    {
        throw new NotImplementedException();
    }
}