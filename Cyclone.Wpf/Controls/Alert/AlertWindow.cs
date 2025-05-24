using System;
using System.Diagnostics.SymbolStore;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

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

    #region AlertButtonGroupHorizontalAlignment

    public HorizontalAlignment AlertButtonGroupHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(AlertButtonGroupHorizontalAlignmentProperty);
        set => SetValue(AlertButtonGroupHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty AlertButtonGroupHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(AlertButtonGroupHorizontalAlignment), typeof(HorizontalAlignment), typeof(AlertWindow), new PropertyMetadata(HorizontalAlignment.Center));

    #endregion AlertButtonGroupHorizontalAlignment

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

    #region AlertIconForeground

    public Brush AlertIconForeground
    {
        get => (Brush)GetValue(AlertIconForegroundProperty);
        set => SetValue(AlertIconForegroundProperty, value);
    }

    public static readonly DependencyProperty AlertIconForegroundProperty =
        DependencyProperty.Register(nameof(AlertIconForeground), typeof(Brush), typeof(AlertWindow), new PropertyMetadata(default(Brush)));

    #endregion AlertIconForeground

    #region AlertButtonGroupHeight

    public double AlertButtonGroupHeight
    {
        get => (double)GetValue(AlertButtonGroupHeightProperty);
        set => SetValue(AlertButtonGroupHeightProperty, value);
    }

    public static readonly DependencyProperty AlertButtonGroupHeightProperty =
        DependencyProperty.Register(nameof(AlertButtonGroupHeight), typeof(double), typeof(AlertWindow), new PropertyMetadata(0d));

    #endregion AlertButtonGroupHeight

    #region ContentForeground

    public Brush ContentForeground
    {
        get => (Brush)GetValue(ContentForegroundProperty);
        set => SetValue(ContentForegroundProperty, value);
    }

    public static readonly DependencyProperty ContentForegroundProperty =
        DependencyProperty.Register(nameof(ContentForeground), typeof(Brush), typeof(AlertWindow), new PropertyMetadata(default(Brush)));

    #endregion ContentForeground

    #region ValidationCallback

    /// <summary>
    /// 验证回调函数，用于在确定按钮点击时进行验证
    /// </summary>
    public Func<bool> ValidationCallback
    {
        get => (Func<bool>)GetValue(ValidationCallbackProperty);
        set => SetValue(ValidationCallbackProperty, value);
    }

    public static readonly DependencyProperty ValidationCallbackProperty =
        DependencyProperty.Register(nameof(ValidationCallback), typeof(Func<bool>), typeof(AlertWindow), new PropertyMetadata(null));

    #endregion ValidationCallback

    #region AsyncValidationCallback

    /// <summary>
    /// 异步验证回调函数，用于在确定按钮点击时进行异步验证
    /// </summary>
    public Func<Task<bool>> AsyncValidationCallback
    {
        get => (Func<Task<bool>>)GetValue(AsyncValidationCallbackProperty);
        set => SetValue(AsyncValidationCallbackProperty, value);
    }

    public static readonly DependencyProperty AsyncValidationCallbackProperty =
        DependencyProperty.Register(nameof(AsyncValidationCallback), typeof(Func<Task<bool>>), typeof(AlertWindow), new PropertyMetadata(null));

    #endregion AsyncValidationCallback

    #region Command

    private void InitializeCommand()
    {
        // OK 命令处理 - 修改为异步处理
        CommandBindings.Add(new CommandBinding(OkCommand, async (sender, e) =>
        {
            await HandleOkCommandAsync();
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

    /// <summary>
    /// 处理OK命令的异步方法
    /// </summary>
    private async Task HandleOkCommandAsync()
    {
        // 如果有异步验证回调，优先使用
        if (AsyncValidationCallback != null)
        {
            try
            {
                // 禁用窗口防止重复点击
                IsEnabled = false;

                // 执行异步验证
                bool validationResult = await AsyncValidationCallback();

                if (validationResult)
                {
                    // 验证通过，正常关闭
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // 验证失败，恢复窗口状态，保持打开
                    IsEnabled = true;
                    System.Diagnostics.Debug.WriteLine("异步验证失败，对话框保持打开");
                }
            }
            catch (Exception ex)
            {
                // 验证过程中出现异常
                System.Diagnostics.Debug.WriteLine($"异步验证过程中出现异常: {ex.Message}");

                // 恢复窗口状态
                IsEnabled = true;

                // 可以选择是否关闭窗口，这里选择保持打开
                // DialogResult = false;
                // Close();
            }
        }
        // 如果有同步验证回调
        else if (ValidationCallback != null)
        {
            try
            {
                bool validationResult = ValidationCallback();

                if (validationResult)
                {
                    // 验证通过，正常关闭
                    DialogResult = true;
                    Close();
                }
                else
                {
                    // 验证失败，窗口保持打开
                    System.Diagnostics.Debug.WriteLine("验证失败，对话框保持打开");
                }
            }
            catch (Exception ex)
            {
                // 验证过程中出现异常，记录并关闭
                System.Diagnostics.Debug.WriteLine($"验证过程中出现异常: {ex.Message}");
                DialogResult = false;
                Close();
            }
        }
        else
        {
            // 没有验证回调，直接关闭
            DialogResult = true;
            Close();
        }
    }

    public static RoutedCommand OkCommand { get; private set; } = new RoutedCommand("Ok", typeof(AlertWindow));
    public static RoutedCommand CancelCommand { get; private set; } = new RoutedCommand("Cancel", typeof(AlertWindow));
    public static RoutedCommand CloseCommand { get; private set; } = new RoutedCommand("Close", typeof(AlertWindow));

    #endregion Command

    #region Override

    /// <summary>
    /// 窗口初始化时，如果设置了SizeToContent.WidthAndHeight，则重新测量窗口大小以适应内容
    /// 解决了窗口在Chrome模式下无法自动适应内容的问题
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        if (SizeToContent == SizeToContent.WidthAndHeight && WindowChrome.GetWindowChrome(this) != null)
        {
            InvalidateMeasure();
        }
    }

    #endregion Override

    public AlertWindow()
    {
        InitializeCommand();

        try
        {
            // 尝试加载资源字典
            var dict = new ResourceDictionary();
            dict.Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Alert.xaml", UriKind.Absolute);
            Resources.MergedDictionaries.Add(dict);
        }
        catch (Exception ex)
        {
            // 记录异常但继续初始化窗口
            System.Diagnostics.Debug.WriteLine($"加载Alert样式资源时出错: {ex.Message}");

            // 可以在这里添加基本样式作为备用
            ApplyFallbackStyles();
        }

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    /// <summary>
    /// 在无法加载资源字典时应用基本样式
    /// </summary>
    private void ApplyFallbackStyles()
    {
        try
        {
            // 创建基本样式作为备用
            // 这些是基本设置，确保窗口在没有样式文件时仍然可见
            Background = Brushes.White;
            BorderBrush = Brushes.DarkGray;
            BorderThickness = new Thickness(1);

            // 其他基本样式设置...
        }
        catch (Exception)
        {
            // 忽略应用备用样式时的异常
        }
    }
}