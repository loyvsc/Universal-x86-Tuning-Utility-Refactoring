using System;
using System.Collections.Generic;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Models;

namespace Universal_x86_Tuning_Utility.Services.PresetServices;

public class PremadePresets : IPremadePresets
{
    private readonly ISystemInfoService _systemInfoService;

    public PrematePresetType PrematePresetType { get; private set; }
    public List<PremadePreset> PremadePresetsList { get; }

    public PremadePresets(ISystemInfoService systemInfoService)
    {
        _systemInfoService = systemInfoService;
        
        PremadePresetsList = new List<PremadePreset>();
    }

    public void InitializePremadePresets()
    {
        if (_systemInfoService.Cpu.AmdProcessorType is AmdProcessorType.Apu or AmdProcessorType.Desktop)
        {
            string cpuName = _systemInfoService.Cpu.Name.Replace("AMD", null).Replace("with", null)
                .Replace("Mobile", null).Replace("Ryzen", null).Replace("Radeon", null).Replace("Graphics", null)
                .Replace("Vega", null).Replace("Gfx", null);

            RyzenAdjParameters.RyzenAdjParametersBuilder ecoPresetParameters = new();
            RyzenAdjParameters.RyzenAdjParametersBuilder balancePresetParameters = new();
            RyzenAdjParameters.RyzenAdjParametersBuilder performancePresetParameters = new();
            RyzenAdjParameters.RyzenAdjParametersBuilder extremePresetParameters = new();

            if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Apu)
            {
                var product = _systemInfoService.Product.ToLower();

                if (product.Contains("laptop 16 (amd ryzen 7040") &&
                    _systemInfoService.Manufacturer.ToLower().Contains("framework"))
                {
                    PrematePresetType = PrematePresetType.Laptop16;
                    // uri = new Uri("pack://application:,,,/Assets/Laptops/Framework/framework-laptop-16.png");
                    bool has7700S = _systemInfoService.IsGPUPresent("AMD Radeon(TM) RX 7700S");

                    ecoPresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(45)
                        .WithVrm(180000, 180000)
                        .WithVrmSoc(180000, 180000)
                        .WithVrmGfx(180000)
                        .WithWinPower(PowerPlan.PowerSave)
                        .WithStampLimit(has7700S ? 30000 : 6000)
                        .WithFastLimit(has7700S ? 35000 : 8000)
                        .WithSlowLimit(has7700S ? 30000 : 6000);

                    balancePresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(50)
                        .WithVrm(180000, 180000)
                        .WithVrmSoc(180000, 180000)
                        .WithVrmGfx(180000)
                        .WithWinPower(PowerPlan.Balance)
                        .WithStampLimit(has7700S ? 95000 : 35000)
                        .WithFastLimit(has7700S ? 95000 : 45000)
                        .WithSlowLimit(has7700S ? 95000 : 38000);

                    performancePresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(50)
                        .WithVrm(180000, 180000)
                        .WithVrmSoc(180000, 180000)
                        .WithVrmGfx(180000)
                        .WithWinPower(PowerPlan.HighPerformance)
                        .WithStampLimit(has7700S ? 100000 : 45000)
                        .WithFastLimit(has7700S ? 100000 : 55000)
                        .WithSlowLimit(has7700S ? 125000 : 50000);

                    extremePresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(50)
                        .WithWinPower(PowerPlan.HighPerformance)
                        .WithStampLimit(has7700S ? 120000 : 55000)
                        .WithFastLimit(has7700S ? 147000 : 70000)
                        .WithSlowLimit(has7700S ? 120000 : 65000)
                        .WithVrm(has7700S ? 200000 : 180000, has7700S ? 200000 : 180000)
                        .WithVrmSoc(has7700S ? 200000 : 180000, has7700S ? 200000 : 180000)
                        .WithVrmGfx(has7700S ? 200000 : 180000);
                }
                else if (product.Contains("laptop 13 (amd ryzen 7040") &&
                         _systemInfoService.Manufacturer.ToLower().Contains("framework"))
                {
                    // uri = new Uri("pack://application:,,,/Assets/Laptops/Framework/framework-laptop-13.png");

                    ecoPresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(45)
                        .WithStampLimit(8000)
                        .WithFastLimit(10000)
                        .WithSlowLimit(8000)
                        .WithVrm(180000, 180000)
                        .WithVrmSoc(180000, 180000)
                        .WithVrmGfx(180000)
                        .WithWinPower(0);

                    balancePresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(50)
                        .WithStampLimit(15000)
                        .WithFastLimit(18000)
                        .WithSlowLimit(15000)
                        .WithVrm(180000, 180000)
                        .WithVrmSoc(180000, 180000)
                        .WithVrmGfx(180000)
                        .WithWinPower(PowerPlan.Balance);

                    performancePresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(50)
                        .WithStampLimit(28000)
                        .WithFastLimit(42000)
                        .WithSlowLimit(28000)
                        .WithVrm(180000, 180000)
                        .WithVrmSoc(180000, 180000)
                        .WithVrmGfx(180000)
                        .WithWinPower(PowerPlan.HighPerformance);

                    extremePresetParameters
                        .WithTctlTemp(100)
                        .WithCHTCTemp(100)
                        .WithApuSkinTemp(50)
                        .WithStampLimit(35000)
                        .WithFastLimit(60000)
                        .WithSlowLimit(35000)
                        .WithVrm(180000, 180000)
                        .WithVrmSoc(180000, 180000)
                        .WithVrmGfx(180000)
                        .WithWinPower(PowerPlan.HighPerformance);
                }
                else
                {
                    if (_systemInfoService.Cpu.RyzenFamily < RyzenFamily.Matisse)
                    {
                        if (cpuName.Contains('U') || cpuName.Contains('e') || cpuName.Contains("Ce"))
                        {
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(6000, 64)
                                .WithFastLimit(8000)
                                .WithSlowLimit(6000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(15000, 64)
                                .WithFastLimit(18000)
                                .WithSlowLimit(16000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(18000, 64)
                                .WithFastLimit(20000)
                                .WithSlowLimit(19000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(28000, 64)
                                .WithFastLimit(28000)
                                .WithSlowLimit(28000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("H"))
                        {
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(6000, 64)
                                .WithFastLimit(8000)
                                .WithSlowLimit(6000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(30000, 64)
                                .WithFastLimit(35000)
                                .WithSlowLimit(33000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(35000, 64)
                                .WithFastLimit(42000)
                                .WithSlowLimit(40000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(56000, 64)
                                .WithFastLimit(56000)
                                .WithSlowLimit(56000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("GE"))
                        {
                            // uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");
                            PrematePresetType = PrematePresetType.AM4;
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(15000, 64)
                                .WithFastLimit(15000)
                                .WithSlowLimit(18000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(45000, 64)
                                .WithFastLimit(55000)
                                .WithSlowLimit(48000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(55000, 64)
                                .WithFastLimit(65000)
                                .WithSlowLimit(60000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(65000, 64)
                                .WithFastLimit(80000)
                                .WithSlowLimit(75000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("G"))
                        {
                            // uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");
                            PrematePresetType = PrematePresetType.AM4;
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(15000, 64)
                                .WithFastLimit(18000)
                                .WithSlowLimit(18000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(65000, 64)
                                .WithFastLimit(75000)
                                .WithSlowLimit(65000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(80000, 64)
                                .WithFastLimit(75000)
                                .WithSlowLimit(75000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(85000, 64)
                                .WithFastLimit(95000)
                                .WithSlowLimit(90000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                    }

                    if (_systemInfoService.Cpu.RyzenFamily > RyzenFamily.Matisse)
                    {
                        if (cpuName.Contains("U"))
                        {
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(6000, 64)
                                .WithFastLimit(8000)
                                .WithSlowLimit(6000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(22000, 64)
                                .WithFastLimit(24000)
                                .WithSlowLimit(22000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(28000, 64)
                                .WithFastLimit(28000)
                                .WithSlowLimit(28000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(30000, 64)
                                .WithFastLimit(34000)
                                .WithSlowLimit(32000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("HX"))
                        {
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(6000, 64)
                                .WithFastLimit(8000)
                                .WithSlowLimit(6000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(55000, 64)
                                .WithFastLimit(65000)
                                .WithSlowLimit(55000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(78000, 64)
                                .WithFastLimit(70000)
                                .WithSlowLimit(70000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(85000, 64)
                                .WithFastLimit(95000)
                                .WithSlowLimit(90000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("HS"))
                        {
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(6000, 64)
                                .WithFastLimit(8000)
                                .WithSlowLimit(6000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(35000, 64)
                                .WithFastLimit(45000)
                                .WithSlowLimit(38000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(45000, 64)
                                .WithFastLimit(55000)
                                .WithSlowLimit(50000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(55000, 64)
                                .WithFastLimit(70000)
                                .WithSlowLimit(65000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("H"))
                        {
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(6000, 64)
                                .WithFastLimit(8000)
                                .WithSlowLimit(6000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(45000, 64)
                                .WithFastLimit(55000)
                                .WithSlowLimit(48000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(55000, 64)
                                .WithFastLimit(65000)
                                .WithSlowLimit(60000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(65000, 64)
                                .WithFastLimit(80000)
                                .WithSlowLimit(75000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("GE"))
                        {
                            // uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");
                            PrematePresetType = PrematePresetType.AM4;
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(15000, 64)
                                .WithFastLimit(15000)
                                .WithSlowLimit(18000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(45000, 64)
                                .WithFastLimit(55000)
                                .WithSlowLimit(48000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(55000, 64)
                                .WithFastLimit(65000)
                                .WithSlowLimit(60000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(65000, 64)
                                .WithFastLimit(80000)
                                .WithSlowLimit(75000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }
                        else if (cpuName.Contains("G"))
                        {
                            // uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");
                            PrematePresetType = PrematePresetType.AM4;
                            ecoPresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(15000, 64)
                                .WithFastLimit(18000)
                                .WithSlowLimit(18000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            balancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(45)
                                .WithStampLimit(65000, 64)
                                .WithFastLimit(75000)
                                .WithSlowLimit(65000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            performancePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(80000, 64)
                                .WithFastLimit(75000)
                                .WithSlowLimit(75000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);

                            extremePresetParameters
                                .WithTctlTemp(95)
                                .WithCHTCTemp(95)
                                .WithApuSkinTemp(95)
                                .WithStampLimit(85000, 64)
                                .WithFastLimit(95000)
                                .WithSlowLimit(90000, 128)
                                .WithVrm(180000, 180000)
                                .WithVrmSoc(180000, 180000)
                                .WithVrmGfx(180000);
                        }

                        if (_systemInfoService.Cpu.RyzenFamily == RyzenFamily.Mendocino)
                        {
                            if (cpuName.Contains('U'))
                            {
                                ecoPresetParameters
                                    .WithTctlTemp(95)
                                    .WithCHTCTemp(95)
                                    .WithApuSkinTemp(45)
                                    .WithStampLimit(6000, 64)
                                    .WithFastLimit(8000)
                                    .WithSlowLimit(6000, 128)
                                    .WithVrm(180000, 180000)
                                    .WithVrmSoc(180000, 180000)
                                    .WithVrmGfx(180000);

                                balancePresetParameters
                                    .WithTctlTemp(95)
                                    .WithCHTCTemp(95)
                                    .WithApuSkinTemp(45)
                                    .WithStampLimit(15000, 64)
                                    .WithFastLimit(18000)
                                    .WithSlowLimit(16000, 128)
                                    .WithVrm(180000, 180000)
                                    .WithVrmSoc(180000, 180000)
                                    .WithVrmGfx(180000);

                                performancePresetParameters
                                    .WithTctlTemp(95)
                                    .WithCHTCTemp(95)
                                    .WithApuSkinTemp(95)
                                    .WithStampLimit(18000, 64)
                                    .WithFastLimit(20000)
                                    .WithSlowLimit(19000, 128)
                                    .WithVrm(180000, 180000)
                                    .WithVrmSoc(180000, 180000)
                                    .WithVrmGfx(180000);

                                extremePresetParameters
                                    .WithTctlTemp(95)
                                    .WithCHTCTemp(95)
                                    .WithApuSkinTemp(95)
                                    .WithStampLimit(28000, 64)
                                    .WithFastLimit(28000)
                                    .WithSlowLimit(28000, 128)
                                    .WithVrm(180000, 180000)
                                    .WithVrmSoc(180000, 180000)
                                    .WithVrmGfx(180000);
                            }
                        }
                    }
                }
            }

            if (_systemInfoService.Cpu.AmdProcessorType == AmdProcessorType.Desktop)
            {
                var cpuNameParts = cpuName.Split(" ");

                // uri = new Uri("pack://application:,,,/Assets/config-DT-AM4.png");
                PrematePresetType = PrematePresetType.AM4;
                
                cpuName = cpuNameParts[3];
                if (_systemInfoService.Cpu.RyzenFamily < RyzenFamily.Raphael)
                {
                    if (cpuName.Contains('E'))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(45000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(95000)
                            .WithEdcLimit(122000)
                            .WithTdcLimit(122000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);
                    }
                    else if (cpuName.Contains("X3D"))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(85000)
                            .WithEdcLimit(120000)
                            .WithTdcLimit(120000);

                        performancePresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);

                        extremePresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(140000)
                            .WithEdcLimit(190000)
                            .WithTdcLimit(190000);
                    }
                    else if (cpuName.Contains("X") && cpuNameParts[2].Contains("9"))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(95000)
                            .WithEdcLimit(130000)
                            .WithTdcLimit(130000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(125000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(170000)
                            .WithEdcLimit(230000)
                            .WithTdcLimit(230000);
                    }
                    else if (cpuName.Contains("X"))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(88000)
                            .WithEdcLimit(125000)
                            .WithTdcLimit(125000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(140000)
                            .WithEdcLimit(190000)
                            .WithTdcLimit(190000);
                    }
                    else
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(45000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(88000)
                            .WithEdcLimit(125000)
                            .WithTdcLimit(125000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);
                    }
                }
                else
                {
                    // uri = new Uri("pack://application:,,,/Assets/config-DT-AM5.png");
                    PrematePresetType = PrematePresetType.AM5;
                    
                    if (cpuName.Contains('E'))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(45000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(95000)
                            .WithEdcLimit(122000)
                            .WithTdcLimit(122000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);
                    }
                    else if (cpuName.Contains("X3D"))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(85000)
                            .WithEdcLimit(120000)
                            .WithTdcLimit(120000);

                        performancePresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);

                        extremePresetParameters
                            .WithTctlTemp(85)
                            .WithPptLimit(140000)
                            .WithEdcLimit(190000)
                            .WithTdcLimit(190000);
                    }
                    else if (cpuName.Contains('X') && cpuNameParts[2].Contains('9'))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(105000)
                            .WithEdcLimit(145000)
                            .WithTdcLimit(145000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(145000)
                            .WithEdcLimit(210000)
                            .WithTdcLimit(210000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(230000)
                            .WithEdcLimit(310000)
                            .WithTdcLimit(310000);
                    }
                    else if (cpuName.Contains("X"))
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(88000)
                            .WithEdcLimit(125000)
                            .WithTdcLimit(125000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(140000)
                            .WithEdcLimit(190000)
                            .WithTdcLimit(190000);
                    }
                    else
                    {
                        ecoPresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(45000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        balancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(65000)
                            .WithEdcLimit(90000)
                            .WithTdcLimit(90000);

                        performancePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(88000)
                            .WithEdcLimit(125000)
                            .WithTdcLimit(125000);

                        extremePresetParameters
                            .WithTctlTemp(95)
                            .WithPptLimit(105000)
                            .WithEdcLimit(142000)
                            .WithTdcLimit(142000);
                    }
                }
            }

            var ecoPreset = new PremadePreset()
            {
                Name = "Eco",
                RyzenAdjParameters = ecoPresetParameters.BuildParamtersString()
            };

            var balancePreset = new PremadePreset()
            {
                Name = "Balance",
                RyzenAdjParameters = balancePresetParameters.BuildParamtersString()
            };

            var performancePreset = new PremadePreset()
            {
                Name = "Performance",
                RyzenAdjParameters = performancePresetParameters.BuildParamtersString()
            };

            var extremePreset = new PremadePreset()
            {
                Name = "Extreme",
                RyzenAdjParameters = extremePresetParameters.BuildParamtersString()
            };

            PremadePresetsList.Add(ecoPreset);
            PremadePresetsList.Add(balancePreset);
            PremadePresetsList.Add(performancePreset);
            PremadePresetsList.Add(extremePreset);
        }
    }
}