using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Universal_x86_Tuning_Utility.Services.Asus;

public class WindowsAsusWmiService : IASUSWmiService
{
    //
    // This is a customised version of ASUSWmi.cs from https://github.com/seerge/g-helper
    // I do not take credit for the full functionality of the code.
    //

    #region PInvoke Declarations

    private const string FILE_NAME = @"\\.\\ATKACPI";
    private const uint CONTROL_CODE = 0x0022240C;

    private const uint DSTS = 0x53545344;
    private const uint DEVS = 0x53564544;
    private const uint INIT = 0x54494E49;

    // public const uint UniversalControl = 0x00100021;
    //
    // public const int KB_Light_Up = 0xc4;
    // public const int KB_Light_Down = 0xc5;
    // public const int Brightness_Down = 0x10;
    // public const int Brightness_Up = 0x20;
    // public const int KB_Sleep = 0x6c;
    // public const int KB_DUO_PgUpDn = 0x4B;
    // public const int KB_DUO_SecondDisplay = 0x6A;
    //
    // public const int Touchpad_Toggle = 0x6B;
    //
    // public const int ChargerMode = 0x0012006C;
    //
    // public const int ChargerUSB = 2;
    // public const int ChargerBarrel = 1;

    private const uint CPU_Fan = 0x00110013;
    private const uint GPU_Fan = 0x00110014;
    private const uint SYS_Fan = 0x00110031;

    public const uint PerformanceMode = 0x00120075;
    public const uint VivoBookMode = 0x00110019;

    public const uint GPUEco = 0x00090020;
    public const uint GPUMux = 0x00090016;
    public const uint GPUMuxVivo = 0x00090026;

    public const uint eGPU = 0x00090019;
    public const uint eGPUConnected = 0x00090018;

    public const int Temp_CPU = 0x00120094;
    public const int Temp_GPU = 0x00120097;

    // public const uint BatteryLimit = 0x00120057;
    // public const uint ScreenOverdrive = 0x00050019;
    // public const uint ScreenMultizone = 0x0005001E;

    private const uint DevsCPUFan = 0x00110022;
    private const uint DevsGPUFan = 0x00110023;

    private const uint DevsCPUFanCurve = 0x00110024;
    private const uint DevsGPUFanCurve = 0x00110025;
    private const uint DevsSYSFanCurve = 0x00110032;

    private const int GPUBoost = 0x001200C0;

    // public const int PPT_TotalA0 = 0x001200A0;
    // public const int PPT_EDCA1 = 0x001200A1;
    // public const int PPT_TDCA2 = 0x001200A2;
    // public const int PPT_APUA3 = 0x001200A3;

    private const int PPT_CPUB0 = 0x001200B0;
    // public const int PPT_CPUB1 = 0x001200B1;
    //
    // public const int PPT_APUC1 = 0x001200C1;
    // public const int PPT_APUC2 = 0x001200C2;

    private const uint TUF_KB_BRIGHTNESS = 0x00050021;
    private const uint TUF_KB = 0x00100056;
    private const uint TUF_KB2 = 0x0010005a;
    private const uint TUF_KB_STATE = 0x00100057;

    // public const int CPU_VOLTAGE = 0x00120079;
    //
    // public const int BootSound = 0x00130022;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFile(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool DeviceIoControl(
        IntPtr hDevice,
        uint dwIoControlCode,
        byte[] lpInBuffer,
        uint nInBufferSize,
        byte[] lpOutBuffer,
        uint nOutBufferSize,
        ref uint lpBytesReturned,
        IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    private const uint GENERIC_READ = 0x80000000;
    private const uint GENERIC_WRITE = 0x40000000;
    private const uint OPEN_EXISTING = 3;
    private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
    private const uint FILE_SHARE_READ = 1;
    private const uint FILE_SHARE_WRITE = 2;

    // Event handling attempt

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateEvent(IntPtr lpEventAttributes, bool bManualReset, bool bInitialState, string lpName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WaitForSingleObject(IntPtr hHandle, int dwMilliseconds);

    #endregion

    private IntPtr _eventHandle;
    private readonly IntPtr _handle;
    private readonly ILogger<WindowsAsusWmiService> _logger;
    private readonly ManagementEventWatcher _eventWatcher;
    
    public WindowsAsusWmiService(ILogger<WindowsAsusWmiService> logger)
    {
        _logger = logger;
        _handle = CreateFile(
            FILE_NAME,
            GENERIC_READ | GENERIC_WRITE,
            FILE_SHARE_READ | FILE_SHARE_WRITE,
            IntPtr.Zero,
            OPEN_EXISTING,
            FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero
        );

        if (_handle == new IntPtr(-1))
        {
            // todo: refactor
            //throw new Exception("Can't connect to ACPI");
        }
        
        _eventWatcher = new ManagementEventWatcher("root\\wmi", "SELECT * FROM AsusAtkWmiEvent");
    }
    
    // todo: still works only with asus optimization service on , if someone knows how to get ACPI events from asus without that - let me know
    public void RunListener()
    {
        _eventHandle = CreateEvent(IntPtr.Zero, false, false, "ATK4001");

        var outBuffer = new byte[16];
        var data = new byte[8];

        data[0] = BitConverter.GetBytes(_eventHandle.ToInt32())[0];
        data[1] = BitConverter.GetBytes(_eventHandle.ToInt32())[1];

        Control(0x222400, data, outBuffer);
            
        while (true)
        {
            WaitForSingleObject(_eventHandle, Timeout.Infinite);
            Control(0x222408, new byte[0], outBuffer);
            int code = BitConverter.ToInt32(outBuffer);
        }
    }

    private void Control(uint dwIoControlCode, byte[] lpInBuffer, byte[] lpOutBuffer)
    {
        uint lpBytesReturned = 0;
        DeviceIoControl(
            _handle,
            dwIoControlCode,
            lpInBuffer,
            (uint)lpInBuffer.Length,
            lpOutBuffer,
            (uint)lpOutBuffer.Length,
            ref lpBytesReturned,
            IntPtr.Zero
        );
    }
        
    private byte[] CallMethod(uint MethodID, byte[] args)
    {
        var acpiBuf = new byte[8 + args.Length];
        var outBuffer = new byte[16];

        BitConverter.GetBytes(MethodID).CopyTo(acpiBuf, 0);
        BitConverter.GetBytes((uint)args.Length).CopyTo(acpiBuf, 4);
        Array.Copy(args, 0, acpiBuf, 8, args.Length);
        
        Control(CONTROL_CODE, acpiBuf, outBuffer);

        return outBuffer;
    }

    public byte[] DeviceInit()
    {
        byte[] args = new byte[8];
        return CallMethod(INIT, args);
    }

    public int DeviceSet(AsusDevice device, int newValue)
    {
        var deviceId = device switch
        {
            AsusDevice.GpuMux => GPUMux,
            AsusDevice.GpuMuxVivo => GPUMuxVivo,
            AsusDevice.GpuEco => GPUEco,
            AsusDevice.EGpu => eGPU,
            AsusDevice.EGpuConnected => eGPUConnected,
            AsusDevice.CpuFan => CPU_Fan,
            AsusDevice.GpuFan => GPU_Fan,
            AsusDevice.SystemFan => SYS_Fan,
            AsusDevice.TufKeyboardState => TUF_KB_STATE,
            AsusDevice.TufKeyboardBrightness => TUF_KB_BRIGHTNESS,
            _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
        };
        
        _logger.LogInformation($"Set value of {device.ToString()}", newValue);

        return DeviceSet(deviceId: deviceId, newValue);
    }

    private int DeviceSet(uint deviceId, int status)
    {
        var args = new byte[8];
        BitConverter.GetBytes(deviceId).CopyTo(args, 0);
        BitConverter.GetBytes((uint)status).CopyTo(args, 4);

        var deviceStatus = CallMethod(DEVS, args);
        int result = BitConverter.ToInt32(deviceStatus, 0);
        
        return result;
    }

    public int DeviceSet(AsusDevice device, byte[] values)
    {
        var deviceId = device switch
        {
            AsusDevice.GpuMux => GPUMux,
            AsusDevice.GpuMuxVivo => GPUMuxVivo,
            AsusDevice.GpuEco => GPUEco,
            AsusDevice.EGpu => eGPU,
            AsusDevice.EGpuConnected => eGPUConnected,
            AsusDevice.CpuFan => CPU_Fan,
            AsusDevice.GpuFan => GPU_Fan,
            AsusDevice.SystemFan => SYS_Fan,
            AsusDevice.TufKeyboardState => TUF_KB_STATE,
            AsusDevice.TufKeyboardBrightness => TUF_KB_BRIGHTNESS,
            _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
        };
        
        _logger.LogInformation($"Set values of {device.ToString()}", values);

        return DeviceSet(deviceId: deviceId, Params: values);
    }

    private int DeviceSet(uint deviceId, byte[] Params)
    {
        var args = new byte[4 + Params.Length];
        BitConverter.GetBytes(deviceId).CopyTo(args, 0);
        Params.CopyTo(args, 4);

        var status = CallMethod(DEVS, args);
        int result = BitConverter.ToInt32(status, 0);

        return result;
    }

    public int DeviceGet(AsusDevice device)
    {
        var deviceId = device switch
        {
            AsusDevice.GpuMux => GPUMux,
            AsusDevice.GpuMuxVivo => GPUMuxVivo,
            AsusDevice.GpuEco => GPUEco,
            AsusDevice.EGpu => eGPU,
            AsusDevice.EGpuConnected => eGPUConnected,
            AsusDevice.CpuFan => CPU_Fan,
            AsusDevice.GpuFan => GPU_Fan,
            AsusDevice.SystemFan => SYS_Fan,
            AsusDevice.TufKeyboardState => TUF_KB_STATE,
            AsusDevice.TufKeyboardBrightness => TUF_KB_BRIGHTNESS,
            _ => throw new ArgumentOutOfRangeException(nameof(device), device, null)
        };
        
        _logger.LogInformation($"Getting status of {device.ToString()}", device);

        return DeviceGet(deviceId: deviceId);
    }
        
    private int DeviceGet(uint deviceId)
    {
        byte[] args = new byte[8];
        BitConverter.GetBytes(deviceId).CopyTo(args, 0);
        byte[] status = CallMethod(DSTS, args);

        return BitConverter.ToInt32(status, 0) - 65536;
    }

    private byte[] DeviceGetBuffer(uint deviceId, uint status = 0)
    {
        var args = new byte[8];
        BitConverter.GetBytes(deviceId).CopyTo(args, 0);
        BitConverter.GetBytes(status).CopyTo(args, 4);

        return CallMethod(DSTS, args);
    }

    public void SetPerformanceMode(AsusMode newMode)
    {
        var gamingLaptopResult = DeviceGet(PerformanceMode);
        uint id = 0;
        if (gamingLaptopResult is >= 0 and < 3)
        {
            id = PerformanceMode;
        }
        else
        {
            var vivoLatopResult = DeviceGet(VivoBookMode);
            if (vivoLatopResult is >= 0 and < 3)
            {
                id = VivoBookMode;
            }
        }

        DeviceSet(id, (int)newMode);
    }

    public AsusMode GetPerformanceMode()
    {
        var gamingLaptopResult = DeviceGet(PerformanceMode);
        if (gamingLaptopResult is >= 0 and < 3)
        {
            return (AsusMode)gamingLaptopResult;
        }

        var vivoLatopResult = DeviceGet(VivoBookMode);
        if (vivoLatopResult is >= 0 and < 3)
        {
            return (AsusMode)vivoLatopResult;
        }

        throw new Exception("Unsupported asus laptop");
    }

    public void SetGPUEco(bool eco)
    {
        int ecoFlag = DeviceGet(GPUEco);
        if (ecoFlag < 0) return;

        var isEcoEnabled = ecoFlag == 1;
        if (isEcoEnabled != eco)
        {
            DeviceSet(GPUEco, eco ? 1 : 0);
        }
    }

    public int GetFan(AsusFan device)
    {
        int fan = device switch
        {
            AsusFan.GPU => DeviceGet(GPU_Fan),
            AsusFan.Mid => DeviceGet(SYS_Fan),
            _ => DeviceGet(CPU_Fan)
        };

        if (fan < 0)
        {
            fan += 65536;
            if (fan <= 0 || fan > 100) fan = -1;
        }

        return fan;
    }

    public void SetFanRange(AsusFan device, byte[] curve)
    {
        if (curve.Length != 16) throw new ArgumentException("curve must be 16 bytes");
        
        byte min = (byte)(curve[8] * 255 / 100);
        byte max = (byte)(curve[15] * 255 / 100);
        byte[] range = { min, max };

        switch (device)
        {
            case AsusFan.GPU: 
                DeviceSet(DevsGPUFan, range);
                break;
            default:
                DeviceSet(DevsCPUFan, range);
                break;
        }
    }
        
    public void SetFanCurve(AsusFan device, byte[] curve)
    {
        if (curve.Length != 16) throw new ArgumentException("curve must be 16 bytes");
        if (curve.All(singleByte => singleByte == 0)) throw new ArgumentException("curve cannot be zero");

        int fanScale = device == AsusFan.CPU ? 130 : 100;
            
        for (int i = 8; i < curve.Length; i++)
        {
            curve[i] = (byte)(Math.Max((byte)0, Math.Min((byte)100, curve[i])) * fanScale / 100);
        }

        switch (device)
        {
            case AsusFan.GPU:
                DeviceSet(DevsGPUFanCurve, curve);
                break;
            case AsusFan.Mid:
                DeviceSet(DevsSYSFanCurve, curve);
                break;
            default:
                DeviceSet(DevsCPUFanCurve, curve);
                break;
        }
    }

    public byte[] GetFanCurve(AsusFan device, int mode = 0)
    {
        // because it's asus, and modes are swapped here
        uint fanMode = mode switch
        {
            1 => 2,
            2 => 1,
            _ => 0
        };

        return device switch
        {
            AsusFan.GPU => DeviceGetBuffer(DevsGPUFanCurve, fanMode),
            AsusFan.Mid => DeviceGetBuffer(DevsSYSFanCurve, fanMode),
            _ => DeviceGetBuffer(DevsCPUFanCurve, fanMode)
        };
    }

    public bool IsInvalidCurve(byte[] curve)
    {
        return curve.Length != 16 || IsEmptyCurve(curve);
    }

    private bool IsEmptyCurve(byte[] curve)
    {
        return curve.All(singleByte => singleByte == 0);
    }

    public byte[] FixFanCurve(byte[] curve)
    {
        if (curve.Length != 16) throw new ArgumentException("Incorrect curve");

        var points = new Dictionary<byte, byte>();
        byte old = 0;

        for (int i = 0; i < 8; i++)
        {
            if (curve[i] == old) curve[i]++; // preventing 2 points in same spot from default asus profiles
            points[curve[i]] = curve[i + 8];
            old = curve[i];
        }

        var pointsFixed = new Dictionary<byte, byte>();
        bool fix = false;

        int count = 0;
        foreach (var pair in points.OrderBy(x => x.Key))
        {
            if (count == 0 && pair.Key >= 40)
            {
                fix = true;
                pointsFixed.Add(30, 0);
            }

            if (count != 3 || !fix)
                pointsFixed.Add(pair.Key, pair.Value);
            count++;
        }

        count = 0;
        foreach (var pair in pointsFixed.OrderBy(x => x.Key))
        {
            curve[count] = pair.Key;
            curve[count + 8] = pair.Value;
            count++;
        }

        return curve;
    }

    public bool IsXGConnected()
    {
        return DeviceGet(eGPUConnected) == 1;
    }

    public bool IsAllAmdPPT()
    {
        return DeviceGet(PPT_CPUB0) >= 0 && DeviceGet(GPUBoost) < 0;
    }
    
    public void ScanRange()
    {
        int value;
        string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\GHelper";
        string logFile = appPath + "\\scan.txt";
        for (uint i = 0x00000000; i <= 0x00160000; i++)
        {
            value = DeviceGet(i);
            if (value >= 0)
            {
                using (var writer = File.AppendText(logFile))
                {
                    writer.WriteLine(i.ToString("X8") + ": " + value.ToString("X4") + " (" + value + ")");
                    writer.Close();
                }
            }
        }
    }

    public void TUFKeyboardBrightness(int brightness)
    {
        int param = 0x80 | (brightness & 0x7F);
        DeviceSet(TUF_KB_BRIGHTNESS, param);
    }

    public void TUFKeyboardRGB(int mode, Color color, int speed)
    {
        byte[] setting = new byte[6];

        setting[0] = 0xb4;
        setting[1] = (byte)mode;
        setting[2] = color.R;
        setting[3] = color.G;
        setting[4] = color.B;
        setting[5] = (byte)speed;

        int result = DeviceSet(TUF_KB, setting);
        if (result != 1) DeviceSet(TUF_KB2, setting);
    }

    private const int ASUS_WMI_KEYBOARD_POWER_BOOT = 0x03 << 16;
    private const int ASUS_WMI_KEYBOARD_POWER_AWAKE = 0x0C << 16;
    private const int ASUS_WMI_KEYBOARD_POWER_SLEEP = 0x30 << 16;
    private const int ASUS_WMI_KEYBOARD_POWER_SHUTDOWN = 0xC0 << 16;
        
    public void TUFKeyboardPower(bool awake = true, bool boot = false, bool sleep = false, bool shutdown = false)
    {
        int state = 0xbd;

        if (boot) state |= ASUS_WMI_KEYBOARD_POWER_BOOT;
        if (awake) state |= ASUS_WMI_KEYBOARD_POWER_AWAKE;
        if (sleep) state |= ASUS_WMI_KEYBOARD_POWER_SLEEP;
        if (shutdown) state |= ASUS_WMI_KEYBOARD_POWER_SHUTDOWN;

        state |= 0x01 << 8;

        DeviceSet(TUF_KB_STATE, state);
    }

    public void SubscribeToEvents(Action<object, EventArgs> eventHandler)
    {
        _eventWatcher.EventArrived += new EventArrivedEventHandler(eventHandler);
    }

    public void Dispose()
    {
        CloseHandle(_handle);
        _eventWatcher.Stop();
        _eventWatcher.Dispose();
    }
}