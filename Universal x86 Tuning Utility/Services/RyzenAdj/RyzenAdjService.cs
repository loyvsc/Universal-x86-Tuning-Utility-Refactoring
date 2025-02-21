using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;
using Universal_x86_Tuning_Utility.Properties;
using Universal_x86_Tuning_Utility.Services.Amd;
using Universal_x86_Tuning_Utility.Services.Asus;
using Universal_x86_Tuning_Utility.Services.PowerPlanServices;

namespace Universal_x86_Tuning_Utility.Services.RyzenAdj;

public class RyzenAdjService : IRyzenAdjService
{
    private readonly ILogger<RyzenAdjService> _logger;
    private readonly IDisplayInfoService _displayInfoService;
    private readonly IIntelManagementService _intelManagementService;
    private readonly IAmdGpuService _amdGpuService;
    private readonly INvidiaGpuService _nvidiaGpuService;
    private readonly ISystemInfoService _systemInfoService;
    private readonly IASUSWmiService _asusWmiService;
    private readonly IPowerPlanService _powerPlanService;

    public RyzenAdjService(ILogger<RyzenAdjService> logger,
                           IDisplayInfoService displayInfoService,
                           IIntelManagementService intelManagementService,
                           IAmdGpuService amdGpuService,
                           INvidiaGpuService nvidiaGpuService,
                           ISystemInfoService systemInfoService,
                           IASUSWmiService asusWmiService,
                           IPowerPlanService powerPlanService)
    {
        _logger = logger;
        _displayInfoService = displayInfoService;
        _intelManagementService = intelManagementService;
        _amdGpuService = amdGpuService;
        _nvidiaGpuService = nvidiaGpuService;
        _systemInfoService = systemInfoService;
        _asusWmiService = asusWmiService;
        _powerPlanService = powerPlanService;
    }

    //Translate RyzenAdj like cli arguments to UXTU
    public async Task Translate(string ryzenAdjArgs, bool isAutoReapply = false)
    {
        try
        {
            //Remove last space off cli arguments 
            ryzenAdjArgs = ryzenAdjArgs.Trim();

            var ryzenAdjCommands = ryzenAdjArgs
                .Split(' ')
                .Distinct()
                .ToArray();

            //Run through array
            foreach (var ryzenAdjCommand in ryzenAdjCommands)
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var command = ryzenAdjCommand;
                        if (!command.Contains('=')) command = ryzenAdjCommand + "=0";
                        // Extract the command string before the "=" sign
                        var ryzenAdjCommandString = command.Split('=')[0].Replace("=", null).Replace("--", null).Trim();
                        // Extract the command string after the "=" sign
                        var ryzenAdjCommandValueString = command.Substring(ryzenAdjCommand.IndexOf('=') + 1);

                        // todo: check this shit
                        if (ryzenAdjCommandString.Contains("UXTUSR"))
                        {
                            UXTUSR(ryzenAdjCommandString, ryzenAdjCommandValueString);
                            Task.Delay(50);
                        }
                        else if (ryzenAdjCommandString.Contains("Power-Plan"))
                        {
                            if (Enum.TryParse<PowerPlan>(ryzenAdjCommandString, out var powerPlan))
                            {
                                _powerPlanService.SetPowerPlan(powerPlan);
                            }
                        }
                        else if (ryzenAdjCommandString.Contains("ASUS"))
                        {
                            AsusWmi(ryzenAdjCommandString, ryzenAdjCommandValueString);
                        }
                        else if (ryzenAdjCommandString.Contains("Refresh-Rate"))
                        {
                            _displayInfoService.ApplySettings(Convert.ToInt32(ryzenAdjCommandValueString));
                        }
                        else if (ryzenAdjCommandString.Contains("ADLX"))
                        {
                            ADLX(ryzenAdjCommandString, ryzenAdjCommandValueString);
                        }
                        else if (ryzenAdjCommandString.Contains("NVIDIA"))
                        {
                            NVIDIA(ryzenAdjCommandString, ryzenAdjCommandValueString);
                        }
                        else if (ryzenAdjCommandString.Contains("intel"))
                        {
                            if (ryzenAdjCommandValueString.Contains('-'))
                            {
                                if (ryzenAdjCommandString == "intel-ratio")
                                {
                                    var stringArray = ryzenAdjCommandValueString.Split('-');
                                    var intArray = stringArray.Select(int.Parse).ToArray();

                                    _intelManagementService.ChangeClockRatioOffset(intArray);
                                }
                            }
                            else
                            {
                                //Convert value of select cli argument to int
                                int ryzenAdjCommandValue = Convert.ToInt32(ryzenAdjCommandValueString);

                                switch (ryzenAdjCommandString)
                                {
                                    case "intel-pl":
                                    {
                                        _intelManagementService.ChangeTdpAll(ryzenAdjCommandValue);
                                        break;
                                    }
                                    case "intel-volt-cpu":
                                    {
                                        _intelManagementService.ChangeVoltageOffset(0,
                                            (IntelVoltagePlan)ryzenAdjCommandValue);
                                        break;
                                    }
                                    case "intel-volt-gpu":
                                    {
                                        _intelManagementService.ChangeVoltageOffset(1,
                                            (IntelVoltagePlan)ryzenAdjCommandValue);
                                        break;
                                    }
                                    case "intel-volt-cache":
                                    {
                                        _intelManagementService.ChangeVoltageOffset(2,
                                            (IntelVoltagePlan)ryzenAdjCommandValue);
                                        break;
                                    }
                                    case "intel-volt-sa":
                                    {
                                        _intelManagementService.ChangeVoltageOffset(3,
                                            (IntelVoltagePlan)ryzenAdjCommandValue);
                                        break;
                                    }
                                    case "intel-bal-cpu":
                                    {
                                        _intelManagementService.ChangePowerBalance(0,
                                            (IntelPowerBalanceUnit)ryzenAdjCommandValue);
                                        break;
                                    }
                                    case "intel-bal-gpu":
                                    {
                                        _intelManagementService.ChangePowerBalance(1,
                                            (IntelPowerBalanceUnit)ryzenAdjCommandValue);
                                        break;
                                    }
                                    case "intel-gpu":
                                    {
                                        _intelManagementService.SetGpuClock(ryzenAdjCommandValue);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Convert value of select cli argument to uint
                            uint ryzenAdjCommandValue = Convert.ToUInt32(ryzenAdjCommandValueString);
                            if (ryzenAdjCommandValue <= 0 && !ryzenAdjCommandString.Contains("co"))
                            {
                                SMUCommands.ApplySettings(ryzenAdjCommandString, 0x0);
                            }
                            else
                            {
                                SMUCommands.ApplySettings(ryzenAdjCommandString, ryzenAdjCommandValue);
                            }

                            Task.Delay(50);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An exception occurred in RyzenAdjService");
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred in RyzenAdjService");
        }
    }
    
    private void ADLX(string command, string value)
    {
        try
        {
            string[] variables = value.Split('-');

            switch (command)
            {
                case "ADLX-Lag":
                    _amdGpuService.SetAntilag(gpuId: int.Parse(variables[0]), 
                                              isEnabled: bool.Parse(variables[1]));
                    break;
                case "ADLX-Boost":
                    _amdGpuService.SetBoost(gpuId: int.Parse(variables[0]),
                                            percent: int.Parse(variables[2]), 
                                            isEnabled: bool.Parse(variables[1]));
                    break;
                case "ADLX-RSR":
                    _amdGpuService.IsRsrEnabled = bool.Parse(variables[0]);
                    _amdGpuService.RsrSharpness = int.Parse(variables[1]);
                    break;
                case "ADLX-Chill":
                    _amdGpuService.SetChill(gpuId: int.Parse(variables[0]), 
                                            maxFps: int.Parse(variables[2]),
                                            minFps: int.Parse(variables[3]), 
                                            isEnabled: bool.Parse(variables[1]));
                    break;
                case "ADLX-Sync":
                    _amdGpuService.SetEnhancedSynchronization(gpuId: int.Parse(variables[0]), 
                                                              isEnabled: bool.Parse(variables[1]));
                    break;
                case "ADLX-ImageSharp":
                    _amdGpuService.SetImageSharpening(gpuId: int.Parse(variables[0]),
                                                      percent: int.Parse(variables[2]),
                                                      isEnabled: bool.Parse(variables[1]));
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when executing ADLX command");
        }
    }

    private void UXTUSR(string command, string value)
    {
        try
        {
            var args = value.Split('-');

            if (command == "UXTUSR")
            {
                Settings.Default.AdapterIdx = 0;
                Settings.Default.isMagpie = Convert.ToBoolean(args[0]);
                Settings.Default.VSync = Convert.ToBoolean(args[1]);
                Settings.Default.Sharpness = Convert.ToDouble(args[2]);
                Settings.Default.ResMode = Convert.ToInt32(args[3]);
                Settings.Default.AutoRestore = Convert.ToBoolean(args[0]);
                Settings.Default.Save();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when executing UXTUSR command");
        }
    }

    private void NVIDIA(string command, string value)
    {
        try
        {
            var args = value.Split('-');

            if (command == "NVIDIA-Clocks")
            {
                switch (args.Length)
                {
                    case 2:
                    {
                        _nvidiaGpuService.SetClocks(int.Parse(args[0]), int.Parse(args[1]));
                        break;
                    }
                    case 3:
                    {
                        _nvidiaGpuService.MaxGpuClock = int.Parse(args[0]);
                        _nvidiaGpuService.SetClocks(int.Parse(args[1]), int.Parse(args[2]));
                        break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when executing NVIDIA command");
        }
    }

    private void AsusWmi(string command, string value)
    {
        try
        {
            uint id = 0;
            int mode = 0;
            switch (command)
            {
                case "ASUS-Power":
                {
                    if (_systemInfoService.Product.Contains("ROG") || _systemInfoService.Product.Contains("TUF"))
                    {
                        id = (uint) AsusDeviceMode.PerformanceMode;
                    }
                    else
                    {
                        id = (uint) AsusDeviceMode.VivoBookMode;
                    }

                    mode = value switch
                    {
                        "1" => (int) AsusMode.Silent,
                        "2" => (int) AsusMode.Balanced,
                        "3" => (int) AsusMode.Turbo,
                    };
                    if (_asusWmiService.DeviceGet(id) != mode)
                    {
                        _asusWmiService.DeviceSet(id, mode, "PowerMode");
                    }
                    break;
                }
                case "ASUS-Eco":
                {
                    if (value.ToLower() == "true")
                    {
                        _asusWmiService.SetGPUEco(1);
                    }
                    else
                    {
                        _asusWmiService.SetGPUEco(0);
                    }
                    break;
                }
                case "ASUS-MUX":
                {
                    if (!_isUpdatingMux)
                    {
                        if (_systemInfoService.Product.Contains("ROG") || _systemInfoService.Product.Contains("TUF"))
                        {
                            id = AsusWmiService.GPUMux;
                        }
                        else
                        {
                            id = AsusWmiService.GPUMuxVivo;
                        }

                        int mux = _asusWmiService.DeviceGet(id);
                        if (mux > 0 && value.ToLower() == "true")
                        {
                            _isUpdatingMux = true;
                            if (_systemInfoService.Product.Contains("ROG") ||
                                _systemInfoService.Product.Contains("TUF"))
                            {
                                id = AsusWmiService.GPUMux;
                            }
                            else
                            {
                                id = AsusWmiService.GPUMuxVivo;
                            }
                            _asusWmiService.DeviceSet(id, 0, "MUX");
                            
                            Process.Start("shutdown", "/r /t 1");
                        }
                        else if (mux < 1 && mux > -1 && value.ToLower() == "false")
                        {
                            _isUpdatingMux = true;
                            if (_systemInfoService.Product.Contains("ROG") ||
                                _systemInfoService.Product.Contains("TUF"))
                            {
                                id = AsusWmiService.GPUMux;
                            }
                            else
                            {
                                id = AsusWmiService.GPUMuxVivo;
                            }
                            _asusWmiService.DeviceSet(id, 1, "MUX");
                            
                            Process.Start("shutdown", "/r /t 1");

                            // todo: add this message before apply settings
                            // messageBox.Show("GPU Ultimate Mode",
                            //     "Disabling GPU Ultimate Mode requires a restart to take\naffect!");
                        }
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when executing Asus Wmi command");
        }
    }

    private bool _isUpdatingMux = false;
}