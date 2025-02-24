namespace ApplicationCore.Interfaces;

public interface IPresetServiceFactory
{
    public IPresetService GetPresetService(string presetsPath);
}