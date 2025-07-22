using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Interfaces;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxIconExtracter : IIconExtracter
{
    public Task<string> ExtractIcon(string pathToExecutable, string directory)
    {
        throw new System.NotImplementedException();
    }
}