using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 提供循环滚动功能的附加属性
/// </summary>
public static class CyclicScrolling
{
    #region 启用循环滚动

    /// <summary>
    /// 获取是否启用循环滚动
    /// </summary>
    public static bool GetIsEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledProperty);
    }

    /// <summary>
    /// 设置是否启用循环滚动
    /// </summary>
    public static void SetIsEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledProperty, value);
    }

    /// <summary>
    /// 是否启用循环滚动的依赖属性
    /// </summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(CyclicScrolling),
            new PropertyMetadata(false, OnIsEnabledChanged));

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
        {
            bool isEnabled = (bool)e.NewValue;

            // 移除之前的事件处理器
            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;

            if (isEnabled)
            {
                // 启用循环滚动
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;

                // 设置可以滚动的标志
                SetCanScrollVertically(scrollViewer, true);
                SetCanScrollHorizontally(scrollViewer, false); // 默认只允许垂直滚动
            }
        }
    }

    #endregion 启用循环滚动

    #region 可垂直滚动

    public static bool GetCanScrollVertically(DependencyObject obj)
    {
        return (bool)obj.GetValue(CanScrollVerticallyProperty);
    }

    public static void SetCanScrollVertically(DependencyObject obj, bool value)
    {
        obj.SetValue(CanScrollVerticallyProperty, value);
    }

    public static readonly DependencyProperty CanScrollVerticallyProperty =
        DependencyProperty.RegisterAttached("CanScrollVertically", typeof(bool), typeof(CyclicScrolling),
            new PropertyMetadata(true));

    #endregion 可垂直滚动

    #region 可水平滚动

    public static bool GetCanScrollHorizontally(DependencyObject obj)
    {
        return (bool)obj.GetValue(CanScrollHorizontallyProperty);
    }

    public static void SetCanScrollHorizontally(DependencyObject obj, bool value)
    {
        obj.SetValue(CanScrollHorizontallyProperty, value);
    }

    public static readonly DependencyProperty CanScrollHorizontallyProperty =
        DependencyProperty.RegisterAttached("CanScrollHorizontally", typeof(bool), typeof(CyclicScrolling),
            new PropertyMetadata(false));

    #endregion 可水平滚动

    #region 可见项目数量

    public static int GetVisibleItemCount(DependencyObject obj)
    {
        return (int)obj.GetValue(VisibleItemCountProperty);
    }

    public static void SetVisibleItemCount(DependencyObject obj, int value)
    {
        obj.SetValue(VisibleItemCountProperty, value);
    }

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.RegisterAttached("VisibleItemCount", typeof(int), typeof(CyclicScrolling),
            new PropertyMetadata(5));

    #endregion 可见项目数量

    #region 事件处理

    private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            if (!GetIsEnabled(scrollViewer))
                return;

            ItemsControl itemsControl = FindItemsControl(scrollViewer);
            if (itemsControl == null || itemsControl.Items.Count == 0)
                return;

            int itemCount = itemsControl.Items.Count;
            double itemHeight = 0;
            double itemWidth = 0;

            // 获取项目尺寸（通过第一个容器）
            if (itemsControl.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated &&
                itemsControl.ItemContainerGenerator.ContainerFromIndex(0) is FrameworkElement firstContainer)
            {
                itemHeight = firstContainer.ActualHeight;
                itemWidth = firstContainer.ActualWidth;
            }
            else
            {
                // 如果容器尚未生成，使用估计值
                if (GetCanScrollVertically(scrollViewer))
                {
                    itemHeight = scrollViewer.ExtentHeight / itemCount;
                }

                if (GetCanScrollHorizontally(scrollViewer))
                {
                    itemWidth = scrollViewer.ExtentWidth / itemCount;
                }
            }

            // 处理垂直滚动
            if (GetCanScrollVertically(scrollViewer) && itemHeight > 0)
            {
                double totalHeight = itemHeight * itemCount;

                // 滚动到底部时，跳转到顶部
                if (scrollViewer.VerticalOffset >= scrollViewer.ScrollableHeight)
                {
                    // 设置一个微小的偏移，以确保不是完全在顶部（避免触发另一个滚动事件）
                    scrollViewer.ScrollToVerticalOffset(0.1);
                }
                // 滚动到顶部时，跳转到底部
                else if (scrollViewer.VerticalOffset <= 0)
                {
                    // 设置接近最大值但不是完全等于最大值的偏移
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight - 0.1);
                }
            }

            // 处理水平滚动
            if (GetCanScrollHorizontally(scrollViewer) && itemWidth > 0)
            {
                double totalWidth = itemWidth * itemCount;

                // 滚动到最右侧时，跳转到最左侧
                if (scrollViewer.HorizontalOffset >= scrollViewer.ScrollableWidth)
                {
                    scrollViewer.ScrollToHorizontalOffset(0.1);
                }
                // 滚动到最左侧时，跳转到最右侧
                else if (scrollViewer.HorizontalOffset <= 0)
                {
                    scrollViewer.ScrollToHorizontalOffset(scrollViewer.ScrollableWidth - 0.1);
                }
            }
        }
    }

    /// <summary>
    /// 查找包含ScrollViewer的ItemsControl
    /// </summary>
    private static ItemsControl FindItemsControl(ScrollViewer scrollViewer)
    {
        DependencyObject parent = scrollViewer;

        // 向上查找直到找到ItemsControl或到达视觉树的顶部
        while (parent != null && !(parent is ItemsControl))
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return parent as ItemsControl;
    }

    #endregion 事件处理
}