using System;
using System.Threading;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Helpers;

public interface IRunCliService
{
    public Task<string> RunCommand(string arguments, 
        bool readOutput,
        CancellationToken cancellationToken = default, 
        bool runAsAdmin = true);
}

public class RunCliService : IRunCliService
{
    public async Task<string> RunCommand(string arguments, 
        bool readOutput, 
        CancellationToken cancellationToken = default, 
        bool runAsAdmin = true)
    {
        try
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo 
            {
                UseShellExecute = false,
                RedirectStandardOutput = readOutput,
                FileName = "cmd.exe",
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardInput = readOutput,
                RedirectStandardError = readOutput
            };
    
            if (runAsAdmin)
            {
                processStartInfo.Verb = "runas";
            }
                
            var process = new System.Diagnostics.Process
            {
                EnableRaisingEvents = true,
                StartInfo = processStartInfo
            };
            
            process.Start();
            await process.WaitForExitAsync(cancellationToken);
            
            if (readOutput)
            {
                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                process.Close();
                return output;
            }
    
            process.Close();
            return "COMPLETE";
        }
        catch (Exception ex)
        {
            return "Error running CLI: " + ex.Message + " " + arguments;
        }
    }
}