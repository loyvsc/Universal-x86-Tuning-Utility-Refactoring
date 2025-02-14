using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.SystemBootServices;

public class LinuxSystemBootService : ISystemBootService
{
    public void CreateTask(string taskName, string pathToExecutable, string arguments = "", string taskDescription = "")
    {
        throw new System.NotImplementedException();
    }

    public void DeleteTask(string taskName)
    {
        throw new System.NotImplementedException();
    }
}