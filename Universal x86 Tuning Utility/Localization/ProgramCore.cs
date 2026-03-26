using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Universal_x86_Tuning_Utility.Localization.Interfaces;

namespace Universal_x86_Tuning_Utility.Localization;

/// <summary>
/// Program core
/// </summary>
public static class ProgramCore
{
    /// <summary>
    /// Localization service
    /// </summary>
    private static ILocalizer? _localizer;

    /// <summary>
    /// Gets localization service
    /// </summary>
    /// <value> Localization service </value>
    public static ILocalizer Localizer
    {
        get
        {
            _localizer ??= new Localizer();

            return _localizer;
        }
    }

    public static Func<Uri, bool>? IsAssetExistsFunc { get; private set; } 
    public static Func<Uri, IEnumerable<Uri>>? GetAssetsFunc { get; private set; } 
    public static Func<Uri, Stream>? OpenAssetFunc { get; private set; }

    /// <summary>
    /// Initialize core
    /// </summary>
    public static void Initialize(Func<Uri, bool> isAssetExists, Func<Uri, IEnumerable<Uri>> getAssets, Func<Uri, Stream> openAssetFnuc, string language = "en-EN")
    {
        IsAssetExistsFunc = isAssetExists;
        GetAssetsFunc = getAssets;
        OpenAssetFunc = openAssetFnuc;
        
        try
        {
            if (string.IsNullOrWhiteSpace(language))
            {
                language = CultureInfo.CurrentCulture.ToString();
            }
            _ = Localizer.SwitchLanguage(language);
        }
        catch
        {
            _ = Localizer.SwitchLanguage("en-EN");
        }
    }

    public static ILocalizer ProvideLocalizer(string languageCode = "en-EN")
    {
        if (InitializedLocalizers.TryGetValue(languageCode, out var initializedLocalizer))
        {
            return initializedLocalizer;
        }
        
        var localizer = new Localizer();
        localizer.SwitchLanguage(languageCode);
        InitializedLocalizers.Add(languageCode, localizer);
        return localizer;
    }
    
    private static readonly Dictionary<string, ILocalizer> InitializedLocalizers = new();
}