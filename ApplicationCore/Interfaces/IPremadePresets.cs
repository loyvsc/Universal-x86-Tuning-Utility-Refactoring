using ApplicationCore.Enums;
using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IPremadePresets
{
    public PrematePresetType PrematePresetType { get; }
    
    public List<PremadePreset> PremadePresetsList { get; }
    public void InitializePremadePresets();
}