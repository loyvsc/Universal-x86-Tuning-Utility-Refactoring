using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using ApplicationCore.Interfaces;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Universal_x86_Tuning_Utility.Services.DisplayInfoServices;

public class WindowsDisplayInfoService : IDisplayInfoService
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
    
    private const int DISP_CHANGE_SUCCESSFUL = 0;
    private const uint ENUM_CURRENT_SETTINGS = 0xFFFFFFFF;

    #endregion
    
    public List<string> UniqueTargetScreenResolutions  { get; }
    public List<int> UniqueTargetRefreshRates { get; }

    private string _targetDisplayName;
    private readonly ILogger<WindowsDisplayInfoService> _logger;
    
    public WindowsDisplayInfoService(ILogger<WindowsDisplayInfoService> logger)
    {
        _logger = logger;
        _targetDisplayName = FindLaptopScreen();
        UniqueTargetScreenResolutions = GetSupportedResolutions(_targetDisplayName);
        UniqueTargetRefreshRates = GetSupportedRefreshRates(_targetDisplayName)
            .Distinct()
            .ToList();
        UniqueTargetRefreshRates.Sort();
        UniqueTargetRefreshRates.Reverse();
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
        _targetDisplayName = FindLaptopScreen();
        if (newHz > 0)
        {
            ChangeDisplaySettings(_targetDisplayName, newHz);
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
        try //todo: check what exception can be thrown this
        {
            var devices = ScreenInterrogatory.GetAllDevices().ToList();

            var deviceIndex = devices.FindIndex(device =>
                device.outputTechnology 
                    is ScreenInterrogatory.DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL
                    or ScreenInterrogatory.DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED);
            
            if (deviceIndex != -1)
            {
                var screenName = devices[deviceIndex].monitorFriendlyDeviceName; // todo: but what about DefaultName ???
                if (screenName == string.Empty) screenName = null;
                return screenName;
            }

            return DefaultDevice;
        }
        catch
        {
            return DefaultDevice;
        }
    }
}

internal class ScreenInterrogatory
{
    #region PInvoke Declarations

    private const int ERROR_SUCCESS = 0;

    public enum QUERY_DEVICE_CONFIG_FLAGS : uint
    {
        QDC_ALL_PATHS = 0x00000001,
        QDC_ONLY_ACTIVE_PATHS = 0x00000002,
        QDC_DATABASE_CURRENT = 0x00000004
    }

    public enum DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY : uint
    {
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_OTHER = 0xFFFFFFFF,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15 = 0,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO = 1,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO = 2,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO = 3,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI = 4,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HDMI = 5,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_LVDS = 6,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_D_JPN = 8,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDI = 9,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EXTERNAL = 10,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DISPLAYPORT_EMBEDDED = 11,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EXTERNAL = 12,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_UDI_EMBEDDED = 13,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SDTVDONGLE = 14,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_MIRACAST = 15,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_INTERNAL = 0x80000000,
        DISPLAYCONFIG_OUTPUT_TECHNOLOGY_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_SCANLINE_ORDERING : uint
    {
        DISPLAYCONFIG_SCANLINE_ORDERING_UNSPECIFIED = 0,
        DISPLAYCONFIG_SCANLINE_ORDERING_PROGRESSIVE = 1,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED = 2,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_UPPERFIELDFIRST = DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED,
        DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED_LOWERFIELDFIRST = 3,
        DISPLAYCONFIG_SCANLINE_ORDERING_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_ROTATION : uint
    {
        DISPLAYCONFIG_ROTATION_IDENTITY = 1,
        DISPLAYCONFIG_ROTATION_ROTATE90 = 2,
        DISPLAYCONFIG_ROTATION_ROTATE180 = 3,
        DISPLAYCONFIG_ROTATION_ROTATE270 = 4,
        DISPLAYCONFIG_ROTATION_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_SCALING : uint
    {
        DISPLAYCONFIG_SCALING_IDENTITY = 1,
        DISPLAYCONFIG_SCALING_CENTERED = 2,
        DISPLAYCONFIG_SCALING_STRETCHED = 3,
        DISPLAYCONFIG_SCALING_ASPECTRATIOCENTEREDMAX = 4,
        DISPLAYCONFIG_SCALING_CUSTOM = 5,
        DISPLAYCONFIG_SCALING_PREFERRED = 128,
        DISPLAYCONFIG_SCALING_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_PIXELFORMAT : uint
    {
        DISPLAYCONFIG_PIXELFORMAT_8BPP = 1,
        DISPLAYCONFIG_PIXELFORMAT_16BPP = 2,
        DISPLAYCONFIG_PIXELFORMAT_24BPP = 3,
        DISPLAYCONFIG_PIXELFORMAT_32BPP = 4,
        DISPLAYCONFIG_PIXELFORMAT_NONGDI = 5,
        DISPLAYCONFIG_PIXELFORMAT_FORCE_UINT32 = 0xffffffff
    }

    public enum DISPLAYCONFIG_MODE_INFO_TYPE : uint
    {
        DISPLAYCONFIG_MODE_INFO_TYPE_SOURCE = 1,
        DISPLAYCONFIG_MODE_INFO_TYPE_TARGET = 2,
        DISPLAYCONFIG_MODE_INFO_TYPE_FORCE_UINT32 = 0xFFFFFFFF
    }

    public enum DISPLAYCONFIG_DEVICE_INFO_TYPE : uint
    {
        DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME = 1,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME = 2,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE = 3,
        DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME = 4,
        DISPLAYCONFIG_DEVICE_INFO_SET_TARGET_PERSISTENCE = 5,
        DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE = 6,
        DISPLAYCONFIG_DEVICE_INFO_FORCE_UINT32 = 0xFFFFFFFF
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LUID
    {
        public uint LowPart;
        public int HighPart;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_SOURCE_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_TARGET_INFO
    {
        public LUID adapterId;
        public uint id;
        public uint modeInfoIdx;
        private DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
        private DISPLAYCONFIG_ROTATION rotation;
        private DISPLAYCONFIG_SCALING scaling;
        private DISPLAYCONFIG_RATIONAL refreshRate;
        private DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
        public bool targetAvailable;
        public uint statusFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_RATIONAL
    {
        public uint Numerator;
        public uint Denominator;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_PATH_INFO
    {
        public DISPLAYCONFIG_PATH_SOURCE_INFO sourceInfo;
        public DISPLAYCONFIG_PATH_TARGET_INFO targetInfo;
        public uint flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_2DREGION
    {
        public uint cx;
        public uint cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_VIDEO_SIGNAL_INFO
    {
        public ulong pixelRate;
        public DISPLAYCONFIG_RATIONAL hSyncFreq;
        public DISPLAYCONFIG_RATIONAL vSyncFreq;
        public DISPLAYCONFIG_2DREGION activeSize;
        public DISPLAYCONFIG_2DREGION totalSize;
        public uint videoStandard;
        public DISPLAYCONFIG_SCANLINE_ORDERING scanLineOrdering;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_MODE
    {
        public DISPLAYCONFIG_VIDEO_SIGNAL_INFO targetVideoSignalInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTL
    {
        private int x;
        private int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_SOURCE_MODE
    {
        public uint width;
        public uint height;
        public DISPLAYCONFIG_PIXELFORMAT pixelFormat;
        public POINTL position;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct DISPLAYCONFIG_MODE_INFO_UNION
    {
        [FieldOffset(0)]
        public DISPLAYCONFIG_TARGET_MODE targetMode;

        [FieldOffset(0)]
        public DISPLAYCONFIG_SOURCE_MODE sourceMode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_MODE_INFO
    {
        public DISPLAYCONFIG_MODE_INFO_TYPE infoType;
        public uint id;
        public LUID adapterId;
        public DISPLAYCONFIG_MODE_INFO_UNION modeInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS
    {
        public uint value;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DISPLAYCONFIG_DEVICE_INFO_HEADER
    {
        public DISPLAYCONFIG_DEVICE_INFO_TYPE type;
        public uint size;
        public LUID adapterId;
        public uint id;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DISPLAYCONFIG_TARGET_DEVICE_NAME
    {
        public DISPLAYCONFIG_DEVICE_INFO_HEADER header;
        public DISPLAYCONFIG_TARGET_DEVICE_NAME_FLAGS flags;
        public DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY outputTechnology;
        public ushort edidManufactureId;
        public ushort edidProductCodeId;
        public uint connectorInstance;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string monitorFriendlyDeviceName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string monitorDevicePath;
    }

    [DllImport("user32.dll")]
    public static extern int GetDisplayConfigBufferSizes(
        QUERY_DEVICE_CONFIG_FLAGS flags, out uint numPathArrayElements, out uint numModeInfoArrayElements);

    [DllImport("user32.dll")]
    public static extern int QueryDisplayConfig(
        QUERY_DEVICE_CONFIG_FLAGS flags,
        ref uint numPathArrayElements, [Out] DISPLAYCONFIG_PATH_INFO[] PathInfoArray,
        ref uint numModeInfoArrayElements, [Out] DISPLAYCONFIG_MODE_INFO[] ModeInfoArray,
        nint currentTopologyId
    );

    [DllImport("user32.dll")]
    public static extern int DisplayConfigGetDeviceInfo(ref DISPLAYCONFIG_TARGET_DEVICE_NAME deviceName);

    #endregion

    private static DISPLAYCONFIG_TARGET_DEVICE_NAME GetDeviceName(LUID adapterId, uint targetId)
    {
        var deviceName = new DISPLAYCONFIG_TARGET_DEVICE_NAME
        {
            header =
            {
                size = (uint)Marshal.SizeOf(typeof (DISPLAYCONFIG_TARGET_DEVICE_NAME)),
                adapterId = adapterId,
                id = targetId,
                type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME
            }
        };
        var error = DisplayConfigGetDeviceInfo(ref deviceName);
        if (error != ERROR_SUCCESS)
            throw new Win32Exception(error);

        return deviceName;
    }

    public static IEnumerable<DISPLAYCONFIG_TARGET_DEVICE_NAME> GetAllDevices()
    {
        var error = GetDisplayConfigBufferSizes(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS, out uint pathCount, out uint modeCount);
        if (error != ERROR_SUCCESS)
            throw new Win32Exception(error);

        var displayPaths = new DISPLAYCONFIG_PATH_INFO[pathCount];
        var displayModes = new DISPLAYCONFIG_MODE_INFO[modeCount];
        error = QueryDisplayConfig(QUERY_DEVICE_CONFIG_FLAGS.QDC_ONLY_ACTIVE_PATHS,
            ref pathCount, displayPaths, ref modeCount, displayModes, nint.Zero);
        if (error != ERROR_SUCCESS)
            throw new Win32Exception(error);

        for (var i = 0; i < modeCount; i++)
            if (displayModes[i].infoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET)
                yield return GetDeviceName(displayModes[i].adapterId, displayModes[i].id);
    }
}