using System;
using System.Diagnostics.SymbolStore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class AlertWindow : Window
{
    #region Icon

    public new static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(AlertWindow), new PropertyMetadata(default(object)));

    public new object Icon
    {
        get => (object)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region TitleForeground

    public Brush TitleForeground
    {
        get => (Brush)GetValue(TitleForegroundProperty);
        set => SetValue(TitleForegroundProperty, value);
    }

    public static readonly DependencyProperty TitleForegroundProperty =
        DependencyProperty.Register(nameof(TitleForeground), typeof(Brush), typeof(AlertWindow), new PropertyMetadata(Brushes.White));

    #endregion TitleForeground

    #region CaptionBackground

    public Brush CaptionBackground
    {
        get => (Brush)GetValue(CaptionBackgroundProperty);
        set => SetValue(CaptionBackgroundProperty, value);
    }

    public static readonly DependencyProperty CaptionBackgroundProperty =
        DependencyProperty.Register(nameof(CaptionBackground), typeof(Brush), typeof(AlertWindow), new PropertyMetadata(SystemColors.ActiveCaptionBrush));

    #endregion CaptionBackground

    #region CaptionHeight

    public double CaptionHeight
    {
        get => (double)GetValue(CaptionHeightProperty);
        set => SetValue(CaptionHeightProperty, value);
    }

    public static readonly DependencyProperty CaptionHeightProperty =
        DependencyProperty.Register(nameof(CaptionHeight), typeof(double), typeof(AlertWindow), new PropertyMetadata(SystemParameters.CaptionHeight));

    #endregion CaptionHeight

    #region AlertButtonHorizontalAlignment

    public HorizontalAlignment AlertButtonHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(AlertButtonHorizontalAlignmentProperty);
        set => SetValue(AlertButtonHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty AlertButtonHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(AlertButtonHorizontalAlignment), typeof(HorizontalAlignment), typeof(AlertWindow), new PropertyMetadata(HorizontalAlignment.Center));

    #endregion AlertButtonHorizontalAlignment

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

    #region AlertButtonGroupBackground

    public Brush AlertButtonGroupBackground
    {
        get => (Brush)GetValue(AlertButtonGroupBackgroundProperty);
        set => SetValue(AlertButtonGroupBackgroundProperty, value);
    }

    public static readonly DependencyProperty AlertButtonGroupBackgroundProperty =
        DependencyProperty.Register(nameof(AlertButtonGroupBackground), typeof(Brush), typeof(AlertWindow), new PropertyMetadata(Brushes.Transparent));

    #endregion AlertButtonGroupBackground

    #region AlertButtonGroupHeight

    public double AlertButtonGroupHeight
    {
        get => (double)GetValue(AlertButtonGroupHeightProperty);
        set => SetValue(AlertButtonGroupHeightProperty, value);
    }

    public static readonly DependencyProperty AlertButtonGroupHeightProperty =
        DependencyProperty.Register(nameof(AlertButtonGroupHeight), typeof(double), typeof(AlertWindow), new PropertyMetadata(0d));

    #endregion AlertButtonGroupHeight

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

        CommandBindings.Add(new CommandBinding(CloseCommand, (sender, e) =>
        {
            DialogResult = null;
            Close();
        }));
    }

    public static RoutedCommand OkCommand { get; private set; } = new RoutedCommand("Ok", typeof(AlertWindow));
    public static RoutedCommand CancelCommand { get; private set; } = new RoutedCommand("Cancel", typeof(AlertWindow));
    public static RoutedCommand CloseCommand { get; private set; } = new RoutedCommand("Close", typeof(AlertWindow));

    #endregion Command

    public AlertWindow()
    {
        InitializeCommand();
        var dict = new ResourceDictionary();
        dict.Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Alert.xaml", UriKind.Absolute);
        Resources.MergedDictionaries.Add(dict);
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}