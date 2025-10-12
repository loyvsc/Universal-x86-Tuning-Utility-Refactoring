using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Management;
using System.Threading;
using ApplicationCore.Enums.Display;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.Win32;
using Universal_x86_Tuning_Utility.Windows.Interfaces;
using WindowsDisplayAPI;
using WindowsDisplayAPI.DisplayConfig;
using WindowsDisplayAPI.Native.DisplayConfig;
using Display = ApplicationCore.Models.Display;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsDisplayInfoService : IDisplayInfoService, IDisposable
{
    public event DisplayAttachedEventHandler? DisplayAttached;
    public event DisplayRemovedEventHandler? DisplayRemoved;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public IReadOnlyCollection<Display> Displays => _displays.Value.AsReadOnly();

    private readonly Lazy<List<Display>> _displays;
    private readonly Serilog.ILogger _logger;
    private readonly IDisposable _installDeviceSubscription;
    private readonly IDisposable _uninstallDeviceEventWatcher;
    private readonly Lock _displaysLock = new();

    public WindowsDisplayInfoService(Serilog.ILogger logger, IManagementEventService managementEventService)
    {
        _logger = logger;
        _displays = new  Lazy<List<Display>>(() => GetDisplays());
        
        _installDeviceSubscription =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")
                .Subscribe(OnNewDeviceInstalled);
        
        _uninstallDeviceEventWatcher =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")
                .Subscribe(OnDeviceUninstalled);
        
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        if (_displays.IsValueCreated)
        {
            _displays.Value.Clear();
            _displays.Value.AddRange(GetDisplays());
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnNewDeviceInstalled(EventArrivedEventArgs e)
    {
        if (_displays.IsValueCreated)
        {
            _displays.Value.Clear();

            foreach (var display in GetDisplays())
            {
                if (_displays.Value.All(d => d.Identifier != display.Identifier))
                {
                    _displays.Value.Add(display);
                    DisplayAttached?.Invoke(display);
                }
            }
        }
    }

    private void OnDeviceUninstalled(EventArrivedEventArgs e)
    {
        if (_displays.IsValueCreated)
        {
            _displays.Value.Clear();

            foreach (var display in GetDisplays())
            {
                if (_displays.Value.All(d => d.Identifier != display.Identifier))
                {
                    _displays.Value.Remove(display);
                    DisplayRemoved?.Invoke(display);
                }
            }
        }
    }

    private List<Display> GetDisplays()
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
                                name: displayDevice.DisplayName,
                                supportedResolutions, 
                                currentResolution: supportedResolutions.First(x => x.Width == currentWidth && x.Height == currentHeight), 
                                supportedRefreshRates, 
                                currentRefreshRate: currentRefreshRate + 1,
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
            lock (_displaysLock)
            {
                foreach (var displayAdapter in DisplayAdapter.GetDisplayAdapters())
                {
                    var displayDevice = displayAdapter.GetDisplayDevices()
                        .FirstOrDefault(x => x.DevicePath == targetDisplay.Identifier && x.IsAvailable);

                    if (displayDevice != null)
                    {
                        var possibleSetting = displayDevice.GetPossibleSettings().FirstOrDefault(x => x.Resolution.Width == targetDisplayResolution.Width && x.Resolution.Height == targetDisplayResolution.Height && x.Frequency == targetHz);
                        if (possibleSetting != null)
                        {
                            var settingsToSave = new Dictionary<DisplayDevice, DisplaySetting>();
                            settingsToSave.Add(displayDevice, new DisplaySetting(possibleSetting));
                    
                            DisplaySetting.SaveDisplaySettings(settingsToSave, true);

                            var changedDisplay = _displays.Value.FirstOrDefault(x => x.Identifier == targetDisplay.Identifier);
                            if (changedDisplay != null)
                            {
                                changedDisplay.UpdateCurrentResolution(targetDisplayResolution, targetHz);
                                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, _displays));
                            }
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            _logger.Error("Invalid input format");
            throw new AggregateException("Invalid input format");
        }
    }

    public void ApplySettings(string targetDisplayIdentifier, int targetHz)
    {
        var targetDisplay = _displays.Value.FirstOrDefault(x => x.Identifier == targetDisplayIdentifier);

        if (targetDisplay != null)
        {
            var currentResolution = targetDisplay.CurrentResolution;
            
            ApplySettings(targetDisplay, currentResolution, targetHz);
            return;
        }
        
        throw new ArgumentException($"Invalid {nameof(targetDisplayIdentifier)}");
    }

    public void Dispose()
    {
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        _installDeviceSubscription.Dispose();
        _uninstallDeviceEventWatcher.Dispose();
    }
}