using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 提供 ScrollViewer 的辅助方法和附加属性
/// </summary>
public static class ScrollViewerHelper
{
    #region HorizontalOffset 附加属性

    /// <summary>
    /// HorizontalOffset 附加属性，用于在动画中控制 ScrollViewer 的水平滚动位置
    /// </summary>
    public static readonly DependencyProperty HorizontalOffsetProperty =
        DependencyProperty.RegisterAttached(
            "HorizontalOffset",
            typeof(double),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(0.0, OnHorizontalOffsetChanged));

    /// <summary>
    /// 获取 HorizontalOffset 附加属性的值
    /// </summary>
    /// <param name="obj">依赖对象</param>
    /// <returns>属性值</returns>
    public static double GetHorizontalOffset(DependencyObject obj)
    {
        return (double)obj.GetValue(HorizontalOffsetProperty);
    }

    /// <summary>
    /// 设置 HorizontalOffset 附加属性的值
    /// </summary>
    /// <param name="obj">依赖对象</param>
    /// <param name="value">属性值</param>
    public static void SetHorizontalOffset(DependencyObject obj, double value)
    {
        obj.SetValue(HorizontalOffsetProperty, value);
    }

    /// <summary>
    /// 当 HorizontalOffset 属性值变化时的回调
    /// </summary>
    private static void OnHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToHorizontalOffset((double)e.NewValue);
        }
    }

    #endregion HorizontalOffset 附加属性

    #region VerticalOffset 附加属性

    /// <summary>
    /// VerticalOffset 附加属性，用于在动画中控制 ScrollViewer 的垂直滚动位置
    /// </summary>
    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached(
            "VerticalOffset",
            typeof(double),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

    /// <summary>
    /// 获取 VerticalOffset 附加属性的值
    /// </summary>
    /// <param name="obj">依赖对象</param>
    /// <returns>属性值</returns>
    public static double GetVerticalOffset(DependencyObject obj)
    {
        return (double)obj.GetValue(VerticalOffsetProperty);
    }

    /// <summary>
    /// 设置 VerticalOffset 附加属性的值
    /// </summary>
    /// <param name="obj">依赖对象</param>
    /// <param name="value">属性值</param>
    public static void SetVerticalOffset(DependencyObject obj, double value)
    {
        obj.SetValue(VerticalOffsetProperty, value);
    }

    /// <summary>
    /// 当 VerticalOffset 属性值变化时的回调
    /// </summary>
    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
        }
    }

    #endregion VerticalOffset 附加属性

    #region 辅助方法

    /// <summary>
    /// 滚动到指定的水平位置（带动画效果）
    /// </summary>
    /// <param name="scrollViewer">ScrollViewer 控件</param>
    /// <param name="offset">滚动位置</param>
    public static void AnimateHorizontalOffset(this ScrollViewer scrollViewer, double offset)
    {
        if (scrollViewer == null) return;

        double current = scrollViewer.HorizontalOffset;
        SetHorizontalOffset(scrollViewer, offset);
    }

    /// <summary>
    /// 滚动到指定的垂直位置（带动画效果）
    /// </summary>
    /// <param name="scrollViewer">ScrollViewer 控件</param>
    /// <param name="offset">滚动位置</param>
    public static void AnimateVerticalOffset(this ScrollViewer scrollViewer, double offset)
    {
        if (scrollViewer == null) return;

        double current = scrollViewer.VerticalOffset;
        SetVerticalOffset(scrollViewer, offset);
    }

    #endregion 辅助方法
}