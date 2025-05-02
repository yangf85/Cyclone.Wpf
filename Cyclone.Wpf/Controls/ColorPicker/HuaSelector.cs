using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 色相选择器控件，提供圆形色轮选择器
/// </summary>
//public class HueSelector : Control
//{
//    private Ellipse _colorWheel;
//    private Canvas _canvas;
//    private Ellipse _selector;
//    private bool _isDragging;
//    private Point _center;
//    private double _radius;

//    static HueSelector()
//    {
//        DefaultStyleKeyProperty.OverrideMetadata(typeof(HueSelector),
//            new FrameworkPropertyMetadata(typeof(HueSelector)));
//    }

//    #region Hue

//    public double Hue
//    {
//        get => (double)GetValue(HueProperty);
//        set => SetValue(HueProperty, value);
//    }

//    public static readonly DependencyProperty HueProperty =
//        DependencyProperty.Register(nameof(Hue), typeof(double), typeof(HueSelector),
//        new FrameworkPropertyMetadata(180.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHueChanged));

//    private static void OnHueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is HueSelector selector)
//        {
//            selector.UpdateSelectorPosition();
//        }
//    }

//    #endregion Hue

//    #region SelectedColor

//    public Color SelectedColor
//    {
//        get => (Color)GetValue(SelectedColorProperty);
//        set => SetValue(SelectedColorProperty, value);
//    }

//    public static readonly DependencyProperty SelectedColorProperty =
//        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(HueSelector),
//        new PropertyMetadata(Colors.Cyan));

//    #endregion SelectedColor

//    public override void OnApplyTemplate()
//    {
//        base.OnApplyTemplate();

//        _canvas = GetTemplateChild("PART_Canvas") as Canvas;
//        _colorWheel = GetTemplateChild("PART_ColorWheel") as Ellipse;
//        _selector = GetTemplateChild("PART_Selector") as Ellipse;

//        if (_canvas != null && _colorWheel != null && _selector != null)
//        {
//            _colorWheel.MouseLeftButtonDown += ColorWheel_MouseLeftButtonDown;
//            _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
//            _canvas.MouseMove += Canvas_MouseMove;
//            _canvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;
//            _canvas.SizeChanged += Canvas_SizeChanged;
//        }
//    }

//    private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
//    {
//        if (_canvas != null)
//        {
//            _center = new Point(_canvas.ActualWidth / 2, _canvas.ActualHeight / 2);
//            _radius = Math.Min(_canvas.ActualWidth, _canvas.ActualHeight) / 2 - _selector.Width / 2;
//            UpdateSelectorPosition();
//        }
//    }

//    private void ColorWheel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//    {
//        _canvas.CaptureMouse();
//        _isDragging = true;
//        Point position = e.GetPosition(_canvas);
//        UpdateHueFromPosition(position);
//    }

//    private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
//    {
//        _canvas.CaptureMouse();
//        _isDragging = true;
//        Point position = e.GetPosition(_canvas);
//        UpdateHueFromPosition(position);
//    }

//    private void Canvas_MouseMove(object sender, MouseEventArgs e)
//    {
//        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
//        {
//            Point position = e.GetPosition(_canvas);
//            UpdateHueFromPosition(position);
//        }
//    }

//    private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
//    {
//        _isDragging = false;
//        _canvas.ReleaseMouseCapture();
//    }

//    private void UpdateHueFromPosition(Point position)
//    {
//        // 根据点击位置计算角度（色相）
//        double deltaX = position.X - _center.X;
//        double deltaY = position.Y - _center.Y;

//        // 计算极坐标角度（0-360）
//        double angle = Math.Atan2(deltaY, deltaX) * 180 / Math.PI;
//        if (angle < 0)
//        {
//            angle += 360;
//        }

//        // 色相范围是0-360度
//        Hue = angle;

//        // 计算颜色
//        UpdateColor();
//    }

//    private void UpdateSelectorPosition()
//    {
//        if (_canvas == null || _selector == null)
//            return;

//        // 根据色相计算选择器位置
//        double angleRadians = Hue * Math.PI / 180;
//        double x = _center.X + _radius * Math.Cos(angleRadians);
//        double y = _center.Y + _radius * Math.Sin(angleRadians);

//        Canvas.SetLeft(_selector, x - _selector.Width / 2);
//        Canvas.SetTop(_selector, y - _selector.Height / 2);

//        // 更新颜色
//        UpdateColor();
//    }

//    private void UpdateColor()
//    {
//        // 从HSL转换为RGB，假设饱和度和亮度为1和0.5
//        HslColor hsl = new HslColor(Hue, 1, 0.5);
//        SelectedColor = ColorHelper.HslToRgb(hsl);
//    }
//}