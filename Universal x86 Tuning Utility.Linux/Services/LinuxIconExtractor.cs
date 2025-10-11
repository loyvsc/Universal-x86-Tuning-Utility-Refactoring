using System;
using System.IO;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using OpenCvSharp;
using Universal_x86_Tuning_Utility.Interfaces;

namespace Universal_x86_Tuning_Utility.Linux.Services;

public class LinuxIconExtractor : IIconExtractor
{
    public Task<string> ExtractIcon(string pathToExecutable, string directory)
    {
        throw new NotImplementedException();
        // var gameName = Path.GetFileNameWithoutExtension(pathToExecutable);
        // var iconPath = Path.Combine(directory, gameName + ".ico");
        //
        // using (var icon = Icon.ExtractAssociatedIcon(pathToExecutable))
        // {
        //     if (icon != null)
        //     {
        //         using (var fileStream = new FileStream(iconPath, FileMode.Create))
        //         {
        //             icon.Save(fileStream);
        //         }
        //     }
        // }
        //
        // return iconPath;
    }

    public async Task<string> ExtractIcon(Stream dataStream, string name, string directory)
    {
        var iconPath = Path.Combine(directory, name + ".ico");
        await using (var fileStream = new FileStream(iconPath, FileMode.Create))
        {
            await dataStream.CopyToAsync(fileStream);
        }
        
        return iconPath;
    }
}