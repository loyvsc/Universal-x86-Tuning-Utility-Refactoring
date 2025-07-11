using System.Collections.ObjectModel;
using ApplicationCore.Enums.Display;

namespace ApplicationCore.Models;

public class Display
{
    public string Identifier { get; }
    public DisplayOutputTechnology OutputTechnology { get; }
    
    public IReadOnlyCollection<DisplayResolution> SupportedResolutions { get; }
    public IReadOnlyCollection<int> SupportedRefreshRates { get; }
    public DisplayResolution CurrentResolution { get; private set; }
    public int CurrentRefreshRate { get; private set; }

    public Display(string identifier, IList<DisplayResolution> resolutions, DisplayResolution currentResolution, IReadOnlyCollection<int> supportedRefreshRates, int currentRefreshRate, DisplayOutputTechnology outputTechnology)
    {
        Identifier = identifier;
        SupportedResolutions = new ReadOnlyCollection<DisplayResolution>(resolutions);
        CurrentResolution = currentResolution;
        SupportedRefreshRates = supportedRefreshRates;
        CurrentRefreshRate = currentRefreshRate;
        OutputTechnology = outputTechnology;
    }

    public void UpdateCurrentResolution(DisplayResolution currentResolution, int currentRefreshRate)
    {
        CurrentRefreshRate = currentRefreshRate;
        CurrentRefreshRate = currentRefreshRate;
    }
}