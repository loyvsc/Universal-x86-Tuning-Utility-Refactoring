using System.Net.NetworkInformation;

namespace Universal_x86_Tuning_Utility.Helpers;

public static class UpdateHelper
{
    public static bool IsInternetAvailable()
    {
        try
        {
            using (var ping = new Ping())
            {
                var result = ping.Send("8.8.8.8", 2000); // ping Google DNS server
                return result.Status == IPStatus.Success;
            }
        }
        catch
        {
            return false;
        }
    }
}