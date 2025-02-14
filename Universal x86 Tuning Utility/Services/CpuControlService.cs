using System;
using System.Threading.Tasks;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services;

public class CpuControlService
{
    private readonly ISystemInfoService _systemInfoService;
    private int MinCurveOptimiser = 0; // CO
    private const int PowerLimitIncrement = 2; // watts
    private const int CurveOptimiserIncrement = 1; // CO

    private int _newPowerLimit = 999; // watts
    public int _currentPowerLimit; // watts
    private int _newCO; // CO
    private int _lastCO; // CO
    private int _lastPowerLimit = 1000; // watts

    public string cpuCommand = "";
    public string coCommand = "";

    public CpuControlService(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
    }

    public async Task UpdatePowerLimit(int temperature,
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
            switch (_systemInfoService.CpuInfo.Manufacturer)
            {
                case Manufacturer.AMD:
                {
                    if (_systemInfoService.CpuInfo.AmdProcessorType == AmdProcessorType.Apu)
                    {
                        int tdp = _newPowerLimit * 1000;

                        if (tdp >= 5000)
                        {
                            // Apply new power and temp limit
                            cpuCommand =
                                $"--tctl-temp={maxTemperature} --cHTC-temp={maxTemperature} --apu-skin-temp={maxTemperature} --stapm-limit={tdp}  --fast-limit={tdp} --stapm-time=64 --slow-limit={tdp} --slow-time=128 --vrm-current=300000 --vrmmax-current=300000 --vrmsoc-current=300000 --vrmsocmax-current=300000 ";
                            // Save new TDP to avoid unnecessary reapplies
                            _lastPowerLimit = _newPowerLimit;
                            iGPUControl._currentPowerLimit = _newPowerLimit;
                        }
                    }
                    else if (_systemInfoService.CpuInfo.AmdProcessorType == AmdProcessorType.Desktop)
                    {
                        int tdp = _newPowerLimit * 1000;

                        // Apply new power and temp limit
                        cpuCommand =
                            $"--tctl-temp={maxTemperature} --ppt-limit={tdp} --edc-limit={(int)(tdp * 1.33)} --tdc-limit={(int)(tdp * 1.33)} ";
                        _lastPowerLimit = _newPowerLimit;
                    }
                    break;
                }
                case Manufacturer.Intel:
                {
                    cpuCommand = $"--intel-pl={_newPowerLimit}";
                    _lastPowerLimit = _newPowerLimit;
                    break;
                }
            }
        }
    }

    private int prevCpuLoad = -1;

    public void CurveOptimiserLimit(int cpuLoad, int maxCurveOptimiser)
    {
        int newMaxCO = maxCurveOptimiser;

        // Change max CO limit based on CPU usage
        if (cpuLoad < 10) newMaxCO = maxCurveOptimiser;
        else if (cpuLoad < 80) newMaxCO = maxCurveOptimiser - CurveOptimiserIncrement * 2;
        else newMaxCO = maxCurveOptimiser;

        if (_lastCO == 0 && prevCpuLoad <= 0) _lastCO = newMaxCO;
        if (prevCpuLoad < 0) prevCpuLoad = 100;

        // Increase CO if the CPU load is increased by 10
        if (cpuLoad > prevCpuLoad + 10)
        {
            _newCO = _lastCO + CurveOptimiserIncrement;

            // Store the current CPU load for the next iteration
            prevCpuLoad += 10;
        }
        // Decrease CO if the CPU load is decreased by 10
        else if (cpuLoad < prevCpuLoad - 10)
        {
            _newCO = _lastCO - CurveOptimiserIncrement;

            // Store the current CPU load for the next iteration
            prevCpuLoad -= 10;
        }

        // Make sure min and max CO is not exceeded
        if (_newCO <= MinCurveOptimiser) _newCO = MinCurveOptimiser;
        if (_newCO >= newMaxCO) _newCO = newMaxCO;

        // Make sure CO is within CO max limit + 5
        if (_newCO > 55) _newCO = 55;

        if (cpuLoad < 5) _newCO = 0;

        if (cpuLoad > 80) _newCO = maxCurveOptimiser;

        // Apply new CO
        if (_newCO != _lastCO) UpdateCO(_newCO);
    }

    private void UpdateCO(int newC0)
    {
        // Apply new CO
        if (newC0 > 0)
        {
            coCommand = $"--set-coall={Convert.ToUInt32(0x100000 - (uint)newC0)} ";
        }
        else
        {
            coCommand = "--set-coall=0 ";
        }

        // Save new CO to avoid unnecessary reapplies
        _lastCO = newC0;
    }
}