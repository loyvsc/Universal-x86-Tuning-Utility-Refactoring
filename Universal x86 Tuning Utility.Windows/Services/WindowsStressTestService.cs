using System.Diagnostics;
using System.IO;
using ApplicationCore.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsStressTestService : IStressTestService
{
    private const string ExecutablePath = @".\Assets\Stress-Test\AVX2 Stress Test.exe";
    
    public void Start()
    {
        if (File.Exists(ExecutablePath))
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = ExecutablePath;
                process.Start();
            }
        }
    }
}