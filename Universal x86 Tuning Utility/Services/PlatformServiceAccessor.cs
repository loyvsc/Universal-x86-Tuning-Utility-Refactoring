using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Universal_x86_Tuning_Utility.Interfaces;

namespace Universal_x86_Tuning_Utility.Services;

public class PlatformServiceAccessor : IPlatformServiceAccessor
{
    public IClipboard Clipboard => _desktop.MainWindow!.Clipboard!;
    public Screen? PrimaryScreen => _desktop.MainWindow!.Screens.Primary;
    public bool IsMinimized => _desktop.MainWindow!.WindowState == WindowState.Minimized;
    public string ProductVersion { get; }
    public string PathToExecutable { get; }

    private readonly IClassicDesktopStyleApplicationLifetime _desktop;

    public PlatformServiceAccessor(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _desktop = desktop;
        
        var assembly = Assembly.GetExecutingAssembly();
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
        ProductVersion = fileVersionInfo.FileVersion!;

        PathToExecutable = Environment.ProcessPath!;
    }
}