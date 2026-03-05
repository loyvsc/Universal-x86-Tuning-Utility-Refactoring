using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using MsBox.Avalonia.Base;

namespace Universal_x86_Tuning_Utility.Extensions;

public static class MessageBoxManagerExtensions
{
    public static Task<T> ShowDialogAsync<T>(this IMsBox<T> dialogService)
    {
        if (App.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop && desktop.MainWindow != null)
        {
            return dialogService.ShowWindowDialogAsync(desktop.MainWindow);
        }
        
        throw new Exception("Cannot show dialog without the main window");
    }
}