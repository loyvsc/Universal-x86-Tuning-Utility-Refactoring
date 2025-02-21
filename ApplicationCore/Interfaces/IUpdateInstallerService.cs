namespace ApplicationCore.Interfaces;

public interface IUpdateInstallerService
{
    public Task DownloadAndInstallNewestPackage();
}