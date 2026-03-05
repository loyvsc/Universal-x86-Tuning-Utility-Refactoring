using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Windows.Helpers;

public static class ProcessHelpers
{
    public static Task RunCmd(string name, string args, CancellationToken cancellationToken = default)
    {
        var cmd = new Process();
        cmd.StartInfo.UseShellExecute = false;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        cmd.StartInfo.FileName = name;
        cmd.StartInfo.Arguments = args;
        cmd.Start();

        return cmd.WaitForExitAsync(cancellationToken);
    }
}