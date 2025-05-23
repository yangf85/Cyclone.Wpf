using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 加载动画控件 - 显示5个旋转的粒子
/// </summary>
public class LoadingSpinner : UserControl
{
    #region 依赖属性

    public static readonly DependencyProperty ParticleColorProperty =
        DependencyProperty.Register(nameof(ParticleColor), typeof(Brush), typeof(LoadingSpinner),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x0D, 0x47, 0xA1)), OnVisualPropertyChanged));

    public static readonly DependencyProperty ParticleRadiusProperty =
        DependencyProperty.Register(nameof(ParticleRadius), typeof(double), typeof(LoadingSpinner),
            new PropertyMetadata(5.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LoadingSpinner),
            new PropertyMetadata(true, OnIsActiveChanged));

    public static readonly DependencyProperty SpinnerSizeProperty =
        DependencyProperty.Register(nameof(SpinnerSize), typeof(double), typeof(LoadingSpinner),
            new PropertyMetadata(75.0, OnVisualPropertyChanged));

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
    private Border[] _particleBorders;
    private Storyboard _storyboard;

    public LoadingSpinner()
    {
        CreateVisualTree();

        if (IsActive)
        {
            Loaded += (s, e) => StartAnimation();
        }
    }

    private void CreateVisualTree()
    {
        // 创建Viewbox
        _viewbox = new Viewbox
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
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
            Width = 1,
            Height = 1,
            Margin = new Thickness(0)
        };

        // 创建5个粒子
        _particles = new Ellipse[5];
        _particleBorders = new Border[5];

        double[] particleOriginAngles = { 0, -10, -20, -30, -40 };

        for (int i = 0; i < 5; i++)
        {
            // 创建Border容器
            var border = new Border
            {
                Background = Brushes.Transparent,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform()
            };

            // 创建粒子(Ellipse)
            var particle = new Ellipse
            {
                Width = ParticleRadius,
                Height = ParticleRadius,
                Fill = ParticleColor,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            // 设置粒子的初始位置和旋转
            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(0, -20));
            transformGroup.Children.Add(new RotateTransform(particleOriginAngles[i]));
            particle.RenderTransform = transformGroup;

            border.Child = particle;
            _canvas.Children.Add(border);

            _particles[i] = particle;
            _particleBorders[i] = border;
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
        var spinner = (LoadingSpinner)d;
        spinner.UpdateVisualProperties();
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var spinner = (LoadingSpinner)d;
        if (spinner.IsLoaded)
        {
            spinner.UpdateAnimationState();
        }
    }

    private void UpdateVisualProperties()
    {
        if (_particles != null)
        {
            foreach (var particle in _particles)
            {
                particle.Width = ParticleRadius;
                particle.Height = ParticleRadius;
                particle.Fill = ParticleColor;
            }
        }

        if (_viewbox?.Child is Grid grid)
        {
            grid.Width = SpinnerSize;
            grid.Height = SpinnerSize;
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

        // 动画参数
        var particleBeginDelays = new[] { 0.0, 0.1, 0.2, 0.3, 0.4 };

        // 动画阶段参数
        var phases = new[]
        {
            new { BeginAngle = 0.0, EndAngle = 90.0, BeginTime = TimeSpan.Zero, Duration = new Duration(TimeSpan.FromSeconds(0.75)) },
            new { BeginAngle = 90.0, EndAngle = 270.0, BeginTime = TimeSpan.FromSeconds(0.751), Duration = new Duration(TimeSpan.FromSeconds(0.3)) },
            new { BeginAngle = 270.0, EndAngle = 360.0, BeginTime = TimeSpan.FromSeconds(1.052), Duration = new Duration(TimeSpan.FromSeconds(0.75)) }
        };

        // 为每个粒子创建动画
        for (int i = 0; i < 5; i++)
        {
            var border = _particleBorders[i];
            var beginDelay = TimeSpan.FromSeconds(particleBeginDelays[i]);

            // 创建包含延迟的子Storyboard
            var particleStoryboard = new Storyboard
            {
                BeginTime = beginDelay,
                Duration = new Duration(TimeSpan.FromSeconds(1.8))
            };

            // 为每个阶段创建动画
            foreach (var phase in phases)
            {
                var animation = new DoubleAnimation
                {
                    From = phase.BeginAngle,
                    To = phase.EndAngle,
                    BeginTime = phase.BeginTime,
                    Duration = phase.Duration
                };

                Storyboard.SetTarget(animation, border);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                particleStoryboard.Children.Add(animation);
            }

            _storyboard.Children.Add(particleStoryboard);
        }
    }
}