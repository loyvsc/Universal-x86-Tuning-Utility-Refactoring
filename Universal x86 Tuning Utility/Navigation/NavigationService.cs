using System;
using ApplicationCore.Interfaces;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;

namespace Universal_x86_Tuning_Utility.Navigation;

public class NavigationService : INavigationService
{
    public static NavigationService Instance { get; } = new NavigationService();

    private Frame _frame;
    
    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public void Navigate(Type type)
    {
        _frame.Navigate(type);
    }

    public void NavigateFromContext(object dataContext)
    {
        _frame.NavigateFromObject(dataContext,
            new FluentAvalonia.UI.Navigation.FrameNavigationOptions
            {
                IsNavigationStackEnabled = true,
                TransitionInfoOverride = new SuppressNavigationTransitionInfo()
            });
    }
}
