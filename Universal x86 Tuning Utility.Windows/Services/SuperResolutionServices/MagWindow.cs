using System;
using System.IO;
using System.Threading;
using Avalonia.Threading;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.Windows.Services.SuperResolutionServices;

//
// This is a customised version of Magpie from https://github.com/Blinue/Magpie
// I do not take credit for the full functionality of the code.
//

public class MagWindow : IDisposable
{
    public event Action? Closed;

    public IntPtr SrcWindow { get; private set; } = IntPtr.Zero;

    private readonly Thread _magThread;

    // Used to indicate that magThread enters full screen
    private readonly AutoResetEvent _runEvent = new(false);

    private enum MagWindowCmd
    {
        None,
        Run,
        Exit,
        SetLogLevel
    }

    private class MagWindowParams
    {
        public volatile IntPtr hwndSrc;
        public volatile string effectsJson = string.Empty;
        public volatile int logLevel;
        public volatile MagWindowCmd cmd = MagWindowCmd.None;
    }

    private enum FlagMasks : uint
    {
        NoCursor = 0x1,
        AdjustCursorSpeed = 0x2,
        SaveEffectSources = 0x4,
        SimulateExclusiveFullscreen = 0x8,
        DisableLowLatency = 0x10,
        BreakpointMode = 0x20,
        DisableWindowResizing = 0x40,
        DisableDirectFlip = 0x80,
        Is3DMode = 0x100,
        CropTitleBarOfUWP = 0x200,
        DisableEffectCache = 0x400,
        DisableVSync = 0x800,
        WarningsAreErrors = 0x1000,
        ShowFPS = 0x2000
    }

    private readonly MagWindowParams _magWindowParams = new();

    // 用于从全屏窗口的线程接收消息
    private event Action<string?> CloseEvent;

    public bool IsRunning { get; private set; }
    
    private readonly string _logFileName = Path.GetFullPath("./SuperResolutionLog.txt");

    public MagWindow(IntPtr platformHandle)
    {
        _magThread = new Thread(() => {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            const int logArchiveAboveSize = 100000;
            const int logMaxArchiveFiles = 1;

            static uint ResolveLogLevel(uint logLevel)
            {
                return logLevel switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => 6,
                };
            }

            bool initSuccess = false;
            try
            {
                initSuccess = NativeMethods.Initialize(
                    ResolveLogLevel(Settings.Default.LoggingLevel),
                    _logFileName,
                    logArchiveAboveSize,
                    logMaxArchiveFiles
                );
            }
            catch (DllNotFoundException e)
            {
                
            }
            catch (Exception e)
            {
                    
            }

            if (!initSuccess)
            {
                CloseEvent?.Invoke("Msg_Error_Init");
                return;
            }

            while (_magWindowParams.cmd != MagWindowCmd.Exit)
            {
                _ = _runEvent.WaitOne(1000);

                var cmd = _magWindowParams.cmd;
                _magWindowParams.cmd = MagWindowCmd.None;

                if (cmd == MagWindowCmd.Exit)
                {
                    break;
                }

                if (cmd == MagWindowCmd.None)
                {
                    continue;
                }

                if (cmd == MagWindowCmd.SetLogLevel)
                {
                    NativeMethods.SetLogLevel(ResolveLogLevel((uint)_magWindowParams.logLevel));
                }
                else
                {
                    uint flags = (Settings.Default.NoCursor ? (uint)FlagMasks.NoCursor : 0) |
                                 (Settings.Default.AdjustCursorSpeed ? (uint)FlagMasks.AdjustCursorSpeed : 0) |
                                 (Settings.Default.DebugSaveEffectSources ? (uint)FlagMasks.SaveEffectSources : 0) |
                                 (Settings.Default.DisableLowLatency ? (uint)FlagMasks.DisableLowLatency : 0) |
                                 (Settings.Default.DebugBreakpointMode ? (uint)FlagMasks.BreakpointMode : 0) |
                                 (Settings.Default.DisableWindowResizing ? (uint)FlagMasks.DisableWindowResizing : 0) |
                                 (Settings.Default.DisableDirectFlip ? (uint)FlagMasks.DisableDirectFlip : 0) |
                                 (Settings.Default.Is3DMode ? (uint)FlagMasks.Is3DMode : 0) |
                                 (Settings.Default.CropTitleBarOfUWP ? (uint)FlagMasks.CropTitleBarOfUWP : 0) |
                                 (Settings.Default.DebugDisableEffectCache ? (uint)FlagMasks.DisableEffectCache : 0) |
                                 (Settings.Default.SimulateExclusiveFullscreen ? (uint)FlagMasks.SimulateExclusiveFullscreen : 0) |
                                 (Settings.Default.DebugWarningsAreErrors ? (uint)FlagMasks.WarningsAreErrors : 0) |
                                 (Settings.Default.VSync ? 0 : (uint)FlagMasks.DisableVSync) |
                                 (Settings.Default.ShowFPS ? (uint)FlagMasks.ShowFPS : 0);

                    bool customCropping = Settings.Default.CustomCropping;

                    string? msg = NativeMethods.Run(
                        _magWindowParams.hwndSrc,
                        _magWindowParams.effectsJson,
                        flags,
                        Settings.Default.CaptureMode,
                        Settings.Default.CursorZoomFactor,
                        Settings.Default.CursorInterpolationMode,
                        Settings.Default.AdapterIdx,
                        Settings.Default.MultiMonitorUsage,
                        customCropping ? Settings.Default.CropLeft : 0,
                        customCropping ? Settings.Default.CropTop : 0,
                        customCropping ? Settings.Default.CropRight : 0,
                        customCropping ? Settings.Default.CropBottom : 0
                    );

                    CloseEvent?.Invoke(msg);
                }
            }
        });

        _magThread.SetApartmentState(ApartmentState.MTA);
        _magThread.Start();

        CloseEvent += (string? errorMsgId) => {
            bool noError = string.IsNullOrEmpty(errorMsgId);

            if (noError && Closed != null)
            {
                Closed.Invoke();
            }
            SrcWindow = IntPtr.Zero;
            IsRunning = false;

            if (!noError)
            {
                Dispatcher.UIThread.Invoke(() =>
                {
                    _ = NativeMethods.SetForegroundWindow(platformHandle);
                });
            }
        };
    }

    public void Create(string effectsJson)
    {
        if (IsRunning)
        {
            return;
        }

        IntPtr hwndSrc = NativeMethods.GetForegroundWindow();
        if (!NativeMethods.IsWindow(hwndSrc)
            || !NativeMethods.IsWindowVisible(hwndSrc)
            || NativeMethods.GetWindowShowCmd(hwndSrc) != NativeMethods.SW_NORMAL
           )
        {
            return;
        }

        SrcWindow = hwndSrc;

        _magWindowParams.cmd = MagWindowCmd.Run;
        _magWindowParams.hwndSrc = hwndSrc;
        _magWindowParams.effectsJson = effectsJson;

        _ = _runEvent.Set();
        IsRunning = true;
    }

    public void SetLogLevel(uint logLevel)
    {
        _magWindowParams.cmd = MagWindowCmd.SetLogLevel;
        _magWindowParams.logLevel = (int)logLevel;

        _ = _runEvent.Set();
    }

    public void Destory()
    {
        if (!IsRunning)
        {
            return;
        }

        _ = NativeMethods.BroadcastMessage(NativeMethods.MAGPIE_WM_DESTORYHOST);
    }

    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        _magWindowParams.cmd = MagWindowCmd.Exit;

        if (IsRunning)
        {
            Destory();

            while (IsRunning)
            {
                Thread.Sleep(1);
            }
        }
        else
        {
            _ = _runEvent.Set();
            Thread.Sleep(1);
        }

        _runEvent.Dispose();
    }
}