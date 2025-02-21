using Avalonia.Input.Platform;

namespace Universal_x86_Tuning_Utility.Interfaces;

public interface IPlatformServiceAccessor
{
    IClipboard Clipboard { get; }
}