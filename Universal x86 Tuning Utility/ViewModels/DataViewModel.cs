using System;
using System.Collections.Generic;
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

    //todo: for what this code?
    private void InitializeViewModel()
    {
        var random = new Random();
        var colorCollection = new List<DataColor>();

        for (int i = 0; i < 8192; i++)
            colorCollection.Add(new DataColor
            {
                Color = new SolidColorBrush(Color.FromArgb(
                    (byte)200,
                    (byte)random.Next(0, 250),
                    (byte)random.Next(0, 250),
                    (byte)random.Next(0, 250)))
            });

        Colors = colorCollection;
    }
}