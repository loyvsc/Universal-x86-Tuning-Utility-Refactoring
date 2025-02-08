
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ApplicationCore.Utilities;
using FluentAvalonia.UI.Controls;
using Universal_x86_Tuning_Utility.Scripts;

namespace Universal_x86_Tuning_Utility.ViewModels
{
    public partial class MainWindowViewModel : NotifyPropertyChanged
    {
        private bool _isInitialized = false;

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationItems = new();

        [ObservableProperty]
        private ObservableCollection<INavigationControl> _navigationFooter = new();

        [ObservableProperty]
        private ObservableCollection<MenuItem> _trayMenuItems = new();

        [ObservableProperty]
        private string _downloads = "Downloads: ";

        [ObservableProperty]
        private bool _isDownloads = false;

        public MainWindowViewModel(INavigationService navigationService)
        {
            if (!_isInitialized)
                InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            if (Family.TYPE == Family.ProcessorType.Intel)
            {
                NavigationItems = new ObservableCollection<INavigationControl>
                {
                new NavigationViewItem()
                {
                    Content = "Home", 
                    PageTag = "dashboard",
                    Icon = SymbolRegular.Home20,
                    PageType = typeof(Views.Pages.DashboardPage)
                },
                //new NavigationItem()
                //{
                //    Content = "Premade",
                //    PageTag = "premade",
                //    Icon = SymbolRegular.Predictions20,
                //    PageType = typeof(Views.Pages.Premade)
                //},
                new NavigationItem()
                {
                    Content = "Custom",
                    PageTag = "custom",
                    Icon = SymbolRegular.Book20,
                    PageType = typeof(Views.Pages.CustomPresets)
                },
                new NavigationItem()
                {
                    Content = "Adaptive",
                    PageTag = "adaptive",
                    Icon = SymbolRegular.Radar20,
                    PageType = typeof(Views.Pages.Adaptive)
                },
                new NavigationItem()
                {
                    Content = "Games",
                    PageTag = "games",
                    Icon = SymbolRegular.Games20,
                    PageType = typeof(Views.Pages.Games)
                },
                new NavigationItem()
                {
                    Content = "Auto",
                    PageTag = "auto",
                    Icon = SymbolRegular.Transmission20,
                    PageType = typeof(Views.Pages.Automations)
                },
                //new NavigationItem()
                //{
                //    Content = "Fan",
                //    PageTag = "fan",
                //    Icon = SymbolRegular.WeatherDuststorm20,
                //    PageType = typeof(Views.Pages.FanControl)
                //},
                // new NavigationItem()
                //{
                //    Content = "Magpie",
                //    PageTag = "magpie",
                //    Icon = SymbolRegular.FullScreenMaximize20,
                //    PageType = typeof(Views.Pages.DataPage)
                //},
                new NavigationItem()
                {
                    Content = "Info",
                    PageTag = "info",
                    Icon = SymbolRegular.Info20,
                    PageType = typeof(Views.Pages.SystemInfo)
                }
            };

                NavigationFooter = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Settings",
                    PageTag = "settings",
                    Icon = SymbolRegular.Settings20,
                    PageType = typeof(Views.Pages.SettingsPage)
                }
            };

                TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Header = "Home",
                    Tag = "tray_home"
                }
            };
            }
            else
            {
                NavigationItems = new ObservableCollection<INavigationControl>
                {
                new NavigationItem()
                {
                    Content = "Home",
                    PageTag = "dashboard",
                    Icon = SymbolRegular.Home20,
                    PageType = typeof(Views.Pages.DashboardPage)
                },
                new NavigationItem()
                {
                    Content = "Premade",
                    PageTag = "premade",
                    Icon = SymbolRegular.Predictions20,
                    PageType = typeof(Views.Pages.Premade)
                },
                new NavigationItem()
                {
                    Content = "Custom",
                    PageTag = "custom",
                    Icon = SymbolRegular.Book20,
                    PageType = typeof(Views.Pages.CustomPresets)
                },
                new NavigationItem()
                {
                    Content = "Adaptive",
                    PageTag = "adaptive",
                    Icon = SymbolRegular.Radar20,
                    PageType = typeof(Views.Pages.Adaptive)
                },
                new NavigationItem()
                {
                    Content = "Games",
                    PageTag = "games",
                    Icon = SymbolRegular.Games20,
                    PageType = typeof(Views.Pages.Games)
                },
                new NavigationItem()
                {
                    Content = "Auto",
                    PageTag = "auto",
                    Icon = SymbolRegular.Transmission20,
                    PageType = typeof(Views.Pages.Automations)
                },
                //new NavigationItem()
                //{
                //    Content = "Fan",
                //    PageTag = "fan",
                //    Icon = SymbolRegular.WeatherDuststorm20,
                //    PageType = typeof(Views.Pages.FanControl)
                //},
                // new NavigationItem()
                //{
                //    Content = "Magpie",
                //    PageTag = "magpie",
                //    Icon = SymbolRegular.FullScreenMaximize20,
                //    PageType = typeof(Views.Pages.DataPage)
                //},
                new NavigationItem()
                {
                    Content = "Info",
                    PageTag = "info",
                    Icon = SymbolRegular.Info20,
                    PageType = typeof(Views.Pages.SystemInfo)
                }
            };

                NavigationFooter = new ObservableCollection<INavigationControl>
            {
                new NavigationItem()
                {
                    Content = "Settings",
                    PageTag = "settings",
                    Icon = SymbolRegular.Settings20,
                    PageType = typeof(Views.Pages.SettingsPage)
                }
            };

                TrayMenuItems = new ObservableCollection<MenuItem>
            {
                new MenuItem
                {
                    Header = "Home",
                    Tag = "tray_home"
                }
            };
            }

            _isInitialized = true;
        }
        
        private ICommand _navigateCommand;
        public ICommand NavigateCommand => _navigateCommand ??= new RelayCommand<string>(OnNavigate);

        private void OnNavigate(string parameter)
        {
            switch (parameter)
            {
                case "download":
                    Process.Start(new ProcessStartInfo("https://github.com/JamesCJ60/Universal-x86-Tuning-Utility/releases") { UseShellExecute = true });
                    return;

                case "discord":
                    Process.Start(new ProcessStartInfo("https://www.discord.gg/3EkYMZGJwq") { UseShellExecute = true });
                    return;

                case "support":
                    Process.Start(new ProcessStartInfo("https://www.paypal.com/paypalme/JamesCJ60") { UseShellExecute = true });
                    Process.Start(new ProcessStartInfo("https://patreon.com/uxtusoftware") { UseShellExecute = true });
                    return;
            }
        }
    }
}
