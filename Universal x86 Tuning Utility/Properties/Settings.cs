using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Universal_x86_Tuning_Utility.Properties;

public sealed partial class Settings
{
    public static Settings Default = new Settings();
    
    private readonly string __saveFileName = "props.dat";
    private Dictionary<string, object> _properties = new Dictionary<string, object>();

    public Settings()
    {
        Reload();
    }
    
    public T? Get<T>(string key)
    {
        if (_properties.Count != 0)
        {
            var value = CollectionsMarshal.GetValueRefOrNullRef(_properties, key);
            return (T?)value;
        }
        
        return default;
    }

    public void Set<T>(string key, T value)
    {
        if (value != null)
        {
            ref var valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_properties, key, out _);
            valueRef = value;
        }
    }

    public void Save()
    {
        var serialized = JsonSerializer.Serialize(_properties);
        System.IO.File.WriteAllText(__saveFileName, serialized);
    }

    public void Reload()
    {
        if (System.IO.File.Exists(__saveFileName))
        {
            var serializedObject = System.IO.File.ReadAllText(__saveFileName);
            if (!string.IsNullOrWhiteSpace(serializedObject))
            {
                _properties = JsonSerializer.Deserialize<Dictionary<string, object>>(serializedObject) ?? new Dictionary<string, object>();
            }
        }
        else
        {
            var propertiesConfig = System.IO.File.ReadAllText("app.config");
            if (!string.IsNullOrWhiteSpace(propertiesConfig))
            {
                var doc = XDocument.Parse(propertiesConfig);

                // Find the <userSettings> -> <SettingsElementName> -> <setting> elements
                var settingElements = doc.Descendants("userSettings")
                    .Descendants("Universal_x86_Tuning_Utility.Properties.Settings")
                    .Elements("setting");

                var settingsType = GetType();

                foreach (var setting in settingElements)
                {
                    var name = setting.Attribute("name")?.Value;
                    var defaultValue = setting.Attribute("defaultValue")?.Value;

                    if (name != null && defaultValue != null)
                    {
                        var property = settingsType.GetProperty(name);
                        if (property != null)
                        {
                            object convertedValue = Convert.ChangeType(defaultValue, property.PropertyType, CultureInfo.InvariantCulture);
                            property.SetValue(this, convertedValue);
                        }
                    }
                }
                Save();
            }
            else
            {
                _properties = new Dictionary<string, object>();
            }
        }
    }
}