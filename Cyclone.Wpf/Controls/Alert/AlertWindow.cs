using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

internal class AlertWindow : Window
{
    #region ButtonType

    public AlertButton ButtonType
    {
        get => (AlertButton)GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }

    public static readonly DependencyProperty ButtonTypeProperty =
        DependencyProperty.Register(nameof(ButtonType), typeof(AlertButton), typeof(AlertWindow),
            new PropertyMetadata(AlertButton.Ok));

    #endregion ButtonType

    #region OkButtonText

    public string OkButtonText
    {
        get => (string)GetValue(OkButtonTextProperty);
        set => SetValue(OkButtonTextProperty, value);
    }

    public static readonly DependencyProperty OkButtonTextProperty =
        DependencyProperty.Register(nameof(OkButtonText), typeof(string), typeof(AlertWindow),
            new PropertyMetadata("Ok"));

    #endregion OkButtonText

    #region CancelButtonText

    public string CancelButtonText
    {
        get => (string)GetValue(CancelButtonTextProperty);
        set => SetValue(CancelButtonTextProperty, value);
    }

    public static readonly DependencyProperty CancelButtonTextProperty =
        DependencyProperty.Register(nameof(CancelButtonText), typeof(string), typeof(AlertWindow),
            new PropertyMetadata("Cancel"));

    #endregion CancelButtonText

    #region Command

    void InitializeCommand()
    {
        CommandBindings.Add(new CommandBinding(OkCommand, (sender, e) =>
        {
            DialogResult = true;
            Close();
        }));

        CommandBindings.Add(new CommandBinding(CancelCommand, (sender, e) =>
        {
            DialogResult = false;
            Close();
        }));
    }

    public static RoutedCommand OkCommand { get; private set; } = new RoutedCommand("Ok", typeof(AlertWindow));
    public static RoutedCommand CancelCommand { get; private set; } = new RoutedCommand("Cancel", typeof(AlertWindow));

    #endregion Command

    public AlertWindow()
    {
        InitializeCommand();
        var dict = new ResourceDictionary();
        dict.Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Alert.xaml", UriKind.Absolute);
        Resources.MergedDictionaries.Add(dict);
    }
}