using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IDeviceManagerService
{
    public IEnumerable<Device> GetDevices();
    public bool Contains(Func<Device, bool> predicate);
}