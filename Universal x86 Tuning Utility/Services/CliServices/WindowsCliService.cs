using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.CliServices;

public class WindowsCliService : ICliService
{
    public async Task<string> RunProcess(string processName,
        string arguments = "",
        bool readOutput = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var processStartInfo = new System.Diagnostics.ProcessStartInfo 
            {
                UseShellExecute = false,
                RedirectStandardOutput = readOutput,
                FileName = processName,
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardInput = readOutput,
                RedirectStandardError = readOutput,
                Verb = "runas"
            };
                
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