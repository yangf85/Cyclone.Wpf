using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 脉冲点加载动画控件 - 显示3个依次脉冲的圆点
/// </summary>
public class LoadingPulse : ContentControl
{
    #region 依赖属性

    public static readonly DependencyProperty ParticleColorProperty =
        DependencyProperty.Register(nameof(ParticleColor), typeof(Brush), typeof(LoadingPulse),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3)), OnVisualPropertyChanged));

    public static readonly DependencyProperty ParticleRadiusProperty =
        DependencyProperty.Register(nameof(ParticleRadius), typeof(double), typeof(LoadingPulse),
            new PropertyMetadata(8.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty ParticleSpacingProperty =
        DependencyProperty.Register(nameof(ParticleSpacing), typeof(double), typeof(LoadingPulse),
            new PropertyMetadata(10.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LoadingPulse),
            new PropertyMetadata(true, OnIsActiveChanged));

    public static readonly DependencyProperty SpinnerSizeProperty =
        DependencyProperty.Register(nameof(SpinnerSize), typeof(double), typeof(LoadingPulse),
            new PropertyMetadata(80.0, OnVisualPropertyChanged));

    /// <summary>
    /// 粒子颜色
    /// </summary>
    public Brush ParticleColor
    {
        get { return (Brush)GetValue(ParticleColorProperty); }
        set { SetValue(ParticleColorProperty, value); }
    }

    /// <summary>
    /// 粒子半径
    /// </summary>
    public double ParticleRadius
    {
        get { return (double)GetValue(ParticleRadiusProperty); }
        set { SetValue(ParticleRadiusProperty, value); }
    }

    /// <summary>
    /// 粒子间距
    /// </summary>
    public double ParticleSpacing
    {
        get { return (double)GetValue(ParticleSpacingProperty); }
        set { SetValue(ParticleSpacingProperty, value); }
    }

    /// <summary>
    /// 是否激活动画
    /// </summary>
    public bool IsActive
    {
        get { return (bool)GetValue(IsActiveProperty); }
        set { SetValue(IsActiveProperty, value); }
    }

    /// <summary>
    /// 动画控件大小
    /// </summary>
    public double SpinnerSize
    {
        get { return (double)GetValue(SpinnerSizeProperty); }
        set { SetValue(SpinnerSizeProperty, value); }
    }

    #endregion 依赖属性

    private Viewbox _viewbox;
    private Canvas _canvas;
    private Ellipse[] _particles;
    private Storyboard _storyboard;

    static LoadingPulse()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingPulse),
            new FrameworkPropertyMetadata(typeof(LoadingPulse)));
    }

    public LoadingPulse()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        CreateVisualTree();
        if (IsActive)
        {
            StartAnimation();
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopAnimation();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    private void CreateVisualTree()
    {
        // 清除现有内容
        this.Content = null;

        // 创建Viewbox
        _viewbox = new Viewbox
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Stretch = Stretch.Uniform
        };

        // 创建Grid容器
        var grid = new Grid
        {
            Width = SpinnerSize,
            Height = SpinnerSize
        };

        // 创建Canvas
        _canvas = new Canvas
        {
            Width = SpinnerSize,
            Height = SpinnerSize,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        // 创建3个粒子
        _particles = new Ellipse[3];
        double totalWidth = (ParticleRadius * 2 * 3) + (ParticleSpacing * 2);
        double startX = (SpinnerSize - totalWidth) / 2;
        double centerY = SpinnerSize / 2;

        for (int i = 0; i < 3; i++)
        {
            var particle = new Ellipse
            {
                Width = ParticleRadius * 2,
                Height = ParticleRadius * 2,
                Fill = ParticleColor,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Opacity = 0.3
            };

            // 设置粒子位置
            double x = startX + i * (ParticleRadius * 2 + ParticleSpacing);
            Canvas.SetLeft(particle, x);
            Canvas.SetTop(particle, centerY - ParticleRadius);

            // 添加缩放变换
            particle.RenderTransform = new ScaleTransform(1, 1);

            _canvas.Children.Add(particle);
            _particles[i] = particle;
        }

        grid.Children.Add(_canvas);
        _viewbox.Child = grid;
        this.Content = _viewbox;

        // 绑定宽高
        _viewbox.SetBinding(Viewbox.WidthProperty, new System.Windows.Data.Binding("Width") { Source = this });
        _viewbox.SetBinding(Viewbox.HeightProperty, new System.Windows.Data.Binding("Height") { Source = this });
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var spinner = (LoadingPulse)d;
        if (spinner.IsLoaded)
        {
            spinner.CreateVisualTree();
            if (spinner.IsActive)
            {
                spinner.StopAnimation();
                spinner.StartAnimation();
            }
        }
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var spinner = (LoadingPulse)d;
        if (spinner.IsLoaded)
        {
            spinner.UpdateAnimationState();
        }
    }

    private void UpdateAnimationState()
    {
        if (IsActive)
        {
            StartAnimation();
        }
        else
        {
            StopAnimation();
        }
    }

    private void StartAnimation()
    {
        if (_particles == null) return;

        if (_storyboard == null)
        {
            CreateAnimation();
        }
        _storyboard?.Begin();
    }

    private void StopAnimation()
    {
        _storyboard?.Stop();
    }

    private void CreateAnimation()
    {
        _storyboard = new Storyboard { RepeatBehavior = RepeatBehavior.Forever };

        for (int i = 0; i < 3; i++)
        {
            var particle = _particles[i];
            var delay = TimeSpan.FromSeconds(i * 0.15);

            // 创建缩放动画
            var scaleAnimation = new DoubleAnimationUsingKeyFrames();
            scaleAnimation.BeginTime = delay;

            // 关键帧
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.8, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3)))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
            });
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6)))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
            });
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.35))));

            // 为X和Y轴应用相同的动画
            var scaleXAnimation = scaleAnimation.Clone();
            var scaleYAnimation = scaleAnimation.Clone();

            Storyboard.SetTarget(scaleXAnimation, particle);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleX)"));

            Storyboard.SetTarget(scaleYAnimation, particle);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(ScaleTransform.ScaleY)"));

            // 创建透明度动画
            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.BeginTime = delay;

            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3)))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseOut }
            });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6)))
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn }
            });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.35))));

            Storyboard.SetTarget(opacityAnimation, particle);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            _storyboard.Children.Add(scaleXAnimation);
            _storyboard.Children.Add(scaleYAnimation);
            _storyboard.Children.Add(opacityAnimation);
        }
    }
}