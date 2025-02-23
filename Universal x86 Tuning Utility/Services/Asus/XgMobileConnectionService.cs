using System;
using System.Collections.Generic;
using System.Linq;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using HidLibrary;
using Microsoft.Extensions.Logging;

namespace Universal_x86_Tuning_Utility.Services.Asus;

public class XgMobileConnectionService : IXgMobileConnectionService
{
    private readonly ILogger<XgMobileConnectionService> _logger;
    private readonly IASUSWmiService _wmiService;
    private static readonly byte[] XG_MOBILE_CURVE_FUNC_NAME = { 0x5e, 0xd1, 0x01 };
    private static readonly byte[] XG_MOBILE_DISABLE_FAN_CONTROL_FUNC_NAME = { 0x5e, 0xd1, 0x02 };

    public bool Connected { get; private set; }
    public bool Detected { get; private set; }
        
    public event EventHandler<XgMobileStatusEventArgs>? XgMobileStatusChanged;

    public XgMobileConnectionService(ILogger<XgMobileConnectionService> logger,
        IASUSWmiService wmiService)
    {
        _wmiService = wmiService;
        _logger = logger;
        
        try
        {
            UpdateXgMobileStatus();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize XgMobileConnectionService");
        }
        _wmiService.SubscribeToEvents((_, _) => UpdateXgMobileStatus());
    }

    private void UpdateXgMobileStatus()
    {
        try
        {
            bool prevDetected = Detected;
            bool prevConnected = Connected;
                
            Detected = IsEGPUDetected();
            Connected = Detected && IsEGPUConnected();
                
            if (prevDetected != Detected || prevConnected != Connected)
            {
                XgMobileStatusChanged?.Invoke(this, new XgMobileStatusEventArgs
                {
                    Detected = Detected,
                    Connected = Connected,
                    DetectedChanged = prevDetected != Detected,
                    ConnectedChanged = prevConnected != Connected
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update UpdateXgMobileStatus");
        }
    }

    private bool IsEGPUDetected()
    {
        return _wmiService.DeviceGet(AsusDevice.EGpuConnected) == 1;
    }

    public bool IsEGPUConnected()
    {
        int deviceStatus = _wmiService.DeviceGet(AsusDevice.EGpuConnected);
        if (deviceStatus != 0 && deviceStatus != 1)
        {
            throw new InvalidOperationException($"Unknown device status: {deviceStatus}");
        }
        return _wmiService.DeviceGet(AsusDevice.EGpu) == 1;
    }

    public void EnableXgMobileLight()
    {
        SendXgMobileUsbCommand(new byte[] { 0x5e, 0xc5, 0x50 });
    }

    public void DisableXgMobileLight()
    {
        SendXgMobileUsbCommand(new byte[] { 0x5e, 0xc5 });
    }

    public bool SetXgMobileFan(List<AsusCurvePoint> points)
    {
        if (!ValidatePoints(points))
        {
            return false;
        }
        var paramsBytes = new List<byte>(XG_MOBILE_CURVE_FUNC_NAME);
        paramsBytes.AddRange(points.Select(point => (byte)point.Temperature)); // 8 bytes of temperature
        paramsBytes.AddRange(points.Select(point => (byte)point.Fan)); // 8 bytes of fan
        return SendXgMobileUsbCommand(paramsBytes.ToArray());
    }

    public bool ResetXgMobileFan()
    {
        return SendXgMobileUsbCommand(XG_MOBILE_DISABLE_FAN_CONTROL_FUNC_NAME);
    }

    private bool ValidatePoints(List<AsusCurvePoint> points)
    {
        if (points.Count != 8)
        {
            return false;
        }
        return true;
    }

    private bool SendXgMobileUsbCommand(byte[] command)
    {
        var devices = HidDevices.Enumerate(0x0b05, new int[] { 0x1970 });
        var xgMobileLight = devices.Where(device => device.IsConnected && device.Description.ToLower().StartsWith("hid") && device.Capabilities.FeatureReportByteLength > 64).ToList();

        if (xgMobileLight.Count != 1)
        {
            return false;
        }
            
        var device = xgMobileLight[0];
        try
        {
            device.OpenDevice();
            var paramsArr = new byte[300];
            Array.Copy(command, paramsArr, command.Length);
            return device.WriteFeatureData(paramsArr);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send xg mobile usb command");
            throw;
        }
        finally
        {
            device.CloseDevice();
        }
    }
}