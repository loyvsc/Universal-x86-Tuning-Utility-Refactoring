namespace ApplicationCore.Interfaces;

public interface IRyzenAdjService : IDisposable
{
    public Task Translate(string ryzenAdjArgs, bool isAutoReapply = false);
}