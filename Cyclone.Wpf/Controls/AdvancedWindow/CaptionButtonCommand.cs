using Cyclone.Wpf.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;


public class CaptionButtonCommand
{
    #region Commands
    static CaptionButtonCommand()
    {
        CommandManager.RegisterClassCommandBinding(typeof(CaptionButtonCommand), new CommandBinding(CloseCommand, OnClose, OnCanClose));
        CommandManager.RegisterClassCommandBinding(typeof(CaptionButtonCommand), new CommandBinding(MaximizeCommand, OnMaximize, OnCanMaximize));
        CommandManager.RegisterClassCommandBinding(typeof(CaptionButtonCommand), new CommandBinding(RestoreCommand, OnRestore, OnCanRestore));
        CommandManager.RegisterClassCommandBinding(typeof(CaptionButtonCommand), new CommandBinding(MinimizeCommand, OnMinimize, OnCanMinimize));
        CommandManager.RegisterClassCommandBinding(typeof(CaptionButtonCommand), new CommandBinding(TopmostCommand, OnTopmost, OnCanTopmost));
    }

    #region Close
    public static RoutedCommand CloseCommand { get; private set; } = new RoutedCommand("Close", typeof(CaptionButtonCommand));
    public static void OnClose(object sender, ExecutedRoutedEventArgs e)
    {
        var window = sender as Window;
        window?.Close();
    }

    public static void OnCanClose(object sender, CanExecuteRoutedEventArgs e)
    {

        if (sender is Window window)
        {
            e.CanExecute = window.Visibility == Visibility.Visible && window.IsActive;
            return;
        }

        e.CanExecute = false;
    }
    #endregion

    #region Maximize

    public static void OnMaximize(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            window.WindowState = (window.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

    }

    public static void OnCanMaximize(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = window.Visibility == Visibility.Visible &&
                           window.WindowState != WindowState.Maximized &&
                           window.IsActive;
            return;
        }

        e.CanExecute = false;

    }

    public static RoutedCommand MaximizeCommand { get; private set; } = new RoutedCommand("Maximize", typeof(CaptionButtonCommand));
    #endregion

    #region Restore
    public static void OnCanRestore(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = window.Visibility == Visibility.Visible &&
                           window.WindowState != WindowState.Normal &&
                           window.IsActive;
            return;
        }

        e.CanExecute = false;

    }

    public static void OnRestore(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

    }
    public static RoutedCommand RestoreCommand { get; private set; } = new RoutedCommand("Restore", typeof(CaptionButtonCommand));


    #endregion

    #region Minimize


    public static void OnCanMinimize(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = window.Visibility == Visibility.Visible &&
                           window.WindowState != WindowState.Minimized &&
                           window.IsActive;
            return;
        }

        e.CanExecute = false;


    }

    public static void OnMinimize(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            window.WindowState = (window.WindowState == WindowState.Minimized) ? WindowState.Normal : WindowState.Minimized;
        }

    }

    public static RoutedCommand MinimizeCommand { get; private set; } = new RoutedCommand("Minimize", typeof(CaptionButtonCommand));
    #endregion

    #region Topmost
    public static void OnCanTopmost(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is Window window)
        {
            e.CanExecute = window.Visibility == Visibility.Visible && window.IsActive;
            return;
        }

        e.CanExecute = false;

    }

    public static void OnTopmost(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is not ToggleButton button) { return; }

        if (sender is not Window window) { return; }


        window.Topmost = button.IsChecked ?? false;

    }

    public static RoutedCommand TopmostCommand { get; private set; } = new RoutedCommand("Topmost", typeof(CaptionButtonCommand));
    #endregion



    #endregion
}
