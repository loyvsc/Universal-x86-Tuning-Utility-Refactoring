using Avalonia.Input.Platform;
using Avalonia.Platform;

namespace Universal_x86_Tuning_Utility.Interfaces;

public interface IPlatformServiceAccessor
{
    public IClipboard Clipboard { get; }
    public Screen? PrimaryScreen { get; }
    public bool IsMinimized { get; }
    public string ProductVersion { get; }
    public string PathToExecutable { get; }
}