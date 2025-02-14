using System;
using System.Linq;
using System.Reflection;
using Splat;

namespace Universal_x86_Tuning_Utility.Extensions;


public static class SplatRegistrationsExtensions
{
    public static void RegisterPlatformSpecificService<TInterface, TWindowsImpl, TLinuxImpl>() 
        where TWindowsImpl : class, TInterface
        where TLinuxImpl : class, TInterface
    {
        if (OperatingSystem.IsWindows())
        {
            var instance = CreateInstance<TWindowsImpl>();
            SplatRegistrations.RegisterConstant<TInterface>(instance);
            return;
        }
        if (OperatingSystem.IsLinux())
        {
            var instance = CreateInstance<TLinuxImpl>();
            SplatRegistrations.RegisterConstant<TInterface>(instance);
            return;
        }

        throw new PlatformNotSupportedException("Current operating system is not supported by ISystemInfoService");
    }

    private static TObject CreateInstance<TObject>()
    {
        var objectType = typeof(TObject);
        
        var ctorInfo = objectType.GetConstructors(BindingFlags.Public)[0];
        var ctorParamsTypes = ctorInfo.GetParameters().Select(x=>x.ParameterType);
        var ctorParams = ctorParamsTypes.Select(serviceType => Locator.Current.GetService(serviceType)!).ToArray();
        
        return (TObject) Activator.CreateInstance(objectType, ctorParams);
    }
}