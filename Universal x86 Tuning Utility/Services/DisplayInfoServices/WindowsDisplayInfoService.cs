using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using ApplicationCore.Enums.Display;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using DynamicData;
using Microsoft.Extensions.Logging;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native.DisplayConfig;
using Display = ApplicationCore.Models.Display;

namespace Universal_x86_Tuning_Utility.Services.DisplayInfoServices;

public class WindowsDisplayInfoService : IDisplayInfoService, IDisposable
{
    public event DisplayAttachedEventHandler? DisplayAttached;
    public event DisplayRemovedEventHandler? DisplayRemoved;
    public Lazy<IReadOnlyCollection<Display>> Displays  { get; private set; }
    
    private readonly ILogger<WindowsDisplayInfoService> _logger;
    private readonly ManagementEventWatcher _installDeviceEventWatcher;
    private readonly ManagementEventWatcher _uninstallDeviceEventWatcher;

    public WindowsDisplayInfoService(ILogger<WindowsDisplayInfoService> logger)
    {
        _logger = logger;
        Displays = new Lazy<IReadOnlyCollection<Display>>(() => GetDisplays().AsReadOnly());
        
        _installDeviceEventWatcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2");
        _installDeviceEventWatcher.EventArrived += OnNewDeviceInstalled;
        _uninstallDeviceEventWatcher = new ManagementEventWatcher("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3");
        _uninstallDeviceEventWatcher.EventArrived += OnDeviceUninstalled;
    }

    private void OnNewDeviceInstalled(object sender, EventArrivedEventArgs e)
    {
        if (Displays.IsValueCreated)
        {
            var currentDisplays = GetDisplays();
            Displays = new Lazy<IReadOnlyCollection<Display>>(currentDisplays.AsReadOnly);
            
            var previousDisplays = Displays.Value.ToList();

            currentDisplays.RemoveMany(previousDisplays);
            
            foreach (var newDisplay in currentDisplays)
            {
                DisplayAttached?.Invoke(newDisplay);
            }
        }
    }

    private void OnDeviceUninstalled(object sender, EventArrivedEventArgs e)
    {
        if (Displays.IsValueCreated)
        {
            var currentDisplays = GetDisplays();
            var previousDisplays = Displays.Value.ToList();

            previousDisplays.RemoveMany(currentDisplays);
            
            foreach (var newDisplay in previousDisplays)
            {
                DisplayRemoved?.Invoke(newDisplay);
            }
        }
    }

    private IList<Display> GetDisplays()
    {
        var displays = new List<Display>();

        foreach (var pathInfo in PathInfo.GetAllPaths())
        {
            if (!pathInfo.IsInUse)
                continue;
                
            foreach (var displayDevice in pathInfo.DisplaySource.Adapter.ToDisplayAdapter().GetDisplayDevices())
            {
                var identifier = displayDevice.DevicePath;
                var supportedResolutions = new List<DisplayResolution>();
                var supportedRefreshRates = new HashSet<int>();
                    
                if (displayDevice.IsAvailable)
                {
                    foreach (var possibleSetting in displayDevice.GetPossibleSettings())
                    {
                        var width = possibleSetting.Resolution.Width;
                        var height = possibleSetting.Resolution.Height;
                        var frequency = possibleSetting.Frequency;
                    
                        if (supportedResolutions.FirstOrDefault(x => x.Width == width && x.Height == height) == null)
                        {
                            supportedResolutions.Add(new DisplayResolution(width, height));
                        }
                        supportedRefreshRates.Add(frequency);
                    }
                        
                    if (supportedResolutions.Count != 0)
                    {
                        var connectionInfo = pathInfo.TargetsInfo.FirstOrDefault(x => x.DisplayTarget.DevicePath == displayDevice.DevicePath);
                            
                        if (connectionInfo != null)
                        {
                            var currentWidth = connectionInfo.SignalInfo.ActiveSize.Width;
                            var currentHeight = connectionInfo.SignalInfo.ActiveSize.Height;
                            var currentRefreshRate = (int) connectionInfo.SignalInfo.VerticalSyncFrequencyInMillihertz / 1000;
                            var outputTechnology = connectionInfo.OutputTechnology switch
                            {
                                DisplayConfigVideoOutputTechnology.HD15 => DisplayOutputTechnology.VGA,
                                DisplayConfigVideoOutputTechnology.SVideo => DisplayOutputTechnology.SVideo,
                                DisplayConfigVideoOutputTechnology.CompositeVideo => DisplayOutputTechnology.Composite,
                                DisplayConfigVideoOutputTechnology.ComponentVideo => DisplayOutputTechnology.Component,
                                DisplayConfigVideoOutputTechnology.DVI => DisplayOutputTechnology.DVI,
                                DisplayConfigVideoOutputTechnology.HDMI => DisplayOutputTechnology.HDMI,
                                DisplayConfigVideoOutputTechnology.LVDS => DisplayOutputTechnology.LVDS,
                                DisplayConfigVideoOutputTechnology.DJPN => DisplayOutputTechnology.D_JPN,
                                DisplayConfigVideoOutputTechnology.SDI => DisplayOutputTechnology.SDI,
                                DisplayConfigVideoOutputTechnology.DisplayPortExternal => DisplayOutputTechnology.DisplayPort,
                                DisplayConfigVideoOutputTechnology.DisplayPortEmbedded => DisplayOutputTechnology.DisplayPort,
                                DisplayConfigVideoOutputTechnology.UDIExternal => DisplayOutputTechnology.UDI,
                                DisplayConfigVideoOutputTechnology.UDIEmbedded => DisplayOutputTechnology.UDI,
                                DisplayConfigVideoOutputTechnology.SDTVDongle => DisplayOutputTechnology.SDTV,
                                DisplayConfigVideoOutputTechnology.Miracast => DisplayOutputTechnology.Miracast,
                                DisplayConfigVideoOutputTechnology.Internal => DisplayOutputTechnology.Internal,
                                _ => DisplayOutputTechnology.Unknown
                            };
                    
                            displays.Add(new Display(identifier, 
                                supportedResolutions, 
                                currentResolution: supportedResolutions.First(x => x.Width == currentWidth && x.Height == currentHeight), 
                                supportedRefreshRates, 
                                currentRefreshRate: currentRefreshRate,
                                outputTechnology: outputTechnology));
                        }
                    }
                        
                }
            }
        }
        return displays;
    }

    public void ApplySettings(Display targetDisplay, DisplayResolution targetDisplayResolution, int targetHz)
    {
        if (targetHz > 0)
        {
            foreach (var displayAdapter in DisplayAdapter.GetDisplayAdapters())
            {
                var displayDevice = displayAdapter.GetDisplayDevices()
                    .FirstOrDefault(x => x.DevicePath == targetDisplay.Identifier && x.IsAvailable);

                var possibleSetting = displayDevice?.GetPossibleSettings().FirstOrDefault(x => x.Resolution.Width == targetDisplayResolution.Width && x.Resolution.Height == targetDisplayResolution.Height && x.Frequency == targetHz);
                if (possibleSetting != null)
                {
                    var settingsToSave = new Dictionary<DisplayDevice, DisplaySetting>();
                    settingsToSave.Add(displayDevice, new DisplaySetting(possibleSetting));
                    
                    DisplaySetting.SaveDisplaySettings(settingsToSave, true);
                    break;
                }
            }
        }
        else
        {
            _logger.LogError("Invalid input format");
            throw new AggregateException("Invalid input format");
        }
    }

    public void ApplySettings(string targetDisplayIdentifier, int targetHz)
    {
        var targetDisplay = Displays.Value.FirstOrDefault(x => x.Identifier == targetDisplayIdentifier);

        if (targetDisplay != null)
        {
            var currentResolution = targetDisplay.CurrentResolution;
            
            ApplySettings(targetDisplay, currentResolution, targetHz);
        }
        else
        {
            throw new ArgumentException("Invalid targetDisplayIdentifier");
        }
    }

    public void Dispose()
    {
        _installDeviceEventWatcher.EventArrived -= OnNewDeviceInstalled;
        _installDeviceEventWatcher.Stop();
        _installDeviceEventWatcher.Dispose();
        
        _uninstallDeviceEventWatcher.EventArrived -= OnDeviceUninstalled;
        _uninstallDeviceEventWatcher.Stop();
        _uninstallDeviceEventWatcher.Dispose();
    }
}