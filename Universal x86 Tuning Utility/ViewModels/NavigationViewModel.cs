using System;
using ApplicationCore.Utilities;
using FluentAvalonia.FluentIcons;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class NavigationViewModel : NotifyPropertyChangedBase
{
    private bool _isInitializing;
    private FluentIconSymbol _iconSymbol;
    private string _title;
    private object? _dataContext;

    public bool IsInitializing
    {
        get => _isInitializing;
        set => SetValue(ref _isInitializing, value);
    }

    public string Title
    {
        get => _title;
        set => SetValue(ref _title, value);
    }

    public FluentIconSymbol IconSymbol
    {
        get => _iconSymbol;
        set => SetValue(ref _iconSymbol, value);
    }

    public Type ViewModelType { get; set; }

    public object? DataContext
    {
        get => _dataContext;
        set => SetValue(ref _dataContext, value);
    }
}