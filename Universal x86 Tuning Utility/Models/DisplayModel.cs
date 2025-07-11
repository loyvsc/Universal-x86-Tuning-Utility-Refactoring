using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ApplicationCore.Enums.Display;
using ApplicationCore.Models;
using ReactiveUI;

namespace Universal_x86_Tuning_Utility.Models;

public class DisplayModel : ReactiveObject
{
    private readonly Display _display;

    public DisplayModel(Display display)
    {
        _display = display;
        SupportedRefreshRates = new ObservableCollection<int>(_display.SupportedRefreshRates);

        Name = display.OutputTechnology == DisplayOutputTechnology.Internal ? "Internal" : display.Identifier;
        Identifier = display.Identifier;
        SupportedOutputTechnology = _display.OutputTechnology;
    }
    
    public string Name { get; }
    public string Identifier { get; }
    
    public ObservableCollection<int> SupportedRefreshRates { get; }
    public DisplayOutputTechnology SupportedOutputTechnology { get; }
}