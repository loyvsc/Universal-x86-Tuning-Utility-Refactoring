using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;
using static Universal_x86_Tuning_Utility.Services.SystemInfoServices.ScreenInterrogatory;
using Screen = System.Windows.Forms.Screen;

namespace Universal_x86_Tuning_Utility.Services.SystemInfoServices;

public class Display : IDisplayService
{
    #region PInvoke Declarations

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplaySettings(string lpszDeviceName, uint iModeNum, ref DEVMODE lpDevMode);

    [DllImport("user32.dll")]
    private static extern int ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    private struct DISPLAY_DEVICE
    {
        public uint cb;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string DeviceName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceString;
        public uint StateFlags;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceID;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DeviceKey;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DEVMODE
    {
        private const int CCHDEVICENAME = 32;
        private const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;

        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;

        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
    }

    #endregion

    private const int DISP_CHANGE_SUCCESSFUL = 0;
    private const uint ENUM_CURRENT_SETTINGS = 0xFFFFFFFF;
    
    private string targetDisplayName;
    
    private readonly ILogger<Display> _logger;
    private readonly List<string> _uniqueResolutions;
    private readonly List<int> _uniqueRefreshRates;

    public Display(ILogger<Display> logger)
    {
        _logger = logger;
        targetDisplayName = FindLaptopScreen();
        _uniqueResolutions = GetSupportedResolutions(targetDisplayName);
        _uniqueRefreshRates = GetSupportedRefreshRates(targetDisplayName)
            .Distinct()
            .ToList();
        _uniqueRefreshRates.Sort();
        _uniqueRefreshRates.Reverse();
    }

    private List<string> GetSupportedResolutions(string targetDisplayName)
    {
        var resolutions = new List<string>();
        var displayDevice = new DISPLAY_DEVICE();
        displayDevice.cb = (uint)Marshal.SizeOf(displayDevice);

        for (uint deviceIndex = 0; EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0); deviceIndex++)
        {
            if (displayDevice.DeviceName == targetDisplayName)
            {
                var devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);
                for (uint modeIndex = 0; EnumDisplaySettings(displayDevice.DeviceName, modeIndex, ref devMode); modeIndex++)
                {
                    var resolution = $"{devMode.dmPelsWidth} x {devMode.dmPelsHeight}";
                    resolutions.Add(resolution);
                }
                break;
            }
        }

        return resolutions;
    }

    private List<int> GetSupportedRefreshRates(string targetDisplayName)
    {
        var refreshRates = new List<int>();
        var displayDevice = new DISPLAY_DEVICE();
        displayDevice.cb = (uint)Marshal.SizeOf(displayDevice);
        for (uint deviceIndex = 0; EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0); deviceIndex++)
        {
            if (displayDevice.DeviceName == targetDisplayName)
            {
                var devMode = new DEVMODE();
                devMode.dmSize = (short)Marshal.SizeOf(devMode);
                for (uint modeIndex = 0; EnumDisplaySettings(displayDevice.DeviceName, modeIndex, ref devMode); modeIndex++)
                {
                    if (devMode.dmDisplayFrequency > 0)
                    {
                        refreshRates.Add(devMode.dmDisplayFrequency);
                    }
                }
                break;
            }
        }

        return refreshRates;
    }

    private void ChangeDisplaySettings(string targetDisplayName, int newRefreshRate)
    {
        var devMode = new DEVMODE();
        devMode.dmSize = (short)Marshal.SizeOf(devMode);

        if (EnumDisplaySettings(targetDisplayName, ENUM_CURRENT_SETTINGS, ref devMode))
        {
            devMode.dmFields = (int)(DisplaySettingsFlags.DM_PELSWIDTH | DisplaySettingsFlags.DM_PELSHEIGHT | DisplaySettingsFlags.DM_DISPLAYFREQUENCY);
            devMode.dmDisplayFrequency = newRefreshRate;

            int result = ChangeDisplaySettingsEx(targetDisplayName, ref devMode, IntPtr.Zero, (uint)ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY, IntPtr.Zero);
            
            if (result == DISP_CHANGE_SUCCESSFUL)
            {
                _logger.LogInformation("Display settings changed successfully.");
            }
            else
            {
                _logger.LogError("Failed to change display settings.");
                throw new AggregateException("Failed to change display settings");
            }
        }
        else
        {
            _logger.LogError("Failed to retrieve current display settings.");
            throw new AggregateException("Failed to retrieve current display settings");
        }
    }

    public void ApplySettings(int newHz)
    {
        targetDisplayName = FindLaptopScreen();
        if (newHz > 0)
        {
            ChangeDisplaySettings(targetDisplayName, newHz);
        }
        else
        {
            _logger.LogError("Invalid input format");
            throw new AggregateException("Invalid input format");
        }
    }

    [Flags]
    private enum DisplaySettingsFlags
    {
        DM_PELSWIDTH = 0x00080000,
        DM_PELSHEIGHT = 0x00100000,
        DM_DISPLAYFREQUENCY = 0x00400000
    }

    [Flags]
    private enum ChangeDisplaySettingsFlags : uint
    {
        CDS_UPDATEREGISTRY = 0x00000001,
    }

    private const string DefaultDevice = @"\\.\DISPLAY1";

    private string FindLaptopScreen()
    {
        var screens = Screen.AllScreens;
        try
        {
            var devices = GetAllDevices().ToList();

            var deviceIndex = devices.FindIndex(device =>
                device.outputTechnology 
                    is DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL
                    or DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED);

            if (deviceIndex != -1)
            {
                return screens[deviceIndex].DeviceName;
            }

            return DefaultDevice;
        }
        catch
        {
            return Screen.PrimaryScreen!.DeviceName;
        }
    }
}