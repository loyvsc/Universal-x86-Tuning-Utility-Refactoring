using System.Linq;
using ApplicationCore.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Universal_x86_Tuning_Utility.Services.SystemBootServices;

public class WindowsSystemBootService : ISystemBootService
{
    public void CreateTask(string taskName, string pathToExecutable, string arguments = "", string taskDescription = "")
    {
        var taskService = TaskService.Instance;
        if (taskService.RootFolder.AllTasks.All(t => t.Name != taskName))
        {
            // Create a new task definition and assign properties
            var taskDefinition = taskService.NewTask();
            taskDefinition.Principal.RunLevel = TaskRunLevel.Highest;
            taskDefinition.RegistrationInfo.Description = taskDescription;
            taskDefinition.Settings.DisallowStartIfOnBatteries = false;
            taskDefinition.Settings.StopIfGoingOnBatteries = false;
            taskDefinition.Settings.DisallowStartOnRemoteAppSession = false;

            // Create a trigger that will fire the task at this time every other day
            taskDefinition.Triggers.Add(new LogonTrigger());
            
            taskDefinition.Actions.Add(pathToExecutable);

            taskService.RootFolder.RegisterTaskDefinition(taskName, taskDefinition);
        }
    }

    public void DeleteTask(string taskName)
    {
        var taskService = TaskService.Instance;
        if (taskService.RootFolder.AllTasks.Any(t => t.Name == taskName))
        {
            taskService.RootFolder.DeleteTask(taskName);
        }
    }
}