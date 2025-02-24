using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform;
using Universal_x86_Tuning_Utility.Interfaces;

namespace Universal_x86_Tuning_Utility.Services;

public class DefaultPlatformServiceAccessor : IPlatformServiceAccessor
{
    public IClipboard Clipboard => _desktop.MainWindow!.Clipboard!;
    public Screen? PrimaryScreen => _desktop.MainWindow!.Screens.Primary;
    
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;

    public DefaultPlatformServiceAccessor(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _desktop = desktop;
    }
}