using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ApplicationCore.Utilities;
using Universal_x86_Tuning_Utility.Models;

namespace Universal_x86_Tuning_Utility.ViewModels;

public partial class DataViewModel : NotifyPropertyChangedBase
{
    private List<DataColor> _colors;

    public List<DataColor> Colors
    {
        get => _colors;
        set => SetValue(ref _colors, value);
    }
    
    public DataViewModel()
    {
        var random = new Random();
        var colorsArray = new DataColor[8192];
        
        for (int i = 0; i < 8192; i++)
        {
            colorsArray[i] = new DataColor
            {
                Color = new SolidColorBrush(Color.FromArgb(
                    a: 200,
                    r: (byte)random.Next(0, 250),
                    g: (byte)random.Next(0, 250),
                    b: (byte)random.Next(0, 250)))
            };
        }
        
        Colors = colorsArray.ToList();
    }
}