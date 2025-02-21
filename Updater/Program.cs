using System.Diagnostics;

namespace Updater;

// You can see description of error codes at https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-

class Program
{
    static async Task<int> Main(string[] args)
    {
        if (args.Length == 0) return 1;
        
        switch (args[0])
        {
            case "-h":
            {
                Console.WriteLine("Updater main function: -p {-path} {path to package}");
                return 0;
            }
            case "-p" or "-path":
            {
                if (args.Length != 2)
                {
                    return 160;
                }

                string packageFilePath = args[1];

                if (!File.Exists(packageFilePath))
                {
                    return 2;
                }

                var mainProcess = Process.GetProcessesByName("Universal x86 Tuning Utility").FirstOrDefault();
                mainProcess?.Kill();

                using (var installPackageProcess = new Process())
                {
                    installPackageProcess.StartInfo.FileName = packageFilePath;
                    try
                    {
                        installPackageProcess.Start();
                    }
                    catch
                    {
                        return 5;
                    }

                    await installPackageProcess.WaitForExitAsync();
                }

                File.Delete(packageFilePath);

                return 0;
        }
            default: return 1;
        }
    }
}