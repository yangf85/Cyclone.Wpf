using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 亮度滑块控件
/// </summary>
public class ColorBrightSlider : Slider
{
    static ColorBrightSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorBrightSlider), new FrameworkPropertyMetadata(typeof(ColorBrightSlider)));

        MinimumProperty.OverrideMetadata(typeof(ColorBrightSlider), new FrameworkPropertyMetadata(0.0));

        MaximumProperty.OverrideMetadata(typeof(ColorBrightSlider), new FrameworkPropertyMetadata(1.0));

        ValueProperty.OverrideMetadata(typeof(ColorBrightSlider), new FrameworkPropertyMetadata(0.5));

        SmallChangeProperty.OverrideMetadata(typeof(ColorBrightSlider), new FrameworkPropertyMetadata(0.01));

        LargeChangeProperty.OverrideMetadata(typeof(ColorBrightSlider), new FrameworkPropertyMetadata(0.1));
    }

    #region IndicatorColor

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public static readonly DependencyProperty IndicatorColorProperty =
        DependencyProperty.Register(nameof(IndicatorColor), typeof(Color), typeof(ColorBrightSlider),
        new PropertyMetadata(Colors.Red));

    #endregion IndicatorColor
}