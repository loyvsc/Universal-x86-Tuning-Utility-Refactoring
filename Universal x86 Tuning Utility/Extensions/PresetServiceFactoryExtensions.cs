using ApplicationCore.Interfaces;
using Universal_x86_Tuning_Utility.Properties;

namespace Universal_x86_Tuning_Utility.Extensions;

public static class PresetServiceFactoryExtensions
{
    private static readonly string IntelPresetServicePath = Settings.Default.Path + "intelPresets.json";
    private static readonly string AmdApuPresetServicePath = Settings.Default.Path + "apuPresets.json";
    private static readonly string AmdDesktopPresetServicePath = Settings.Default.Path + "amdDtCpuPresets.json";
    
    public static IPresetService GetIntelPresetService(this IPresetServiceFactory presetServiceFactory)
    {
        return presetServiceFactory.GetPresetService(IntelPresetServicePath);
    }
    
    public static IPresetService GetAmdApuPresetService(this IPresetServiceFactory presetServiceFactory)
    {
        return presetServiceFactory.GetPresetService(AmdApuPresetServicePath);
    }
    
    public static IPresetService GetAmdDesktopPresetService(this IPresetServiceFactory presetServiceFactory)
    {
        return presetServiceFactory.GetPresetService(AmdDesktopPresetServicePath);
    }
}