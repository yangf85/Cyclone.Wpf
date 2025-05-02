using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 透明度滑块控件
/// </summary>
public class ColorOpacitySlider : Slider
{
    static ColorOpacitySlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorOpacitySlider), new FrameworkPropertyMetadata(typeof(ColorOpacitySlider)));

        MinimumProperty.OverrideMetadata(typeof(ColorOpacitySlider), new FrameworkPropertyMetadata(0.0));

        MaximumProperty.OverrideMetadata(typeof(ColorOpacitySlider), new FrameworkPropertyMetadata(1.0));

        ValueProperty.OverrideMetadata(typeof(ColorOpacitySlider), new FrameworkPropertyMetadata(0.5));

        SmallChangeProperty.OverrideMetadata(typeof(ColorOpacitySlider), new FrameworkPropertyMetadata(0.5));

        LargeChangeProperty.OverrideMetadata(typeof(ColorOpacitySlider), new FrameworkPropertyMetadata(0.02));
    }

    #region IndicatorColor

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public static readonly DependencyProperty IndicatorColorProperty =
        DependencyProperty.Register(nameof(IndicatorColor), typeof(Color), typeof(ColorOpacitySlider),
        new PropertyMetadata(Colors.Black));

    #endregion IndicatorColor
}