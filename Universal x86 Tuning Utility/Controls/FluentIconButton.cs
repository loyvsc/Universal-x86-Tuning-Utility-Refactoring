using Avalonia;
using Avalonia.Controls;
using FluentIcons.Common;

namespace Universal_x86_Tuning_Utility.Controls;

public class FluentIconButton : Button
{
    public static readonly StyledProperty<Icon> IconProperty = AvaloniaProperty.Register<FluentIconButton, Icon>(nameof(Icon));

    public Icon Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<IconVariant> IconVariantProperty = AvaloniaProperty.Register<FluentIconButton, IconVariant>(nameof(IconVariant));

    public IconVariant IconVariant
    {
        get => GetValue(IconVariantProperty);
        set => SetValue(IconVariantProperty, value);
    }
    
    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<FluentIconButton, double>(nameof(IconSize), 20.0);

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
}