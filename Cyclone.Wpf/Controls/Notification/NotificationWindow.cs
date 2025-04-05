using System.Windows;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class NotificationWindow : Window
{
    public NotificationWindow()
    {
        // 注册命令绑定
        CommandBindings.Add(new CommandBinding(CloseWindowCommand, ExecuteCloseWindow, CanExecuteCloseWindow));
        var dict = new ResourceDictionary();
        dict.Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Notification.xaml", UriKind.Absolute);
        Resources.MergedDictionaries.Add(dict);
    }

    #region CloseCommand

    public static RoutedCommand CloseWindowCommand { get; private set; } =
        new RoutedCommand("CloseWindow", typeof(NotificationWindow));

    private void ExecuteCloseWindow(object sender, ExecutedRoutedEventArgs e)
    {
        Close();
    }

    private void CanExecuteCloseWindow(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    #endregion CloseCommand
}