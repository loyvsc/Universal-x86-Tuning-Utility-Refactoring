using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Threading;
using DialogHostAvalonia;
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
    
    private const string DialogHostIdentifier = "Main";
    private static bool IsDialogOpened = false;
    private static readonly App.ViewLocator ViewLocator = new();
    
    public static IDisposable Show<TChild>(this INotifyPropertyChanged _) where TChild : INotifyPropertyChanged
    {
        var childViewModel = Locator.Current.GetService<TChild>();
        
        ArgumentNullException.ThrowIfNull(childViewModel);

        var content = Dispatcher.UIThread.Invoke(() => ViewLocator.Build(childViewModel));

        Dispatcher.UIThread.Invoke(() => DialogHost.Show(content, DialogHostIdentifier, openedEventHandler: (_, _) => IsDialogOpened = true));
        
        return Disposable.Create(async () =>
        {
            if (IsDialogOpened)
            {
                CloseDialog();
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    await Task.Delay(1000);
                }

                if (IsDialogOpened)
                {
                    CloseDialog();
                }
            }
        });
    }

    private static void CloseDialog()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (DialogHost.IsDialogOpen(DialogHostIdentifier))
            {
                DialogHost.Close(DialogHostIdentifier);
            }
        });

        IsDialogOpened = false;
    }
}