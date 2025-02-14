using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;
using Octokit;
using FileMode = System.IO.FileMode;

namespace DAL.Services;

public class UpdateService : IUpdateService
{
    private readonly ILogger<UpdateService> _logger;
    private const string Owner = "JamesCJ60";
    private const string RepositoryName = "Universal-x86-Tuning-Utility";

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> IsUpdatesAvailable(string currentVersion)
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue(RepositoryName));
            var releases = await client.Repository.Release.GetAll(Owner, RepositoryName);
            
            var latestRelease = releases[0];
            var latestVersion = new Version(latestRelease.TagName);
            
            return latestVersion.CompareTo(Version.Parse(currentVersion)) > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when checking for updates");
        }

        return false;
    }
    
    public async Task DownloadNewestPackage(string downloadPath)
    {
        try
        {
            var client = new GitHubClient(new ProductHeaderValue(RepositoryName));
            var releases = await client.Repository.Release.GetAll(Owner, RepositoryName);

            var latestRelease = releases[0];
            var assets = latestRelease.Assets;
            
            var downloadUrl = assets[0].BrowserDownloadUrl;

            var packageName = Path.GetFileName(downloadUrl);
            var savePackagePath = Path.Combine(downloadPath, packageName);

            using (var httpClient = new HttpClient())
            {
                await using (var stream = await httpClient.GetStreamAsync(downloadUrl))
                {
                    await using (var fileStream = new FileStream(savePackagePath, FileMode.Create))
                    {
                        await fileStream.CopyToAsync(stream);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when download last update");
            throw;
        }
    }
}