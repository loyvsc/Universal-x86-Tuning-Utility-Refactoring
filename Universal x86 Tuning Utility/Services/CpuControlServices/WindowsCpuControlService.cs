using System;
using System.Threading.Tasks;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.CpuControlServices;

public class WindowsCpuControlService : ICpuControlService
{
    /// <summary>
    /// Current power limit in watts
    /// </summary>
    public int CurrentPowerLimit { get; private set; }
    
    public string CpuCommand { get; private set; } = string.Empty;
    public string CoCommand { get; private set; } = string.Empty;
    
    private const int MinCurveOptimiserValue = 0; // CO
    private const int PowerLimitIncrement = 2; // watts
    private const int CurveOptimiserIncrement = 1; // CO

    private int _newPowerLimit = 999; // watts
    private int _newC0; // CO
    private int _lastC0; // CO
    private int _lastPowerLimit = 1000; // watts
    private int _prevCpuLoad = -1;
    
    private readonly ISystemInfoService _systemInfoService;
    private readonly IAmdApuControlService _amdApuControlService;

    public WindowsCpuControlService(ISystemInfoService systemInfoService,
                                    IAmdApuControlService amdApuControlService)
    {
        _systemInfoService = systemInfoService;
        _amdApuControlService = amdApuControlService;
    }

    public void UpdatePowerLimit(int temperature,
        int cpuLoad,
        int maxPowerLimit,
        int minPowerLimit,
        int maxTemperature)
    {
        if (temperature >= maxTemperature - 2)
        {
            // Reduce power limit if temperature is too high
            _newPowerLimit = Math.Max(minPowerLimit, _newPowerLimit - PowerLimitIncrement);
        }
        else if (cpuLoad > 10 && temperature <= maxTemperature - 5)
        {
            // Increase power limit if temperature allows and CPU load is high
            _newPowerLimit = Math.Min(maxPowerLimit, _newPowerLimit + PowerLimitIncrement);
        }
        
        if (_newPowerLimit < minPowerLimit) _newPowerLimit = minPowerLimit;
        if (_newPowerLimit > maxPowerLimit) _newPowerLimit = maxPowerLimit;
        
        // Apply new power limit if power limit has changed
        if (_newPowerLimit <= _lastPowerLimit - 1 || _newPowerLimit >= _lastPowerLimit + 1)
        {
            switch (_systemInfoService.Cpu.Manufacturer)
            {
                case Manufacturer.AMD:
                {
                    if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Apu)
                    {
                        int tdp = _newPowerLimit * 1000;

                        if (tdp >= 5000)
                        {
                            // Apply new power and temp limit
                            CpuCommand =
                                $"--tctl-temp={maxTemperature} --cHTC-temp={maxTemperature} --apu-skin-temp={maxTemperature} --stapm-limit={tdp}  --fast-limit={tdp} --stapm-time=64 --slow-limit={tdp} --slow-time=128 --vrm-current=300000 --vrmmax-current=300000 --vrmsoc-current=300000 --vrmsocmax-current=300000 ";
                            // Save new TDP to avoid unnecessary reapplies
                            _lastPowerLimit = _newPowerLimit;
                            _amdApuControlService.PowerLimit = _newPowerLimit;
                        }
                    }
                    else if (_systemInfoService.Cpu.ProcessorType == ProcessorType.Desktop)
                    {
                        int tdp = _newPowerLimit * 1000;

                        // Apply new power and temp limit
                        CpuCommand =
                            $"--tctl-temp={maxTemperature} --ppt-limit={tdp} --edc-limit={(int)(tdp * 1.33)} --tdc-limit={(int)(tdp * 1.33)} ";
                        _lastPowerLimit = _newPowerLimit;
                    }
                    break;
                }
                case Manufacturer.Intel:
                {
                    CpuCommand = $"--intel-pl={_newPowerLimit}";
                    _lastPowerLimit = _newPowerLimit;
                    break;
                }
                default: throw new ArgumentOutOfRangeException(nameof(_systemInfoService.Cpu.Manufacturer), _systemInfoService.Cpu.Manufacturer, "Unsupported cpu");
            }

            CurrentPowerLimit = _newPowerLimit;
        }
    }

    public void CurveOptimiserLimit(int cpuLoad, int maxCurveOptimiser)
    {
        int newMaxCO = maxCurveOptimiser;

        // Change max CO limit based on CPU usage
        if (cpuLoad < 10) newMaxCO = maxCurveOptimiser;
        else if (cpuLoad < 80) newMaxCO = maxCurveOptimiser - CurveOptimiserIncrement * 2;
        else newMaxCO = maxCurveOptimiser;

        if (_lastC0 == 0 && _prevCpuLoad <= 0) _lastC0 = newMaxCO;
        if (_prevCpuLoad < 0) _prevCpuLoad = 100;

        // Increase CO if the CPU load is increased by 10
        if (cpuLoad > _prevCpuLoad + 10)
        {
            _newC0 = _lastC0 + CurveOptimiserIncrement;

            // Store the current CPU load for the next iteration
            _prevCpuLoad += 10;
        }
        // Decrease CO if the CPU load is decreased by 10
        else if (cpuLoad < _prevCpuLoad - 10)
        {
            _newC0 = _lastC0 - CurveOptimiserIncrement;

            // Store the current CPU load for the next iteration
            _prevCpuLoad -= 10;
        }

        // Make sure min and max CO is not exceeded
        if (_newC0 <= MinCurveOptimiserValue) _newC0 = MinCurveOptimiserValue;
        if (_newC0 >= newMaxCO) _newC0 = newMaxCO;

        // Make sure CO is within CO max limit + 5
        if (_newC0 > 55) _newC0 = 55;

        if (cpuLoad < 5) _newC0 = 0;

        if (cpuLoad > 80) _newC0 = maxCurveOptimiser;

        // Apply new CO
        if (_newC0 != _lastC0) UpdateC0(_newC0);
    }

    private void UpdateC0(int newC0)
    {
        // Apply new CO
        if (newC0 > 0)
        {
            CoCommand = $"--set-coall={Convert.ToUInt32(0x100000 - (uint)newC0)} ";
        }
        else
        {
            CoCommand = "--set-coall=0 ";
        }

        // Save new CO to avoid unnecessary reapplies
        _lastC0 = newC0;
    }
}