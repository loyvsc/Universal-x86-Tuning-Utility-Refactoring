using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Universal_x86_Tuning_Utility.Services.UpdateInstallerServices;

public class WindowsUpdateInstallerService : IUpdateInstallerService
{
    private readonly ILogger<WindowsUpdateInstallerService> _logger;
    private readonly IUpdateService _updateService;

    public WindowsUpdateInstallerService(ILogger<WindowsUpdateInstallerService> logger,
                                         IUpdateService updateService)
    {
        _logger = logger;
        _updateService = updateService;
    }

    public async Task DownloadAndInstallNewestPackage()
    {
        try
        {
            var downloadPackageDirectory = Directory.CreateTempSubdirectory();
            var packageFileName = Path.Combine(downloadPackageDirectory.FullName, "package.exe");
            if (File.Exists(packageFileName))
            {
                File.Delete(packageFileName);
            }

            await _updateService.DownloadNewestPackage(packageFileName);
            
            var updaterProcess = new Process();
            updaterProcess.StartInfo.FileName = "Updater.exe";
            updaterProcess.StartInfo.Arguments = $"-p {packageFileName}";
            updaterProcess.Start();
            await updaterProcess.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred when download last update");
            throw new AggregateException("Failed to download newest update");
        }
    }
}