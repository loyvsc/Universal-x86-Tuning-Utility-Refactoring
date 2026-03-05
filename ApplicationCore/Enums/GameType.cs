using System.ComponentModel;

namespace ApplicationCore.Enums;

public enum GameType
{
    Custom,
    Steam,
    
    [Description("Battle.Net")]
    BattleNet,
    
    [Description("GOG")]
    Gog,
    
    [Description("EA")]
    Origin,
    
    [Description("Epic Games Store")]
    EpicGamesStore,
    
    [Description("Microsoft Store")]
    MicrosoftStore,
    
    [Description("Ubisoft Store")]
    UbisoftStore,
}