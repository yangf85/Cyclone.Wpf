using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.ViewModels;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Demo;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class ShellWindow : AdvancedWindow
{
    public ShellWindow()
    {
        InitializeComponent();
        DataContext = new ShellWindowViewModel();
        Loaded += ShellWindow_Loaded;
    }

    private void ShellWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine($"ShellWindow Top:{Top} \n Left:{Left} \n Bottom:{Top + ActualHeight} \n Right:{Left + ActualWidth}");
        Debug.WriteLine($"ShellWindow Width:{ActualWidth} \n Height:{ActualHeight} ");
    }
}