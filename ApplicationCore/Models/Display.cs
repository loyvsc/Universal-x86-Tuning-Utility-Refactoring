using System.Collections.ObjectModel;
using ApplicationCore.Enums.Display;

namespace ApplicationCore.Models;

public class Display
{
    public string Identifier { get; }
    public string Name { get; }
    public DisplayOutputTechnology OutputTechnology { get; }
    
    public IReadOnlyCollection<DisplayResolution> SupportedResolutions { get; }
    public IReadOnlyCollection<int> SupportedRefreshRates { get; }
    public DisplayResolution CurrentResolution { get; private set; }
    public int CurrentRefreshRate { get; private set; }

    public Display(string identifier, string name, IList<DisplayResolution> resolutions, DisplayResolution currentResolution, IReadOnlyCollection<int> supportedRefreshRates, int currentRefreshRate, DisplayOutputTechnology outputTechnology)
    {
        Identifier = identifier;
        Name = name;
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

    public override bool Equals(object? obj)
    {
        if (obj is Display otherDisplay)
        {
            return Identifier == otherDisplay.Identifier &&
                   Name == otherDisplay.Name &&
                   OutputTechnology == otherDisplay.OutputTechnology &&
                   CurrentResolution.Equals(otherDisplay.CurrentResolution) &&
                   CurrentRefreshRate == otherDisplay.CurrentRefreshRate &&
                   SupportedRefreshRates.SequenceEqual(otherDisplay.SupportedRefreshRates) &&
                   SupportedResolutions.SequenceEqual(otherDisplay.SupportedResolutions);
        }

        return false;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Identifier, Name, (int)OutputTechnology, SupportedResolutions, SupportedRefreshRates, CurrentResolution, CurrentRefreshRate);
    }
}