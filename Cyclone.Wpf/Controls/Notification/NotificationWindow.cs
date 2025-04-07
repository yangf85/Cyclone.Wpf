using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
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

    public AnimationDirection AnimationDirection
    {
        get => (AnimationDirection)GetValue(AnimationDirectionProperty);
        set => SetValue(AnimationDirectionProperty, value);
    }

    public static readonly DependencyProperty AnimationDirectionProperty =
        DependencyProperty.Register(nameof(AnimationDirection), typeof(AnimationDirection), typeof(NotificationWindow),
            new PropertyMetadata(AnimationDirection.FromRight));

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

    public NotificationWindow()
    {
        // 注册命令绑定
        CommandBindings.Add(new CommandBinding(CloseWindowCommand, ExecuteCloseWindow, CanExecuteCloseWindow));
        var dict = new ResourceDictionary();
        dict.Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Styles/Notification.xaml", UriKind.Absolute);
        Resources.MergedDictionaries.Add(dict);

        SystemMenuManager.DisableCloseMenuItem(this);

        // 初始化自动关闭计时器
        _autoCloseTimer = new DispatcherTimer();
        _autoCloseTimer.Tick += AutoCloseTimer_Tick;

        Loaded += NotificationWindow_Loaded;
        Unloaded += NotificationWindow_Unloaded;
    }

    private void NotificationWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // 保存原始位置用于动画
        _originalLeft = this.Left;
        _originalTop = this.Top;

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
            case AnimationDirection.FromLeft:
                Left = _originalLeft - ActualWidth;
                break;

            case AnimationDirection.FromRight:
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
            case AnimationDirection.FromLeft:
                positionAnimation = new DoubleAnimation
                {
                    From = _originalLeft - ActualWidth,
                    To = _originalLeft,
                    Duration = TimeSpan.FromMilliseconds(200),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                animatedProperty = Window.LeftProperty;
                break;

            case AnimationDirection.FromRight:
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
            case AnimationDirection.FromLeft:
                positionAnimation = new DoubleAnimation
                {
                    From = Left,
                    To = _originalLeft - ActualWidth,
                    Duration = TimeSpan.FromMilliseconds(300),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
                };
                animatedProperty = Window.LeftProperty;
                break;

            case AnimationDirection.FromRight:
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

        // Check if _autoCloseTimer is not null before stopping it
        if (_autoCloseTimer != null)
        {
            StopAutoCloseTimer();
        }

        PlayCloseAnimation(() =>
        {
            base.Close();
        });
    }

    private void StartAutoCloseTimer()
    {
        // Add null check for _autoCloseTimer
        if (_autoCloseTimer == null)
        {
            _autoCloseTimer = new DispatcherTimer();
            _autoCloseTimer.Tick += AutoCloseTimer_Tick;
        }

        // If timer is already running or mouse is over window, don't start timer
        if (_autoCloseTimer.IsEnabled || this.IsMouseOver || DisplayDuration <= TimeSpan.Zero)
        {
            return;
        }

        // Set timer interval and start
        _autoCloseTimer.Interval = DisplayDuration;
        _autoCloseTimer.Start();

        // Debug output
        Console.WriteLine($"Auto close timer started: {DisplayDuration.TotalMilliseconds}ms");
    }

    private void StopAutoCloseTimer()
    {
        if (_autoCloseTimer != null && _autoCloseTimer.IsEnabled)
        {
            _autoCloseTimer.Stop();
            Console.WriteLine("Auto close timer stopped");
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