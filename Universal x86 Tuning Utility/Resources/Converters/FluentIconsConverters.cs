using ApplicationCore.Models;
using Avalonia.Data.Converters;
using FluentIcons.Common;

namespace Universal_x86_Tuning_Utility.Resources.Converters;

public static class FluentIconsConverters
{
    public static readonly IValueConverter PremadePresetToFluentIcon =
        new FuncValueConverter<PremadePreset, FluentIcons.Common.Icon?>(x =>
        {
            return x?.Name switch
            {
                "Eco" => Icon.LeafTwo,
                "Balance" => Icon.Scales,
                "Performance" => Icon.Gauge,
                "Extreme" => Icon.FastAcceleration,
                _ => Icon.LeafTwo
            };
        });
}