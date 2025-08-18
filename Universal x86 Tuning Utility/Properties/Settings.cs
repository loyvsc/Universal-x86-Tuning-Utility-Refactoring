using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Accord;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Universal_x86_Tuning_Utility.Properties;

public sealed partial class Settings
{
    public static readonly Settings Default = new Settings();
    
    private readonly string __saveFileName = "props.json";
    private Dictionary<string, object> _properties = new Dictionary<string, object>();

    public Settings()
    {
        Reload();
    }
    
    public T? Get<T>(string key)
    {
        if (_properties.Count != 0)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_properties, key, out _);
            return (T?) Convert.ChangeType(value, typeof(T));
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
                _properties = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedObject) ?? new Dictionary<string, object>();
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

                    if (name != null)
                    {
                        var property = settingsType.GetProperty(name);
                        if (property != null)
                        {
                            if (defaultValue != null)
                            {
                                object convertedValue = Convert.ChangeType(defaultValue, property.PropertyType, CultureInfo.InvariantCulture);
                                property.SetValue(this, convertedValue);
                            }
                            else
                            {
                                property.SetValue(this, property.PropertyType.GetDefaultValue());
                            }
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