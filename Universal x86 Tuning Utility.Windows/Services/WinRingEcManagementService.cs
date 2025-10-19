using System;
using System.Threading;
using Serilog;
using Universal_x86_Tuning_Utility.Windows.Interfaces;
using Universal_x86_Tuning_Utility.Windows.Services.Amd.Windows;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WinRingEcManagementService : IWinRingEcManagementService, IDisposable
{
    public ushort RegAddress { get; set; }
    public ushort RegData { get; set; }

    private bool _isInitialized;
    private FanOls? _ols = new FanOls();
    private readonly ILogger _logger;
    private readonly Lock _lock = new();

    public WinRingEcManagementService(ILogger logger)
    {
        _logger = logger;
    }
    
    public void InitECWin4()
    {
        if (_ols == null)
            Initialize();

        if (_ols == null)
            return;

        try
        {
            byte ecChipId1 = ECRamReadWin4(0x2000);

            if (ecChipId1 == 0x55)
            {
                byte ecChipVer = ECRamReadWin4(0x1060);
                ecChipVer = (byte)(ecChipVer | 0x80);

                ECRamWriteWin4(0x1060, ecChipVer);
                
                _logger.Information("ECWin4 initialized");
            }
        }
        catch
        {
            _logger.Error("ECWin4 initialization failed");
            Free();
        }
    }
        
    public byte ECRamReadWin4(ushort address)
    {
        if (_ols == null)
            Initialize();

        if (_ols == null)
            return 0;

        byte data;
        byte highByte = (byte)((address >> 8) & 0xFF);
        byte lowByte = (byte)(address & 0xFF);
        
        try
        {
            lock (_lock)
            {
                RegAddress = 0x2E;
                RegData = 0x2F;

                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x11);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, highByte);
                
                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x10);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, lowByte);
    
                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x12);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                
                data = _ols.ReadIoPortByte(RegData);
            }
                
            _logger.Information("ECRam read (win4) of {address} has been finished with value {value}", address, data);
        }
        catch
        {
            _logger.Error("ECRam read (win4) failed");
            Free();
            return 0;
        }

        return data;
    }

    public void ECRamWriteWin4(ushort address, byte data)
    {
        if (_ols == null)
            Initialize();

        if (_ols == null)
            return;

        byte highByte = (byte)((address >> 8) & 0xFF);
        byte lowByte = (byte)(address & 0xFF);

        try
        {
            lock (_lock)
            {
                RegAddress = 0x2E;
                RegData = 0x2F;

                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x11);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, highByte);

                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x10);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, lowByte);
                
                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x12);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, data);
            }
            _logger.Information("ECRam write (win4) {value} at {address}  has been finished", data, address);
        }
        catch
        {
            _logger.Error("ECRam write (win4) failed");
            Free();
        }
    }
    
    public void ECRamWrite(ushort address, byte data)
    {
        if (_ols == null)
            Initialize();
        if (_ols == null)
            return;
        
        byte highByte = (byte)((address >> 8) & 0xFF);
        byte lowByte = (byte)(address & 0xFF);
        
        try
        {
            lock (_lock)
            {
                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x11);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, highByte);

                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x10);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, lowByte);

                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x12);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, data);
            }
            _logger.Information("ECRam write {value} at {address}  has been finished", data, address);
        }
        catch
        {
            _logger.Error("ECRam write failed");
            _ols = null;
        }
    }

    public byte ECRamRead(ushort address)
    {
        if (_ols == null)
            Initialize();
        if (_ols == null)
            return 0;
        
        byte data;
        byte highByte = (byte)((address >> 8) & 0xFF);
        byte lowByte = (byte)(address & 0xFF);
        
        try
        {
            lock (_lock)
            {
                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x11);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, highByte);

                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x10);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                _ols.WriteIoPortByte(RegData, lowByte);

                _ols.WriteIoPortByte(RegAddress, 0x2E);
                _ols.WriteIoPortByte(RegData, 0x12);
                _ols.WriteIoPortByte(RegAddress, 0x2F);
                data = _ols.ReadIoPortByte(RegData);
            }
            _logger.Information("ECRam read (win4) of {address} has been finished with value {value}", address, data);
        }
        catch
        {
            _logger.Error("ECRam read failed");
            _ols = null;
            return 0;
        }

        return data;
    }

    public void Initialize()
    {
        _isInitialized = false;
        var status = _ols?.GetStatus();
        if (status != Ols.Status.NO_ERROR)
        {
            _ols = null;
            _logger.Error("WinRing0 Status Error: {status}", status);
            return;
        }
        
        var dllStatus = _ols!.GetDllStatus();
        if (dllStatus != (uint)OpenLibSys_Mem.Ols.OlsDllStatus.OLS_DLL_NO_ERROR)
        {
            _ols = null;
            _logger.Error("WinRing0 DllStatus Error: {dllStatus}", (OpenLibSys_Mem.Ols.OlsDllStatus)dllStatus);
        }
        else
        {
            _isInitialized = true;
            _logger.Information("WinRing0 initialized");
        }
    }

    public void Free()
    {
        lock (_lock)
        {
            if (_isInitialized)
            {
                _ols?.DeinitializeOls();
                _isInitialized = false;
                _logger.Information("WinRing0 resources are released");
            }
        }
    }

    public void Dispose()
    {
        Free();
    }
}