using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 带间距的堆栈面板控件，支持水平和垂直布局，并提供元素间距和权重设置能力。
/// 类似于WPF原生StackPanel，但增加了元素间距和类似Grid的Star尺寸功能。
/// </summary>
/// <remarks>
/// <para>实现思路：</para>
/// <para>1. 继承自Panel基类，支持水平和垂直两种布局方向</para>
/// <para>2. 通过Spacing属性控制元素之间的统一间距</para>
/// <para>3. 引入附加属性Weight，允许子元素使用类似Grid的尺寸定义方式：</para>
/// <para>   - Auto：根据元素内容自动确定尺寸</para>
/// <para>   - 固定值：使用指定的像素值作为尺寸</para>
/// <para>   - Star(*)：按比例分配剩余空间</para>
/// <para>4. 先测量和布局固定尺寸和Auto尺寸的元素，再处理Star尺寸元素</para>
/// <para>5. 计算并应用Star尺寸单位值，确保所有Star类型元素按权重比例分配剩余空间</para>
/// </remarks>
/// <example>
/// <code>
/// &lt;SpacingStackPanel Orientation="Horizontal" Spacing="10"&gt;
///     &lt;Button Content="Auto尺寸按钮"/&gt;
///     &lt;TextBlock SpacingStackPanel.Weight="100" Text="固定尺寸文本"/&gt;
///     &lt;Rectangle SpacingStackPanel.Weight="1*" Fill="Blue"/&gt;
///     &lt;Rectangle SpacingStackPanel.Weight="2*" Fill="Red"/&gt;
/// &lt;/SpacingStackPanel&gt;
/// </code>
/// </example>
public class SpacingStackPanel : Panel
{
    #region 依赖属性

    /// <summary>
    /// 定义元素间距依赖属性。
    /// </summary>
    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(
        nameof(Spacing), typeof(double), typeof(SpacingStackPanel),
        new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// 获取或设置元素之间的间距。
    /// </summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// 定义布局方向依赖属性。
    /// </summary>
    public static readonly DependencyProperty OrientationProperty =
              StackPanel.OrientationProperty.AddOwner(typeof(SpacingStackPanel),
          new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// 获取或设置面板的布局方向（水平或垂直）。
    /// </summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// 定义元素权重的附加属性。
    /// </summary>
    public static readonly DependencyProperty WeightProperty = DependencyProperty.RegisterAttached(
       "Weight", typeof(GridLength), typeof(SpacingStackPanel),
       new FrameworkPropertyMetadata(GridLength.Auto, OnCellAttachedPropertyChanged));

    /// <summary>
    /// 获取元素的权重值。
    /// </summary>
    public static GridLength GetWeight(UIElement element)
    {
        return (GridLength)element.GetValue(WeightProperty);
    }

    /// <summary>
    /// 设置元素的权重值。
    /// </summary>
    public static void SetWeight(UIElement element, GridLength gridLength)
    {
        element.SetValue(WeightProperty, gridLength);
    }

    #endregion 依赖属性

    #region 私有辅助方法

    /// <summary>
    /// 当附加属性改变时的回调函数。
    /// </summary>
    private static void OnCellAttachedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Visual child && VisualTreeHelper.GetParent(child) is SpacingStackPanel panel)
        {
            panel.InvalidateMeasure();
        }
    }

    /// <summary>
    /// 表示Star尺寸计算信息的内部类。
    /// </summary>
    private class StarCalcInfo
    {
        /// <summary>
        /// 非Star类型元素总长度
        /// </summary>
        public double FixedTotalLength { get; set; }

        /// <summary>
        /// Star类型可用长度
        /// </summary>
        public double StarAvailableLength { get; set; }

        /// <summary>
        /// Star权重总和
        /// </summary>
        public double StarWeightSum { get; set; }

        /// <summary>
        /// 每单位Star权重的长度
        /// </summary>
        public double StarUnitLength { get; set; }

        /// <summary>
        /// 间距总长度
        /// </summary>
        public double TotalSpacingLength { get; set; }
    }

    /// <summary>
    /// 计算Star尺寸布局信息。
    /// </summary>
    private StarCalcInfo CalculateStarInfo(Size arrangeSize)
    {
        var children = InternalChildren.Cast<UIElement>();
        var info = new StarCalcInfo();

        // 计算间距总长度
        int visibleChildCount = children.Count(child => child.Visibility != Visibility.Collapsed);
        double totalSpacing = Math.Max(0, (visibleChildCount - 1) * Spacing);
        info.TotalSpacingLength = totalSpacing;

        bool isHorizontal = Orientation == Orientation.Horizontal;

        // 计算固定尺寸元素总长度和Star权重和
        info.FixedTotalLength = 0;
        info.StarWeightSum = 0;

        foreach (var child in children)
        {
            var weight = GetWeight(child);
            if (weight.IsStar)
            {
                info.StarWeightSum += weight.Value;
            }
            else
            {
                // 计算固定尺寸总长度
                info.FixedTotalLength += isHorizontal ? child.DesiredSize.Width : child.DesiredSize.Height;
            }
        }

        // 计算Star尺寸可用空间和单位长度
        double totalAvailableLength = isHorizontal ? arrangeSize.Width : arrangeSize.Height;
        info.StarAvailableLength = Math.Max(0, totalAvailableLength - info.FixedTotalLength - totalSpacing);

        // 计算每单位Star的长度
        info.StarUnitLength = info.StarWeightSum > 0 ? info.StarAvailableLength / info.StarWeightSum : 0;

        return info;
    }

    #endregion 私有辅助方法

    #region 布局重写

    /// <summary>
    /// 测量面板及其子元素的期望尺寸。
    /// </summary>
    protected override Size MeasureOverride(Size constraint)
    {
        if (InternalChildren.Count == 0)
            return new Size(0, 0);

        var result = new Size();
        bool isHorizontal = Orientation == Orientation.Horizontal;

        // 获取所有子元素
        var children = InternalChildren.Cast<UIElement>().ToList();

        // 先测量非Star元素
        var fixedChildren = children.Where(child => !GetWeight(child).IsStar).ToList();
        foreach (var child in fixedChildren)
        {
            Size childConstraint = isHorizontal
                ? new Size(constraint.Width, constraint.Height)
                : new Size(constraint.Width, constraint.Height);

            child.Measure(childConstraint);

            if (isHorizontal)
            {
                result.Height = Math.Max(result.Height, child.DesiredSize.Height);
                result.Width += child.DesiredSize.Width;
            }
            else
            {
                result.Width = Math.Max(result.Width, child.DesiredSize.Width);
                result.Height += child.DesiredSize.Height;
            }
        }

        // 计算Star信息
        var starInfo = CalculateStarInfo(constraint);

        // 测量Star元素
        var starChildren = children.Where(child => GetWeight(child).IsStar).ToList();
        foreach (var child in starChildren)
        {
            GridLength weight = GetWeight(child);
            Size childConstraint = constraint;

            if (isHorizontal)
            {
                childConstraint.Width = starInfo.StarUnitLength * weight.Value;
            }
            else
            {
                childConstraint.Height = starInfo.StarUnitLength * weight.Value;
            }

            child.Measure(childConstraint);

            if (isHorizontal)
            {
                result.Height = Math.Max(result.Height, child.DesiredSize.Height);
                result.Width += child.DesiredSize.Width;
            }
            else
            {
                result.Width = Math.Max(result.Width, child.DesiredSize.Width);
                result.Height += child.DesiredSize.Height;
            }
        }

        // 添加间距
        if (isHorizontal)
        {
            result.Width += starInfo.TotalSpacingLength;
            result.Width = Math.Min(result.Width, constraint.Width);
        }
        else
        {
            result.Height += starInfo.TotalSpacingLength;
            result.Height = Math.Min(result.Height, constraint.Height);
        }

        return result;
    }

    /// <summary>
    /// 排列面板的子元素。
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count == 0)
            return finalSize;

        bool isHorizontal = Orientation == Orientation.Horizontal;
        StarCalcInfo starInfo = CalculateStarInfo(finalSize);

        // 创建用于放置子元素的矩形
        Rect childRect = new Rect(0, 0, 0, 0);

        foreach (UIElement child in InternalChildren)
        {
            if (child.Visibility == Visibility.Collapsed)
                continue;

            GridLength weight = GetWeight(child);
            double length;

            if (isHorizontal)
            {
                // 水平方向布局
                if (weight.IsStar)
                {
                    // Star尺寸
                    length = weight.Value * starInfo.StarUnitLength;
                }
                else
                {
                    // 固定或自动尺寸
                    double remainingWidth = finalSize.Width - childRect.X;
                    length = remainingWidth <= 0 ? 0 : Math.Min(remainingWidth, child.DesiredSize.Width);
                }

                // 设置子元素尺寸
                childRect.Width = length;
                childRect.Height = finalSize.Height;

                // 布局子元素
                child.Arrange(childRect);

                // 更新下一个元素的位置
                childRect.X += length + Spacing;
                childRect.X = Math.Min(childRect.X, finalSize.Width);
            }
            else
            {
                // 垂直方向布局
                if (weight.IsStar)
                {
                    // Star尺寸
                    length = weight.Value * starInfo.StarUnitLength;
                }
                else
                {
                    // 固定或自动尺寸
                    double remainingHeight = finalSize.Height - childRect.Y;
                    length = remainingHeight <= 0 ? 0 : Math.Min(remainingHeight, child.DesiredSize.Height);
                }

                // 设置子元素尺寸
                childRect.Height = length;
                childRect.Width = finalSize.Width;

                // 布局子元素
                child.Arrange(childRect);

                // 更新下一个元素的位置
                childRect.Y += length + Spacing;
                childRect.Y = Math.Min(childRect.Y, finalSize.Height);
            }
        }

        return finalSize;
    }

    #endregion 布局重写
}