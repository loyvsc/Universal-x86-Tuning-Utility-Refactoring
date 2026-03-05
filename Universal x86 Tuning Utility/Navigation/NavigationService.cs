using System;
using System.Linq;
using ApplicationCore.Interfaces;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using Splat;
using Universal_x86_Tuning_Utility.ViewModels;

namespace Universal_x86_Tuning_Utility.Navigation;

public class NavigationService : INavigationService
{
    public static NavigationService? Instance => Locator.Current.GetService<INavigationService>() as NavigationService;

    private Frame _frame;
    private NavigationView _navigationView;
    
    private readonly MainWindowViewModel _mainWindowViewModel;

    public NavigationService()
    {
        _mainWindowViewModel = Locator.Current.GetService<MainWindowViewModel>()!;
    }
    
    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }
    
    public void SetNavigationView(NavigationView navigationView)
    {
        _navigationView = navigationView;
    }

    public void Navigate(Type type)
    {
        var vm = Locator.Current.GetService(type);
        if (vm != null)
        {
            NavigateFromContext(vm);
        }
    }

    public void NavigateFromContext(object dataContext)
    {
        var targetNavItem = _mainWindowViewModel.NavigationItems.FirstOrDefault(x => x.ViewModelType == dataContext.GetType());
        if (_mainWindowViewModel.SelectedNavigationItem?.ViewModelType != targetNavItem?.ViewModelType && _mainWindowViewModel.SelectedNavigationItem?.ViewModelType != dataContext.GetType())
        {
            _mainWindowViewModel.SelectedNavigationItem = targetNavItem;
            _frame.NavigateFromObject(dataContext,
                new FluentAvalonia.UI.Navigation.FrameNavigationOptions
                {
                    IsNavigationStackEnabled = true,
                    TransitionInfoOverride = new SuppressNavigationTransitionInfo()
                });   
        }
    }
}
