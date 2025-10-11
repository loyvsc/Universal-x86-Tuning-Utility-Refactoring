using System;
using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsCliService : ICliService
{
    public async Task<string> RunProcess(string processName,
        string arguments = "",
        bool readOutput = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var isUri = Uri.IsWellFormedUriString(processName, UriKind.RelativeOrAbsolute);
            
            var processStartInfo = new System.Diagnostics.ProcessStartInfo 
            {
                UseShellExecute = isUri,
                RedirectStandardOutput = readOutput,
                FileName = processName,
                Arguments = arguments,
                CreateNoWindow = true,
                RedirectStandardInput = readOutput,
                RedirectStandardError = readOutput,
                Verb = isUri ? "open" : "runas"
            };
                
            var process = new System.Diagnostics.Process
            {
                EnableRaisingEvents = true,
                StartInfo = processStartInfo
            };
            
            process.Start();
            
            if (readOutput)
            {
                await process.WaitForExitAsync(cancellationToken);
                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                process.Close();
                return output;
            }
    
            return "COMPLETE";
        }
        catch (Exception ex)
        {
            return "Error running CLI: " + ex.Message + " " + arguments;
        }
    }
}