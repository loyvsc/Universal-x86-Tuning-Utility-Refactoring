using System;
using System.Collections.Generic;
using System.ComponentModel;
using Universal_x86_Tuning_Utility.Localization.Models;

namespace Universal_x86_Tuning_Utility.Localization.Interfaces;

/// <summary>
/// Interface for localization service
/// </summary>
public interface ILocalizer : INotifyPropertyChanged
{
    public event EventHandler<Language>? LanguageChanged; 
    
    /// <summary>
    /// Gets current language code in format: 'en-US'
    /// </summary>
    /// <value> Current language code </value>
    Language CurrentLanguage { get; set; }

    /// <summary>
    /// Gets list of languages with available localization packages in format: 'en-US'
    /// </summary>
    /// <value> List of available languages </value>
    List<Language> AvailableLanguages { get; }

    /// <summary>
    /// Localize to current language by key in the package
    /// </summary>
    /// <param name="key"> Key </param>
    /// <returns> Localized sentence </returns>
    string this[string key] { get; }

    /// <summary>
    /// Switch current language
    /// </summary>
    /// <param name="languageCode"> Localization code in format: 'en-US' </param>
    /// <returns> True, if switched </returns>
    bool SwitchLanguage(string languageCode);
}