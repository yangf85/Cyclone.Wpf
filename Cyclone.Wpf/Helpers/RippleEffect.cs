// RippleEffect.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 提供水波纹动画效果的附加属性
/// </summary>
public static class RippleEffect
{
    #region IsRippleEnabled

    public static bool GetIsRippleEnabled(DependencyObject obj) => (bool)obj.GetValue(IsRippleEnabledProperty);

    public static void SetIsRippleEnabled(DependencyObject obj, bool value) => obj.SetValue(IsRippleEnabledProperty, value);

    public static readonly DependencyProperty IsRippleEnabledProperty =
                DependencyProperty.RegisterAttached("IsRippleEnabled", typeof(bool), typeof(RippleEffect), new PropertyMetadata(default(bool)));

    #endregion IsRippleEnabled

    #region Color

    public static Brush GetColor(DependencyObject obj) => (Brush)obj.GetValue(ColorProperty);

    public static void SetColor(DependencyObject obj, Brush value) => obj.SetValue(ColorProperty, value);

    public static readonly DependencyProperty ColorProperty =
                DependencyProperty.RegisterAttached("Color", typeof(Brush), typeof(RippleEffect), new PropertyMetadata(default(Brush)));

    #endregion Color

    #region Duration

    public static TimeSpan GetDuration(DependencyObject obj) => (TimeSpan)obj.GetValue(DurationProperty);

    public static void SetDuration(DependencyObject obj, TimeSpan value) => obj.SetValue(DurationProperty, value);

    public static readonly DependencyProperty DurationProperty =
                DependencyProperty.RegisterAttached("Duration", typeof(TimeSpan), typeof(RippleEffect), new PropertyMetadata(default(TimeSpan)));

    #endregion Duration
}