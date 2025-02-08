namespace ApplicationCore.Interfaces;

public interface IUpdateService
{
    Task<bool> IsUpdateAvailable(string currentVersion);
    Task<bool> DownloadLastUpdate(string currentVersion, string downloadPath);
}