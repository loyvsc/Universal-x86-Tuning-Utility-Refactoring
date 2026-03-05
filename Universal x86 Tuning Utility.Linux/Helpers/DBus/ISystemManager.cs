using System.Threading.Tasks;
using Tmds.DBus;

namespace Universal_x86_Tuning_Utility.Linux.Helpers.DBus;

[DBusInterface("org.freedesktop.systemd1.Manager")]
public interface ISystemdManager : IDBusObject
{
    public Task<string> StartUnitAsync(string name, string mode);
    public Task<string> StopUnitAsync(string name, string mode);
    public Task<string> EnableUnitFilesAsync(string[] files, bool runtime, bool force);
    public Task<string> DisableUnitFilesAsync(string[] files, bool runtime);
    public Task ReloadAsync();
    public Task<string> GetUnitAsync(string name);
}