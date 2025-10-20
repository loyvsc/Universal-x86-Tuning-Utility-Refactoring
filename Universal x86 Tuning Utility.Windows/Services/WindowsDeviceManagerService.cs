using System;
using System.Collections.Generic;
using System.Management;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Serilog;
using Universal_x86_Tuning_Utility.Windows.Extensions;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsDeviceManagerService : IDeviceManagerService
{
    private readonly ILogger _logger;
    private readonly ManagementObjectSearcher _searcher;

    public WindowsDeviceManagerService(ILogger logger)
    {
        _logger = logger;
        _searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
    }
    
    public IEnumerable<Device> GetDevices()
    {
        var result = new List<Device>();
        try
        {
            foreach (var obj in _searcher.Get())
            {
                var device = new Device(name: obj.Get<string>("Name") ?? string.Empty,
                    deviceId: obj.Get<string>("DeviceID") ?? string.Empty,
                    pnpDeviceId: obj.Get<string>("PNPDeviceID") ?? string.Empty,
                    description: obj.Get<string>("Description") ?? string.Empty);
                result.Add(device);
            }
        }
        catch
        {
            _logger.Information("Failed to get devices");
        }

        return result;
    }

    public bool Contains(Func<Device, bool> predicate)
    {
        foreach (var obj in _searcher.Get())
        {
            var device = new Device(name: obj.Get<string>("Name") ?? string.Empty,
                deviceId: obj.Get<string>("DeviceID") ?? string.Empty,
                pnpDeviceId: obj.Get<string>("PNPDeviceID") ?? string.Empty,
                description: obj.Get<string>("Description") ?? string.Empty);

            if (predicate(device))
            {
                return true;
            }
        }

        return false;
    }
}