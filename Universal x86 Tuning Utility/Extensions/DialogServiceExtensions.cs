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
    
    public static Task Show<TParent, TChild>(this IDialogService dialogService, TParent parent)
    {
        return dialogService.Show<TChild>(parent, Locator.Current.GetService<TChild>());
    }
}