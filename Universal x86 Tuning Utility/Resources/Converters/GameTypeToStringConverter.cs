using System;
using System.Globalization;
using ApplicationCore.Enums;
using ApplicationCore.Extensions;
using Avalonia;
using Avalonia.Data.Converters;

namespace Universal_x86_Tuning_Utility.Resources.Converters;

public static class GameTypeConverters
{
    public static readonly IValueConverter GameTypeToStringConverter = new GameTypeToStringConverter(); 
}

public class GameTypeToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GameType gameType)
        {
            return gameType switch
            {
                GameType.Custom => "Manually added game",
                _ => gameType.GetDescription()
            };
        }

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        AvaloniaProperty.UnsetValue;
}