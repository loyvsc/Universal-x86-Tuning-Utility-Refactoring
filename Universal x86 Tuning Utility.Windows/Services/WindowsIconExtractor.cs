using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using ApplicationCore.Interfaces;
using Universal_x86_Tuning_Utility.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsIconExtractor : IIconExtractor
{
    public async Task<string> ExtractIcon(string pathToExecutable, string directory)
    {
        var gameName = Path.GetFileNameWithoutExtension(pathToExecutable);
        var iconPath = Path.Combine(directory, gameName + ".ico");
        using (var icon = Icon.ExtractAssociatedIcon(pathToExecutable))
        {
            if (icon != null)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                await using (var fileStream = new FileStream(iconPath, FileMode.Create))
                {
                    icon.Save(fileStream);
                }
            }
        }
        
        return iconPath;
    }
    
    public async Task<string> ExtractIcon(Stream dataStream, string name, string directory)
    {
        var iconPath = Path.Combine(directory, name + ".ico");
        using (var icon = new Icon(dataStream))
        {
            await using (var fileStream = new FileStream(iconPath, FileMode.Create))
            {
                icon.Save(fileStream);
            }
        }
        
        return iconPath;
    }
}