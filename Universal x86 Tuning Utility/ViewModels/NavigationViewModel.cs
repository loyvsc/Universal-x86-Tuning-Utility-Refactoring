using System;
using ReactiveUI;

namespace Universal_x86_Tuning_Utility.ViewModels;

public class NavigationViewModel : ReactiveObject
{
    private bool _isInitializing;
    private object _iconSymbol;
    private string _title;
    private object? _dataContext;

    public bool IsInitializing
    {
        get => _isInitializing;
        set => this.RaiseAndSetIfChanged(ref _isInitializing, value);
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public object IconSymbol
    {
        get => _iconSymbol;
        set => this.RaiseAndSetIfChanged(ref _iconSymbol, value);
    }

    public Type ViewModelType { get; set; }

    public object? DataContext
    {
        get => _dataContext;
        set => this.RaiseAndSetIfChanged(ref _dataContext, value);
    }
}