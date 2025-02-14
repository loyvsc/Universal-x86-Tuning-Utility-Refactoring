namespace ApplicationCore.Interfaces;

public interface IUpdateService
{
    public Task<bool> IsUpdatesAvailable(string currentVersion);
    public Task DownloadNewestPackage(string downloadPath);
}