using System.Diagnostics;
using System.Threading.Tasks;

namespace Universal_x86_Tuning_Utility.Services.PowerPlanServices;

public class WindowsPowerPlanService
{
    // public async Task HideAttribute(string subGroup, string attribute)
    // {
    //     await Task.Run(() =>
    //     {
    //         // Execute the "powercfg -attributes" command to hide the attribute
    //         var processStartInfo = new ProcessStartInfo
    //         {
    //             FileName = "powercfg",
    //             Arguments = $"-attributes {subGroup} {attribute} -ATTRIB_HIDE",
    //             UseShellExecute = false,
    //             CreateNoWindow = true,
    //         };
    //         
    //         using(var process = new Process())
    //         {
    //             process.StartInfo = processStartInfo;
    //             process.Start();
    //             await process.WaitForExitAsync();
    //         }
    //     });
    // }
    
    public async Task SetPowerValue(string scheme, string subGroup, string powerSetting, uint value, bool isAC)
    {
        // Execute the "powercfg /setacvalueindex" or "powercfg /setdcvalueindex" command to set the power value
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "powercfg",
            Arguments = $"/set{(isAC ? "ac" : "dc")}valueindex {scheme} {subGroup} {powerSetting} {value}",
            UseShellExecute = false,
            CreateNoWindow = true,
        };
            
        using (var process = new Process())
        {
            process.StartInfo = processStartInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }

    public async Task SetActiveScheme(string scheme)
    {
        // Execute the "powercfg /setactive" command to activate the power scheme
        var processStartInfo = new ProcessStartInfo
        {
            FileName = "powercfg",
            Arguments = $"/setactive {scheme}",
            UseShellExecute = false,
            CreateNoWindow = true,
        };
            
        using (var process = new Process())
        {
            process.StartInfo = processStartInfo;
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}