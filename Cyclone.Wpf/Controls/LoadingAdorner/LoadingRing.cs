using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 环形加载动画控件 - 显示多个圆环的收缩扩张动画
/// </summary>
public class LoadingRing : ContentControl
{
    #region 依赖属性

    public static readonly DependencyProperty ParticleColorProperty =
        DependencyProperty.Register(nameof(ParticleColor), typeof(Brush), typeof(LoadingRing),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xFF, 0x52, 0x52)), OnVisualPropertyChanged));

    public static readonly DependencyProperty ParticleRadiusProperty =
        DependencyProperty.Register(nameof(ParticleRadius), typeof(double), typeof(LoadingRing),
            new PropertyMetadata(3.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty RingCountProperty =
        DependencyProperty.Register(nameof(RingCount), typeof(int), typeof(LoadingRing),
            new PropertyMetadata(3, OnVisualPropertyChanged), ValidateRingCount);

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LoadingRing),
            new PropertyMetadata(true, OnIsActiveChanged));

    public static readonly DependencyProperty SpinnerSizeProperty =
        DependencyProperty.Register(nameof(SpinnerSize), typeof(double), typeof(LoadingRing),
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
    /// 粒子（线条）粗细
    /// </summary>
    public double ParticleRadius
    {
        get { return (double)GetValue(ParticleRadiusProperty); }
        set { SetValue(ParticleRadiusProperty, value); }
    }

    /// <summary>
    /// 环形数量
    /// </summary>
    public int RingCount
    {
        get { return (int)GetValue(RingCountProperty); }
        set { SetValue(RingCountProperty, value); }
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
    private Ellipse[] _rings;
    private Storyboard _storyboard;

    static LoadingRing()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingRing),
            new FrameworkPropertyMetadata(typeof(LoadingRing)));
    }

    public LoadingRing()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private static bool ValidateRingCount(object value)
    {
        int count = (int)value;
        return count >= 1 && count <= 5;
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

        // 创建环形
        _rings = new Ellipse[RingCount];
        double centerX = SpinnerSize / 2;
        double centerY = SpinnerSize / 2;
        double maxRadius = SpinnerSize / 2 - ParticleRadius;

        for (int i = 0; i < RingCount; i++)
        {
            var ring = new Ellipse
            {
                Stroke = ParticleColor,
                StrokeThickness = ParticleRadius,
                Fill = Brushes.Transparent,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Opacity = 1.0 - (i * 0.2)
            };

            // 初始化大小和位置
            double initialRadius = maxRadius * (1.0 - i * 0.3);
            ring.Width = initialRadius * 2;
            ring.Height = initialRadius * 2;

            Canvas.SetLeft(ring, centerX - initialRadius);
            Canvas.SetTop(ring, centerY - initialRadius);

            // 添加缩放变换
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new ScaleTransform(1, 1, centerX, centerY));
            ring.RenderTransform = transformGroup;

            _canvas.Children.Add(ring);
            _rings[i] = ring;
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
        var spinner = (LoadingRing)d;
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
        var spinner = (LoadingRing)d;
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
        if (_rings == null) return;

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

        double centerX = SpinnerSize / 2;
        double centerY = SpinnerSize / 2;

        for (int i = 0; i < RingCount; i++)
        {
            var ring = _rings[i];
            var delay = TimeSpan.FromSeconds(i * 0.15);

            // 创建缩放动画
            var scaleAnimation = new DoubleAnimationUsingKeyFrames();
            scaleAnimation.BeginTime = delay;

            // 收缩和扩张效果
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.3, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1.2, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.2)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
            scaleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.8)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });

            // 为X和Y轴应用相同的动画
            var scaleXAnimation = scaleAnimation.Clone();
            var scaleYAnimation = scaleAnimation.Clone();

            Storyboard.SetTarget(scaleXAnimation, ring);
            Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleX)"));

            Storyboard.SetTarget(scaleYAnimation, ring);
            Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleTransform.ScaleY)"));

            // 创建透明度动画
            var opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.BeginTime = delay;

            double baseOpacity = 1.0 - (i * 0.2);
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(baseOpacity, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(0.1, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.6)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(baseOpacity, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.2)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            });
            opacityAnimation.KeyFrames.Add(new EasingDoubleKeyFrame(baseOpacity, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.8))));

            Storyboard.SetTarget(opacityAnimation, ring);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            _storyboard.Children.Add(scaleXAnimation);
            _storyboard.Children.Add(scaleYAnimation);
            _storyboard.Children.Add(opacityAnimation);
        }
    }
}