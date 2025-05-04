using System.Windows.Controls;

using System.Windows;


namespace Cyclone.Wpf.Controls;
/// <summary>
/// 色相滑块控件，用于选择HSV颜色空间中的色相值
/// </summary>
public class ColorHueSlider : Slider
{
    static ColorHueSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorHueSlider),
            new FrameworkPropertyMetadata(typeof(ColorHueSlider)));

        // 设置Slider的默认属性
        MinimumProperty.OverrideMetadata(typeof(ColorHueSlider),
            new FrameworkPropertyMetadata(0.0));

        MaximumProperty.OverrideMetadata(typeof(ColorHueSlider),
            new FrameworkPropertyMetadata(360.0));

        ValueProperty.OverrideMetadata(typeof(ColorHueSlider),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        SmallChangeProperty.OverrideMetadata(typeof(ColorHueSlider),
            new FrameworkPropertyMetadata(1.0));

        LargeChangeProperty.OverrideMetadata(typeof(ColorHueSlider),
            new FrameworkPropertyMetadata(30.0));
    }


}