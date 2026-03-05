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
            foreach (var display in GetDisplays())
            {
                if (_displays.Value.FirstOrDefault(d => d.Identifier == display.Identifier) == null)
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
            var currentDisplays = GetDisplays();
            foreach (var display in _displays.Value)
            {
                if (currentDisplays.FirstOrDefault(d => d.Identifier != display.Identifier) == null)
                {
                    _displays.Value.Remove(display);
                    DisplayRemoved?.Invoke(display);
                }
            }
        }
    }

    private List<Display> GetDisplays()
    {
        var displays = new Dictionary<string, Display>();

        foreach (var display in WindowsDisplayAPI.Display.GetDisplays())
        {
            if (display.IsAvailable)
            {
                var identifier = display.DevicePath;
                var supportedResolutions = new List<DisplayResolution>();
                var supportedRefreshRates = new SortedSet<int>();
                
                foreach (var possibleSetting in display.GetPossibleSettings())
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
                    var pathInfo = PathInfo.GetActivePaths().FirstOrDefault(x =>
                    {
                        return x.TargetsInfo.Any(y =>
                        {
                            try
                            {
                                return y.DisplayTarget.DevicePath == display.DevicePath;
                            }
                            catch
                            {
                                return false;
                            }
                        });
                    });
                            
                    if (pathInfo != null)
                    {
                        var connectionInfo = pathInfo.TargetsInfo.First(x =>
                        {
                            try
                            {
                                return x.DisplayTarget.DevicePath == display.DevicePath;
                            }
                            catch
                            {
                                return false;
                            }
                        });
                        
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

                        displays.TryAdd(identifier, new Display(identifier,
                            name: display.DeviceName,
                            supportedResolutions,
                            currentResolution: supportedResolutions.First(x => x.Width == currentWidth && x.Height == currentHeight),
                            supportedRefreshRates,
                            currentRefreshRate: currentRefreshRate + 1,
                            outputTechnology: outputTechnology));
                    }
                }
            }
        }
        
        return displays.Values.ToList();
    }

    public void ApplySettings(Display targetDisplay, DisplayResolution targetDisplayResolution, int targetHz)
    {
        if (targetHz > 0)
        {
            lock (_displaysLock)
            {
                foreach (var display in WindowsDisplayAPI.Display.GetDisplays())
                {
                    if (display.DevicePath == targetDisplay.Identifier && display.IsAvailable)
                    {
                        var possibleSetting = display.GetPossibleSettings().FirstOrDefault(x => x.Resolution.Width == targetDisplayResolution.Width && x.Resolution.Height == targetDisplayResolution.Height && x.Frequency == targetHz);
                        if (possibleSetting != null)
                        {
                            display.SetSettings(new DisplaySetting(possibleSetting), true);

                            var changedDisplay = _displays.Value.FirstOrDefault(x => x.Identifier == targetDisplay.Identifier);
                            if (changedDisplay != null)
                            {
                                changedDisplay.UpdateRefreshRate(targetHz);
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