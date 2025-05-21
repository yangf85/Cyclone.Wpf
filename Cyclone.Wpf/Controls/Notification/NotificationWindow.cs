using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Shell;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

internal class NotificationWindow : Window
{
    #region DisplayDuration

    public TimeSpan DisplayDuration
    {
        get => (TimeSpan)GetValue(DisplayDurationProperty);
        set => SetValue(DisplayDurationProperty, value);
    }

    public static readonly DependencyProperty DisplayDurationProperty =
        DependencyProperty.Register(nameof(DisplayDuration), typeof(TimeSpan), typeof(NotificationWindow),
            new PropertyMetadata(TimeSpan.FromMilliseconds(2400), OnDisplayDurationChanged));

    private static void OnDisplayDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NotificationWindow window)
        {
            var newDelay = (TimeSpan)e.NewValue;

            if (newDelay <= TimeSpan.Zero)
            {
                window.StopAutoCloseTimer();
            }
            else
            {
                window.ResetAutoCloseTimer();
            }
        }
    }

    #endregion DisplayDuration

    #region IsShowCloseButton

    public bool IsShowCloseButton
    {
        get => (bool)GetValue(IsShowCloseButtonProperty);
        set => SetValue(IsShowCloseButtonProperty, value);
    }

    public static readonly DependencyProperty IsShowCloseButtonProperty =
        DependencyProperty.Register(nameof(IsShowCloseButton), typeof(bool), typeof(NotificationWindow), new PropertyMetadata(true));

    #endregion IsShowCloseButton

    #region AnimationDirection

    public NotificationAnimationDirection AnimationDirection
    {
        get => (NotificationAnimationDirection)GetValue(AnimationDirectionProperty);
        set => SetValue(AnimationDirectionProperty, value);
    }

    public static readonly DependencyProperty AnimationDirectionProperty =
        DependencyProperty.Register(nameof(AnimationDirection), typeof(NotificationAnimationDirection), typeof(NotificationWindow),
            new PropertyMetadata(NotificationAnimationDirection.FromRight));

    #endregion AnimationDirection

    #region CloseCommand

    public static RoutedCommand CloseWindowCommand { get; private set; } =
        new RoutedCommand("CloseWindow", typeof(NotificationWindow));

    private void ExecuteCloseWindow(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is NotificationWindow window)
        {
            window.CloseWithAnimation();
        }
    }

    private void CanExecuteCloseWindow(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    #endregion CloseCommand

    private DispatcherTimer _autoCloseTimer;
    private bool _isClosing = false;
    private double _originalLeft;
    private double _originalTop;

    // 用于获取安全的Dispatcher
    private Dispatcher GetSafeDispatcher()
    {
        return Dispatcher ?? (Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher);
    }

    public NotificationWindow()
    {
        // 注册命令绑定
        CommandBindings.Add(new CommandBinding(CloseWindowCommand, ExecuteCloseWindow, CanExecuteCloseWindow));

        try
        {
            var dict = new ResourceDictionary();
            dict.Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Notification.xaml", UriKind.Absolute);
            Resources.MergedDictionaries.Add(dict);
        }
        catch (Exception ex)
        {
            // 在资源加载失败的情况下提供异常处理
            Console.WriteLine($"Error loading notification resources: {ex.Message}");
            // 可以在这里添加默认资源或降级处理
        }

        // 初始化自动关闭计时器（在Loaded事件中）
        Loaded += NotificationWindow_Loaded;
        Unloaded += NotificationWindow_Unloaded;
    }

    private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 初始化计时器
        if (_autoCloseTimer == null)
        {
            _autoCloseTimer = new DispatcherTimer(DispatcherPriority.Normal, GetSafeDispatcher());
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;
        }

        // 保存原始位置用于动画
        _originalLeft = Left;
        _originalTop = Top;

        // 根据动画方向设置初始位置
        SetInitialPositionForAnimation();

        // 播放显示动画
        PlayOpenAnimation();

        // 检查是否应该启动自动关闭计时器
        if (DisplayDuration > TimeSpan.Zero)
        {
            StartAutoCloseTimer();
        }
    }

    private void SetInitialPositionForAnimation()
    {
        switch (AnimationDirection)
        {
            case NotificationAnimationDirection.FromLeft:
                Left = _originalLeft - ActualWidth;
                break;

            case NotificationAnimationDirection.FromRight:
                Left = _originalLeft + ActualWidth;
                break;
        }
    }

    private void NotificationWindow_Unloaded(object sender, RoutedEventArgs e)
    {
        CleanupResources();
    }

    private void AutoCloseTimer_Tick(object sender, EventArgs e)
    {
        // 停止计时器，防止多次触发
        StopAutoCloseTimer();

        // 调用关闭方法
        CloseWithAnimation();
    }

    private void PlayOpenAnimation()
    {
        DoubleAnimation positionAnimation = null;
        DependencyProperty animatedProperty = null;

        // 根据方向创建适当的动画
        switch (AnimationDirection)
        {
            case NotificationAnimationDirection.FromLeft:
                positionAnimation = new DoubleAnimation
                {
                    From = _originalLeft - ActualWidth,
                    To = _originalLeft,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                animatedProperty = Window.LeftProperty;
                break;

            case NotificationAnimationDirection.FromRight:
                positionAnimation = new DoubleAnimation
                {
                    From = _originalLeft + ActualWidth,
                    To = _originalLeft,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                animatedProperty = Window.LeftProperty;
                break;
        }

        // 创建透明度动画
        DoubleAnimation opacityAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        // 开始动画
        if (positionAnimation != null && animatedProperty != null)
        {
            BeginAnimation(animatedProperty, positionAnimation);
        }
        BeginAnimation(Window.OpacityProperty, opacityAnimation);
    }

    private void PlayCloseAnimation(Action completedCallback)
    {
        if (_isClosing) return;
        _isClosing = true;

        DoubleAnimation positionAnimation = null;
        DependencyProperty animatedProperty = null;

        // 根据方向创建适当的动画
        switch (AnimationDirection)
        {
            case NotificationAnimationDirection.FromLeft:
                positionAnimation = new DoubleAnimation
                {
                    From = Left,
                    To = _originalLeft - ActualWidth,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                animatedProperty = Window.LeftProperty;
                break;

            case NotificationAnimationDirection.FromRight:
                positionAnimation = new DoubleAnimation
                {
                    From = Left,
                    To = _originalLeft + ActualWidth,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                animatedProperty = Window.LeftProperty;
                break;
        }

        // 创建透明度动画
        DoubleAnimation opacityAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };

        // 设置透明度动画的完成回调，确保窗口关闭在透明度动画完成后
        opacityAnimation.Completed += (s, e) => completedCallback?.Invoke();

        // 开始动画
        if (positionAnimation != null && animatedProperty != null)
        {
            BeginAnimation(animatedProperty, positionAnimation);
        }
        BeginAnimation(Window.OpacityProperty, opacityAnimation);
    }

    public void CloseWithAnimation()
    {
        if (_isClosing) return;

        // 停止计时器
        StopAutoCloseTimer();

        PlayCloseAnimation(() =>
        {
            base.Close();
        });
    }

    private void StartAutoCloseTimer()
    {
        // 如果计时器为空，创建一个新的
        if (_autoCloseTimer == null)
        {
            _autoCloseTimer = new DispatcherTimer(DispatcherPriority.Normal, GetSafeDispatcher());
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;
        }

        // 如果计时器已在运行或鼠标在窗口上，不启动计时器
        if (_autoCloseTimer.IsEnabled || this.IsMouseOver || DisplayDuration <= TimeSpan.Zero)
        {
            return;
        }

        // 设置计时器间隔并启动
        _autoCloseTimer.Interval = DisplayDuration;
        _autoCloseTimer.Start();
    }

    private void StopAutoCloseTimer()
    {
        if (_autoCloseTimer != null && _autoCloseTimer.IsEnabled)
        {
            _autoCloseTimer.Stop();
        }
    }

    private void ResetAutoCloseTimer()
    {
        StopAutoCloseTimer();
        if (DisplayDuration > TimeSpan.Zero)
        {
            StartAutoCloseTimer();
        }
    }

    private void CleanupResources()
    {
        // 停止计时器
        StopAutoCloseTimer();

        // 解除事件订阅
        if (_autoCloseTimer != null)
        {
            _autoCloseTimer.Tick -= AutoCloseTimer_Tick;
            _autoCloseTimer = null;
        }

        Loaded -= NotificationWindow_Loaded;
        Unloaded -= NotificationWindow_Unloaded;
    }

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

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        CleanupResources();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        StopAutoCloseTimer();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (DisplayDuration > TimeSpan.Zero)
        {
            StartAutoCloseTimer();
        }
    }

    public new void Close()
    {
        CloseWithAnimation();
    }

    #endregion Override
}