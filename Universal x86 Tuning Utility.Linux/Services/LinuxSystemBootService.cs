using System;
using ApplicationCore.Interfaces;
using ApplicationCore.Utilities;
using Serilog;
using Tmds.DBus;
using Universal_x86_Tuning_Utility.Linux.Helpers.DBus;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxSystemBootService : ISystemBootService
{
    private readonly ILogger _logger;

    public LinuxSystemBootService(ILogger logger)
    {
        _logger = logger;
    }
    
    public void CreateTask(string taskName, string pathToExecutable, string arguments = "", string taskDescription = "")
    {
        try
        {
            var connection = Connection.System;
            var manager = connection.CreateProxy<ISystemdManager>(
                "org.freedesktop.systemd1",
                "/org/freedesktop/systemd1");

            manager.ReloadAsync().Wait();

            var sb = StringBuilderPool.Rent();
            sb.Append(taskName);
            sb.Append(".service");

            var serviceName = sb.ToString();

            StringBuilderPool.Return(sb);

            var isServiceAvailable = IsServiceAvailable(manager, serviceName);
            
            manager.EnableUnitFilesAsync(new[] { serviceName }, false, false).Wait();
            manager.StartUnitAsync(serviceName, "replace").Wait();
            
            if (isServiceAvailable)
            {
                _logger.Information("Service {serviceName} updated", serviceName);
            }
            else
            {
                _logger.Information("Service {serviceName} created", serviceName);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when creating service");
        }
    }

    private bool IsServiceAvailable(ISystemdManager manager, string serviceName)
    {
        try
        {
            manager.GetUnitAsync(serviceName).Wait();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void DeleteTask(string taskName)
    {
        try
        {
            var connection = Connection.System;
            var manager = connection.CreateProxy<ISystemdManager>(
                "org.freedesktop.systemd1",
                "/org/freedesktop/systemd1");

            manager.ReloadAsync().Wait();

            var sb = StringBuilderPool.Rent();
            sb.Append(taskName);
            sb.Append(".service");

            var serviceName = sb.ToString();

            StringBuilderPool.Return(sb);

            if (IsServiceAvailable(manager, serviceName))
            {
                manager.DisableUnitFilesAsync(new[] { serviceName }, false).Wait();

                manager.StopUnitAsync(serviceName, "replace").Wait();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occurred when deleting service");
        }
    }
}