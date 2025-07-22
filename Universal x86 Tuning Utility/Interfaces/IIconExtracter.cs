using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Interfaces;

public interface IIconExtracter
{
    public Task<string> ExtractIcon(string pathToExecutable, string directory);
}