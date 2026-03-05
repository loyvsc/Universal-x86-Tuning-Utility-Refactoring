using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ReactiveUI;

namespace Universal_x86_Tuning_Utility.Extensions;

public static class ReactiveObjectExtensions
{
    public static TRet RaiseAndSetIfChangedWithAfterSetAction<TObj, TRet>(
        this TObj reactiveObject,
        ref TRet backingField,
        TRet newValue,
        Action? afterSetAction = null,
        [CallerMemberName] string? propertyName = null)
        where TObj : ReactiveObject
    {
        if (string.IsNullOrWhiteSpace(propertyName))
        {
            throw new ArgumentException(nameof(propertyName));
        }

        if (EqualityComparer<TRet>.Default.Equals(backingField, newValue))
        {
            return newValue;
        }

        reactiveObject.RaisePropertyChanging(propertyName!);
        backingField = newValue;
        reactiveObject.RaisePropertyChanged(propertyName!);
        afterSetAction?.Invoke();
        return newValue;
    }
}