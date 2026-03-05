using ReactiveUI;

namespace Universal_x86_Tuning_Utility.ViewModels.Dialogs;

public class ReloadingGamesDialogViewModel : ReactiveObject
{
    public string Title { get; set; } = "Locating Games";
    public string Description { get; set; } = "This should only take a few moments!";
}