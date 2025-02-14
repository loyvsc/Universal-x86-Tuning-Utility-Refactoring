using ApplicationCore.Models;

namespace ApplicationCore.Interfaces;

public interface IPremadePresets
{
    public List<PremadePreset> PremadePresetsList { get; }
    public void InitializePremadePresets();
}