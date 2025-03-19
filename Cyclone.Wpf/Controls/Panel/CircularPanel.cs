using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

public class CircularPanel : Panel
{
    #region 依赖属性

    #region InnerRadiusRatio

    public static readonly DependencyProperty InnerRadiusRatioProperty =
       DependencyProperty.Register("InnerRadiusRatio", typeof(double), typeof(CircularPanel),
           new FrameworkPropertyMetadata(0.5,
               FrameworkPropertyMetadataOptions.AffectsArrange |
               FrameworkPropertyMetadataOptions.AffectsMeasure |
               FrameworkPropertyMetadataOptions.AffectsRender,
               null, CoerceInnerRadiusRatio));

    public double InnerRadiusRatio
    {
        get => (double)GetValue(InnerRadiusRatioProperty);
        set => SetValue(InnerRadiusRatioProperty, value);
    }

    #endregion InnerRadiusRatio

    #region BorderBrush

    public static readonly DependencyProperty BorderBrushProperty =
      DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(CircularPanel),
          new FrameworkPropertyMetadata(Brushes.Black,
              FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    #endregion BorderBrush

    #region BorderThickness

    public static readonly DependencyProperty BorderThicknessProperty =
      DependencyProperty.Register("BorderThickness", typeof(double), typeof(CircularPanel),
          new FrameworkPropertyMetadata(1.0,
              FrameworkPropertyMetadataOptions.AffectsRender,
              null, CoerceBorderThickness));

    public double BorderThickness
    {
        get => (double)GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    #endregion BorderThickness

    #region ShowOuterBorder

    public static readonly DependencyProperty ShowOuterBorderProperty =
        DependencyProperty.Register("ShowOuterBorder", typeof(bool), typeof(CircularPanel),
            new FrameworkPropertyMetadata(true,
                FrameworkPropertyMetadataOptions.AffectsRender));

    public bool ShowOuterBorder
    {
        get => (bool)GetValue(ShowOuterBorderProperty);
        set => SetValue(ShowOuterBorderProperty, value);
    }

    #endregion ShowOuterBorder

    #region ShowInnerBorder

    public static readonly DependencyProperty ShowInnerBorderProperty =
      DependencyProperty.Register("ShowInnerBorder", typeof(bool), typeof(CircularPanel),
          new FrameworkPropertyMetadata(true,
              FrameworkPropertyMetadataOptions.AffectsRender));

    public bool ShowInnerBorder
    {
        get => (bool)GetValue(ShowInnerBorderProperty);
        set => SetValue(ShowInnerBorderProperty, value);
    }

    #endregion ShowInnerBorder

    #region ShowGridLines

    public static readonly DependencyProperty ShowGridLinesProperty =
      DependencyProperty.Register("ShowGridLines", typeof(bool), typeof(CircularPanel),
          new FrameworkPropertyMetadata(true,
              FrameworkPropertyMetadataOptions.AffectsRender));

    public bool ShowGridLines
    {
        get => (bool)GetValue(ShowGridLinesProperty);
        set => SetValue(ShowGridLinesProperty, value);
    }

    #endregion ShowGridLines

    #endregion 依赖属性

    #region 布局逻辑

    protected override Size MeasureOverride(Size availableSize)
    {
        double diameter = CalculateSafeDiameter(availableSize);
        int count = InternalChildren.Count;

        if (count == 0) return new Size(diameter, diameter);

        double outerRadius = diameter / 2;
        double innerRadius = outerRadius * InnerRadiusRatio;
        double angleStep = 360.0 / count;
        double sectorWidth = outerRadius - innerRadius;

        // 计算最大子元素尺寸
        double chordLength = 2 * outerRadius * Math.Sin(angleStep * Math.PI / 360);
        double maxChildWidth = Math.Max(chordLength * 0.9, 20); // 最小20px
        double maxChildHeight = Math.Max(sectorWidth * 0.9, 20);

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(maxChildWidth, maxChildHeight));
        }

        return new Size(diameter, diameter);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int count = InternalChildren.Count;
        if (count == 0) return finalSize;

        double diameter = Math.Min(finalSize.Width, finalSize.Height);
        double outerRadius = diameter / 2;
        double innerRadius = outerRadius * InnerRadiusRatio;
        Point center = new Point(finalSize.Width / 2, finalSize.Height / 2);
        double angleStep = 360.0 / count;
        double sectorWidth = outerRadius - innerRadius;

        for (int i = 0; i < count; i++)
        {
            UIElement child = InternalChildren[i];
            if (child == null) continue;

            double midAngle = i * angleStep + angleStep / 2;
            double rotationAngle = midAngle;
            double positionRadius = innerRadius + sectorWidth * 0.5;
            Point sectorCenter = CalculatePosition(center, positionRadius, midAngle);

            // 旋转变换（锚点在底部中心）
            TransformGroup transform = new TransformGroup();
            transform.Children.Add(new RotateTransform(
                rotationAngle,
                child.DesiredSize.Width / 2,
                child.DesiredSize.Height
            ));

            Rect arrangeRect = new Rect(
                sectorCenter.X - child.DesiredSize.Width / 2,
                sectorCenter.Y - child.DesiredSize.Height / 2,
                child.DesiredSize.Width,
                child.DesiredSize.Height
            );

            child.RenderTransform = transform;
            child.Arrange(arrangeRect);
        }

        return finalSize;
    }

    #endregion 布局逻辑

    #region 辅助方法

    private static Point CalculatePosition(Point center, double radius, double angle)
    {
        double radians = (angle - 90) * Math.PI / 180; // 转换为数学坐标系
        return new Point(
            center.X + radius * Math.Cos(radians),
            center.Y + radius * Math.Sin(radians));
    }

    private double CalculateSafeDiameter(Size availableSize)
    {
        double maxWidth = double.IsInfinity(availableSize.Width) ? 1000 : availableSize.Width;
        double maxHeight = double.IsInfinity(availableSize.Height) ? 1000 : availableSize.Height;
        return Math.Min(maxWidth, maxHeight);
    }

    #endregion 辅助方法

    #region 属性验证

    private static object CoerceInnerRadiusRatio(DependencyObject d, object value)
    {
        double ratio = (double)value;
        return Math.Max(0.0, Math.Min(1.0, ratio));
    }

    private static object CoerceBorderThickness(DependencyObject d, object value)
    {
        double thickness = (double)value;
        return Math.Max(0.0, thickness);
    }

    #endregion 属性验证

    #region 渲染逻辑

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        double diameter = Math.Min(RenderSize.Width, RenderSize.Height);
        if (diameter <= 0) return;

        double outerRadius = diameter / 2;
        double innerRadius = outerRadius * InnerRadiusRatio;
        Point center = new Point(RenderSize.Width / 2, RenderSize.Height / 2);

        Pen pen = new Pen(BorderBrush, BorderThickness)
        {
            DashCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            StartLineCap = PenLineCap.Round
        };

        // 绘制外环
        if (ShowOuterBorder && outerRadius > 0)
        {
            dc.DrawEllipse(null, pen, center, outerRadius, outerRadius);
        }

        // 绘制内环
        if (ShowInnerBorder && innerRadius > 0)
        {
            dc.DrawEllipse(null, pen, center, innerRadius, innerRadius);
        }

        // 绘制分界线
        if (ShowGridLines && InternalChildren.Count > 0)
        {
            double angleStep = 360.0 / InternalChildren.Count;
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                double angle = i * angleStep;
                Point outerPoint = CalculatePosition(center, outerRadius, angle);
                Point innerPoint = innerRadius > 0
                    ? CalculatePosition(center, innerRadius, angle)
                    : center;

                dc.DrawLine(pen, innerPoint, outerPoint);
            }
        }
    }

    #endregion 渲染逻辑
}