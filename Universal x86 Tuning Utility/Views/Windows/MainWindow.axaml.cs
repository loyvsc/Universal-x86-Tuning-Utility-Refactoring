using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Windowing;
using Universal_x86_Tuning_Utility.Navigation;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.ViewModels;

namespace Universal_x86_Tuning_Utility.Views.Windows;

public partial class MainWindow : AppWindow, IDisposable
{
    public MainWindow()
    {
        InitializeComponent();
        
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        
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
            NavigationService.Instance?.Navigate(nvi.Tag as Type);
        }
    }

    private void MainWindowLoaded(object? sender, RoutedEventArgs e)
    {
        NavigationService.Instance?.SetFrame(FrameView);
        NavigationService.Instance?.SetNavigationView(NavView);
        if (DataContext is MainWindowViewModel viewModel)
        {
            NavigationService.Instance?.Navigate(viewModel.NavigationItems[0].ViewModelType);
            if (Settings.Default.StartMini)
            {
                WindowState = WindowState.Minimized;
            }
            else
            {
                if (viewModel.IsPortableConsole)
                {
                    var topLevel = GetTopLevel(this);
                    var display = topLevel?.Screens?.ScreenFromWindow(this);
                    if (display?.IsPrimary == true)
                    {
                        WindowState = WindowState.Maximized;
                    }
                }
            }
        }
    }
    
    public void Dispose()
    {
        Closing -= UiWindow_Closing;
        Loaded -= MainWindowLoaded;
        PropertyChanged -= OnPropertyChanged;
    }
}