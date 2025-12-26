using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Splat;
using Universal_x86_Tuning_Utility.ViewModels;

namespace Universal_x86_Tuning_Utility.Extensions;

public static class DialogServiceExtensions
{
    public static Task<IDialogStorageFile?> ShowOpenFileDialogAsync(this IDialogService dialogService, OpenFileDialogSettings settings)
    {
        return dialogService.ShowOpenFileDialogAsync(Locator.Current.GetService<MainWindowViewModel>(), settings);
    }
    
    public static IDisposable Show<TChild>(this INotifyPropertyChanged parent) where TChild : INotifyPropertyChanged
    {
        var dialogService = Locator.Current.GetService<IDialogService>();
        var childViewModel = Locator.Current.GetService<TChild>();
        
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(childViewModel);
        
        dialogService.Show<TChild>(parent, childViewModel);
         
        return Disposable.Create((dialogService, childViewModel), data => data.dialogService.Close(data.childViewModel));
    }
}