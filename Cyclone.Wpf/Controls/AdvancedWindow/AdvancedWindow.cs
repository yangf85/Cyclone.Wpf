using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_MinimizeButton, Type = typeof(Button))]
[TemplatePart(Name = PART_MaximizeButton, Type = typeof(Button))]
[TemplatePart(Name = PART_CloseButton, Type = typeof(Button))]
[TemplatePart(Name = PART_RestoreButton, Type = typeof(Button))]
[TemplatePart(Name = PART_TopmostButton, Type = typeof(ToggleButton))]
public class AdvancedWindow : System.Windows.Window
{
    private const string PART_CloseButton = nameof(PART_CloseButton);

    private const string PART_RestoreButton = nameof(PART_RestoreButton);

    private const string PART_MaximizeButton = nameof(PART_MaximizeButton);

    private const string PART_MinimizeButton = nameof(PART_MinimizeButton);

    private const string PART_TopmostButton = nameof(PART_TopmostButton);

    private Button _close;

    private Button _restore;

    private Button _maximize;

    private Button _minimize;

    private ToggleButton _topmost;

    static AdvancedWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedWindow), new FrameworkPropertyMetadata(typeof(AdvancedWindow)));
    }

    public AdvancedWindow()
    {
        CommandBindings.Add(new CommandBinding(CloseCommand, OnClose, OnCanClose));
        CommandBindings.Add(new CommandBinding(MaximizeCommand, OnMaximize, OnCanMaximize));
        CommandBindings.Add(new CommandBinding(RestoreCommand, OnRestore, OnCanRestore));
        CommandBindings.Add(new CommandBinding(MinimizeCommand, OnMinimize, OnCanMinimize));
        CommandBindings.Add(new CommandBinding(TopmostCommand, OnTopmost, OnCanTopmost));
    }

    #region Icon

    public new static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(AdvancedWindow), new PropertyMetadata(default(object)));

    public new object Icon
    {
        get => (object)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

    #region CaptionBackground

    public static readonly DependencyProperty CaptionBackgroundProperty =
        DependencyProperty.Register(nameof(CaptionBackground), typeof(Brush), typeof(AdvancedWindow), new PropertyMetadata(default(Brush)));

    public Brush CaptionBackground
    {
        get => (Brush)GetValue(CaptionBackgroundProperty);
        set => SetValue(CaptionBackgroundProperty, value);
    }

    #endregion CaptionBackground

    #region CaptionButtonType

    public static readonly DependencyProperty CaptionButtonTypeProperty =
        DependencyProperty.Register(nameof(CaptionButtonType), typeof(CaptionButtonType), typeof(AdvancedWindow), new PropertyMetadata(default(CaptionButtonType)));

    public CaptionButtonType CaptionButtonType
    {
        get => (CaptionButtonType)GetValue(CaptionButtonTypeProperty);
        set => SetValue(CaptionButtonTypeProperty, value);
    }

    #endregion CaptionButtonType

    #region TitleBrush

    public static readonly DependencyProperty TitleBrushProperty =
        DependencyProperty.Register(nameof(TitleBrush), typeof(Brush), typeof(AdvancedWindow), new PropertyMetadata(default(Brush)));

    public Brush TitleBrush
    {
        get => (Brush)GetValue(TitleBrushProperty);
        set => SetValue(TitleBrushProperty, value);
    }

    #endregion TitleBrush

    #region CaptionHeight

    public static readonly DependencyProperty CaptionHeightProperty =
        DependencyProperty.Register(nameof(CaptionHeight), typeof(double), typeof(AdvancedWindow), new PropertyMetadata(default(double)));

    public double CaptionHeight
    {
        get => (double)GetValue(CaptionHeightProperty);
        set => SetValue(CaptionHeightProperty, value);
    }

    #endregion CaptionHeight

    #region FunctionalZone

    public static readonly DependencyProperty FunctionalZoneProperty =
        DependencyProperty.Register(nameof(FunctionalZone), typeof(object), typeof(AdvancedWindow), new PropertyMetadata(default(object)));

    public object FunctionalZone
    {
        get => (object)GetValue(FunctionalZoneProperty);
        set => SetValue(FunctionalZoneProperty, value);
    }

    #endregion FunctionalZone

    #region Close

    public static RoutedCommand CloseCommand { get; private set; } = new RoutedCommand("Close", typeof(AdvancedWindow));

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

    #endregion Close

    #region Maximize

    public static RoutedCommand MaximizeCommand { get; private set; } = new RoutedCommand("Maximize", typeof(AdvancedWindow));

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

    #endregion Maximize

    #region Restore

    public static RoutedCommand RestoreCommand { get; private set; } = new RoutedCommand("Restore", typeof(AdvancedWindow));

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

    #endregion Restore

    #region Minimize

    public static RoutedCommand MinimizeCommand { get; private set; } = new RoutedCommand("Minimize", typeof(AdvancedWindow));

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

    #endregion Minimize

    #region Topmost

    public static RoutedCommand TopmostCommand { get; private set; } = new RoutedCommand("Topmost", typeof(AdvancedWindow));

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

    #endregion Topmost

    #region Override

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        //解决WindowChrome样式下，设置SizeToContent="WidthAndHeight"以后出现的黑色区域
        if (SizeToContent == SizeToContent.WidthAndHeight && WindowChrome.GetWindowChrome(this) != null)
        {
            InvalidateMeasure();
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    #endregion Override
}