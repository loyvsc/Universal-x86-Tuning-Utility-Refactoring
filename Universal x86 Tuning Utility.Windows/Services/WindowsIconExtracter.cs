using System.IO;
using System.Threading.Tasks;
using Universal_x86_Tuning_Utility.Interfaces;

namespace Universal_x86_Tuning_Utility.Windows.Services;

public class WindowsIconExtracter : IIconExtracter
{
    public async Task<string> ExtractIcon(string pathToExecutable, string directory)
    {
        var gameName = Path.GetFileNameWithoutExtension(pathToExecutable);
        var iconPath = Path.Combine(directory, gameName + ".ico");
        using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(pathToExecutable))
        {
            if (icon != null)
            {
                await using (var fileStream = new FileStream(iconPath, FileMode.Create))
                {
                    icon.Save(fileStream);
                }
            }
        }
        
        return iconPath;
    }
}