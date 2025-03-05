using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using Universal_x86_Tuning_Utility.Navigation;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.ViewModels;

namespace Universal_x86_Tuning_Utility.Views.Windows;

public partial class MainWindow : Window, IDisposable
{
    public MainWindow()
    {
        InitializeComponent();
        
        Closing += UiWindow_Closing;
        Loaded += MainWindowLoaded;
        PropertyChanged += OnPropertyChanged;
    }
    
    private void UiWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
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
    
    public void OnNavigationViewItemInvoked(object sender, NavigationViewItemInvokedEventArgs e)
    {
        // Change the current selected item back to normal
        // SetNVIIcon(sender as NavigationViewItem, false);

        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            NavigationService.Instance.NavigateFromContext(nvi.Tag);
        }
    }

    private void MainWindowLoaded(object? sender, RoutedEventArgs e)
    {
        NavigationService.Instance.SetFrame(FrameView);
        if (DataContext is MainWindowViewModel viewModel)
        {
            FrameView.NavigationPageFactory = viewModel.NavigationPageFactory;
            NavigationService.Instance.NavigateFromContext(viewModel.NavigationItems[0]);
            if (Settings.Default.StartMini)
            {
                WindowState = WindowState.Minimized;
            }
            else
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