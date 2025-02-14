using System;
using System.IO;
using System.Text.Json;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Universal_x86_Tuning_Utility.Services.Amd;

namespace Universal_x86_Tuning_Utility.Services.FanControlServices;

public class WindowsFanControlService : IFanControlService
{
    public int MaxFanSpeed { get; private set; } = 100;
    public int MinFanSpeed { get; private set; }
    public int MinFanSpeedPercentage  { get; private set; } = 25;

    public double FanSpeed { get; private set; }

    public bool FanControlEnabled { get; private set; }

    public bool IsFanEnabled => WinRingECManagement.ECRamRead(FanToggleAddress) == 0;
    
    private ushort FanToggleAddress;
    private ushort FanChangeAddress;

    private byte EnableToggleAddress;
    private byte DisableToggleAddress;

    private ushort RegAddress;
    private ushort RegData;
    
    private readonly ISystemInfoService _systemInfoService;

    public WindowsFanControlService(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
    }

    public void UpdateAddresses()
    {
        string path = $@"\Fan Configs\{_systemInfoService.Manufacturer.ToUpper()}_{_systemInfoService.Product.ToUpper()}.json";

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            var dataForDevice = JsonSerializer.Deserialize<FanData>(json);

            // Access data for the device
            MinFanSpeed = dataForDevice.MinFanSpeed;
            MaxFanSpeed = dataForDevice.MaxFanSpeed;
            MinFanSpeedPercentage = dataForDevice.MinFanSpeedPercentage;
            FanToggleAddress = Convert.ToUInt16(dataForDevice.FanControlAddress, 16);
            FanChangeAddress = Convert.ToUInt16(dataForDevice.FanSetAddress, 16);
            EnableToggleAddress = Convert.ToByte(dataForDevice.EnableToggleAddress, 16);
            DisableToggleAddress = Convert.ToByte(dataForDevice.DisableToggleAddress, 16);

            RegAddress = Convert.ToUInt16(dataForDevice.RegAddress, 16);
            RegData = Convert.ToUInt16(dataForDevice.RegData, 16);

            WinRingECManagement.reg_addr = RegAddress;
            WinRingECManagement.reg_data = RegData;
        }
    }

    public void EnableFanControl()
    {
        WinRingECManagement.ECRamWrite(FanToggleAddress, EnableToggleAddress);
        FanControlEnabled = true;
    }

    public void DisableFanControl()
    {
        WinRingECManagement.ECRamWrite(FanToggleAddress, DisableToggleAddress);
        FanControlEnabled = false;
    }

    public void SetFanSpeed(int speedPercentage)
    {
        if (speedPercentage < MinFanSpeedPercentage && speedPercentage > 0)
        {
            speedPercentage = MinFanSpeedPercentage;
        }

        byte setValue = (byte)Math.Round((double)speedPercentage / 100 * MaxFanSpeed, 0);
        WinRingECManagement.ECRamWrite(FanChangeAddress, setValue);

        FanSpeed = speedPercentage;
    }

    public void ReadFanSpeed()
    {
        byte returnValue = WinRingECManagement.ECRamRead(FanChangeAddress);

        double fanPercentage = Math.Round(100 * (Convert.ToDouble(returnValue) / MaxFanSpeed), 0);
        FanSpeed = fanPercentage;
    }
}

internal class WinRingECManagement
{
    public static ushort reg_addr;
    public static ushort reg_data;
    private static FanOls _ols = new FanOls();
    private static object _lock = new();
    
    public static void InitECWin4()
    {
        if (_ols == null)
            OlsInit();

        if (_ols == null)
            return;

        try
        {
            byte EC_Chip_ID1 = ECRamReadWin4(0x2000);

            if (EC_Chip_ID1 == 0x55)
            {
                byte EC_Chip_Ver = ECRamReadWin4(0x1060);
                EC_Chip_Ver = (byte)(EC_Chip_Ver | 0x80);

                ECRamWriteWin4(0x1060, EC_Chip_Ver);
            }

            //if (ols != null)
            //    OlsFree();
        }
        catch
        {
            OlsFree();
        }
    }
        
    public static byte ECRamReadWin4(ushort address)
    {
        if (_ols == null)
            OlsInit();

        if (_ols == null)
            return 0;

        byte data;
        byte highByte = (byte)((address >> 8) & 0xFF);
        byte lowByte = (byte)(address & 0xFF);
        
        try
        {
            lock (_lock)
            {
                reg_addr = 0x2E;
                reg_data = 0x2F;

                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x11);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, highByte);
                
                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x10);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, lowByte);
    
                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x12);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                
                data = _ols.ReadIoPortByte(reg_data);
            }

            //if (ols != null)
            //   OlsFree();
        }
        catch
        {
            OlsFree();
            return 0;
        }

        return data;

    }

    public static void ECRamWriteWin4(ushort address, byte data)
    {
        if (_ols == null)
            OlsInit();

        if (_ols == null)
            return;

        byte highByte = (byte)((address >> 8) & 0xFF);
        byte lowByte = (byte)(address & 0xFF);

        try
        {
            lock (_lock)
            {
                reg_addr = 0x2E;
                reg_data = 0x2F;

                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x11);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, highByte);

                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x10);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, lowByte);
                
                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x12);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, data);
            }
            
            //if (ols != null)
            //    OlsFree();
        }
        catch
        {
            OlsFree();
        }
    }
    
    public static void ECRamWrite(ushort address, byte data)
    {
        if (_ols == null)
            OlsInit();
        if (_ols == null)
            return;
        
        byte highByte = (byte)((address >> 8) & 0xFF);
        byte lowByte = (byte)(address & 0xFF);
        
        try
        {
            lock (_lock)
            {
                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x11);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, highByte);

                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x10);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, lowByte);

                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x12);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, data);
            }
        }
        catch
        {
            _ols = null;
        }
    }

    public static byte ECRamRead(ushort address)
    {
        if (_ols == null)
            OlsInit();
        if (_ols == null)
            return 0;
        
        byte data;
        byte high_byte = (byte)((address >> 8) & 0xFF);
        byte low_byte = (byte)(address & 0xFF);
        
        try
        {
            lock (_lock)
            {
                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x11);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, high_byte);

                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x10);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                _ols.WriteIoPortByte(reg_data, low_byte);

                _ols.WriteIoPortByte(reg_addr, 0x2E);
                _ols.WriteIoPortByte(reg_data, 0x12);
                _ols.WriteIoPortByte(reg_addr, 0x2F);
                data = _ols.ReadIoPortByte(reg_data);
            }
        }
        catch
        {
            _ols = null;
            return 0;
        }

        return data;
    }

    public static void OlsInit()
    {
        // Check support library sutatus
        switch (_ols.GetStatus())
        {
            case (uint)FanOls.Status.NO_ERROR:
                break;
            case (uint)FanOls.Status.DLL_NOT_FOUND:
                _ols = null;
                // MessageBox.Show("WingRing0 Status Error!! DLL_NOT_FOUND");
                break;
            case (uint)FanOls.Status.DLL_INCORRECT_VERSION:
                _ols = null;
                //  MessageBox.Show("WingRing0 Status Error!! DLL_INCORRECT_VERSION");
                break;
            case (uint)FanOls.Status.DLL_INITIALIZE_ERROR:
                _ols = null;
                //  MessageBox.Show("WingRing0 Status Error!! DLL_INITIALIZE_ERROR");
                break;
        }
        if (_ols == null)
        {
            //RaiseOlsInitFailedEvent();
            return;
        }

        // Check WinRing0 status
        switch (_ols.GetDllStatus())
        {
            case (uint)FanOls.OlsDllStatus.OLS_DLL_NO_ERROR:
                break;
            case (uint)FanOls.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED:
                _ols = null;
                break;
            case (uint)FanOls.OlsDllStatus.OLS_DLL_UNSUPPORTED_PLATFORM:
                _ols = null;
                break;
            case (uint)FanOls.OlsDllStatus.OLS_DLL_DRIVER_NOT_FOUND:
                _ols = null;
                break;
            case (uint)FanOls.OlsDllStatus.OLS_DLL_DRIVER_UNLOADED:
                _ols = null;
                break;
            case (uint)FanOls.OlsDllStatus.OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK:
                _ols = null;
                break;
            case (uint)FanOls.OlsDllStatus.OLS_DLL_UNKNOWN_ERROR:
                _ols = null;
                break;
        }
        if (_ols == null)
        {
            //RaiseOlsInitFailedEvent();
        }
    }

    private static void OlsFree()
    {
        if (_ols != null)
        {
            _ols.DeinitializeOls();
        }
    }
}