namespace ApplicationCore.Interfaces;

public interface IRyzenAdjService
{
    public Task Translate(string ryzenAdjArgs, bool isAutoReapply = false);
}