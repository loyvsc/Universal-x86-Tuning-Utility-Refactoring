using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IFanConfigService
{
    public FanData GetDataForDevice();
}