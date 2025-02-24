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

    #region Dependency Properties

    public static readonly DependencyProperty StartNumberProperty =
        DependencyProperty.Register(
            "StartNumber", typeof(int), typeof(NumberCountdown),
            new PropertyMetadata(10, OnStartNumberChanged)
        );

    public static readonly DependencyProperty CurrentNumberProperty =
        DependencyProperty.Register(
            "CurrentNumber", typeof(int), typeof(NumberCountdown),
            new FrameworkPropertyMetadata(
                0,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnCurrentNumberChanged
            )
        );

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(
            "AnimationDuration", typeof(double), typeof(NumberCountdown),
            new PropertyMetadata(0.5)
        );

    public static readonly DependencyProperty IsAnimatingProperty =
        DependencyProperty.Register(
            "IsAnimating", typeof(bool), typeof(NumberCountdown),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsAnimatingChanged
            )
        );

    public static readonly DependencyProperty NumberColorProperty =
        DependencyProperty.Register(
            "NumberColor", typeof(Brush), typeof(NumberCountdown),
            new PropertyMetadata(Brushes.Blue)
        );

    public static readonly DependencyProperty AnimationTypeProperty =
        DependencyProperty.Register(
            "AnimationType", typeof(AnimationType), typeof(NumberCountdown),
            new FrameworkPropertyMetadata(
                AnimationType.Fade,
                FrameworkPropertyMetadataOptions.AffectsRender,
                OnAnimationTypeChanged
            )
        );

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

    #endregion Dependency Properties

    #region Private Fields

    private Storyboard _animation;

    private FrameworkElement _numberDisplay;

    #endregion Private Fields

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _numberDisplay = GetTemplateChild("PART_NumberDisplay") as FrameworkElement;
        SetupAnimation();
    }

    #endregion Overrides

    #region Animation Logic

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

    private void CreateFadeAnimation()
    {
        var fadeAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(OpacityProperty));
        _animation.Children.Add(fadeAnimation);
    }

    private void CreateScaleAnimation()
    {
        var scaleX = new DoubleAnimation
        {
            From = 1.5,
            To = 1,
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            EasingFunction = new ElasticEase { Oscillations = 1 }
        };
        Storyboard.SetTargetProperty(scaleX, new PropertyPath("RenderTransform.Children[0].ScaleX"));

        var scaleY = new DoubleAnimation
        {
            From = 1.5,
            To = 1,
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            EasingFunction = new ElasticEase { Oscillations = 1 }
        };
        Storyboard.SetTargetProperty(scaleY, new PropertyPath("RenderTransform.Children[0].ScaleY"));

        _animation.Children.Add(scaleX);
        _animation.Children.Add(scaleY);
    }

    private void CreateFlipAnimation()
    {
        var flipAnimation = new DoubleAnimation
        {
            From = 0,
            To = 360,
            Duration = TimeSpan.FromSeconds(AnimationDuration),
            EasingFunction = new BounceEase { Bounces = 2 }
        };
        Storyboard.SetTargetProperty(flipAnimation, new PropertyPath("RenderTransform.Children[1].Angle"));
        _animation.Children.Add(flipAnimation);
    }

    private void BeginAnimation()
    {
        if (_numberDisplay == null || _animation == null) return;

        PrepareTransform();
        ClearPreviousAnimations();

        Storyboard.SetTarget(_animation, _numberDisplay);
        _animation.Begin(_numberDisplay, true);
    }

    private void PrepareTransform()
    {
        // 强制创建新的 TransformGroup 避免属性被锁定
        var group = new TransformGroup();
        group.Children.Add(new ScaleTransform());
        group.Children.Add(new RotateTransform());
        _numberDisplay.RenderTransform = group;
        _numberDisplay.RenderTransformOrigin = new Point(0.5, 0.5);
    }

    private void ClearPreviousAnimations()
    {
        _numberDisplay.BeginAnimation(OpacityProperty, null);

        if (_numberDisplay.RenderTransform is TransformGroup group)
        {
            foreach (var transform in group.Children)
            {
                if (transform is ScaleTransform scale)
                {
                    scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
                    scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);
                }
                else if (transform is RotateTransform rotate)
                {
                    rotate.BeginAnimation(RotateTransform.AngleProperty, null);
                }
            }
        }
    }

    private void StopAnimation()
    {
        if (_numberDisplay == null)
        {
            return;
        }
        _animation?.Stop(_numberDisplay);
    }

    #endregion Animation Logic

    #region Event Handlers

    private static void OnStartNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumberCountdown)d;
        control.Reset();
    }

    private static void OnCurrentNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumberCountdown)d;
        control.IsAnimating = true; // 数字变化时触发动画
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
        var control = (NumberCountdown)d;
        control.SetupAnimation();
    }

    #endregion Event Handlers

    #region Public Methods

    public void Start()
    {
        CurrentNumber = StartNumber;
    }

    public void Reset()
    {
        CurrentNumber = StartNumber;
        IsAnimating = false;
    }

    #endregion Public Methods

    #region Completed Event

    public static readonly RoutedEvent CompletedEvent =
        EventManager.RegisterRoutedEvent(
            "Completed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(NumberCountdown)
        );

    private void RaiseCompletedEvent()
    {
        RaiseEvent(new RoutedEventArgs(CompletedEvent));
    }

    public event RoutedEventHandler Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    #endregion Completed Event
}