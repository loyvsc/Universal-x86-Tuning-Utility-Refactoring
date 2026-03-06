using System;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using ApplicationCore.Interfaces;
using Universal_x86_Tuning_Utility.Windows.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsCpuAffinityService : ICpuAffinityService
{
    private readonly IManagementEventService _managementEventService;
    private readonly Lock _syncRoot = new();
    private IDisposable? _watcher;
    private ulong _mask;
    private int _currentMode = -1;

    public WindowsCpuAffinityService(IManagementEventService managementEventService)
    {
        _managementEventService = managementEventService;
    }

    public void SetGlobalAffinity(int mode)
    {
        if (mode == _currentMode) return;

        var newMask = BuildMask(mode);

        lock (_syncRoot)
        {
            if (mode == _currentMode) return;

            _currentMode = mode;
            _mask = newMask;

            foreach (var p in Process.GetProcesses())
                TrySetAffinity(p, _mask);

            _watcher ??= _managementEventService.SubscribeToQuery("SELECT ProcessID FROM Win32_ProcessStartTrace")
                .Subscribe(OnProcessStarted);
        }
    }
    
    private void OnProcessStarted(EventArrivedEventArgs e)
    {
        if (e.NewEvent?.Properties["ProcessID"]?.Value is int pid)
        {
            try
            {
                using var np = Process.GetProcessById(pid);
                TrySetAffinity(np, _mask);
            }
            catch { /* permission or race; ignore */ }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void TrySetAffinity(Process proc, ulong mask)
    {
        try { proc.ProcessorAffinity = (IntPtr)mask; }
        catch { /* safe to ignore */ }
    }

    private ulong BuildMask(int mode)
    {
        int logical = (int)GetActiveProcessorCount(ALL_GROUPS);

        if (logical < 2) throw new NotSupportedException("Needs more than one logical processor.");
        if (logical > 64) throw new NotSupportedException("Only one processor group supported.");

        int half = logical / 2;

        return mode switch
        {
            0 => (1UL << logical) - 1,                               // all cores
            1 => (1UL << half) - 1,                                  // lower half
            2 => ((1UL << logical) - 1) ^ ((1UL << half) - 1),       // upper half
            _ => throw new ArgumentOutOfRangeException(nameof(mode), "Mode must be 0, 1, or 2.")
        };
    }

    private const uint ALL_GROUPS = 0xFFFF;
    private const uint ERROR_CODE = 0;
    [DllImport("kernel32.dll")] private static extern uint GetActiveProcessorCount(uint groupNumber);

    public uint GetActiveProcessorsCount(uint groupNumber)
    {
        var num = GetActiveProcessorCount(groupNumber);
        if (num == ERROR_CODE)
            throw new Exception("Exception occured on get active processors count");

        return num;
    }

    public uint GetAllGroupsActiveProcessorsCount()
    {
        return GetActiveProcessorCount(ALL_GROUPS);
    }

    public void Stop()
    {
        lock (_syncRoot)
        {
            _watcher?.Dispose();
            _watcher = null;
            _currentMode = -1;
        }
    }
    
    public void Dispose()
    {
        Stop();
    }
}