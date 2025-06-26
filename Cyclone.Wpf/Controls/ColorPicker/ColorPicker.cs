using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 颜色选择器控件，提供下拉式的颜色选择功能
/// </summary>
[TemplatePart(Name = "PART_ColorDisplay", Type = typeof(Border))]
[TemplatePart(Name = "PART_OpenButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_ColorSelector", Type = typeof(ColorSelector))]
public class ColorPicker : Control
{
    #region 私有字段

    private Border _colorDisplay;
    private ToggleButton _openButton;
    private Popup _popup;
    private ColorSelector _colorSelector;
    private bool _isUpdatingColor;

    #endregion 私有字段

    #region 构造函数

    static ColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker),
            new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    public ColorPicker()
    {
        // 注册键盘事件
        KeyDown += ColorPicker_KeyDown;
    }

    #endregion 构造函数

    #region 依赖属性

    #region SelectedColor - 当前选中的颜色

    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
            new FrameworkPropertyMetadata(Colors.Red,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorChanged));

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPicker picker)
        {
            picker.OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
        }
    }

    #endregion SelectedColor - 当前选中的颜色

    #region IsOpen - 下拉框是否打开

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ColorPicker),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsOpenChanged));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion IsOpen - 下拉框是否打开

    #region DisplayColorText - 是否显示颜色文本

    public static readonly DependencyProperty DisplayColorTextProperty =
        DependencyProperty.Register(nameof(DisplayColorText), typeof(bool), typeof(ColorPicker),
            new PropertyMetadata(true));

    public bool DisplayColorText
    {
        get => (bool)GetValue(DisplayColorTextProperty);
        set => SetValue(DisplayColorTextProperty, value);
    }

    #endregion DisplayColorText - 是否显示颜色文本

    #region ColorTextFormat - 颜色文本格式

    public static readonly DependencyProperty ColorTextFormatProperty =
        DependencyProperty.Register(nameof(ColorTextFormat), typeof(ColorTextMode), typeof(ColorPicker),
            new PropertyMetadata(ColorTextMode.HEX, OnColorTextFormatChanged));

    public ColorTextMode ColorTextFormat
    {
        get => (ColorTextMode)GetValue(ColorTextFormatProperty);
        set => SetValue(ColorTextFormatProperty, value);
    }

    private static void OnColorTextFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPicker picker)
        {
            picker.UpdateColorText();
        }
    }

    #endregion ColorTextFormat - 颜色文本格式

    #region ColorText - 颜色文本显示

    private static readonly DependencyPropertyKey ColorTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(ColorText), typeof(string), typeof(ColorPicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ColorTextProperty = ColorTextPropertyKey.DependencyProperty;

    public string ColorText
    {
        get => (string)GetValue(ColorTextProperty);
        private set => SetValue(ColorTextPropertyKey, value);
    }

    #endregion ColorText - 颜色文本显示

    #endregion 依赖属性

    #region 路由事件

    public static readonly RoutedEvent SelectedColorChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectedColorChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(ColorPicker));

    public event RoutedEventHandler SelectedColorChanged
    {
        add { AddHandler(SelectedColorChangedEvent, value); }
        remove { RemoveHandler(SelectedColorChangedEvent, value); }
    }

    #endregion 路由事件

    #region 重写方法

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 解除旧控件事件
        UnregisterEvents();

        // 获取新控件
        _colorDisplay = GetTemplateChild("PART_ColorDisplay") as Border;
        _openButton = GetTemplateChild("PART_OpenButton") as ToggleButton;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _colorSelector = GetTemplateChild("PART_ColorSelector") as ColorSelector;

        // 注册新控件事件
        RegisterEvents();

        // 更新显示
        UpdateColorText();
    }

    #endregion 重写方法

    #region 私有方法

    private void RegisterEvents()
    {
        if (_colorSelector != null)
        {
            _colorSelector.ColorChanged += ColorSelector_ColorChanged;
            // 防止点击ColorSelector时Popup关闭
            _colorSelector.MouseDown += ColorSelector_MouseDown;
        }
    }

    private void UnregisterEvents()
    {
        if (_colorSelector != null)
        {
            _colorSelector.ColorChanged -= ColorSelector_ColorChanged;
            _colorSelector.MouseDown -= ColorSelector_MouseDown;
        }
    }

    // 更新颜色文本显示
    private void UpdateColorText()
    {
        // 使用ColorHelper处理颜色文本转换
        ColorText = ColorTextFormat == ColorTextMode.HEX
            ? ColorHelper.ColorToHexString(SelectedColor, true)
            : ColorHelper.ColorToRgbString(SelectedColor, true);
    }

    #endregion 私有方法

    #region 事件处理

    private void OnSelectedColorChanged(Color oldColor, Color newColor)
    {
        if (!_isUpdatingColor)
        {
            try
            {
                _isUpdatingColor = true;

                // 更新选择器颜色
                _colorSelector?.SelectedColor = newColor;

                // 更新文本显示
                UpdateColorText();

                // 触发事件
                RaiseEvent(new RoutedEventArgs(SelectedColorChangedEvent, this));
            }
            finally
            {
                _isUpdatingColor = false;
            }
        }
    }

    private void ColorSelector_MouseDown(object sender, MouseButtonEventArgs e)
    {
        // 防止点击ColorSelector时Popup关闭
        e.Handled = true;
    }

    private void ColorSelector_ColorChanged(object sender, RoutedEventArgs e)
    {
        if (_colorSelector != null && !_isUpdatingColor)
        {
            SelectedColor = _colorSelector.SelectedColor;
        }
    }

    private void ColorPicker_KeyDown(object sender, KeyEventArgs e)
    {
        // 处理键盘事件
        switch (e.Key)
        {
            case Key.Escape:
                if (IsOpen)
                {
                    IsOpen = false;
                    e.Handled = true;
                }
                break;

            default:
                break;
        }
    }

    #endregion 事件处理
}