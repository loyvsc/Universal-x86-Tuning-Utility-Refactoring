using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace Universal_x86_Tuning_Utility.Resources.Converters;

public static class BitmapConverters
{
    public static readonly IValueConverter StringToBitmapConverter = new FuncValueConverter<string, Bitmap?>(path => string.IsNullOrWhiteSpace(path) ? null : new Bitmap(path));
}