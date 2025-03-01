using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.ViewModels;

namespace Universal_x86_Tuning_Utility.Views.Windows;

public partial class MainWindow : Window, IDisposable
{
    public MainWindow()
    {
        InitializeComponent();
        
        PropertyChanged += OnPropertyChanged;
    }
    
    private void UiWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        if (Settings.Default.MinimizeClose)
        {
            WindowState = WindowState.Minimized;
            e.Cancel = true;
        }
        
        Settings.Default.isAdaptiveModeRunning = false;
        Settings.Default.Save();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == WindowStateProperty)
        {
            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }
    }

    private void MainWindowLoaded(object? sender, RoutedEventArgs e)
    {
        if (Settings.Default.StartMini)
        {
            WindowState = WindowState.Minimized;
        }
        else
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                var manufacturer = viewModel.ProductManufacturer.ToUpper();
                if (manufacturer.Contains("AYANEO") ||
                    manufacturer.Contains("GPD") ||
                    manufacturer.Contains("ONEXPLAYER"))
                {
                    var topLevel = GetTopLevel(this)!;
                    var displayCount = topLevel.Screens!.ScreenCount;
                    if (displayCount == 1)
                    {
                        var maxHeight = topLevel.Screens.Primary!.Bounds.Height;
                        MaxHeight = maxHeight;
                        WindowState = WindowState.Maximized;
                    }
                }
            }
        }
    }
    
    public void Dispose()
    {
        PropertyChanged -= OnPropertyChanged;
    }
}