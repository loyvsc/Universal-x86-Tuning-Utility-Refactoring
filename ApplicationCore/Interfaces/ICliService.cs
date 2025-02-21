namespace ApplicationCore.Interfaces;

public interface ICliService
{
    /// <summary>
    /// Run command as administrator
    /// </summary>
    public Task<string> RunProcess(string processName,
        string arguments = "",
        bool readOutput = false,
        CancellationToken cancellationToken = default);
}