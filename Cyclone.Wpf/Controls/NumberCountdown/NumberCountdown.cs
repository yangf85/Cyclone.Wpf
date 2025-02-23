using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

public enum AnimationType
{
    None,

    Fade,

    Scale,

    Flip
}

public class NumberCountdown : Control
{
    static NumberCountdown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(NumberCountdown),
            new FrameworkPropertyMetadata(typeof(NumberCountdown)));
    }

    #region 依赖属性

    public static readonly DependencyProperty StartNumberProperty =
        DependencyProperty.Register("StartNumber", typeof(int), typeof(NumberCountdown),
            new PropertyMetadata(10, OnStartNumberChanged));

    public static readonly DependencyProperty CurrentNumberProperty =
            DependencyProperty.Register("CurrentNumber", typeof(int), typeof(NumberCountdown),
                new PropertyMetadata(0));

    public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register("AnimationDuration", typeof(double), typeof(NumberCountdown),
                new PropertyMetadata(0.5));

    public static readonly DependencyProperty IsAnimatingProperty =
            DependencyProperty.Register("IsAnimating", typeof(bool), typeof(NumberCountdown),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsAnimatingChanged));

    public static readonly DependencyProperty NumberColorProperty =
            DependencyProperty.Register("NumberColor", typeof(Brush), typeof(NumberCountdown),
                new PropertyMetadata(Brushes.Blue));

    public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.Register("AnimationType", typeof(AnimationType), typeof(NumberCountdown),
                new FrameworkPropertyMetadata(AnimationType.Fade,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnAnimationTypeChanged));

    public int StartNumber
    {
        get => (int)GetValue(StartNumberProperty);
        set => SetValue(StartNumberProperty, value);
    }

    public int CurrentNumber
    {
        get => (int)GetValue(CurrentNumberProperty);
        private set => SetValue(CurrentNumberProperty, value);
    }

    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public bool IsAnimating
    {
        get => (bool)GetValue(IsAnimatingProperty);
        set => SetValue(IsAnimatingProperty, value);
    }

    public Brush NumberColor
    {
        get => (Brush)GetValue(NumberColorProperty);
        set => SetValue(NumberColorProperty, value);
    }

    public AnimationType AnimationType
    {
        get => (AnimationType)GetValue(AnimationTypeProperty);
        set => SetValue(AnimationTypeProperty, value);
    }

    #endregion 依赖属性

    #region 私有字段

    private DispatcherTimer _timer;

    private Storyboard _animation;

    #endregion 私有字段

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        InitializeTimer();
        SetupAnimation();
    }

    #region 初始化方法

    private void InitializeTimer()
    {
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => UpdateNumber();
    }

    private void SetupAnimation()
    {
        _animation = new Storyboard();
        ConfigureAnimation();
        _animation.Completed += (s, e) => IsAnimating = false;
    }

    private void ConfigureAnimation()
    {
        _animation.Children.Clear();

        switch (AnimationType)
        {
            case AnimationType.Fade:
                CreateFadeAnimation();
                break;

            case AnimationType.Scale:
                CreateScaleAnimation();
                break;

            case AnimationType.Flip:
                CreateFlipAnimation();
                break;
        }
    }

    #endregion 初始化方法

    #region 动画配置

    private void CreateFadeAnimation()
    {
        var da = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            From = 0,
            To = 1,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(da, new PropertyPath(OpacityProperty));
        _animation.Children.Add(da);
    }

    private void CreateScaleAnimation()
    {
        var scaleX = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            From = 1.5,
            To = 1,
            EasingFunction = new ElasticEase { Oscillations = 1 }
        };
        Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.ScaleX"));

        var scaleY = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            From = 1.5,
            To = 1,
            EasingFunction = new ElasticEase { Oscillations = 1 }
        };
        Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.ScaleY"));

        _animation.Children.Add(scaleX);
        _animation.Children.Add(scaleY);
    }

    private void CreateFlipAnimation()
    {
        var rotate = new DoubleAnimation
        {
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            From = 0,
            To = 360,
            EasingFunction = new BounceEase { Bounces = 2 }
        };
        Storyboard.SetTargetProperty(rotate, new PropertyPath("RenderTransform.Angle"));

        _animation.Children.Add(rotate);
    }

    #endregion 动画配置

    #region 事件处理

    private static void OnStartNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumberCountdown)d;
        control.Reset();
    }

    private static void OnIsAnimatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumberCountdown)d;
        if ((bool)e.NewValue)
        {
            control.BeginAnimation();
        }
        else
        {
            control.StopAnimation();
        }
    }

    private static void OnAnimationTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumberCountdown control && control.IsInitialized)
        {
            control.SetupAnimation();
        }
    }

    #endregion 事件处理

    #region 公共方法

    public void Start()
    {
        CurrentNumber = StartNumber;
        _timer?.Start();
    }

    public void Reset()
    {
        _timer?.Stop();
        CurrentNumber = StartNumber;
        IsAnimating = false;
    }

    #endregion 公共方法

    #region 私有方法

    private void UpdateNumber()
    {
        if (CurrentNumber > 0)
        {
            IsAnimating = true;
            CurrentNumber--;
        }
        else
        {
            _timer.Stop();
            RaiseCompletedEvent();
            IsAnimating = false;
        }
    }

    private void BeginAnimation()
    {
        var target = GetTemplateChild("PART_NumberDisplay") as FrameworkElement;
        if (target == null || _animation == null) return;

        PrepareTargetTransform(target);
        ClearPreviousAnimations(target);

        Storyboard.SetTarget(_animation, target);
        _animation.Begin(target, true);
    }

    private void PrepareTargetTransform(FrameworkElement target)
    {
        switch (AnimationType)
        {
            case AnimationType.Scale:
                target.RenderTransform = new ScaleTransform();
                break;

            case AnimationType.Flip:
                target.RenderTransform = new RotateTransform();
                break;

            default:
                target.RenderTransform = null;
                break;
        }
    }

    private void ClearPreviousAnimations(FrameworkElement target)
    {
        target.BeginAnimation(OpacityProperty, null);
        if (target.RenderTransform != null)
        {
            target.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            target.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            target.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, null);
        }
    }

    private void StopAnimation()
    {
        var target = GetTemplateChild("PART_NumberDisplay") as FrameworkElement;
        _animation?.Stop(target);
    }

    #endregion 私有方法

    #region 完成事件

    public static readonly RoutedEvent CompletedEvent =
        EventManager.RegisterRoutedEvent("Completed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(NumberCountdown));

    private void RaiseCompletedEvent()
    {
        RaiseEvent(new RoutedEventArgs(CompletedEvent));
    }

    public event RoutedEventHandler Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    #endregion 完成事件
}