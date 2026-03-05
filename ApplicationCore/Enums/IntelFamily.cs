using System.ComponentModel;

namespace ApplicationCore.Enums;

public enum IntelFamily
{
    /// <summary>
    /// Unknown or unspecified Intel platform.
    /// </summary>
    Unknown,

    /// <summary>
    /// 4th generation Core
    /// </summary>
    Haswell,

    /// <summary>
    /// 5th generation Core
    /// </summary>
    Broadwell,

    /// <summary>
    /// 6th generation Core
    /// </summary>
    Skylake,

    /// <summary>
    /// 7th generation Core
    /// </summary>
    [Description("Kaby Lake")]
    Kabylake,

    /// <summary>
    /// 7th generation mobile Y-series
    /// </summary>
    [Description("Amber Lake")]
    Amberlake,

    /// <summary>
    /// 8th & 9th generation Core
    /// </summary>
    [Description("Coffee Lake")]
    Coffeelake,

    /// <summary>
    /// 8th optimized mobile
    /// </summary>
    [Description("Whiskey Lake")]
    WhiskeyLake,

    /// <summary>
    /// 8th mobile experimental
    /// </summary>
    [Description("Cannon Lake")]
    CannonLake,

    /// <summary>
    /// 10th generation Core (10nm).
    /// </summary>
    [Description("Ice Lake")]
    Icelake,

    /// <summary>
    /// 10th generation Core (14nm).
    /// </summary>
    [Description("Comet Lake")]
    Cometlake,

    /// <summary>
    /// 11th generation Core (10nm SuperFin).
    /// </summary>
    [Description("Tiger Lake")]
    Tigerlake,

    /// <summary>
    /// 11th generation Core (14nm).
    /// </summary>
    [Description("Rocket Lake")]
    RocketLake,

    /// <summary>
    /// Low-power Pentium/Celeron (10nm).
    /// </summary>
    [Description("Jasper Lake")]
    Jasperlake,

    /// <summary>
    /// 12th generation Core
    /// </summary>
    [Description("Alder Lake")]
    Alderlake,

    /// <summary>
    /// 13th & 14th generation Core
    /// </summary>
    [Description("Raptor Lake")]
    Raptorlake,

    /// <summary>
    /// Core Ultra Series 1
    /// </summary>
    [Description("Meteor Lake")]
    Meteorlake,

    /// <summary>
    /// Core Ultra Series 2
    /// </summary>
    [Description("Arrow Lake")]
    Arrowlake,

    /// <summary>
    /// Core Ultra Series 2 mobile
    /// </summary>
    [Description("Lunar Lake")]
    Lunarlake,

    /// <summary>
    /// Core Ultra Series 3
    /// </summary>
    [Description("Panther Lake")]
    Pantherlake,

    /// <summary>
    /// Upcoming Core Ultra Series 4
    /// </summary>
    [Description("Nova Lake")]
    Novalake,

    /// <summary>
    /// Rumored future client platform.
    /// </summary>
    [Description("Wildcat Lake")]
    Wildcatlake
}