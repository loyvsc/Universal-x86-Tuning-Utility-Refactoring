using Avalonia.Controls;

namespace Universal_x86_Tuning_Utility.Views.Pages;

/// <summary>
/// Interaction logic for SystemInfo.xaml
/// </summary>
public class SystemInfoPage : UserControl
{
    public SystemInfoPage()
    {
        InitializeComponent();
    }

    // private void mainScroll_ScrollChanged(object sender, ScrollChangedEventArgs e)
    // {
    //     if (IsScrollBarVisible(mainScroll)) mainCon.Margin = new Thickness(0, 0, -12, 0);
    //     else mainCon.Margin = new Thickness(0, 0, 0, 0);
    // }
    //
    // public bool IsScrollBarVisible(ScrollViewer scrollViewer)
    // {
    //     if (scrollViewer == null) throw new ArgumentNullException(nameof(scrollViewer));
    //
    //     return scrollViewer.ExtentHeight > scrollViewer.ViewportHeight;
    // }
}