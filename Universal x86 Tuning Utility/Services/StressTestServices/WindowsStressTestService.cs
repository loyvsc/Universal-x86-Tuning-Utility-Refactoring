using System.Diagnostics;
using System.IO;
using ApplicationCore.Interfaces;
namespace Universal_x86_Tuning_Utility.Services.StressTestServices;

public class WindowsStressTestService : IStressTestService
{
    private readonly string _executablePath = @".\Assets\Stress-Test\AVX2 Stress Test.exe";
    
    public void Start()
    {
        if (File.Exists(_executablePath))
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = _executablePath;
                process.Start();
            }
        }
    }
}