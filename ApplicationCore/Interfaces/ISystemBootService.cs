namespace ApplicationCore.Interfaces;

public interface ISystemBootService
{
    public void CreateTask(string taskName, string pathToExecutable, string arguments = "", string taskDescription = "");
    public void DeleteTask(string taskName);
}