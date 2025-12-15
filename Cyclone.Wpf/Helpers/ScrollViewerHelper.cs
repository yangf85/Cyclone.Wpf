using System.Collections.Specialized;
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

    #region IsAutoScrollToEnd 附加属性

    /// <summary>
    /// IsAutoScrollToEnd 附加属性，用于在集合添加新项时自动滚动到底部
    /// </summary>
    public static readonly DependencyProperty IsAutoScrollToEndProperty =
        DependencyProperty.RegisterAttached(
            "IsAutoScrollToEnd",
            typeof(bool),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(false, OnIsAutoScrollToEndChanged));

    // 用于存储事件处理器的私有附加属性
    private static readonly DependencyProperty CollectionChangedHandlerProperty =
        DependencyProperty.RegisterAttached(
            "CollectionChangedHandler",
            typeof(NotifyCollectionChangedEventHandler),
            typeof(ScrollViewerHelper),
            new PropertyMetadata(null));

    /// <summary>
    /// 获取 IsAutoScrollToEnd 附加属性的值
    /// </summary>
    /// <param name="obj">依赖对象</param>
    /// <returns>属性值</returns>
    public static bool GetIsAutoScrollToEnd(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsAutoScrollToEndProperty);
    }

    /// <summary>
    /// 设置 IsAutoScrollToEnd 附加属性的值
    /// </summary>
    /// <param name="obj">依赖对象</param>
    /// <param name="value">属性值</param>
    public static void SetIsAutoScrollToEnd(DependencyObject obj, bool value)
    {
        obj.SetValue(IsAutoScrollToEndProperty, value);
    }

    /// <summary>
    /// 当 IsAutoScrollToEnd 属性值变化时的回调
    /// </summary>
    private static void OnIsAutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollViewer scrollViewer)
            return;

        bool newValue = (bool)e.NewValue;

        if (newValue)
        {
            scrollViewer.Loaded += ScrollViewer_Loaded;
        }
        else
        {
            scrollViewer.Loaded -= ScrollViewer_Loaded;
            // 清理事件订阅
            ClearCollectionChangedHandler(scrollViewer);
        }
    }

    /// <summary>
    /// ScrollViewer 加载完成时的处理
    /// </summary>
    private static void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;

        // 查找 ItemsControl
        if (scrollViewer.Content is ItemsControl itemsControl)
        {
            if (itemsControl.ItemsSource is INotifyCollectionChanged collection)
            {
                // 先清理旧的事件订阅
                ClearCollectionChangedHandler(scrollViewer);

                // 创建新的事件处理器
                NotifyCollectionChangedEventHandler handler = (s, args) =>
                {
                    if (args.Action == NotifyCollectionChangedAction.Add)
                    {
                        scrollViewer.ScrollToEnd();
                    }
                };

                // 保存事件处理器引用，以便后续清理
                scrollViewer.SetValue(CollectionChangedHandlerProperty, handler);

                // 订阅集合变化事件
                collection.CollectionChanged += handler;
            }
        }
    }

    /// <summary>
    /// 清理集合变化事件的订阅
    /// </summary>
    private static void ClearCollectionChangedHandler(ScrollViewer scrollViewer)
    {
        if (scrollViewer.Content is ItemsControl itemsControl &&
            itemsControl.ItemsSource is INotifyCollectionChanged collection)
        {
            var handler = scrollViewer.GetValue(CollectionChangedHandlerProperty) as NotifyCollectionChangedEventHandler;
            if (handler != null)
            {
                collection.CollectionChanged -= handler;
                scrollViewer.SetValue(CollectionChangedHandlerProperty, null);
            }
        }
    }

    #endregion IsAutoScrollToEnd 附加属性

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