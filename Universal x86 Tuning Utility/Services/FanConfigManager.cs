using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Services;

internal class FanConfigManager : IFanConfigService
{
    private readonly string _configDirectory;

    public FanConfigManager(string configDirectory)
    {
        _configDirectory = configDirectory;
    }

    public FanData GetDataForDevice()
    {
        var json = File.ReadAllText(_configDirectory);
        return JsonConvert.DeserializeObject<FanData>(json);
    }
}