using System;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using Universal_x86_Tuning_Utility.ViewModels;
using Universal_x86_Tuning_Utility.Views.Pages;

namespace Universal_x86_Tuning_Utility.Navigation;

public class NavigationFactory : INavigationPageFactory
{
    public NavigationFactory(MainWindowViewModel owner)
    {
        Owner = owner;
    }

    public MainWindowViewModel Owner { get; }

    public Control GetPage(Type srcType)
    {
        return null;
    }

    public Control GetPageFromObject(object target)
    {
        return target switch
        {
            DashboardViewModel => new DashboardPage() { DataContext = target },
            PremadePresetsViewModel => new PremadePage() { DataContext = target },
            CustomPresetsViewModel => new CustomPresetsPage() { DataContext = target },
            AdaptiveViewModel => new AdaptivePage() { DataContext = target },
            GamesViewModel => new GamesPage() { DataContext = target },
            AutomationsViewModel => new AutomationsPage() { DataContext = target },
            SystemInfoViewModel => new SystemInfoPage() { DataContext = target },
            _ => new UserControl()
            {
                Background = Brushes.White, 
                Foreground = Brushes.Black,
                Content = $"Target {target} not resolved",
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            }
        };
    }
}