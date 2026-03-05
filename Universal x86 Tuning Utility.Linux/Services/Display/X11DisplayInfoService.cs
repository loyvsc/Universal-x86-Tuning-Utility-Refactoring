using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using ApplicationCore.Enums.Display;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Serilog;

namespace Universal_x86_Tuning_Utility.Linux.Services.Display;

public class X11DisplayInfoService : IDisplayInfoService, IDisposable
{
    private readonly ILogger _logger;
    private readonly List<ApplicationCore.Models.Display> _displays = new();
    private readonly Lock _displaysLock = new();
    private readonly IntPtr _xDisplayPtr;
    private readonly Timer _monitorTimer;
    
    private const int POLLING_INTERVAL_MS = 2000;

    public event DisplayAttachedEventHandler? DisplayAttached;
    public event DisplayRemovedEventHandler? DisplayRemoved;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public IReadOnlyCollection<ApplicationCore.Models.Display> Displays => _displays.AsReadOnly();

    public X11DisplayInfoService(ILogger logger)
    {
        _logger = logger;
        _xDisplayPtr = X11Interop.XOpenDisplay(IntPtr.Zero);
        if (_xDisplayPtr == IntPtr.Zero)
            throw new Exception("Cannot open X display");

        _displays.AddRange(GetDisplays());
        _monitorTimer = new Timer(_ => CheckForDisplayChanges(), null,
            POLLING_INTERVAL_MS, POLLING_INTERVAL_MS);
    }

    private void CheckForDisplayChanges()
    {
        var previousDisplays = _displays;
        var currentDisplays = GetDisplays().ToList();

        bool isChanged = false;
        
        foreach (var currentDisplay in currentDisplays)
        {
            var prevDisplayInfo = previousDisplays.FirstOrDefault(d => d.Identifier != currentDisplay.Identifier);
            if (prevDisplayInfo == null)
            {
                DisplayAttached?.Invoke(currentDisplay);
                isChanged = true;
            }
            else
            {
                isChanged = !currentDisplay.Equals(prevDisplayInfo);
            }
        }
        
        foreach (var previousDisplay in previousDisplays)
        {
            if (currentDisplays.All(d => d.Identifier != previousDisplay.Identifier))
            {
                DisplayRemoved?.Invoke(previousDisplay);
                isChanged = true;
            }
        }

        if (isChanged)
        {
            lock (_displaysLock)
            {
                _displays.Clear();
                _displays.AddRange(currentDisplays);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, currentDisplays));
        }
    }

    private IEnumerable<ApplicationCore.Models.Display> GetDisplays()
    {
        var displayList = new List<ApplicationCore.Models.Display>();
        lock (_displaysLock)
        {
            int screen = X11Interop.XDefaultScreen(_xDisplayPtr);
            IntPtr root = X11Interop.XRootWindow(_xDisplayPtr, screen);

            IntPtr resPtr = X11Interop.XRRGetScreenResourcesCurrent(_xDisplayPtr, root);
            var resources = Marshal.PtrToStructure<X11Interop.XRRScreenResources>(resPtr);

            for (int i = 0; i < resources.noutput; i++)
            {
                IntPtr outputPtr = Marshal.ReadIntPtr(resources.outputs, i * IntPtr.Size);
                IntPtr outputInfoPtr = X11Interop.XRRGetOutputInfo(_xDisplayPtr, resPtr, outputPtr);
                if (outputInfoPtr == IntPtr.Zero)
                    continue;

                var outputInfo = Marshal.PtrToStructure<X11Interop.XRROutputInfo>(outputInfoPtr);
                string name = Marshal.PtrToStringAnsi(outputInfo.name, outputInfo.nameLen);

                if (outputInfo.ncrtc > 0)
                {
                    IntPtr crtc = Marshal.ReadIntPtr(outputInfo.crtcs);
                    IntPtr crtcInfoPtr = X11Interop.XRRGetCrtcInfo(_xDisplayPtr, resPtr, crtc);
                    if (crtcInfoPtr != IntPtr.Zero)
                    {
                        var crtcInfo = Marshal.PtrToStructure<X11Interop.XRRCrtcInfo>(crtcInfoPtr);

                        var currentResolution = new DisplayResolution(crtcInfo.width, crtcInfo.height);
                        int currentRefreshRate = 0;

                        GetSupportedModes(resources, outputInfo, out var supportedRes, out var supportedRates,
                            crtcInfo.mode, ref currentRefreshRate);

                        displayList.Add(new ApplicationCore.Models.Display(
                            identifier: outputPtr.ToString(),
                            name: name,
                            resolutions: supportedRes,
                            currentResolution: currentResolution,
                            supportedRefreshRates: supportedRates,
                            currentRefreshRate: currentRefreshRate,
                            outputTechnology: DetectTechnology(name)
                        ));


                        X11Interop.XRRFreeCrtcInfo(crtcInfoPtr);
                    }
                }

                X11Interop.XRRFreeOutputInfo(outputInfoPtr);
            }

            X11Interop.XRRFreeScreenResources(resPtr);
        }

        return displayList;
    }

    private void GetSupportedModes(X11Interop.XRRScreenResources resources, X11Interop.XRROutputInfo outputInfo,
        out List<DisplayResolution> resolutions, out List<int> refreshRates, int currentMode,
        ref int currentRefreshRate)
    {
        resolutions = new List<DisplayResolution>();
        refreshRates = new List<int>();
        
        for (int i = 0; i < outputInfo.nmode; i++)
        {
            IntPtr modeIdPtr = Marshal.ReadIntPtr(outputInfo.modes, i * IntPtr.Size);
            int modeId = modeIdPtr.ToInt32();

            for (int j = 0; j < resources.nmode; j++)
            {
                IntPtr modeInfoPtr = resources.modes + j * Marshal.SizeOf(typeof(X11Interop.XRRModeInfo));
                var modeInfo = Marshal.PtrToStructure<X11Interop.XRRModeInfo>(modeInfoPtr);

                if (modeInfo.id == modeId)
                {
                    var res = new DisplayResolution((int)modeInfo.width, (int)modeInfo.height);
                    if (!resolutions.Exists(r => r.Width == res.Width && r.Height == res.Height))
                        resolutions.Add(res);

                    int refresh = 0;
                    if (modeInfo.hTotal > 0 && modeInfo.vTotal > 0)
                    {
                        refresh = (int)(modeInfo.dotClock / (modeInfo.hTotal * modeInfo.vTotal));
                        if (refresh > 0 && !refreshRates.Contains(refresh))
                            refreshRates.Add(refresh);
                    }

                    if (modeId == currentMode)
                        currentRefreshRate = refresh;
                }
            }
        }
    }

    private static DisplayOutputTechnology DetectTechnology(string outputName)
    {
        var nameValues = outputName.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (nameValues.Length > 1)
        {
            if (Enum.TryParse<DisplayOutputTechnology>(nameValues[0], true, out var outputTechnology))
            {
                return outputTechnology;
            }

            switch (nameValues[0])
            {
                case "TV" or "SVIDEO":
                    return DisplayOutputTechnology.SVideo;
                case "LVDS" or "eDP":
                    return DisplayOutputTechnology.Internal;
                case "DP":
                    return DisplayOutputTechnology.DisplayPort;
            }
        }

        return DisplayOutputTechnology.Unknown;
    }


    public void ApplySettings(ApplicationCore.Models.Display targetDisplay, DisplayResolution targetDisplayResolution,
        int targetHz)
    {
        ApplySettings(targetDisplay.Identifier, targetDisplayResolution.Width,
            targetDisplayResolution.Height, targetHz);
    }

    public void ApplySettings(string targetDisplayIdentifier, int targetHz)
    {
        var display = _displays.FirstOrDefault(d => d.Identifier == targetDisplayIdentifier);
        if (display != null)
        {
            ApplySettings(targetDisplayIdentifier, display.CurrentResolution.Width,
                display.CurrentResolution.Height, targetHz);
        }
    }

    private void ApplySettings(string displayIdentifier, int width, int height, int refreshRate)
    {
        try
        {
            int screen = X11Interop.XDefaultScreen(_xDisplayPtr);
            IntPtr root = X11Interop.XRootWindow(_xDisplayPtr, screen);

            IntPtr resPtr = X11Interop.XRRGetScreenResourcesCurrent(_xDisplayPtr, root);
            var resources = Marshal.PtrToStructure<X11Interop.XRRScreenResources>(resPtr);

            for (int i = 0; i < resources.noutput; i++)
            {
                IntPtr output = Marshal.ReadIntPtr(resources.outputs, i * IntPtr.Size);
                string id = output.ToString();

                if (id != displayIdentifier)
                    continue;

                IntPtr outputInfoPtr = X11Interop.XRRGetOutputInfo(_xDisplayPtr, resPtr, output);
                
                try
                {
                    var outputInfo = Marshal.PtrToStructure<X11Interop.XRROutputInfo>(outputInfoPtr);

                    if (outputInfo.ncrtc <= 0)
                        continue;

                    IntPtr crtc = Marshal.ReadIntPtr(outputInfo.crtcs);
                    int chosenModeId = 0;

                    for (int j = 0; j < resources.nmode; j++)
                    {
                        IntPtr modeInfoPtr = resources.modes + j * Marshal.SizeOf(typeof(X11Interop.XRRModeInfo));
                        var modeInfo = Marshal.PtrToStructure<X11Interop.XRRModeInfo>(modeInfoPtr);

                        int rr = 0;
                        if (modeInfo.hTotal > 0 && modeInfo.vTotal > 0)
                            rr = (int)(modeInfo.dotClock / (modeInfo.hTotal * modeInfo.vTotal));

                        if (modeInfo.width == width && modeInfo.height == height &&
                            (refreshRate == 0 || rr == refreshRate))
                        {
                            chosenModeId = modeInfo.id;
                            break;
                        }
                    }

                    if (chosenModeId == 0)
                        throw new Exception("Requested mode not found");

                    var crtcInfo = Marshal.PtrToStructure<X11Interop.XRRCrtcInfo>(crtc);

                    int status = X11Interop.XRRSetCrtcConfig(
                        dpy: _xDisplayPtr,
                        resources: resPtr,
                        crtc: crtc,
                        timestamp: 0,
                        0, 0,
                        chosenModeId,
                        crtcInfo.rotation,
                        output, 1
                    );
                    
                    if (status != 0)
                        throw new Exception("Failed to set CRTC config");
                }
                finally
                {
                    X11Interop.XRRFreeOutputInfo(outputInfoPtr);
                }
            }

            X11Interop.XRRFreeScreenResources(resPtr);
            
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error applying display settings: {0}", ex.Message);
            throw;
        }
    }

    public void Dispose()
    {
        X11Interop.XCloseDisplay(_xDisplayPtr);
        _monitorTimer.Dispose();
    }
}

internal static class X11Interop
{
    private const string X11 = "libX11.so.6";
    private const string Xrandr = "libXrandr.so.2";

    [DllImport(X11)]
    public static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport(X11)]
    public static extern int XDefaultScreen(IntPtr display);

    [DllImport(X11)]
    public static extern IntPtr XRootWindow(IntPtr display, int screen);

    [DllImport(X11)]
    public static extern void XCloseDisplay(IntPtr display);

    [DllImport(Xrandr)]
    public static extern IntPtr XRRGetScreenResourcesCurrent(IntPtr dpy, IntPtr window);

    [DllImport(Xrandr)]
    public static extern void XRRFreeScreenResources(IntPtr resources);

    [DllImport(Xrandr)]
    public static extern IntPtr XRRGetOutputInfo(IntPtr dpy, IntPtr resources, IntPtr output);

    [DllImport(Xrandr)]
    public static extern void XRRFreeOutputInfo(IntPtr outputInfo);

    [DllImport(Xrandr)]
    public static extern IntPtr XRRGetCrtcInfo(IntPtr dpy, IntPtr resources, IntPtr crtc);

    [DllImport(Xrandr)]
    public static extern int XRRSetCrtcConfig(
        IntPtr dpy,
        IntPtr resources,
        IntPtr crtc,
        int timestamp,
        int x,
        int y,
        int mode,
        int rotation,
        IntPtr outputs,
        int noutputs
    );

    [DllImport(Xrandr)]
    public static extern void XRRFreeCrtcInfo(IntPtr crtcInfo);

    [StructLayout(LayoutKind.Sequential)]
    public struct XRRScreenResources
    {
        public int timestamp;
        public int configTimestamp;
        public int ncrtc;
        public IntPtr crtcs;
        public int noutput;
        public IntPtr outputs;
        public int nmode;
        public IntPtr modes;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XRROutputInfo
    {
        public IntPtr name;
        public int nameLen;
        public int mm_width;
        public int mm_height;
        public int connection;
        public int subpixel_order;
        public int ncrtc;
        public IntPtr crtcs;
        public int nclone;
        public IntPtr clones;
        public int nmode;
        public IntPtr modes;
        public int npreferred;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XRRCrtcInfo
    {
        public int x, y;
        public int width, height;
        public int mode;
        public int rotation;
        public int noutput;
        public IntPtr outputs;
        public int rotations;
        public int npossible;
        public IntPtr possible;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct XRRModeInfo
    {
        public int id;
        public uint width;
        public uint height;
        public ulong dotClock;
        public uint hSyncStart;
        public uint hSyncEnd;
        public uint hTotal;
        public uint hSkew;
        public uint vSyncStart;
        public uint vSyncEnd;
        public uint vTotal;
        public uint nameLength;
        public IntPtr name;
        public uint modeFlags;
    }
}