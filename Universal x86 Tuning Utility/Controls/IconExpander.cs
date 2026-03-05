using Avalonia;
using Avalonia.Controls;
using FluentIcons.Common;

namespace Universal_x86_Tuning_Utility.Controls;

public class IconExpander : Expander
{
    public static readonly StyledProperty<Icon?> IconProperty = AvaloniaProperty.Register<FluentIconButton, Icon?>(nameof(IconExpander));
    
    public Icon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<FluentIconButton, double>(nameof(IconExpander), 24.0);
    
    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
}