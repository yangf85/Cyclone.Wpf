using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 动画方向枚举
/// </summary>
public enum AnimationDirection
{
    FromLeft,
    FromRight,
}

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

        // 开始动画
        if (positionAnimation != null && animatedProperty != null)
        {
            BeginAnimation(animatedProperty, positionAnimation);
        }
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

        // 设置完成回调
        if (positionAnimation != null)
        {
            positionAnimation.Completed += (s, e) => completedCallback?.Invoke();
        }
        else
        {
            // 如果没有位置动画，直接调用回调
            completedCallback?.Invoke();
        }

        // 开始动画
        if (positionAnimation != null && animatedProperty != null)
        {
            BeginAnimation(animatedProperty, positionAnimation);
        }
    }

    public void CloseWithAnimation()
    {
        if (_isClosing) return;

        StopAutoCloseTimer();
        PlayCloseAnimation(() =>
        {
            base.Close();
        });
    }

    private void StartAutoCloseTimer()
    {
        // 如果计时器已经在运行或者鼠标在窗口上，则不启动计时器
        if (_autoCloseTimer.IsEnabled || this.IsMouseOver || DisplayDuration <= TimeSpan.Zero)
        {
            return;
        }

        // 设置计时器间隔并启动
        _autoCloseTimer.Interval = DisplayDuration;
        _autoCloseTimer.Start();

        // 调试提示
        Console.WriteLine($"Auto close timer started: {DisplayDuration.TotalMilliseconds}ms");
    }

    private void StopAutoCloseTimer()
    {
        if (_autoCloseTimer.IsEnabled)
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

    // 重写Close方法，使用动画关闭
    public new void Close()
    {
        CloseWithAnimation();
    }

    #endregion Override

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
}

internal static class SystemMenuManager
{
    [DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
    private static extern IntPtr GetSystemMenu(IntPtr hwnd, int bRevert);

    [DllImport("user32.dll", EntryPoint = "RemoveMenu")]
    private static extern int RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

    [DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
    private static extern IntPtr DrawMenuBar(IntPtr hwnd);

    [DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
    private static extern int GetMenuItemCount(IntPtr hMenu);

    private const uint MF_BYPOSITION = 0x0400;

    public static void DisableCloseMenuItem(Window window)
    {
        if (window == null) return;

        window.SourceInitialized += (s, e) =>
        {
            var helper = new WindowInteropHelper(window);
            IntPtr hwnd = helper.Handle;
            IntPtr hMenu = GetSystemMenu(hwnd, 0);

            // 移除所有系统菜单项
            int itemCount = GetMenuItemCount(hMenu);
            for (int i = itemCount - 1; i >= 0; i--)
            {
                RemoveMenu(hMenu, (uint)i, MF_BYPOSITION);
            }

            DrawMenuBar(hwnd);
        };
    }
}