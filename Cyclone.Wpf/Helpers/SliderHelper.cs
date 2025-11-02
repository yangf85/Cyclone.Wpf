using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Helpers;

public static class SliderHelper
{
    #region IsEnabledMagneticSnap 附加属性

    //磁性吸附功能
    public static readonly DependencyProperty IsEnabledMagneticSnapProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabledMagneticSnap",
            typeof(bool),
            typeof(SliderHelper),
            new PropertyMetadata(false, OnIsEnabledMagneticSnapChanged));

    public static bool GetIsEnabledMagneticSnap(DependencyObject obj)
    {
        return (bool)obj.GetValue(IsEnabledMagneticSnapProperty);
    }

    public static void SetIsEnabledMagneticSnap(DependencyObject obj, bool value)
    {
        obj.SetValue(IsEnabledMagneticSnapProperty, value);
    }

    private static void OnIsEnabledMagneticSnapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Slider slider)
        {
            slider.ValueChanged -= Slider_ValueChanged;

            if ((bool)e.NewValue)
            {
                slider.ValueChanged += Slider_ValueChanged;
            }
        }
    }

    #endregion IsEnabledMagneticSnap 附加属性

    #region SnapThreshold 附加属性

    public static readonly DependencyProperty SnapThresholdProperty =
        DependencyProperty.RegisterAttached(
            "SnapThreshold",
            typeof(double),
            typeof(SliderHelper),
            new PropertyMetadata(5.0));

    public static double GetSnapThreshold(DependencyObject obj)
    {
        return (double)obj.GetValue(SnapThresholdProperty);
    }

    public static void SetSnapThreshold(DependencyObject obj, double value)
    {
        obj.SetValue(SnapThresholdProperty, value);
    }

    #endregion SnapThreshold 附加属性

    private static readonly Dictionary<Slider, bool> _snappingFlags = [];

    private static void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var slider = (Slider)sender;

        // 防止递归
        if (_snappingFlags.ContainsKey(slider) && _snappingFlags[slider])
        {
            return;
        }

        if (slider.Ticks == null || slider.Ticks.Count == 0)
        {
            return;
        }

        var threshold = GetSnapThreshold(slider);
        var currentValue = e.NewValue;

        // 找到最近的刻度
        var nearestTick = slider.Ticks.OrderBy(t => Math.Abs(t - currentValue)).First();
        var distance = Math.Abs(currentValue - nearestTick);

        // 如果在吸附范围内，则吸附
        if (distance <= threshold)
        {
            _snappingFlags[slider] = true;
            slider.Value = nearestTick;
            _snappingFlags[slider] = false;
        }
    }
}