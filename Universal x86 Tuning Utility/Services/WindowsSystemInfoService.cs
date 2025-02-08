using System;
using System.Management;
using Accord.Math;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using Microsoft.Extensions.Logging;
using Universal_x86_Tuning_Utility.Scripts.AMD_Backend;
using Universal_x86_Tuning_Utility.Scripts.Intel_Backend;

namespace Universal_x86_Tuning_Utility.Services;

public class WindowsSystemInfoService : ISystemInfoService
{
    private readonly ILogger<WindowsSystemInfoService> _logger;
    public int NvidiaGpuCount { get; private set; }
    public int RadeonGpuCount { get; private set; }
    public CpuInfo CpuInfo { get; set; }
    
    public WindowsSystemInfoService(ILogger<WindowsSystemInfoService> logger)
    {
        _logger = logger;
        CpuInfo = new CpuInfo();
    }

    public void AnalyzeSystem()
    {
        using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController"))
        {
            foreach (var videoController in searcher.Get())
            {
                if (videoController["Name"] is string name)
                {
                    if (name.Contains("Radeon"))
                    {
                        RadeonGpuCount++;
                    } else if (name.Contains("NVIDIA"))
                    {
                        NvidiaGpuCount++;
                    }
                }
            }
        }
        
        try //todo try find another solution for getting information
        {
            var processorIdentifier = Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER");

            // Split the string into individual words
            var words = processorIdentifier.Split(' ');

            // Find the indices of the words "Family", "Model", and "Stepping"
            var familyIndex = Array.IndexOf(words, "Family") + 1;
            var modelIndex = Array.IndexOf(words, "Model") + 1;
            var steppingIndex = Array.IndexOf(words, "Stepping") + 1;

            // Extract the family, model, and stepping values from the corresponding words
            CpuInfo.Family = int.Parse(words[familyIndex]);
            CpuInfo.Model = int.Parse(words[modelIndex]);
            CpuInfo.Stepping = int.Parse(words[steppingIndex].TrimEnd(','));

            using (var mos = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
            {
                foreach (var cpuInfo in mos.Get()) //todo test this shit
                {
                    CpuInfo.Name = cpuInfo["Name"].ToString();
                }
            }
        }
        catch (ManagementException ex)
        {
            _logger.LogError(ex, "Error occurred when analyzing cpu information");
        }

        if (CpuInfo.Name.Contains("Intel"))
        {
            CpuInfo.Manufacturer = Manufacturer.Intel;
            Intel_Management.determineCPU();
        }
        else
        {
            switch (CpuInfo.RyzenGeneration)
            {
                case RyzenGenerations.Zen1_2:
                {
                    CpuInfo.RyzenFamily = CpuInfo.Model switch
                    {
                        1 => RyzenFamily.SummitRidge,
                        8 => RyzenFamily.PinnacleRidge,
                        17 or 18 => RyzenFamily.RavenRidge,
                        24 => RyzenFamily.Picasso,
                        32 => RyzenFamily.Dali,
                        80 => RyzenFamily.FireFlight,
                        96 => RyzenFamily.Renoir,
                        104 => RyzenFamily.Lucienne,
                        113 => RyzenFamily.Matisse,
                        114 or 145 => RyzenFamily.VanGogh,
                        160 => RyzenFamily.Mendocino
                    };
                    if (CpuInfo.Model == 32 && 
                        (CpuInfo.Name.Contains("15e") || CpuInfo.Name.Contains("15Ce") || CpuInfo.Name.Contains("20e")))
                    {
                        CpuInfo.RyzenFamily = RyzenFamily.Pollock;
                    }
                    break;
                }
                case RyzenGenerations.Zen3_4:
                {
                    CpuInfo.RyzenFamily = CpuInfo.Model switch
                    {
                        33 => RyzenFamily.Vermeer,
                        63 or 68 => RyzenFamily.Rembrandt,
                        80 => RyzenFamily.Cezanne_Barcelo,
                        116 => RyzenFamily.PhoenixPoint,
                        120 => RyzenFamily.PhoenixPoint2,
                        117 => RyzenFamily.HawkPoint
                    };
                    if (CpuInfo.Model == 97)
                    {
                        CpuInfo.RyzenFamily = CpuInfo.Name.Contains("HX") ? RyzenFamily.DragonRange : RyzenFamily.Raphael;
                    }
                    break;
                }
                case RyzenGenerations.Zen5_6:
                {
                    CpuInfo.RyzenFamily = CpuInfo.Model switch
                    {
                        32 or 36 => RyzenFamily.StrixPoint,
                        68 => RyzenFamily.GraniteRidge,
                        112 => RyzenFamily.StrixHalo,
                        _ => RyzenFamily.StrixPoint2
                    };
                    break;
                }
            }

            CpuInfo.AmdProcessorType = CpuInfo.RyzenFamily is RyzenFamily.SummitRidge 
                or RyzenFamily.PinnacleRidge 
                or RyzenFamily.Matisse 
                or RyzenFamily.Vermeer 
                or RyzenFamily.Raphael 
                or RyzenFamily.DragonRange 
                or RyzenFamily.GraniteRidge ? AmdProcessorType.Desktop : AmdProcessorType.Apu;

            Addresses.SetAddresses(CpuInfo);
        }
    }
}