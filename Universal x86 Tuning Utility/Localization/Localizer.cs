using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Avalonia;
using Universal_x86_Tuning_Utility.Localization.Interfaces;
using Universal_x86_Tuning_Utility.Localization.Models;

namespace Universal_x86_Tuning_Utility.Localization;

/// <summary>
/// Localization service
/// </summary>
internal sealed class Localizer : ILocalizer
{
    /// <summary>
    /// Path to localization assets
    /// </summary>
    private const string LocalizationPath = "avares://Universal x86 Tuning Utility/Resources/Localizations";

    /// <summary>
    /// Property name for changing notification
    /// </summary>
    private const string IndexerPropertyName = "Item";

    /// <summary>
    /// Property name for changing notification
    /// </summary>
    private const string IndexerPropertyArrayName = "Item[]";

    /// <summary>
    /// Dictionary to compare 'key' - 'localized sentence'
    /// </summary>
    private ConcurrentDictionary<string, string>? _dict;

    private Language _currentLanguage = Language.UnknownLang;

    /// <summary>
    /// Initializes a new instance of the <see cref="Localizer"/> class.
    /// </summary>
    public Localizer()
    {
        LoadLocalizationPackages();
    }

    /// <summary>
    /// Event on the localization property changed
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;
    
    public event EventHandler<Language>? LanguageChanged; 

    /// <inheritdoc/>
    public Language CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage?.Key != value.Key)
            {
                _currentLanguage = value;
                SwitchLanguage(_currentLanguage.Key);
            }
        }
    }

    /// <inheritdoc/>
    public List<Language> AvailableLanguages { get; set; } = new();

    /// <inheritdoc/>
    public string this[string key]
    {
        get
        {
            if (_dict == null)
            {
#if DEBUG
                return key;
#else
                throw new CultureNotFoundException("Localization packages don't initialized.");
#endif
            }

            if (string.IsNullOrWhiteSpace(CurrentLanguage.Key))
            {
                throw new CultureNotFoundException(
                    "Local language doesn't selected. Call to the 'SwitchLanguage' method.");
            }

            if (_dict.TryGetValue(key, out var res))
            {
                return res.Replace("\\n", "\n");
            }

            return key;
        }
    }

    /// <summary>
    /// Switch current language of the program
    /// </summary>
    /// <param name="languageCode"> New language code in format: 'en-US' </param>
    /// <returns> True, if success </returns>
    /// <exception cref="CultureNotFoundException"> Missing a localization package for this language code </exception>
    public bool SwitchLanguage(string languageCode)
    {
        var uri = new Uri($"{LocalizationPath}/{languageCode}.json");

        if (ProgramCore.IsAssetExistsFunc?.Invoke(uri) != true)
        {
            throw new CultureNotFoundException("Missing a localization package.");
        }

        if(ProgramCore.OpenAssetFunc != null)
            using (var streamReader = new StreamReader(ProgramCore.OpenAssetFunc(uri), Encoding.UTF8))
            {
                var packageData = streamReader.ReadToEnd();
                if (packageData == null)
                {
                    throw new DataException("Incorrect localization package format.");
                }

                var result = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(packageData);
                _dict = result ?? throw new DataException("Incorrect localization package format.");
            }

        _currentLanguage = AvailableLanguages.First(x => x.Key == languageCode);

        LanguageChanged?.Invoke(this, _currentLanguage);
        OnPropertyChanged();
        return true;
    }

    /// <summary>
    /// Raise event on localization property changed
    /// </summary>
    private void OnPropertyChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerPropertyName));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(IndexerPropertyArrayName));
    }

    /// <summary>
    /// Load localization packages
    /// </summary>
    /// <exception cref="AvaloniaInternalException"> Service initialization called before Avalonia has been initialized </exception>
    /// <exception cref="CultureNotFoundException"> Missing localization packages </exception>
    private void LoadLocalizationPackages()
    {
        AvailableLanguages = new List<Language>();

        var dirUri = new Uri(LocalizationPath);
        IEnumerable<Uri> assets = ProgramCore.GetAssetsFunc?.Invoke(dirUri) ?? Enumerable.Empty<Uri>();

        foreach (var asset in assets)
        {
            var assetName = asset.Segments.LastOrDefault();

            if (string.IsNullOrWhiteSpace(assetName))
            {
                continue;
            }

            var localeName = assetName[..assetName.LastIndexOf(".", StringComparison.Ordinal)];

            if (!string.IsNullOrWhiteSpace(localeName) && ProgramCore.OpenAssetFunc != null)
            {
                using (var streamReader = new StreamReader(ProgramCore.OpenAssetFunc(new Uri(asset.AbsoluteUri)), Encoding.UTF8))
                {
                    var packageData = streamReader.ReadToEnd();
                    if (packageData == null)
                    {
                        throw new DataException("Incorrect localization package format.");
                    }

                    var result = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(packageData);

                    if (result == null)
                    {
                        throw new DataException("Incorrect localization package format.");
                    }
                    
                    AvailableLanguages.Add(new Language(localeName, result["LangPackageName"]));
                }
            }
        }

        if (AvailableLanguages.Count == 0)
        {
#if !DEBUG
            throw new CultureNotFoundException("Missing localization packages");
#endif
        }
    }
}