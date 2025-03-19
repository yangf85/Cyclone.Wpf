using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Cyclone.Wpf.Controls;

public class CircularGauge : RangeBase
{
    private bool _isDragging = false;

    private Point _lastMousePosition;

    public CircularGauge()
    {
        PreviewMouseDown += CircularGauge_PreviewMouseDown;
        PreviewMouseMove += CircularGauge_PreviewMouseMove;
        PreviewMouseUp += CircularGauge_PreviewMouseUp;
    }

    static CircularGauge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(typeof(CircularGauge)));

        ValueProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));
        WidthProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(150d, FrameworkPropertyMetadataOptions.AffectsRender));
        HeightProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(150d, FrameworkPropertyMetadataOptions.AffectsRender));
        LargeChangeProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(10d));
        SmallChangeProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(1d));
    }

    #region TickColor

    public Brush TickColor
    {
        get => (Brush)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    public static readonly DependencyProperty TickColorProperty =
       DependencyProperty.Register("TickColor", typeof(Brush), typeof(CircularGauge),
           new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion TickColor

    #region LabelFontSize

    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }

    public static readonly DependencyProperty LabelFontSizeProperty =
        DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(CircularGauge),
            new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion LabelFontSize

    #region IsLabelInside

    public bool IsLabelInside
    {
        get => (bool)GetValue(IsLabelInsideProperty);
        set => SetValue(IsLabelInsideProperty, value);
    }

    public static readonly DependencyProperty IsLabelInsideProperty =
        DependencyProperty.Register(nameof(IsLabelInside), typeof(bool), typeof(CircularGauge),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion IsLabelInside

    #region TickLengthRatio

    // 刻度线长度比例(相对于半径)
    public double TickLengthRatio
    {
        get => (double)GetValue(TickLengthRatioProperty);
        set => SetValue(TickLengthRatioProperty, value);
    }

    public static readonly DependencyProperty TickLengthRatioProperty =
        DependencyProperty.Register("TickLengthRatio", typeof(double), typeof(CircularGauge),
            new FrameworkPropertyMetadata(0.04, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion TickLengthRatio

    #region LongTickRatio

    //长刻度线比例(相对于短刻度线)
    public double LongTickRatio
    {
        get => (double)GetValue(LongTickRatioProperty);
        set => SetValue(LongTickRatioProperty, value);
    }

    public static readonly DependencyProperty LongTickRatioProperty =
        DependencyProperty.Register("LongTickRatio", typeof(double), typeof(CircularGauge),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion LongTickRatio

    #region PointerWidth

    public double PointerWidth
    {
        get => (double)GetValue(PointerWidthProperty);
        set => SetValue(PointerWidthProperty, value);
    }

    public static readonly DependencyProperty PointerWidthProperty =
       DependencyProperty.Register(nameof(PointerWidth), typeof(double), typeof(CircularGauge),
           new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion PointerWidth

    #region PointerColor

    public Brush PointerColor
    {
        get => (Brush)GetValue(PointerColorProperty);
        set => SetValue(PointerColorProperty, value);
    }

    public static readonly DependencyProperty PointerColorProperty =
        DependencyProperty.Register(nameof(PointerColor), typeof(Brush), typeof(CircularGauge),
            new FrameworkPropertyMetadata(Brushes.DarkRed, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion PointerColor

    #region StartAngle

    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    public static readonly DependencyProperty StartAngleProperty =
       DependencyProperty.Register("StartAngle", typeof(double), typeof(CircularGauge),
           new FrameworkPropertyMetadata(135.0, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion StartAngle

    #region EndAngle

    public double EndAngle
    {
        get => (double)GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }

    public static readonly DependencyProperty EndAngleProperty =
       DependencyProperty.Register("EndAngle", typeof(double), typeof(CircularGauge),
           new FrameworkPropertyMetadata(45.0, FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion EndAngle

    #region InnerRingBackground

    public Brush InnerRingBackground
    {
        get => (Brush)GetValue(InnerRingBackgroundProperty);
        set => SetValue(InnerRingBackgroundProperty, value);
    }

    public static readonly DependencyProperty InnerRingBackgroundProperty =
       DependencyProperty.Register(nameof(InnerRingBackground), typeof(Brush), typeof(CircularGauge),
           new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion InnerRingBackground

    #region InnerRingBorderBrush

    public Brush InnerRingBorderBrush
    {
        get => (Brush)GetValue(InnerRingBorderBrushProperty);
        set => SetValue(InnerRingBorderBrushProperty, value);
    }

    public static readonly DependencyProperty InnerRingBorderBrushProperty =
      DependencyProperty.Register(nameof(InnerRingBorderBrush), typeof(Brush), typeof(CircularGauge),
          new FrameworkPropertyMetadata(default(Brush), FrameworkPropertyMetadataOptions.AffectsRender));

    #endregion InnerRingBorderBrush

    #region IsDraggable

    public bool IsDraggable
    {
        get => (bool)GetValue(IsDraggableProperty);
        set => SetValue(IsDraggableProperty, value);
    }

    public static readonly DependencyProperty IsDraggableProperty =
       DependencyProperty.Register("IsDraggable", typeof(bool), typeof(CircularGauge),
           new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnIsDraggableChanged));

    private static void OnIsDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gauge = (CircularGauge)d;
        gauge._isDragging = false; // 禁用时强制结束拖动
    }

    #endregion IsDraggable

    #region 鼠标事件处理

    private void CircularGauge_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsDraggable) return;

        if (e.ChangedButton == MouseButton.Left)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(this);
        }
    }

    private void CircularGauge_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!IsDraggable) return;

        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            UpdateValueFromMousePosition(e.GetPosition(this));
        }
    }

    private void CircularGauge_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsDraggable) return;

        if (e.ChangedButton == MouseButton.Left)
        {
            _isDragging = false;
        }
    }

    #endregion 鼠标事件处理

    #region 坐标到值的转换

    private void UpdateValueFromMousePosition(Point position)
    {
        double centerX = ActualWidth / 2;
        double centerY = ActualHeight / 2;
        double dx = position.X - centerX;
        double dy = position.Y - centerY;

        // 计算距离中心的距离，确保在控件半径内才有效
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double radius = Math.Min(ActualWidth, ActualHeight) * 0.5;
        if (distance > radius)
            return;

        // 计算角度
        double angle = Math.Atan2(dy, dx) * (180 / Math.PI);

        // 计算角度范围
        double startAngle = StartAngle;
        double endAngle = EndAngle;
        double totalAngleSpan = (endAngle - startAngle + 360) % 360;

        // 调整角度到起始角度的相对位置
        double angleFromStart = (angle - startAngle + 360) % 360;

        // 确保角度在有效范围内
        if (angleFromStart > totalAngleSpan)
            angleFromStart = totalAngleSpan;
        else if (angleFromStart < 0)
            angleFromStart = 0;

        // 映射到 Value 范围
        double value = Minimum + (angleFromStart / totalAngleSpan) * (Maximum - Minimum);
        value = Math.Max(Minimum, Math.Min(Maximum, value));

        // 设置 Value 属性触发更新
        Value = value;

        _lastMousePosition = position;
    }

    #endregion 坐标到值的转换

    #region 绘制逻辑

    private string GetBindingStringFormat(DependencyProperty property)
    {
        // 获取绑定表达式
        var bindingExpression = BindingOperations.GetBindingExpression(this, property);

        // 检查是否为有效绑定且包含StringFormat
        if (bindingExpression?.ParentBinding?.StringFormat != null)
        {
            return bindingExpression.ParentBinding.StringFormat;
        }

        // 处理其他场景（如MultiBinding）
        var multiBindingExpression = BindingOperations.GetMultiBindingExpression(this, property);
        if (multiBindingExpression?.ParentMultiBinding?.StringFormat != null)
        {
            return multiBindingExpression.ParentMultiBinding.StringFormat;
        }

        return null; // 未找到格式
    }

    //protected override void OnValueChanged(double oldValue, double newValue)
    //{
    //    base.OnValueChanged(oldValue, newValue);
    //    InvalidateVisual();
    //}

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // 基础参数计算
        double width = ActualWidth;
        double height = ActualHeight;
        double radius = Math.Min(width, height) * 0.5;
        double centerX = width / 2;
        double centerY = height / 2;

        // 绘制背景圆
        drawingContext.DrawEllipse(
            Background,
            new Pen(BorderBrush, 1),
            new Point(centerX, centerY),
            radius,
            radius
        );

        // 角度范围计算
        double startAngle = StartAngle;
        double endAngle = EndAngle;
        double range = (endAngle >= startAngle) ? (endAngle - startAngle) : (360 - startAngle + endAngle);
        double totalSteps = Maximum - Minimum;
        double stepAngle = range / totalSteps;

        // 刻度参数
        double tickLength = radius * TickLengthRatio;
        double longTickLength = tickLength * LongTickRatio;

        // 主绘制循环（刻度和标签）
        for (double i = Minimum; i <= Maximum; i += SmallChange)
        {
            // 计算当前刻度的角度
            double currentAngle = startAngle + (i - Minimum) * stepAngle;
            currentAngle = (currentAngle % 360 + 360) % 360;
            double radian = currentAngle * Math.PI / 180;

            // 判断是否为长刻度
            bool isLongTick = i % LargeChange == 0;
            double currentTickLength = isLongTick ? longTickLength : tickLength;

            // 刻度线方向（内外）
            double tickDirection = IsLabelInside ? -1 : 1;
            Point tickEnd = new Point(
                centerX + (radius + tickDirection * currentTickLength) * Math.Cos(radian),
                centerY + (radius + tickDirection * currentTickLength) * Math.Sin(radian)
            );

            // 绘制刻度线
            drawingContext.DrawLine(
                new Pen(TickColor, isLongTick ? 2 : 1),
                new Point(
                    centerX + radius * Math.Cos(radian),
                    centerY + radius * Math.Sin(radian)
                ),
                tickEnd
            );

            // 绘制标签（仅长刻度）
            if (isLongTick)
            {
                // 格式化文本
                FormattedText text = new FormattedText(
                    i.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                    LabelFontSize,
                    TickColor,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip
                );

                // 标签偏移位置
                double labelOffset = IsLabelInside ? -(longTickLength * 1.75) : longTickLength * 1.75;

                Point labelCenter = new Point(
                    centerX + (radius + labelOffset) * Math.Cos(radian),
                    centerY + (radius + labelOffset) * Math.Sin(radian)
                );

                // 旋转标签使其水平
                drawingContext.PushTransform(new RotateTransform(
                    currentAngle + (IsLabelInside ? -270 : 90),
                    labelCenter.X,
                    labelCenter.Y
                ));
                drawingContext.DrawText(text, new Point(
                    labelCenter.X - text.Width / 2,
                    labelCenter.Y - text.Height / 2
                ));
                drawingContext.Pop();
            }
        }

        // --- 指针绘制（整个指针为狭长三角形）---
        // 指针角度计算
        double pointerAngle = startAngle + (Value - Minimum) * stepAngle;
        pointerAngle = (pointerAngle % 360 + 360) % 360;
        double pointerRadian = pointerAngle * Math.PI / 180;

        // 指针参数
        double pointerLength = IsLabelInside ? radius * 0.65 : radius * 0.85; // 指针总长度
        double triangleBaseWidth = PointerWidth;        // 三角形底边宽度

        // 计算方向向量
        Vector direction = new Vector(
            Math.Cos(pointerRadian),
            Math.Sin(pointerRadian)
        );

        // 垂直方向向量（用于左右偏移）
        Vector perpendicular = new Vector(-direction.Y, direction.X); // 逆时针旋转90度

        // 三角形顶点：
        // 1. 尖端指向末端
        // 2. 底边两端点位于中心附近
        Point tipPoint = new Point(
            centerX + pointerLength * Math.Cos(pointerRadian),
            centerY + pointerLength * Math.Sin(pointerRadian)
        );

        // 底边顶点坐标计算
        Point baseLeft = new Point(
            centerX - perpendicular.X * (triangleBaseWidth / 2),
            centerY - perpendicular.Y * (triangleBaseWidth / 2)
        );

        Point baseRight = new Point(
            centerX + perpendicular.X * (triangleBaseWidth / 2),
            centerY + perpendicular.Y * (triangleBaseWidth / 2)
        );

        // 创建三角形路径
        PathGeometry pointerGeometry = new PathGeometry();
        PathFigure figure = new PathFigure();

        // 路径路径：尖端 → 左底 → 右底 → 闭合
        figure.StartPoint = tipPoint;
        figure.Segments.Add(new LineSegment(baseLeft, true));
        figure.Segments.Add(new LineSegment(baseRight, true));
        figure.Segments.Add(new LineSegment(tipPoint, true));
        figure.IsClosed = true;

        pointerGeometry.Figures.Add(figure);

        // 绘制三角形指针（填充）
        drawingContext.DrawGeometry(PointerColor, null, pointerGeometry);

        // 绘制中心内环
        double innerRadius = FontSize * 1.2;
        drawingContext.DrawEllipse(
            InnerRingBackground,
            new Pen(InnerRingBorderBrush, 1),
            new Point(centerX, centerY),
            innerRadius,
            innerRadius
        );

        // 绘制当前值文本
        var format = GetBindingStringFormat(ValueProperty);
        string formattedValue = string.IsNullOrEmpty(format) ? Value.ToString() : string.Format(CultureInfo.CurrentCulture, format, Value);
        FormattedText valueText = new FormattedText(
            formattedValue,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize,
            Foreground,
            VisualTreeHelper.GetDpi(this).PixelsPerDip
        );
        drawingContext.DrawText(valueText, new Point(
            centerX - valueText.Width / 2,
            centerY - valueText.Height / 2
        ));
    }

    #endregion 绘制逻辑
}