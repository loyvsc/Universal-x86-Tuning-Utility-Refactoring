using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Universal_x86_Tuning_Utility.Linux.Services.Display.Wayland.Compositors;

public class GnomeCompositor : IWaylandCompositor
{
    public void ApplyDisplaySettings(string identifier, int width, int height, int hz)
    {
        try
        {
            var psi = new ProcessStartInfo("gdbus",
                $"call --session --dest org.gnome.Mutter.DisplayConfig --object-path /org/gnome/Mutter/DisplayConfig --method org.gnome.Mutter.DisplayConfig.ApplyMonitorsConfig '\"default\"' 0 []")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var proc = Process.Start(psi);
            proc?.WaitForExit();
            string result = proc?.StandardOutput.ReadToEnd() ?? "";
            string error = proc?.StandardError.ReadToEnd() ?? "";

            if (!string.IsNullOrEmpty(error))
                Console.Error.WriteLine("[Wayland ERROR] " + error);
            else
                Console.WriteLine("[Wayland OK] " + result);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("[Wayland ERROR] " + ex.Message);
        }
    }

    public IReadOnlyList<ApplicationCore.Models.Display> GetDisplayInfo()
    {
        throw new System.NotImplementedException();
    }

    public event NotifyCollectionChangedEventHandler? DisplayInfoChanged;
}