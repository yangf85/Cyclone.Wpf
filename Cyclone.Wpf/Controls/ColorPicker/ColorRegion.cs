using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// HSV颜色空间选择器，用于通过二维区域同时选择饱和度和明度
/// </summary>
[TemplatePart(Name = "PART_ColorArea", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_Selector", Type = typeof(FrameworkElement))]
[TemplatePart(Name = "PART_HueGradient", Type = typeof(GradientStop))]
public class ColorRegion : Control
{
    static ColorRegion()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorRegion),
            new FrameworkPropertyMetadata(typeof(ColorRegion)));
    }

    #region Hue

    public double Hue
    {
        get => (double)GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    public static readonly DependencyProperty HueProperty =
        DependencyProperty.Register(nameof(Hue), typeof(double), typeof(ColorRegion),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHueChanged));

    private static void OnHueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorRegion selector)
        {
            selector.OnHueChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    protected virtual void OnHueChanged(double oldValue, double newValue)
    {
        UpdateHueGradient();
        UpdateSelectedColor();
    }

    #endregion Hue

    #region Saturation

    public double Saturation
    {
        get => (double)GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public static readonly DependencyProperty SaturationProperty =
        DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(ColorRegion),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSaturationChanged)); // 将初始值改为1.0

    private static void OnSaturationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorRegion selector)
        {
            selector.OnSaturationChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    protected virtual void OnSaturationChanged(double oldValue, double newValue)
    {
        UpdateSelectorPosition();
        UpdateSelectedColor();
    }

    #endregion Saturation

    #region Value

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ColorRegion),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorRegion selector)
        {
            selector.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    protected virtual void OnValueChanged(double oldValue, double newValue)
    {
        UpdateSelectorPosition();
        UpdateSelectedColor();
    }

    #endregion Value

    #region SelectedColor

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        private set => SetValue(SelectedColorPropertyKey, value);
    }

    private static readonly DependencyPropertyKey SelectedColorPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedColor), typeof(Color), typeof(ColorRegion),
            new PropertyMetadata(Colors.Red));

    public static readonly DependencyProperty SelectedColorProperty = SelectedColorPropertyKey.DependencyProperty;

    #endregion SelectedColor

    public event EventHandler<ColorChangedEventArgs> ColorChanged;

    // 模板部分
    private FrameworkElement _colorArea;

    private FrameworkElement _selector;
    private GradientStop _hueGradient;
    private Border _colorAreaBorder;
    private LinearGradientBrush _saturationGradient;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 取消旧元素的事件绑定
        if (_colorArea != null)
        {
            _colorArea.MouseLeftButtonDown -= ColorArea_MouseLeftButtonDown;
            _colorArea.MouseMove -= ColorArea_MouseMove;
            _colorArea.MouseLeftButtonUp -= ColorArea_MouseLeftButtonUp;
            _colorArea.SizeChanged -= ColorArea_SizeChanged;
        }

        // 获取模板部分
        _colorArea = GetTemplateChild("PART_ColorArea") as FrameworkElement;
        _selector = GetTemplateChild("PART_Selector") as FrameworkElement;
        _hueGradient = GetTemplateChild("PART_HueGradient") as GradientStop;
        _colorAreaBorder = _colorArea as Border;

        // 如果使用了新的模板结构，获取LinearGradientBrush
        if (_colorAreaBorder != null)
        {
            _saturationGradient = _colorAreaBorder.Background as LinearGradientBrush;
        }

        // 绑定新元素的事件
        if (_colorArea != null)
        {
            _colorArea.MouseLeftButtonDown += ColorArea_MouseLeftButtonDown;
            _colorArea.MouseMove += ColorArea_MouseMove;
            _colorArea.MouseLeftButtonUp += ColorArea_MouseLeftButtonUp;
            _colorArea.SizeChanged += ColorArea_SizeChanged;
        }

        Debug.WriteLine($"OnApplyTemplate: _colorArea={_colorArea}, _selector={_selector}, _hueGradient={_hueGradient}");

        // 延迟初始化以确保UI元素加载完成
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // 初始更新
            UpdateHueGradient();
            UpdateSelectorPosition();
            UpdateSelectedColor();
        }), DispatcherPriority.Loaded);
    }

    // 更新方法
    private void UpdateHueGradient()
    {
        if (_hueGradient != null)
        {
            // 计算当前色相对应的纯色（饱和度和明度均为1）
            ColorHelper.HsvToRgb(Hue, 1, 1, out byte r, out byte g, out byte b);
            Color color = Color.FromRgb(r, g, b);

            // 更新渐变色
            _hueGradient.Color = color;

            // 强制刷新背景
            if (_colorAreaBorder != null)
            {
                // 如果使用新模板方式，刷新LinearGradientBrush
                if (_saturationGradient != null)
                {
                    var stop = _saturationGradient.GradientStops[1];
                    stop.Color = color;
                    _saturationGradient.GradientStops[1] = stop;
                }

                // 强制刷新视觉元素
                _colorAreaBorder.InvalidateVisual();
            }
            else if (_colorArea != null)
            {
                // 原模板方式，刷新整个元素
                _colorArea.InvalidateVisual();
            }
        }
    }

    private void UpdateSelectorPosition()
    {
        if (_colorArea != null && _selector != null)
        {
            // 计算选择器位置
            double x = Saturation * _colorArea.ActualWidth;
            double y = (1 - Value) * _colorArea.ActualHeight;

            // 更新选择器位置
            Canvas.SetLeft(_selector, x - _selector.ActualWidth / 2);
            Canvas.SetTop(_selector, y - _selector.ActualHeight / 2);
        }
    }

    private void UpdateSelectedColor()
    {
        ColorHelper.HsvToRgb(Hue, Saturation, Value, out byte r, out byte g, out byte b);
        Color color = Color.FromRgb(r, g, b);
        SelectedColor = color;

        ColorChanged?.Invoke(this, new ColorChangedEventArgs(SelectedColor, color));
    }

    // 鼠标交互处理
    private bool _isDragging;

    private void ColorArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _colorArea.CaptureMouse();
        _isDragging = true;
        UpdateFromMousePosition(e.GetPosition(_colorArea));
    }

    private void ColorArea_MouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging)
        {
            UpdateFromMousePosition(e.GetPosition(_colorArea));
        }
    }

    private void ColorArea_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        _colorArea.ReleaseMouseCapture();
        _isDragging = false;
    }

    private void ColorArea_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateSelectorPosition();
    }

    private void UpdateFromMousePosition(Point pos)
    {
        if (_colorArea == null) return;

        // 根据鼠标位置计算饱和度和明度值，使用 Math.Min 和 Math.Max 代替 Math.Clamp
        double s = Math.Min(Math.Max(pos.X / _colorArea.ActualWidth, 0), 1);
        double v = Math.Min(Math.Max(1 - (pos.Y / _colorArea.ActualHeight), 0), 1);

        // 更新属性
        Saturation = s;
        Value = v;
    }

    // 覆盖渲染方法，确保更新正确显示
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // 强制更新一次，以防止渲染问题
        if (_hueGradient != null)
        {
            ColorHelper.HsvToRgb(Hue, 1, 1, out byte r, out byte g, out byte b);
            _hueGradient.Color = Color.FromRgb(r, g, b);
        }
    }
}

public class ColorChangedEventArgs : EventArgs
{
    public Color NewColor { get; }

    public Color OldColor { get; }

    public ColorChangedEventArgs(Color oldColor, Color newColor)
    {
        OldColor = oldColor;
        NewColor = newColor;
    }
}