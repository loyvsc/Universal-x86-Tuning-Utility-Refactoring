using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace Universal_x86_Tuning_Utility.Properties;

public sealed partial class Settings
{
    public static readonly Settings Default = new Settings();
    
    private const string SaveSettingsPath = "props.json";
    private Dictionary<string, object> _properties = new Dictionary<string, object>();

    public Settings()
    {
        Reload();
    }
    
    private T? Get<T>(string key)
    {
        if (_properties.Count != 0)
        {
            ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_properties, key, out _);
            return (T?) Convert.ChangeType(value, typeof(T));
        }
        
        return default;
    }

    private void Set<T>(string key, T value)
    {
        if (value != null)
        {
            ref var valueRef = ref CollectionsMarshal.GetValueRefOrAddDefault(_properties, key, out _);
            valueRef = value;
        }
    }

    public void Save()
    {
        var serialized = JsonConvert.SerializeObject(_properties);
        System.IO.File.WriteAllText(SaveSettingsPath, serialized);
    }

    public void Reload()
    {
        if (System.IO.File.Exists(SaveSettingsPath))
        {
            var serializedObject = System.IO.File.ReadAllText(SaveSettingsPath);
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
                            else if (property.PropertyType.GetTypeInfo().IsValueType)
                            {
                                property.SetValue(this, Activator.CreateInstance(property.PropertyType));
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