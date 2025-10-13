using System;
using System.Management;

namespace Universal_x86_Tuning_Utility.Windows.Extensions;

public static class ManagementExtensions
{
    public static T? Get<T>(this ManagementBaseObject managementObject, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            return default;
        }
        
        try
        {
            var property =  managementObject.Properties[propertyName];
            if (property.Value == null) return default;
            return (T) Convert.ChangeType(property.Value, typeof(T));
        }
        catch
        {
            return default;
        }
    }
    
    public static ManagementBaseObject? Find(this ManagementObjectSearcher managementObjectSearcher, Func<ManagementBaseObject, bool> selector)
    {
        foreach (var obj in managementObjectSearcher.Get())
        {
            try
            {
                if (selector(obj))
                {
                    return obj;
                }
            }
            catch
            {
                // Ignored
            }
        }
        return null;
    }
}