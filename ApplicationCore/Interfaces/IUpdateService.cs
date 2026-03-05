namespace ApplicationCore.Interfaces;

public interface IUpdateService
{
    public bool IsUpdateAvailable { get; }
    public Task<bool> CheckIsUpdatesAvailableAsync(string currentVersion);
    public Task DownloadNewestPackageAsync(string downloadPath);
}