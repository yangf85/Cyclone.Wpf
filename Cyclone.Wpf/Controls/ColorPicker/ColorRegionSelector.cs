using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 颜色区域选择器控件，提供二维颜色选择
/// </summary>
//public class ColorRegionSelector : Control
//{
//    private Canvas _canvas;
//    private Rectangle _colorRect;
//    private Ellipse _selector;
//    private bool _isDragging;

//    static ColorRegionSelector()
//    {
//        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorRegionSelector),
//            new FrameworkPropertyMetadata(typeof(ColorRegionSelector)));
//    }

//    #region BaseHue

//    public double BaseHue
//    {
//        get => (double)GetValue(BaseHueProperty);
//        set => SetValue(BaseHueProperty, value);
//    }

//    public static readonly DependencyProperty BaseHueProperty =
//        DependencyProperty.Register(nameof(BaseHue), typeof(double), typeof(ColorRegionSelector),
//        new PropertyMetadata(180.0, OnBaseHueChanged));

//    private static void OnBaseHueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is ColorRegionSelector selector)
//        {
//            selector.UpdateColorGradient();
//        }
//    }

//    #endregion BaseHue

//    #region Saturation

//    public double Saturation
//    {
//        get => (double)GetValue(SaturationProperty);
//        set => SetValue(SaturationProperty, value);
//    }

//    public static readonly DependencyProperty SaturationProperty =
//        DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(ColorRegionSelector),
//        new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPositionChanged));

//    #endregion Saturation

//    #region Lightness

//    public double Lightness
//    {
//        get => (double)GetValue(LightnessProperty);
//        set => SetValue(LightnessProperty, value);
//    }

//    public static readonly DependencyProperty LightnessProperty =
//        DependencyProperty.Register(nameof(Lightness), typeof(double), typeof(ColorRegionSelector),
//        new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPositionChanged));

//    #endregion Lightness

//    #region SelectedColor

//    public Color SelectedColor
//    {
//        get => (Color)GetValue(SelectedColorProperty);
//        set => SetValue(SelectedColorProperty, value);
//    }

//    public static readonly DependencyProperty SelectedColorProperty =
//        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorRegionSelector),
//        new PropertyMetadata(Colors.Cyan));

//    #endregion SelectedColor

//    private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is ColorRegionSelector selector)
//        {
//            selector.UpdateSelectorPosition();
//            selector.UpdateSelectedColor();
//        }
//    }

//    public override void OnApplyTemplate()
//    {
//        base.OnApplyTemplate();

//        _canvas = GetTemplateChild("PART_Canvas") as Canvas;
//        _colorRect = GetTemplateChild("PART_ColorRect") as Rectangle;
//        _selector = GetTemplateChild("PART_Selector") as Ellipse;

//        if (_canvas != null && _colorRect != null && _selector != null)
//        {
//            _colorRect.MouseLeftButtonDown += ColorRect_MouseLeftButtonDown;
//            _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
//            _canvas.MouseMove += Canvas_MouseMove;
//            _canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
//            _canvas.SizeChanged += Canvas_SizeChanged;

//            UpdateColorGradient();
//            UpdateSelectorPosition();
//        }
//    }

//    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
//    {
//        UpdateSelectorPosition();
//    }

//    private void ColorRect_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//    {
//        _canvas.CaptureMouse();
//        _isDragging = true;
//        Point position = e.GetPosition(_colorRect);
//        UpdateValuesFromPosition(position);
//    }

//    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//    {
//        _canvas.CaptureMouse();
//        _isDragging = true;
//        Point position = e.GetPosition(_colorRect);
//        UpdateValuesFromPosition(position);
//    }

//    private void Canvas_MouseMove(object sender, MouseEventArgs e)
//    {
//        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
//        {
//            Point position = e.GetPosition(_colorRect);
//            UpdateValuesFromPosition(position);
//        }
//    }

//    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
//    {
//        _isDragging = false;
//        _canvas.ReleaseMouseCapture();
//    }

//    private void UpdateValuesFromPosition(Point position)
//    {
//        if (_colorRect == null || _colorRect.ActualWidth <= 0 || _colorRect.ActualHeight <= 0)
//            return;

//        // 计算饱和度和亮度
//        double s = Clamp(position.X / _colorRect.ActualWidth, 0, 1);
//        double l = Clamp(1 - position.Y / _colorRect.ActualHeight, 0, 1);

//        // 更新属性（会触发OnPositionChanged）
//        Saturation = s;
//        Lightness = l;
//    }

//    // 自定义Clamp方法，替代Math.Clamp（在.NET Framework中不可用）
//    private double Clamp(double value, double min, double max)
//    {
//        if (value < min) return min;
//        if (value > max) return max;
//        return value;
//    }

//    private void UpdateSelectorPosition()
//    {
//        if (_colorRect == null || _selector == null || _colorRect.ActualWidth <= 0 || _colorRect.ActualHeight <= 0)
//            return;

//        // 根据饱和度和亮度计算选择器位置
//        double x = Saturation * _colorRect.ActualWidth;
//        double y = (1 - Lightness) * _colorRect.ActualHeight;

//        Canvas.SetLeft(_selector, x - _selector.Width / 2);
//        Canvas.SetTop(_selector, y - _selector.Height / 2);
//    }

//    private void UpdateColorGradient()
//    {
//        if (_colorRect == null)
//            return;

//        // 创建颜色渐变
//        // 左上角：白色 (S=0, L=1)
//        // 右上角：Base+白色 (S=1, L=1)
//        // 左下角：黑色 (S=0, L=0)
//        // 右下角：Base+黑色 (S=1, L=0)

//        Color baseColor = ColorHelper.HslToRgb(new HslColor(BaseHue, 1, 0.5));

//        // 创建渐变刷
//        LinearGradientBrush horizontalGradient = new LinearGradientBrush
//        {
//            StartPoint = new Point(0, 0),
//            EndPoint = new Point(1, 0)
//        };

//        // 白到基础色的渐变
//        GradientStop white = new GradientStop(Colors.White, 0);
//        GradientStop baseLight = new GradientStop(Color.FromRgb(
//            (byte)Math.Min(255, baseColor.R + 128),
//            (byte)Math.Min(255, baseColor.G + 128),
//            (byte)Math.Min(255, baseColor.B + 128)), 1);

//        horizontalGradient.GradientStops.Add(white);
//        horizontalGradient.GradientStops.Add(baseLight);

//        LinearGradientBrush verticalGradient = new LinearGradientBrush
//        {
//            StartPoint = new Point(0, 0),
//            EndPoint = new Point(0, 1),
//            Opacity = 1
//        };

//        // 透明到黑色的渐变
//        GradientStop transparent = new GradientStop(Color.FromArgb(0, 0, 0, 0), 0);
//        GradientStop black = new GradientStop(Color.FromArgb(255, 0, 0, 0), 1);

//        verticalGradient.GradientStops.Add(transparent);
//        verticalGradient.GradientStops.Add(black);

//        // 应用渐变
//        _colorRect.Fill = horizontalGradient;
//        _colorRect.OpacityMask = verticalGradient;

//        // 更新选中的颜色
//        UpdateSelectedColor();
//    }

//    private void UpdateSelectedColor()
//    {
//        // 根据HSL值更新颜色
//        HslColor hsl = new HslColor(BaseHue, Saturation, Lightness);
//        SelectedColor = ColorHelper.HslToRgb(hsl);
//    }
//}