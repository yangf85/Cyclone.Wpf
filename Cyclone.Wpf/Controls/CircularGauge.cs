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
    static CircularGauge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CircularGauge), new FrameworkPropertyMetadata(typeof(CircularGauge)));
    }

    #region TickColor
    public static readonly DependencyProperty TickColorProperty =
       DependencyProperty.Register("TickColor", typeof(Brush), typeof(CircularGauge),
           new PropertyMetadata(Brushes.White));

    public Brush TickColor
    {
        get => (Brush)GetValue(TickColorProperty);
        set => SetValue(TickColorProperty, value);
    }

    #endregion
    #region LabelFontSize
    public double LabelFontSize
    {
        get => (double)GetValue(LabelFontSizeProperty);
        set => SetValue(LabelFontSizeProperty, value);
    }


    public static readonly DependencyProperty LabelFontSizeProperty =
        DependencyProperty.Register(nameof(LabelFontSize), typeof(double), typeof(CircularGauge), new PropertyMetadata(10d));

    #endregion

    

    #region TickLengthRatio
    // 刻度线长度比例（相对于半径）
    public static readonly DependencyProperty TickLengthRatioProperty =
        DependencyProperty.Register("TickLengthRatio", typeof(double), typeof(CircularGauge),
            new PropertyMetadata(0.04));

    public double TickLengthRatio
    {
        get => (double)GetValue(TickLengthRatioProperty);
        set => SetValue(TickLengthRatioProperty, value);
    }
    #endregion
    #region LongTickRatio
    // 长刻度线比例（相对于短刻度线）
    public static readonly DependencyProperty LongTickRatioProperty =
        DependencyProperty.Register("LongTickRatio", typeof(double), typeof(CircularGauge),
            new PropertyMetadata(2.0));

    public double LongTickRatio
    {
        get => (double)GetValue(LongTickRatioProperty);
        set => SetValue(LongTickRatioProperty, value);
    }

    #endregion

    #region PointerThickness
    public double PointerThickness
    {
        get => (double)GetValue(PointerThicknessProperty);
        set => SetValue(PointerThicknessProperty, value);
    }

    public static readonly DependencyProperty PointerThicknessProperty =
        DependencyProperty.Register(nameof(PointerThickness), typeof(double), typeof(CircularGauge), new PropertyMetadata(3d));

    #endregion

    #region PointerColor
    public Brush PointerColor
    {
        get => (Brush)GetValue(PointerColorProperty);
        set => SetValue(PointerColorProperty, value);
    }

    public static readonly DependencyProperty PointerColorProperty =
        DependencyProperty.Register(nameof(PointerColor), typeof(Brush), typeof(CircularGauge), new PropertyMetadata(Brushes.DarkRed));

    #endregion




    #region StartAngle


    public static readonly DependencyProperty StartAngleProperty =
        DependencyProperty.Register("StartAngle", typeof(double), typeof(CircularGauge),
            new PropertyMetadata(135.0));

    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }
    #endregion

    #region InnerRingBackground
    public Brush InnerRingBackground
    {
        get => (Brush)GetValue(InnerRingBackgroundProperty);
        set => SetValue(InnerRingBackgroundProperty, value);
    }

    public static readonly DependencyProperty InnerRingBackgroundProperty =
        DependencyProperty.Register(nameof(InnerRingBackground), typeof(Brush), typeof(CircularGauge), new UIPropertyMetadata(default(Brush)));

    #endregion

    #region InnerRingBorderBrush
    public Brush InnerRingBorderBrush
    {
        get => (Brush)GetValue(InnerRingBorderBrushProperty);
        set => SetValue(InnerRingBorderBrushProperty, value);
    }

    public static readonly DependencyProperty InnerRingBorderBrushProperty =
        DependencyProperty.Register(nameof(InnerRingBorderBrush), typeof(Brush), typeof(CircularGauge), new UIPropertyMetadata(default(Brush)));

   #endregion


    #region

    public static readonly DependencyProperty EndAngleProperty =
       DependencyProperty.Register("EndAngle", typeof(double), typeof(CircularGauge),
           new PropertyMetadata(45.0));

    public double EndAngle
    {
        get => (double)GetValue(EndAngleProperty);
        set => SetValue(EndAngleProperty, value);
    }
    #endregion
    #region IsDraggable
  
    public static readonly DependencyProperty IsDraggableProperty =
        DependencyProperty.Register("IsDraggable", typeof(bool), typeof(CircularGauge), new PropertyMetadata(true, OnIsDraggableChanged));
    public bool IsDraggable
    {
        get => (bool)GetValue(IsDraggableProperty);
        set => SetValue(IsDraggableProperty, value);
    }

    private static void OnIsDraggableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var gauge = (CircularGauge)d;
        gauge.isDragging = false; // 禁用时强制结束拖动
    }
    #endregion

    public CircularGauge()
    {
        // 订阅鼠标事件
        this.PreviewMouseDown += CircularGauge_PreviewMouseDown;
        this.PreviewMouseMove += CircularGauge_PreviewMouseMove;
        this.PreviewMouseUp += CircularGauge_PreviewMouseUp;
        this.IsHitTestVisible = true; // 确保控件接收鼠标事件
    }

    #region 鼠标事件处理
    private bool isDragging = false;
    private Point lastMousePosition;
    private void CircularGauge_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (!IsDraggable) return;

        if (e.ChangedButton == MouseButton.Left)
        {
            isDragging = true;
            lastMousePosition = e.GetPosition(this);
        }
    }

    private void CircularGauge_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!IsDraggable) return;

        if (isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            UpdateValueFromMousePosition(e.GetPosition(this));
        }
    }

    private void CircularGauge_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!IsDraggable) return;

        if (e.ChangedButton == MouseButton.Left)
        {
            isDragging = false;
        }
    }
    #endregion

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

        lastMousePosition = position;
    }
    #endregion

    #region 绘制逻辑
    protected override void OnValueChanged(double oldValue, double newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        // 获取控件尺寸
        double width = ActualWidth;
        double height = ActualHeight;
        double radius = Math.Min(width, height) * 0.5;
        double centerX = width / 2;
        double centerY = height / 2;

        // 绘制背景
        drawingContext.DrawEllipse(Background, null, new Point(centerX, centerY), radius, radius);
        

        // 计算角度范围
        double startAngle = StartAngle;
        double endAngle = EndAngle;
        double range = (endAngle + 360 - startAngle) % 360;
        double totalSteps = Maximum - Minimum;
        double stepAngle = range / totalSteps;

        // 刻度线长度
        double tickLength = radius * TickLengthRatio;
        double longTickLength = tickLength * LongTickRatio;

        // 绘制刻度和标签
        for (double i = Minimum; i <= Maximum; i += SmallChange)
        {
            double angle = startAngle + i * stepAngle;
            double x = centerX + radius * Math.Cos(angle * Math.PI / 180);
            double y = centerY + radius * Math.Sin(angle * Math.PI / 180);

            // 绘制刻度线
            if (i % LargeChange == 0)
            {
                // 长刻度线和标签
                double textX = centerX + (radius - longTickLength) * Math.Cos(angle * Math.PI / 180);
                double textY = centerY + (radius - longTickLength) * Math.Sin(angle * Math.PI / 180);
                drawingContext.DrawLine(new Pen(TickColor, 2), new Point(x, y), new Point(textX, textY));

                // 标签文本
                FormattedText text = new FormattedText(
                    i.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(FontFamily,FontStyle,FontWeight,FontStretch),
                    LabelFontSize,
                    TickColor,
                    1.0
                );

                // 应用旋转变换
                drawingContext.PushTransform(new RotateTransform(
                    angle + 90,
                    textX + text.Width / 2,
                    textY + text.Height / 2
                ));

                drawingContext.DrawText(text, new Point(textX, textY));

                // 恢复变换
                drawingContext.Pop();
            }
            else
            {
                // 短刻度线
                drawingContext.DrawLine(new Pen(TickColor, 1), new Point(x, y), new Point(
                    centerX + (radius - tickLength) * Math.Cos(angle * Math.PI / 180),
                    centerY + (radius - tickLength) * Math.Sin(angle * Math.PI / 180)
                ));
            }
        }

        // 绘制指针
        double pointerAngle = startAngle + Value * stepAngle;
        double pointerLength = radius * 0.8;
        double pointerX = centerX + (radius - tickLength - 5) * Math.Cos(pointerAngle * Math.PI / 180);
        double pointerY = centerY + (radius - tickLength - 5) * Math.Sin(pointerAngle * Math.PI / 180);
        drawingContext.DrawLine(new Pen(PointerColor, PointerThickness), new Point(centerX, centerY), new Point(pointerX, pointerY));

        // 绘制内环
        drawingContext.DrawEllipse(InnerRingBackground, new Pen(InnerRingBorderBrush,1), new Point(centerX, centerY), FontSize , FontSize);//绘制遮罩



        // 绘制当前值文本

        string format = GetBindingStringFormat(ValueProperty);
        var formattedValue= string.IsNullOrEmpty(format) ? Value.ToString() : string.Format(CultureInfo.CurrentCulture, format, Value);

        FormattedText currentValueText = new FormattedText(
            formattedValue, 
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
            FontSize,
            Foreground,
            1.0
        );
        drawingContext.DrawText(currentValueText, new Point(
            centerX - currentValueText.Width / 2,
            centerY - currentValueText.Height / 2
        ));
    }


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

    #endregion
}