using System;
using Avalonia;
using Avalonia.Controls;
using Universal_x86_Tuning_Utility.Properties;
using Window = Avalonia.Controls.Window;

namespace Universal_x86_Tuning_Utility.Views.Windows;

public partial class MainWindow : Window, IDisposable
{
    public MainWindow()
    {
        InitializeComponent();
        
        this.PropertyChanged += OnPropertyChanged;
    }
    
    private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (Settings.Default.MinimizeClose)
        {
            WindowState = WindowState.Minimized;
            e.Cancel = true;
        }
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            
        }
    }
    
    private void miClose_Click(object sender, RoutedEventArgs e)
    {
        Settings.Default.isAdaptiveModeRunning = false;
        Settings.Default.Save();
        Application.Current.Shutdown();
    }

    private void MainWindowLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Settings.Default.StartMini)
        {
            WindowState = WindowState.Minimized;
        }
        else
        {
            if (GetSystemInfo.Manufacturer.ToUpper().Contains("AYANEO") ||
                GetSystemInfo.Manufacturer.ToUpper().Contains("GPD") ||
                GetSystemInfo.Product.ToUpper().Contains("ONEXPLAYER"))
            {
                int displayCount = Screen.AllScreens.Length;
                if (displayCount < 2)
                {
                    MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
                    WindowState = WindowState.Maximized;
                }
            }
        }

        PremadePresets.InitializePremadePresets();
    }
    
    private void UiWindow_StateChanged(object sender, EventArgs e)
    {
        if (this.WindowState == WindowState.Minimized)
        {
            isMini = true;
            this.WindowStyle = WindowStyle.ToolWindow;
            ShowInTaskbar = false;
        }
        else
        {
            isMini = false;
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            ShowInTaskbar = true;
        }
    }
    
    private void NotifyIcon_LeftClick(Wpf.Ui.Controls.NotifyIcon sender, RoutedEventArgs e)
    {
        if (WindowState != WindowState.Minimized)
        {
            WindowState = WindowState.Minimized;
        }
        else
        {
            WindowState = WindowState.Normal;
            Activate();
        }
    }

    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
    }
}