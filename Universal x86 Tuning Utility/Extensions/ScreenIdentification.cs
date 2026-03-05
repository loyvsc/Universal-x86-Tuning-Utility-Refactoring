using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace Universal_x86_Tuning_Utility.Extensions;

public static class ScreenIdentification
{
    public static void Show(int timeout = 2)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var screens = desktop.MainWindow?.Screens;
            
            for (var i = 0; i < screens?.All.Count; i++)
            {
                var screen = screens.All[i];
                var window = new Window
                {
                    SystemDecorations = SystemDecorations.None,
                    Background = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0)), // полупрозрачный чёрный
                    CanResize = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Position = new PixelPoint(screen.Bounds.X, screen.Bounds.Y),
                    Width = screen.Bounds.Width,
                    Height = screen.Bounds.Height,
                    Topmost = true,
                    ShowInTaskbar = false,
                    Content = new TextBlock
                    {
                        Text = (i + 1).ToString(),
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = screen.Bounds.Height / 2.0,
                        FontWeight = FontWeight.Bold
                    }
                };

                window.Show();

                DispatcherTimer.RunOnce(() => window.Close(), TimeSpan.FromSeconds(timeout));
            }
        }
    }
}