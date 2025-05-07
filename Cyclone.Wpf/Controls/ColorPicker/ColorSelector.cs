using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 颜色选择器控件，整合多种颜色选择方式
/// </summary>
[TemplatePart(Name = "PART_ColorRegion", Type = typeof(ColorRegion))]
[TemplatePart(Name = "PART_HueSlider", Type = typeof(Slider))]
[TemplatePart(Name = "PART_OpacitySlider", Type = typeof(Slider))]
[TemplatePart(Name = "PART_BrightSlider", Type = typeof(Slider))]
[TemplatePart(Name = "PART_SaturationSlider", Type = typeof(Slider))]
[TemplatePart(Name = "PART_HexTextBox", Type = typeof(ColorTextBox))]
[TemplatePart(Name = "PART_RgbTextBox", Type = typeof(ColorTextBox))]
[TemplatePart(Name = "PART_Eyedropper", Type = typeof(ColorEyedropper))]
[TemplatePart(Name = "PART_PresetColors", Type = typeof(ColorPalette))]
[TemplatePart(Name = "PART_HistoryColors", Type = typeof(ColorPalette))]
[TemplatePart(Name = "PART_OpacityGradient", Type = typeof(GradientStop))]
[TemplatePart(Name = "PART_BrightnessGradient", Type = typeof(GradientStop))]
[TemplatePart(Name = "PART_SaturationGradient", Type = typeof(GradientStop))]
public class ColorSelector : Control, INotifyPropertyChanged
{
    #region 私有字段

    private ColorRegion _colorRegion;
    private Slider _hueSlider;
    private Slider _opacitySlider;
    private Slider _brightSlider;
    private Slider _saturationSlider;
    private ColorTextBox _hexTextBox;
    private ColorTextBox _rgbTextBox;
    private ColorEyedropper _eyedropper;
    private ColorPalette _presetColors;
    private ColorPalette _historyColors;

    // 滑块渐变对象
    private GradientStop _opacityGradient;

    private GradientStop _brightnessGradient;
    private GradientStop _saturationGradient;

    private bool _isUpdatingControls;
    private ObservableCollection<ColorRepresentation> _colorHistory;

    private const int MaxHistoryCount = 27; // 历史记录最大数量

    #endregion 私有字段

    #region INotifyPropertyChanged 实现

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion INotifyPropertyChanged 实现

    #region 构造函数

    static ColorSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorSelector),
            new FrameworkPropertyMetadata(typeof(ColorSelector)));
    }

    public ColorSelector()
    {
        _colorHistory = new ObservableCollection<ColorRepresentation>();
        HistoryColors = _colorHistory;
    }

    #endregion 构造函数

    #region 依赖属性

    #region ColorPreviewLabel

    public string ColorPreviewLabel
    {
        get => (string)GetValue(ColorPreviewLabelProperty);
        set => SetValue(ColorPreviewLabelProperty, value);
    }

    public static readonly DependencyProperty ColorPreviewLabelProperty =
        DependencyProperty.Register(nameof(ColorPreviewLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("颜色预览"));

    #endregion ColorPreviewLabel

    #region HexInputLabel

    public string HexInputLabel
    {
        get => (string)GetValue(HexInputLabelProperty);
        set => SetValue(HexInputLabelProperty, value);
    }

    public static readonly DependencyProperty HexInputLabelProperty =
        DependencyProperty.Register(nameof(HexInputLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("Hex文本显示"));

    #endregion HexInputLabel

    #region RgbInputLabel

    public string RgbInputLabel
    {
        get => (string)GetValue(RgbInputLabelProperty);
        set => SetValue(RgbInputLabelProperty, value);
    }

    public static readonly DependencyProperty RgbInputLabelProperty =
        DependencyProperty.Register(nameof(RgbInputLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("Rgb文本显示"));

    #endregion RgbInputLabel

    #region PresetColorLabel

    public string PresetColorLabel
    {
        get => (string)GetValue(PresetColorLabelProperty);
        set => SetValue(PresetColorLabelProperty, value);
    }

    public static readonly DependencyProperty PresetColorLabelProperty =
        DependencyProperty.Register(nameof(PresetColorLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("预定义颜色"));

    #endregion PresetColorLabel

    #region HistoryColorLabel

    public string HistoryColorLabel
    {
        get => (string)GetValue(HistoryColorLabelProperty);
        set => SetValue(HistoryColorLabelProperty, value);
    }

    public static readonly DependencyProperty HistoryColorLabelProperty =
        DependencyProperty.Register(nameof(HistoryColorLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("历史记录颜色"));

    #endregion HistoryColorLabel

    #region HueLabel

    public string HueLabel
    {
        get => (string)GetValue(HueLabelProperty);
        set => SetValue(HueLabelProperty, value);
    }

    public static readonly DependencyProperty HueLabelProperty =
        DependencyProperty.Register(nameof(HueLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("Hue"));

    #endregion HueLabel

    #region AlphaLabel

    public string AlphaLabel
    {
        get => (string)GetValue(AlphaLabelProperty);
        set => SetValue(AlphaLabelProperty, value);
    }

    public static readonly DependencyProperty AlphaLabelProperty =
        DependencyProperty.Register(nameof(AlphaLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("透明度"));

    #endregion AlphaLabel

    #region BrightnessLabel

    public string BrightnessLabel
    {
        get => (string)GetValue(BrightnessLabelProperty);
        set => SetValue(BrightnessLabelProperty, value);
    }

    public static readonly DependencyProperty BrightnessLabelProperty =
        DependencyProperty.Register(nameof(BrightnessLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("亮度"));

    #endregion BrightnessLabel

    #region SaturationLabel

    public string SaturationLabel
    {
        get => (string)GetValue(SaturationLabelProperty);
        set => SetValue(SaturationLabelProperty, value);
    }

    public static readonly DependencyProperty SaturationLabelProperty =
        DependencyProperty.Register(nameof(SaturationLabel), typeof(string), typeof(ColorSelector), new PropertyMetadata("饱和度"));

    #endregion SaturationLabel

    #region SelectedColor - 当前选中的颜色

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorSelector),
            new FrameworkPropertyMetadata(Colors.Red,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged));

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSelector selector)
        {
            selector.OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
        }
    }

    #endregion SelectedColor - 当前选中的颜色

    #region AlphaValue - 透明度值(0.0-1.0)

    public double AlphaValue
    {
        get => (double)GetValue(AlphaValueProperty);
        set => SetValue(AlphaValueProperty, value);
    }

    public static readonly DependencyProperty AlphaValueProperty =
        DependencyProperty.Register(nameof(AlphaValue), typeof(double), typeof(ColorSelector),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnAlphaValueChanged));

    private static void OnAlphaValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSelector selector)
        {
            selector.OnAlphaValueChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    #endregion AlphaValue - 透明度值(0.0-1.0)

    #region Hue - 色相值

    public double Hue
    {
        get => (double)GetValue(HueProperty);
        set => SetValue(HueProperty, value);
    }

    public static readonly DependencyProperty HueProperty =
        DependencyProperty.Register(nameof(Hue), typeof(double), typeof(ColorSelector),
            new FrameworkPropertyMetadata(0.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnHueChanged));

    private static void OnHueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSelector selector)
        {
            selector.OnHueChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    #endregion Hue - 色相值

    #region Saturation - 饱和度值

    public double Saturation
    {
        get => (double)GetValue(SaturationProperty);
        set => SetValue(SaturationProperty, value);
    }

    public static readonly DependencyProperty SaturationProperty =
        DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(ColorSelector),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSaturationChanged));

    private static void OnSaturationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSelector selector)
        {
            selector.OnSaturationChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    #endregion Saturation - 饱和度值

    #region Value - 明度值

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ColorSelector),
            new FrameworkPropertyMetadata(1.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged));

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorSelector selector)
        {
            selector.OnValueChanged((double)e.OldValue, (double)e.NewValue);
        }
    }

    #endregion Value - 明度值

    #region HistoryColors - 颜色历史记录

    public ObservableCollection<ColorRepresentation> HistoryColors
    {
        get => (ObservableCollection<ColorRepresentation>)GetValue(HistoryColorsProperty);
        private set => SetValue(HistoryColorsPropertyKey, value);
    }

    private static readonly DependencyPropertyKey HistoryColorsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HistoryColors),
            typeof(ObservableCollection<ColorRepresentation>),
            typeof(ColorSelector),
            new PropertyMetadata(null));

    public static readonly DependencyProperty HistoryColorsProperty =
        HistoryColorsPropertyKey.DependencyProperty;

    #endregion HistoryColors - 颜色历史记录

    #endregion 依赖属性

    #region 路由事件

    public static readonly RoutedEvent ColorChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ColorChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ColorSelector));

    public event RoutedEventHandler ColorChanged
    {
        add { AddHandler(ColorChangedEvent, value); }
        remove { RemoveHandler(ColorChangedEvent, value); }
    }

    #endregion 路由事件

    #region 重写方法

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解除旧控件事件
        UnregisterEvents();

        // 获取新控件
        _colorRegion = GetTemplateChild("PART_ColorRegion") as ColorRegion;
        _hueSlider = GetTemplateChild("PART_HueSlider") as Slider;
        _opacitySlider = GetTemplateChild("PART_OpacitySlider") as Slider;
        _brightSlider = GetTemplateChild("PART_BrightSlider") as Slider;
        _saturationSlider = GetTemplateChild("PART_SaturationSlider") as Slider;
        _hexTextBox = GetTemplateChild("PART_HexTextBox") as ColorTextBox;
        _rgbTextBox = GetTemplateChild("PART_RgbTextBox") as ColorTextBox;
        _eyedropper = GetTemplateChild("PART_Eyedropper") as ColorEyedropper;
        _presetColors = GetTemplateChild("PART_PresetColors") as ColorPalette;
        _historyColors = GetTemplateChild("PART_HistoryColors") as ColorPalette;

        // 获取渐变对象
        if (_opacitySlider != null)
        {
            _opacityGradient = GetGradientStopFromSlider(_opacitySlider, "PART_OpacityGradient");
        }

        if (_brightSlider != null)
        {
            _brightnessGradient = GetGradientStopFromSlider(_brightSlider, "PART_BrightnessGradient");
        }

        if (_saturationSlider != null)
        {
            _saturationGradient = GetGradientStopFromSlider(_saturationSlider, "PART_SaturationGradient");
        }

        // 注册新控件事件
        RegisterEvents();

        // 初始化控件值
        UpdateControlsFromColor(SelectedColor);
    }

    #endregion 重写方法

    #region 私有方法

    // 从Slider中查找GradientStop对象
    private GradientStop GetGradientStopFromSlider(Slider slider, string partName)
    {
        if (slider == null)
            return null;

        // 等待应用模板
        slider.ApplyTemplate();

        // 查找渐变对象
        return slider.Template.FindName(partName, slider) as GradientStop;
    }

    private void RegisterEvents()
    {
        if (_colorRegion != null)
        {
            _colorRegion.ColorChanged += ColorRegion_ColorChanged;
        }

        if (_hueSlider != null)
        {
            _hueSlider.ValueChanged += HueSlider_ValueChanged;
        }

        if (_opacitySlider != null)
        {
            _opacitySlider.ValueChanged += OpacitySlider_ValueChanged;
        }

        if (_brightSlider != null)
        {
            _brightSlider.ValueChanged += BrightSlider_ValueChanged;
        }

        if (_saturationSlider != null)
        {
            _saturationSlider.ValueChanged += SaturationSlider_ValueChanged;
        }

        if (_hexTextBox != null)
        {
            _hexTextBox.TextMode = ColorTextMode.HEX;
            _hexTextBox.ColorTextValidated += HexTextBox_ColorTextValidated;
        }

        if (_rgbTextBox != null)
        {
            _rgbTextBox.TextMode = ColorTextMode.RGB;
            _rgbTextBox.ColorTextValidated += RgbTextBox_ColorTextValidated;
        }

        if (_eyedropper != null)
        {
            _eyedropper.ColorPicked += Eyedropper_ColorPicked;
        }

        if (_presetColors != null)
        {
            _presetColors.Mode = ColorPaletteMode.Preset;
            _presetColors.SelectionChanged += PresetColors_SelectionChanged;
        }

        if (_historyColors != null)
        {
            _historyColors.Mode = ColorPaletteMode.Custom;
            _historyColors.SelectionChanged += HistoryColors_SelectionChanged;
        }
    }

    private void UnregisterEvents()
    {
        if (_colorRegion != null)
        {
            _colorRegion.ColorChanged -= ColorRegion_ColorChanged;
        }

        if (_hueSlider != null)
        {
            _hueSlider.ValueChanged -= HueSlider_ValueChanged;
        }

        if (_opacitySlider != null)
        {
            _opacitySlider.ValueChanged -= OpacitySlider_ValueChanged;
        }

        if (_brightSlider != null)
        {
            _brightSlider.ValueChanged -= BrightSlider_ValueChanged;
        }

        if (_saturationSlider != null)
        {
            _saturationSlider.ValueChanged -= SaturationSlider_ValueChanged;
        }

        if (_hexTextBox != null)
        {
            _hexTextBox.ColorTextValidated -= HexTextBox_ColorTextValidated;
        }

        if (_rgbTextBox != null)
        {
            _rgbTextBox.ColorTextValidated -= RgbTextBox_ColorTextValidated;
        }

        if (_eyedropper != null)
        {
            _eyedropper.ColorPicked -= Eyedropper_ColorPicked;
        }

        if (_presetColors != null)
        {
            _presetColors.SelectionChanged -= PresetColors_SelectionChanged;
        }

        if (_historyColors != null)
        {
            _historyColors.SelectionChanged -= HistoryColors_SelectionChanged;
        }
    }

    // 更新所有控件显示
    private void UpdateControlsFromColor(Color color)
    {
        if (_isUpdatingControls)
            return;

        try
        {
            _isUpdatingControls = true;

            // 解析颜色到HSV
            ColorHelper.RgbToHsv(color.R, color.G, color.B, out double h, out double s, out double v);

            // 更新属性值
            AlphaValue = color.A / 255.0;
            Hue = h;
            Saturation = s;
            Value = v;

            // 更新区域选择器
            if (_colorRegion != null)
            {
                _colorRegion.Hue = h;
                _colorRegion.Saturation = s;
                _colorRegion.Value = v;
            }

            // 更新滑块
            if (_hueSlider != null)
            {
                _hueSlider.Value = h;
            }

            // 更新透明度滑块
            if (_opacitySlider != null)
            {
                _opacitySlider.Value = AlphaValue;

                // 设置带透明度的指示颜色
                Color fullColor = Color.FromArgb(255, color.R, color.G, color.B);

                // 更新渐变色
                if (_opacityGradient != null)
                {
                    _opacityGradient.Color = fullColor;
                }
            }

            // 更新亮度滑块
            if (_brightSlider != null)
            {
                _brightSlider.Value = v;

                // 计算指示颜色 - 使用当前色相和饱和度
                ColorHelper.HsvToRgb(h, s, 0.5, out byte r, out byte g, out byte b);
                Color indicatorColor = Color.FromRgb(r, g, b);

                // 更新渐变色
                if (_brightnessGradient != null)
                {
                    _brightnessGradient.Color = indicatorColor;
                }
            }

            // 更新饱和度滑块
            if (_saturationSlider != null)
            {
                _saturationSlider.Value = s;

                // 计算指示颜色 - 使用当前色相和最大饱和度
                ColorHelper.HsvToRgb(h, 1, v, out byte r, out byte g, out byte b);
                Color indicatorColor = Color.FromRgb(r, g, b);

                // 更新渐变色
                if (_saturationGradient != null)
                {
                    _saturationGradient.Color = indicatorColor;
                }
            }

            // 更新文本框
            if (_hexTextBox != null)
            {
                _hexTextBox.Color = color;
            }

            if (_rgbTextBox != null)
            {
                _rgbTextBox.Color = color;
            }

            // 更新取色器
            if (_eyedropper != null)
            {
                _eyedropper.SelectedColor = color;
            }
        }
        finally
        {
            _isUpdatingControls = false;
        }
    }

    // 更新颜色历史记录
    private void AddColorToHistory(Color color)
    {
        // 检查是否已存在相同颜色
        bool exists = false;
        foreach (var item in _colorHistory)
        {
            if (ColorHelper.AreColorsClose(item.Color.Value, color))
            {
                exists = true;
                break;
            }
        }

        // 如果不存在，添加到历史记录
        if (!exists)
        {
            // 如果达到最大数量，移除最旧的
            if (_colorHistory.Count >= MaxHistoryCount)
            {
                _colorHistory.RemoveAt(_colorHistory.Count - 1);
            }

            // 添加到历史记录最前面
            _colorHistory.Insert(0, new ColorRepresentation(color));
        }
    }

    #endregion 私有方法

    #region 事件处理

    private void OnSelectedColorChanged(Color oldColor, Color newColor)
    {
        if (!_isUpdatingControls)
        {
            UpdateControlsFromColor(newColor);

            // 添加到历史记录
            AddColorToHistory(newColor);

            // 触发事件
            RaiseEvent(new RoutedEventArgs(ColorChangedEvent, this));
        }
    }

    private void OnAlphaValueChanged(double oldValue, double newValue)
    {
        if (!_isUpdatingControls)
        {
            byte alpha = (byte)(newValue * 255);
            Color newColor = Color.FromArgb(alpha, SelectedColor.R, SelectedColor.G, SelectedColor.B);
            SelectedColor = newColor;
        }
    }

    private void OnHueChanged(double oldValue, double newValue)
    {
        if (!_isUpdatingControls)
        {
            // 构建新颜色并更新
            ColorHelper.HsvToRgb(newValue, Saturation, Value, out byte r, out byte g, out byte b);
            byte alpha = (byte)(AlphaValue * 255);
            SelectedColor = Color.FromArgb(alpha, r, g, b);
        }
    }

    private void OnSaturationChanged(double oldValue, double newValue)
    {
        if (!_isUpdatingControls)
        {
            // 构建新颜色并更新
            ColorHelper.HsvToRgb(Hue, newValue, Value, out byte r, out byte g, out byte b);
            byte alpha = (byte)(AlphaValue * 255);
            SelectedColor = Color.FromArgb(alpha, r, g, b);
        }
    }

    private void OnValueChanged(double oldValue, double newValue)
    {
        if (!_isUpdatingControls)
        {
            // 构建新颜色并更新
            ColorHelper.HsvToRgb(Hue, Saturation, newValue, out byte r, out byte g, out byte b);
            byte alpha = (byte)(AlphaValue * 255);
            SelectedColor = Color.FromArgb(alpha, r, g, b);
        }
    }

    private void ColorRegion_ColorChanged(object sender, ColorChangedEventArgs e)
    {
        if (!_isUpdatingControls)
        {
            SelectedColor = e.NewColor;
        }
    }

    private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isUpdatingControls)
        {
            Hue = e.NewValue;
        }
    }

    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isUpdatingControls)
        {
            AlphaValue = e.NewValue;
        }
    }

    private void BrightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isUpdatingControls)
        {
            Value = e.NewValue;
        }
    }

    private void SaturationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_isUpdatingControls)
        {
            Saturation = e.NewValue;
        }
    }

    private void HexTextBox_ColorTextValidated(object sender, RoutedEventArgs e)
    {
        if (!_isUpdatingControls && _hexTextBox != null)
        {
            // 保持当前Alpha值
            byte alpha = (byte)(AlphaValue * 255);
            Color newColor = Color.FromArgb(alpha, _hexTextBox.Color.R, _hexTextBox.Color.G, _hexTextBox.Color.B);
            SelectedColor = newColor;
        }
    }

    private void RgbTextBox_ColorTextValidated(object sender, RoutedEventArgs e)
    {
        if (!_isUpdatingControls && _rgbTextBox != null)
        {
            // 保持当前Alpha值
            byte alpha = (byte)(AlphaValue * 255);
            Color newColor = Color.FromArgb(alpha, _rgbTextBox.Color.R, _rgbTextBox.Color.G, _rgbTextBox.Color.B);
            SelectedColor = newColor;
        }
    }

    private void Eyedropper_ColorPicked(object sender, RoutedEventArgs e)
    {
        if (!_isUpdatingControls && _eyedropper != null)
        {
            SelectedColor = _eyedropper.SelectedColor;
        }
    }

    private void PresetColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdatingControls && _presetColors != null && _presetColors.SelectedItem is IColorRepresentation colorItem && colorItem.Color.HasValue)
        {
            // 保持当前Alpha值
            byte alpha = (byte)(AlphaValue * 255);
            Color newColor = Color.FromArgb(alpha, colorItem.Color.Value.R, colorItem.Color.Value.G, colorItem.Color.Value.B);
            SelectedColor = newColor;
        }
    }

    private void HistoryColors_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdatingControls && _historyColors != null && _historyColors.SelectedItem is IColorRepresentation colorItem && colorItem.Color.HasValue)
        {
            // 应用历史颜色，包括其Alpha值
            SelectedColor = colorItem.Color.Value;
        }
    }

    #endregion 事件处理
}