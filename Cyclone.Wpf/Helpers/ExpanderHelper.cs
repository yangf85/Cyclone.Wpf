using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows;

namespace Cyclone.Wpf.Helpers;

public static class ExpanderHelper
{
    #region AnimationDuration 附加属性

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.RegisterAttached(
            "AnimationDuration",
            typeof(TimeSpan),
            typeof(ExpanderHelper),
            new PropertyMetadata(TimeSpan.FromMilliseconds(200)));

    public static TimeSpan GetAnimationDuration(DependencyObject obj)
    {
        return (TimeSpan)obj.GetValue(AnimationDurationProperty);
    }

    public static void SetAnimationDuration(DependencyObject obj, TimeSpan value)
    {
        obj.SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration 附加属性

    #region IsAnimationEnabled 附加属性

    public static readonly DependencyProperty IsAnimationEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsAnimationEnabled",
            typeof(bool),
            typeof(ExpanderHelper),
            new PropertyMetadata(false, OnIsAnimationEnabledChanged));

    public static bool GetIsAnimationEnabled(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsAnimationEnabledProperty);
    }

    public static void SetIsAnimationEnabled(DependencyObject obj, bool value)
    {
        obj.SetValue(IsAnimationEnabledProperty, value);
    }

    private static void OnIsAnimationEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var expander = d as Expander;
        if (expander == null)
            return;

        if ((bool)e.NewValue)
        {
            // 添加展开状态改变的事件处理
            expander.Expanded += Expander_Expanded;
            expander.Collapsed += Expander_Collapsed;
        }
        else
        {
            // 移除事件处理
            expander.Expanded -= Expander_Expanded;
            expander.Collapsed -= Expander_Collapsed;
        }
    }

    #endregion IsAnimationEnabled 附加属性

    // 处理 Expander 展开的事件
    private static void Expander_Expanded(object sender, RoutedEventArgs e)
    {
        var expander = sender as Expander;
        if (expander?.Content is FrameworkElement content)
        {
            // 确保 Content 可见
            content.Visibility = Visibility.Visible;

            // 获取动画时长
            var duration = GetAnimationDuration(expander);

            // 存储当前高度以便用于动画
            double originalHeight = content.ActualHeight;

            // 创建并应用高度动画
            content.Height = 0;
            var animation = new DoubleAnimation
            {
                From = 0,
                To = originalHeight,
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.Stop
            };

            animation.Completed += (s, args) =>
            {
                content.ClearValue(FrameworkElement.HeightProperty);
            };

            content.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }
    }

    // 处理 Expander 收缩的事件
    private static void Expander_Collapsed(object sender, RoutedEventArgs e)
    {
        var expander = sender as Expander;
        if (expander?.Content is FrameworkElement content)
        {
            // 获取动画时长
            var duration = GetAnimationDuration(expander);

            // 获取当前高度用于动画
            double originalHeight = content.ActualHeight;

            // 创建并应用高度动画
            var animation = new DoubleAnimation
            {
                From = originalHeight,
                To = 0,
                Duration = new Duration(duration),
                FillBehavior = FillBehavior.HoldEnd
            };

            animation.Completed += (s, args) =>
            {
                content.Visibility = Visibility.Collapsed;
                content.ClearValue(FrameworkElement.HeightProperty);
            };

            content.BeginAnimation(FrameworkElement.HeightProperty, animation);
        }
    }
}