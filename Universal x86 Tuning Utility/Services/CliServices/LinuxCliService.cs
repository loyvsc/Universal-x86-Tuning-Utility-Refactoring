using System.Threading;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Services.CliServices;

public class LinuxCliService : ICliService
{
    public Task<string> RunProcess(string processName, 
                                   string arguments = "", 
                                   bool readOutput = false,
                                   CancellationToken cancellationToken = default)
    {
        throw new System.NotImplementedException();
    }
}