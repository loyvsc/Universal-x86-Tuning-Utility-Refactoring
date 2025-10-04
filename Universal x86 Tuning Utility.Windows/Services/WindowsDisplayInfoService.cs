using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Management;
using System.Threading;
using ApplicationCore.Enums.Display;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using DynamicData;
using Microsoft.Extensions.Logging;
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
    public IReadOnlyCollection<Display> Displays => _displays.AsReadOnly();
    
    private readonly List<Display> _displays = new();
    private readonly Serilog.ILogger _logger;
    private readonly IDisposable _installDeviceSubscription;
    private readonly IDisposable _uninstallDeviceEventWatcher;
    private readonly Lock _displaysLock = new();

    public WindowsDisplayInfoService(Serilog.ILogger logger, IManagementEventService managementEventService)
    {
        _logger = logger;
        Initialize();
        
        _installDeviceSubscription =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 2")
                .Subscribe(OnNewDeviceInstalled);
        
        _uninstallDeviceEventWatcher =
            managementEventService.SubscribeToQuery("SELECT * FROM Win32_DeviceChangeEvent WHERE EventType = 3")
                .Subscribe(OnDeviceUninstalled);
        
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
    }

    private void Initialize()
    {
        _displays.AddRange(GetDisplays());
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e)
    {
        lock (_displaysLock)
        {
            _displays.Clear();
            _displays.AddRange(GetDisplays());
        }
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnNewDeviceInstalled(EventArrivedEventArgs e)
    {
        lock (_displaysLock)
        {
            var currentDisplays = GetDisplays();

            foreach (var display in currentDisplays)
            {
                if (_displays.All(d => d.Identifier != display.Identifier))
                {
                    DisplayAttached?.Invoke(display);
                }
            }
            
            _displays.Clear();
            _displays.AddRange(currentDisplays);
        }
    }

    private void OnDeviceUninstalled(EventArrivedEventArgs e)
    {
        lock (_displaysLock)
        {
            var currentDisplays = GetDisplays();

            foreach (var display in _displays)
            {
                if (currentDisplays.All(d => d.Identifier != display.Identifier))
                {
                    DisplayRemoved?.Invoke(display);
                }
            }
            
            _displays.Clear();
            _displays.AddRange(currentDisplays);
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

                    var changedDisplay = _displays.FirstOrDefault(x => x.Identifier == targetDisplay.Identifier);
                    if (changedDisplay != null)
                    {
                        changedDisplay.UpdateCurrentResolution(targetDisplayResolution, targetHz);
                        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, _displays));
                    }
                    break;
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
        var targetDisplay = _displays.FirstOrDefault(x => x.Identifier == targetDisplayIdentifier);

        if (targetDisplay != null)
        {
            var currentResolution = targetDisplay.CurrentResolution;
            
            ApplySettings(targetDisplay, currentResolution, targetHz);
        }
        else
        {
            throw new ArgumentException($"Invalid {nameof(targetDisplayIdentifier)}");
        }
    }

    public void Dispose()
    {
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        _installDeviceSubscription.Dispose();
        _uninstallDeviceEventWatcher.Dispose();
    }
}