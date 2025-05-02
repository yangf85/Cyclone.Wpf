using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 十六进制颜色输入控件
/// </summary>
//public class HexInput : Control
//{
//    private TextBox _textBox;
//    private static readonly Regex _hexRegex = new Regex("^#?([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");

//    static HexInput()
//    {
//        DefaultStyleKeyProperty.OverrideMetadata(typeof(HexInput),
//            new FrameworkPropertyMetadata(typeof(HexInput)));
//    }

//    #region HexValue

//    public string HexValue
//    {
//        get => (string)GetValue(HexValueProperty);
//        set => SetValue(HexValueProperty, value);
//    }

//    public static readonly DependencyProperty HexValueProperty =
//        DependencyProperty.Register(nameof(HexValue), typeof(string), typeof(HexInput),
//        new FrameworkPropertyMetadata("#00FFFF", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHexValueChanged));

//    private static void OnHexValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is HexInput hexInput && e.NewValue is string newValue)
//        {
//            // 确保值是有效的十六进制颜色格式
//            string formattedValue = FormatHexValue(newValue);

//            // 避免循环设置
//            if (formattedValue != newValue)
//            {
//                hexInput.HexValue = formattedValue;
//                return;
//            }

//            // 更新关联的颜色
//            if (ColorHelper.TryParseColor(formattedValue, out Color color))
//            {
//                hexInput.Color = color;
//            }

//            // 更新文本框
//            if (hexInput._textBox != null && hexInput._textBox.Text != formattedValue)
//            {
//                hexInput._textBox.Text = formattedValue;
//            }
//        }
//    }

//    #endregion HexValue

//    #region Color

//    public Color Color
//    {
//        get => (Color)GetValue(ColorProperty);
//        set => SetValue(ColorProperty, value);
//    }

//    public static readonly DependencyProperty ColorProperty =
//        DependencyProperty.Register(nameof(Color), typeof(Color), typeof(HexInput),
//        new FrameworkPropertyMetadata(Colors.Cyan, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnColorChanged));

//    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is HexInput hexInput && e.NewValue is Color newColor)
//        {
//            // 更新十六进制值
//            string hexValue = ColorHelper.ColorToHex(newColor);

//            // 避免循环设置
//            if (hexValue != hexInput.HexValue)
//            {
//                hexInput.HexValue = hexValue;
//            }
//        }
//    }

//    #endregion Color

//    #region IsWithAlpha

//    public bool IsWithAlpha
//    {
//        get => (bool)GetValue(IsWithAlphaProperty);
//        set => SetValue(IsWithAlphaProperty, value);
//    }

//    public static readonly DependencyProperty IsWithAlphaProperty =
//        DependencyProperty.Register(nameof(IsWithAlpha), typeof(bool), typeof(HexInput),
//        new PropertyMetadata(true, OnIsWithAlphaChanged));

//    private static void OnIsWithAlphaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is HexInput hexInput)
//        {
//            // 重新格式化十六进制值
//            hexInput.HexValue = FormatHexValue(hexInput.HexValue, (bool)e.NewValue);
//        }
//    }

//    #endregion IsWithAlpha

//    public override void OnApplyTemplate()
//    {
//        base.OnApplyTemplate();

//        _textBox = GetTemplateChild("PART_TextBox") as TextBox;
//        if (_textBox != null)
//        {
//            _textBox.Text = HexValue;
//            _textBox.TextChanged += TextBox_TextChanged;
//            _textBox.LostFocus += TextBox_LostFocus;
//            _textBox.KeyDown += TextBox_KeyDown;
//        }
//    }

//    private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
//    {
//        if (_textBox.IsFocused)
//        {
//            // 在编辑过程中不立即更新，等待失去焦点或按下回车键
//            string text = _textBox.Text;

//            // 简单验证：确保文本以#开头
//            if (text.Length > 0 && !text.StartsWith("#"))
//            {
//                text = "#" + text;
//                _textBox.Text = text;
//                _textBox.SelectionStart = text.Length;
//            }
//        }
//    }

//    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
//    {
//        ApplyTextValue();
//    }

//    private void TextBox_KeyDown(object sender, KeyEventArgs e)
//    {
//        if (e.Key == Key.Enter)
//        {
//            ApplyTextValue();
//            e.Handled = true;
//        }
//    }

//    private void ApplyTextValue()
//    {
//        string text = _textBox.Text;

//        // 验证并格式化十六进制值
//        if (_hexRegex.IsMatch(text))
//        {
//            HexValue = FormatHexValue(text, IsWithAlpha);
//        }
//        else
//        {
//            // 如果无效，还原为当前值
//            _textBox.Text = HexValue;
//        }
//    }

//    /// <summary>
//    /// 格式化十六进制颜色值
//    /// </summary>
//    private static string FormatHexValue(string value, bool withAlpha = true)
//    {
//        // 确保值以#开头
//        if (string.IsNullOrEmpty(value))
//        {
//            return "#000000";
//        }

//        if (!value.StartsWith("#"))
//        {
//            value = "#" + value;
//        }

//        // 处理简写形式（如#ABC）
//        if (value.Length == 4)
//        {
//            char r = value[1];
//            char g = value[2];
//            char b = value[3];
//            value = $"#{r}{r}{g}{g}{b}{b}";
//        }

//        // 根据是否包含Alpha处理
//        if (withAlpha)
//        {
//            // 如果是6位，添加Alpha通道
//            if (value.Length == 7)
//            {
//                value = value + "FF"; // 添加不透明度
//            }
//        }
//        else
//        {
//            // 如果是8位，去掉Alpha通道
//            if (value.Length == 9)
//            {
//                value = value.Substring(0, 7);
//            }
//        }

//        return value.ToUpper();
//    }
//}