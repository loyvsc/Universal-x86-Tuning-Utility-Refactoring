using System;
using System.Collections.Generic;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.GPUs.AMD.Apu;

public class AmdApuControlService : IAmdApuControlService
{
    private const int WindowSize = 2; // Number of samples in the sliding window
    private const int LastWindowSize = 8; // Number of samples in the sliding window
    
    public int CurrentPowerLimit { get; set; } = 28;
    public string? Commmand { get; private set; }
    
    private int _lastClock; // mhz
    private double _averageLastGpuLoad;
    private double _averageGpuLoad;
    
    private readonly Queue<int> _gpuLastLoadSamples = new Queue<int>();
    private readonly Queue<int> _gpuLoadSamples = new Queue<int>();
    
    public void UpdateiGPUClock(int maxClock,
        int minClock,
        int maxTemperature,
        int powerDraw,
        int temperature,
        int currentClock,
        int gpuLoad,
        int memClock,
        int cpuClocks,
        int minCpuClock,
        int fps = 0,
        int fpsLimit = 0)
    {
        try
        {
            if (_lastClock <= 0) _lastClock = (int)(maxClock / 1.6);
            int newClock = currentClock;

            if (currentClock <= 0) currentClock = _lastClock;
            if (_averageLastGpuLoad <= 0) _averageLastGpuLoad = gpuLoad;
            if (_averageGpuLoad <= 0) _averageGpuLoad = gpuLoad;

            _gpuLoadSamples.Enqueue(gpuLoad);

            // Remove oldest sample if the window is full
            if (_gpuLoadSamples.Count > WindowSize)
            {
                int oldestSample = _gpuLastLoadSamples.Dequeue();
                _averageGpuLoad = ((_averageGpuLoad * WindowSize) - oldestSample + gpuLoad) / WindowSize;
            }
            else _averageGpuLoad = ((_averageGpuLoad * (_gpuLoadSamples.Count - 1)) + gpuLoad) / _gpuLoadSamples.Count;

            gpuLoad = (int)_averageGpuLoad;

            if (gpuLoad >= 87 && gpuLoad <= 92 && temperature <= maxTemperature && memClock >= 550 &&
                cpuClocks > minCpuClock) newClock = _lastClock;
            else
            {
                // Remove oldest sample if the window is full
                if (_gpuLastLoadSamples.Count > LastWindowSize)
                {
                    int oldestSample = _gpuLastLoadSamples.Dequeue();
                    _averageLastGpuLoad = ((_averageLastGpuLoad * LastWindowSize) - oldestSample + gpuLoad) /
                                          LastWindowSize;
                }
                else
                    _averageLastGpuLoad = ((_averageLastGpuLoad * (_gpuLastLoadSamples.Count - 1)) + gpuLoad) /
                                          _gpuLastLoadSamples.Count;

                if ((int)_averageLastGpuLoad <= 40 && gpuLoad > 60 && currentClock < 650 && cpuClocks >= minCpuClock &&
                    memClock > 550) newClock = (int)(maxClock / 1.6);

                if (fps > 0 && fpsLimit > 0)
                {
                    if (gpuLoad > 92 && temperature <= maxTemperature && memClock >= 550 && cpuClocks > minCpuClock ||
                        fps < fpsLimit)
                    {
                        if (currentClock < maxClock / 4) newClock = currentClock + 75;
                        else if (currentClock < maxClock / 3) newClock = currentClock + 50;
                        else if (currentClock < maxClock / 2) newClock = currentClock + 35;
                        else if (currentClock < maxClock / 1.33) newClock = currentClock + 25;
                        else newClock = currentClock + 25;
                    }
                    else if (temperature > maxTemperature || gpuLoad < 87 || memClock < 550 ||
                             cpuClocks < minCpuClock || fps > fpsLimit)
                    {
                        if (currentClock > minClock)
                        {
                            if (currentClock > minClock && gpuLoad > 50) newClock = currentClock - 25;
                            else if (currentClock > minClock && gpuLoad < 20) newClock = currentClock - 50;
                        }
                    }
                }
                else
                {
                    if (gpuLoad > 92 && temperature <= maxTemperature && memClock >= 550 && cpuClocks > minCpuClock)
                    {
                        if (currentClock < maxClock / 4) newClock = currentClock + 75;
                        else if (currentClock < maxClock / 3) newClock = currentClock + 50;
                        else if (currentClock < maxClock / 2) newClock = currentClock + 35;
                        else if (currentClock < maxClock / 1.33) newClock = currentClock + 25;
                        else newClock = currentClock + 25;
                    }

                    else if (temperature > maxTemperature || gpuLoad < 87 || memClock < 550 || cpuClocks < minCpuClock)
                    {
                        if (currentClock > minClock)
                        {
                            if (currentClock > minClock && gpuLoad > 50) newClock = currentClock - 25;
                            else if (currentClock > minClock && gpuLoad < 20) newClock = currentClock - 50;
                        }
                    }
                }
            }

            if (currentClock > maxClock) newClock = maxClock - 10;
            if (currentClock < minClock) newClock = minClock + 10;

            if (newClock <= (_lastClock - 15) && newClock > 0 || newClock >= (_lastClock + 15) && newClock > 0)
            {
                Commmand = $"--gfx-clk={newClock} ";
                _lastClock = newClock;
            }

            _gpuLastLoadSamples.Enqueue(gpuLoad);
        }
        catch (Exception ex)
        {
            throw new AggregateException("Exception occurred when updating iGpu clock", ex);
        }
    }
}