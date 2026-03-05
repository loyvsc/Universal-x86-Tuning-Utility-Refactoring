namespace ApplicationCore.Models;

public class Device
{
    public string Name { get; set; }
    public string DeviceID { get; set; }
    public string PnpDeviceID { get; set; }
    public string Description { get; set; }

    public Device(string name, string deviceId, string pnpDeviceId, string description)
    {
        Name = name;
        DeviceID = deviceId;
        PnpDeviceID = pnpDeviceId;
        Description = description;
    }
}