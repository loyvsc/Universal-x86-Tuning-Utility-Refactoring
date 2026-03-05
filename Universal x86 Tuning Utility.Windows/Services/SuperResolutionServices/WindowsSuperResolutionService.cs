using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Gma.System.MouseKeyHook;
using Universal_x86_Tuning_Utility.Interfaces;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Views.Windows;

namespace Universal_x86_Tuning_Utility.Windows.Services.SuperResolutionServices;

//
// This is a customised version of Magpie from https://github.com/Blinue/Magpie
// I do not take credit for the full functionality of the code.
//

public class WindowsSuperResolutionService
{
    private readonly IPlatformServiceAccessor _platformServiceAccessor;
    public MagWindow? MagWindow;

    private readonly ScaleModelManager _scaleModelManager = new();

    private string _appName = string.Empty;
    private string _currentAppName = string.Empty;
    private bool _canReapply;
    
    public WindowsSuperResolutionService(IPlatformServiceAccessor platformServiceAccessor)
    {
        _platformServiceAccessor = platformServiceAccessor;
    }

    private IntPtr _handle;
    // private IntPtr _prevSrcWindow = IntPtr.Zero; 

    #region PInvoke declarations

    private const uint SWP_SHOWWINDOW = 0x0040;
    // private const int SPI_GETWORKAREA = 48;
    // private const int SM_CYBORDER = 6;
    // private const int GW_OWNER = 4;
    private const int GWL_STYLE = -16;
    private const int WS_CHILD = 0x40000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    
    private IKeyboardMouseEvents? keyboardEvents = null;

    private void ToggleOverlay()
    {
        NativeMethods.BroadcastMessage(NativeMethods.MAGPIE_WM_TOGGLE_OVERLAY);
    }

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    // [DllImport("user32.dll")]
    // private static extern bool SystemParametersInfo(int uAction, int uParam, ref RECT lpvParam, int fuWinIni);
    
    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
    
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    #endregion

    private void ToggleMagWindow()
    {
        if (Settings.Default.isMagpie || MagWindow.IsRunning)
        {
            if (!_scaleModelManager.IsValid() || MagWindow == null)
            {
                return;
            }

            if (MagWindow.IsRunning)
            {
                MagWindow.Destory();
                _canReapply = false;
                using (var currentProcess = Process.GetCurrentProcess())
                {
                    currentProcess.PriorityClass = ProcessPriorityClass.Normal;
                }
                return;
            }

            using (var currentProcess = Process.GetCurrentProcess())
            {
                currentProcess.PriorityClass = ProcessPriorityClass.AboveNormal;
            }
            _canReapply = true;

            string sharpness = Convert.ToString(Settings.Default.Sharpness, CultureInfo.InvariantCulture);

            double scaleFactor = Settings.Default.ResMode switch
            {
                1 => 0.77,
                2 => 0.67,
                3 => 0.59,
                4 => 0.50,
                5 => 0.33,
                _ => 0.59
            };

            var foregroundWindow = GetForegroundWindow();
            if (foregroundWindow != IntPtr.Zero && Settings.Default.ResMode > 0)
            {
                if (GetWindowRect(foregroundWindow, out _))
                {
                    var primaryScreen = _platformServiceAccessor.PrimaryScreen!;
                    int screenWidth = primaryScreen.Bounds.Width;
                    int screenHeight = primaryScreen.Bounds.Height;
                    
                    int newWidth = (int)Math.Floor(screenWidth * scaleFactor);
                    int newHeight = (int)Math.Floor(screenHeight * scaleFactor);

                    int windowStyle = GetWindowLong(foregroundWindow, GWL_STYLE);

                    var windowText = new StringBuilder(256);
                    _ = GetWindowText(foregroundWindow, windowText, windowText.Capacity);

                    _appName = windowText.ToString();

                    // Check if the window has the WS_BORDER style bit set
                    bool isBorderless = (windowStyle & 0x00800000) == 0;

                    if (!isBorderless)
                    {
                        windowStyle = GetWindowLong(foregroundWindow, GWL_STYLE);
                        int extendedWindowStyle = GetWindowLong(foregroundWindow, GWL_STYLE);

                        // Check if it's a Win32 application by looking for certain window styles
                        var isWin32Application = (windowStyle & WS_CHILD) == 0 && (extendedWindowStyle & WS_EX_TOOLWINDOW) == 0;

                        if (isWin32Application)
                        {
                            newHeight += 32;
                        }
                        else
                        {
                            newHeight += 40;
                        }
                    }

                    int newX = (screenWidth - newWidth) / 2;
                    int newY = (screenHeight - newHeight) / 2;

                    SetWindowPos(foregroundWindow, IntPtr.Zero, newX, newY, newWidth, newHeight, SWP_SHOWWINDOW);
                }
            }

            string effectsJson = _scaleModelManager.GetScaleModels()![Settings.Default.ScaleMode].Effects;

            int index = effectsJson.LastIndexOf(":");
            if (index >= 0)
                effectsJson = effectsJson.Substring(0, index);

            effectsJson = effectsJson + ":" + sharpness + "}]";

            MagWindow.Create(effectsJson);

            // _prevSrcWindow = MagWindow.SrcWindow;
        }
    }

    public void AutoRestore_Tick(object sender, EventArgs e)
    {
        if (Settings.Default.AutoRestore && Settings.Default.isMagpie && _canReapply)
        {
            var foregroundWindow = GetForegroundWindow();

            if (foregroundWindow != IntPtr.Zero)
            {
                var windowText = new StringBuilder(256);
                _ = GetWindowText(foregroundWindow, windowText, windowText.Capacity);
                _currentAppName = windowText.ToString();

                if (_currentAppName == _appName && MagWindow?.IsRunning == false) ToggleMagWindow();
            }
        }
    }
   
    // private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    // {
    //     if (msg == NativeMethods.MAGPIE_WM_SHOWME)
    //     {
    //         _ = NativeMethods.SetForegroundWindow(_handle);
    //         handled = true;
    //     }
    //     return IntPtr.Zero;
    // }

    public void SetUpMagWindow(MainWindow main)
    {
        _handle = TopLevel.GetTopLevel(main)!.TryGetPlatformHandle()!.Handle;

        MagWindow = new MagWindow(_handle);

        OnHotkeyChanged();
    }

    private bool OnHotkeyChanged()
    {
        keyboardEvents?.Dispose();
        keyboardEvents = Hook.GlobalEvents();

        try
        {
            keyboardEvents.OnCombination(new Dictionary<Combination, Action> 
            {
                {Combination.FromString(Settings.Default.MagpieHotkey), ToggleMagWindow},
            });
        }
        catch
        {
            return false;
        }

        return true;
    }

    public void Dispose()
    {
        MagWindow?.Dispose();
    }
}