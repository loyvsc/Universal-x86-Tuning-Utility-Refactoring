using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using Serilog;
using Universal_x86_Tuning_Utility.Linux.Services.GPUs.Native;

namespace Universal_x86_Tuning_Utility.Linux.Services.GPUs;

public class LinuxAmdGpuService : IAmdGpuService, IDisposable
{
    private const string CardPath = "/sys/class/drm/card{0}/device/";
    private const string PP_FEATURES_PATH = "pp_features";
    private const string PP_FEATURE_MASK_PATH = "pp_feature_mask";
    
    private const ulong RSR_FEATURE_MASK = 0x8000000000;
    private const string SharpnessControlPath = "rsr_sharpness";
    private const string DEBUG_FS_SHARPNESS_PATH = "/sys/kernel/debug/dri/0/amdgpu_rsr_sharpnes";
    
    private readonly ILogger _logger;

    public bool IsRsrEnabled
    {
        set => SetRsrStatus(value);
        get => GetRsrStatus();
    }
    
    public int RsrSharpness 
    {
        set => SetSharpness(value);
        get => GetRsrSharpness();
    }

    public LinuxAmdGpuService(ILogger logger)
    {
        _logger = logger;

        if (AmiSmiWrapper.Initialize() != LibStatus.AMDSMI_STATUS_SUCCESS)
            throw new Exception("Unable to initialize AMDSMI service");
    }
    
    private void SetRsrStatus(bool value)
    {
        try
        {
            string ppFeatureMaskPath = Path.Combine(string.Format(CardPath, 0), PP_FEATURE_MASK_PATH);
        
            if (File.Exists(ppFeatureMaskPath))
            {
                string currentMask = File.ReadAllText(ppFeatureMaskPath).Trim();
                if (ulong.TryParse(currentMask, System.Globalization.NumberStyles.HexNumber, null, out ulong mask))
                {
                    if (value)
                    {
                        mask |= RSR_FEATURE_MASK;
                    }
                    else
                    {
                        mask &= ~RSR_FEATURE_MASK;
                    }
                    _logger.Information("RSR {isEnabled}", value ? "enabled" : "disabled");
                    File.WriteAllText(ppFeatureMaskPath, $"0x{mask:X}");
                    return;
                }
            }

            throw new NotSupportedException();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error settings RSR status: {errorMessage}", ex.Message);
            throw new Exception("Error settings RSR status. See inner exception for details.", ex);
        }
    }
    
    private bool GetRsrStatus()
    {
        try
        {
            string ppFeatureMaskPath = Path.Combine(string.Format(CardPath, 0), PP_FEATURE_MASK_PATH);
            if (File.Exists(ppFeatureMaskPath))
            {
                string currentMask = File.ReadAllText(ppFeatureMaskPath).Trim();
                if (ulong.TryParse(currentMask, System.Globalization.NumberStyles.HexNumber, null, out ulong mask))
                {
                    return (mask & RSR_FEATURE_MASK) != 0;
                }
            }

            throw new NotSupportedException();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error getting RSR status: {errorMessage}", ex.Message);
            throw new Exception("Error getting RSR status. See inner exception for details.", ex);
        }
    }
    
    private void SetSharpness(int sharpness)
    {
        sharpness = Math.Clamp(sharpness, 0, 100);

        try
        {
            string ppFeaturesPath = Path.Combine(string.Format(CardPath, 0), PP_FEATURES_PATH);
            if (File.Exists(ppFeaturesPath))
            {
                // Формат может быть разным, попробуем несколько вариантов
                string[] commands = new[] 
                {
                    $"rsr_sharpness {sharpness}",
                    $"sharpness {sharpness}",
                    $"set sharpness {sharpness}"
                };

                foreach (var cmd in commands)
                {
                    try
                    {
                        File.WriteAllText(ppFeaturesPath, cmd);
                        _logger.Information("Set sharpness set to {currentValue} via pp_features: {usedCommand}", sharpness, cmd);
                        return;
                    }
                    catch
                    {
                        // Continue
                    }
                }
            }

            throw new NotSupportedException();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error settings RSR Sharpness: {errorMessage}", ex.Message);
            throw new Exception("Error settings RSR Sharpness. See inner exception for details.", ex);
        }
    }
    
    private int GetRsrSharpness()
    {
        try
        {
            string[] possiblePaths = new[]
            {
                Path.Combine(string.Format(CardPath, 0), SharpnessControlPath),
                DEBUG_FS_SHARPNESS_PATH
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    string value = File.ReadAllText(path).Trim();
                    if (int.TryParse(value, out int sharpness))
                    {
                        return sharpness;
                    }
                }
            }

            throw new NotSupportedException();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error settings RSR Sharpness: {errorMessage}", ex.Message);
            throw new Exception("Error settings RSR Sharpness. See inner exception for details.", ex);
        }
    }

    public int GetGpuMetrics(int gpuId, AmdGpuSensor gpuSensor)
    {
        var metrics = AmiSmiWrapper.GetGpuMetrics(gpuId);

        return gpuSensor switch
        {
            AmdGpuSensor.GpuLoad => (int) metrics.gfx_activity_acc,
            AmdGpuSensor.GpuClock => metrics.current_gfxclk,
            AmdGpuSensor.GpuMemClock => metrics.current_uclk,
        };
    }

    public void SetAntilag(int gpuId, bool isEnabled)
    {
        SetParameterValue(gpuId, "pp_od_clk_voltage", $"anti_lag {(isEnabled ? "1" : "0")}");
    }

    public void SetBoost(int gpuId, int percent, bool isEnabled)
    {
        SetParameterValue(gpuId, "power_dpm_force_performance_level", isEnabled ? "high" : "auto");
    }

    public void SetChill(int gpuId, int maxFps, int minFps, bool isEnabled)
    {
        var sb = StringBuilderPool.Rent();
        sb.Append("amdu --set-chill ");
        sb.Append(isEnabled ? "enabled" : "disabled");
        sb.Append(' ');
        sb.Append("--min-fps ");
        sb.Append(minFps);
        sb.Append(' ');
        sb.Append("--max-fps ");
        sb.Append(maxFps);
        sb.AppendLine();
        sb.Append("exit");

        try
        {
            RunCliCommand(sb.ToString());
        }
        finally
        {
            StringBuilderPool.Return(sb);
        }
    }

    public void SetImageSharpening(int gpuId, int percent, bool isEnabled)
    {
        SetParameterValue(gpuId, PP_FEATURES_PATH, $"sharpening {(isEnabled ? "1" : "0")}");
    }

    public void SetEnhancedSynchronization(int gpuId, bool isEnabled)
    {
        SetParameterValue(gpuId, PP_FEATURES_PATH, $"enhanced_sync {(isEnabled ? "1" : "0")}");
    }
    
    /// <summary>
    /// Writing to SysFS
    /// </summary>
    private void SetParameterValue(int targetGpuId, string parameter, string value)
    {
        try
        {
            string fullPath = Path.Combine(string.Format(CardPath, targetGpuId), parameter);
            File.WriteAllText(fullPath, value);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error writing to {target}: {errorMessage}", parameter, ex.Message);
            throw new Exception($"Error settings parameter {parameter}. See inner exception for details.", ex);
        }
    }

    private void RunCliCommand(string command, bool waitToExit = true)
    {
        var proc = new Process()
        {
            StartInfo = new ProcessStartInfo()
            {
                FileName = "/bin/bash", 
                Arguments = command,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true
            }
        };
        
        proc.Start();
        if (waitToExit)
        {
            proc.WaitForExit();
            if (proc.ExitCode != 0)
                throw new COMException($"Error running command {command}. ExitCode: {proc.ExitCode}", proc.ExitCode);
        }
    }

    public void Dispose()
    {
        AmiSmiWrapper.Free();
    }
}