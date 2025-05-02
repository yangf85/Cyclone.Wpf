using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 饱和度滑块控件
/// </summary>
public class ColorSaturationSlider : Slider
{
    static ColorSaturationSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSaturationSlider), new FrameworkPropertyMetadata(typeof(ColorSaturationSlider)));

        MinimumProperty.OverrideMetadata(typeof(ColorSaturationSlider), new FrameworkPropertyMetadata(0.0));

        MaximumProperty.OverrideMetadata(typeof(ColorSaturationSlider), new FrameworkPropertyMetadata(1.0));

        ValueProperty.OverrideMetadata(typeof(ColorSaturationSlider), new FrameworkPropertyMetadata(0.5));

        SmallChangeProperty.OverrideMetadata(typeof(ColorSaturationSlider), new FrameworkPropertyMetadata(0.01));

        LargeChangeProperty.OverrideMetadata(typeof(ColorSaturationSlider), new FrameworkPropertyMetadata(0.1));
    }

    #region IndicatorColor

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public static readonly DependencyProperty IndicatorColorProperty =
        DependencyProperty.Register(nameof(IndicatorColor), typeof(Color), typeof(ColorSaturationSlider),
        new PropertyMetadata(Colors.Red, OnIndicatorColorChanged));

    private static void OnIndicatorColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSaturationSlider slider)
        {
            Color newColor = (Color)e.NewValue;
            slider.DesaturatedColor = ColorHelper.GetDesaturatedColor(newColor);
        }
    }

    #endregion IndicatorColor

    #region DesaturatedColor

    public Color DesaturatedColor
    {
        get => (Color)GetValue(DesaturatedColorProperty);
        set => SetValue(DesaturatedColorProperty, value);
    }

    public static readonly DependencyProperty DesaturatedColorProperty =
        DependencyProperty.Register(nameof(DesaturatedColor), typeof(Color), typeof(ColorSaturationSlider),
        new PropertyMetadata(Colors.Gray));

    #endregion DesaturatedColor
}