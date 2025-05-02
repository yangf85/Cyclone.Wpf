using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 颜色选择器控件，提供了完整的颜色选择功能
/// </summary>
//public class ColorPicker : Control
//{
//    static ColorPicker()
//    {
//        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker),
//            new FrameworkPropertyMetadata(typeof(ColorPicker)));
//    }

//    #region SelectedColor

//    public Color SelectedColor
//    {
//        get => (Color)GetValue(SelectedColorProperty);
//        set => SetValue(SelectedColorProperty, value);
//    }

//    public static readonly DependencyProperty SelectedColorProperty =
//        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
//        new PropertyMetadata(Colors.Cyan, OnSelectedColorChanged));

//    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is ColorPicker picker)
//        {
//            Color newColor = (Color)e.NewValue;

//            // 更新HSL值
//            HslColor hsl = ColorHelper.RgbToHsl(newColor);
//            picker.Hue = hsl.H;
//            picker.Saturation = hsl.S;
//            picker.Lightness = hsl.L;
//            picker.Alpha = newColor.A / 255.0;

//            // 更新十六进制值
//            picker.HexValue = ColorHelper.ColorToHex(newColor);

//            // 触发值改变事件
//            picker.ColorChanged?.Invoke(picker, new RoutedEventArgs());
//        }
//    }

//    #endregion SelectedColor

//    #region HexValue

//    public string HexValue
//    {
//        get => (string)GetValue(HexValueProperty);
//        set => SetValue(HexValueProperty, value);
//    }

//    public static readonly DependencyProperty HexValueProperty =
//        DependencyProperty.Register(nameof(HexValue), typeof(string), typeof(ColorPicker),
//        new PropertyMetadata("#00FFFF", OnHexValueChanged));

//    private static void OnHexValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is ColorPicker picker && e.NewValue is string hexValue)
//        {
//            if (ColorHelper.TryParseColor(hexValue, out Color color))
//            {
//                // 避免循环设置
//                if (color != picker.SelectedColor)
//                {
//                    picker.SelectedColor = color;
//                }
//            }
//        }
//    }

//    #endregion HexValue

//    #region Hue

//    public double Hue
//    {
//        get => (double)GetValue(HueProperty);
//        set => SetValue(HueProperty, value);
//    }

//    public static readonly DependencyProperty HueProperty =
//        DependencyProperty.Register(nameof(Hue), typeof(double), typeof(ColorPicker),
//        new PropertyMetadata(180.0, OnHslChanged));

//    #endregion Hue

//    #region Saturation

//    public double Saturation
//    {
//        get => (double)GetValue(SaturationProperty);
//        set => SetValue(SaturationProperty, value);
//    }

//    public static readonly DependencyProperty SaturationProperty =
//        DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(ColorPicker),
//        new PropertyMetadata(1.0, OnHslChanged));

//    #endregion Saturation

//    #region Lightness

//    public double Lightness
//    {
//        get => (double)GetValue(LightnessProperty);
//        set => SetValue(LightnessProperty, value);
//    }

//    public static readonly DependencyProperty LightnessProperty =
//        DependencyProperty.Register(nameof(Lightness), typeof(double), typeof(ColorPicker),
//        new PropertyMetadata(0.5, OnHslChanged));

//    #endregion Lightness

//    #region Alpha

//    public double Alpha
//    {
//        get => (double)GetValue(AlphaProperty);
//        set => SetValue(AlphaProperty, value);
//    }

//    public static readonly DependencyProperty AlphaProperty =
//        DependencyProperty.Register(nameof(Alpha), typeof(double), typeof(ColorPicker),
//        new PropertyMetadata(1.0, OnHslChanged));

//    #endregion Alpha

//    private static void OnHslChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is ColorPicker picker)
//        {
//            // 根据HSL值更新颜色
//            HslColor hsl = new HslColor(picker.Hue, picker.Saturation, picker.Lightness);
//            Color rgbColor = ColorHelper.HslToRgb(hsl);

//            // 设置Alpha通道
//            rgbColor.A = (byte)(picker.Alpha * 255);

//            // 避免循环设置
//            if (rgbColor != picker.SelectedColor)
//            {
//                picker.SelectedColor = rgbColor;
//            }
//        }
//    }

//    #region IsDropperActive

//    public bool IsDropperActive
//    {
//        get => (bool)GetValue(IsDropperActiveProperty);
//        set => SetValue(IsDropperActiveProperty, value);
//    }

//    public static readonly DependencyProperty IsDropperActiveProperty =
//        DependencyProperty.Register(nameof(IsDropperActive), typeof(bool), typeof(ColorPicker),
//        new PropertyMetadata(false, OnIsDropperActiveChanged));

//    private static void OnIsDropperActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is ColorPicker picker)
//        {
//            bool isActive = (bool)e.NewValue;
//            if (isActive)
//            {
//                picker.CaptureMouse();
//                Mouse.OverrideCursor = Cursors.Pen;
//            }
//            else
//            {
//                picker.ReleaseMouseCapture();
//                Mouse.OverrideCursor = null;
//            }
//        }
//    }

//    #endregion IsDropperActive

//    #region ShowAlphaChannel

//    public bool ShowAlphaChannel
//    {
//        get => (bool)GetValue(ShowAlphaChannelProperty);
//        set => SetValue(ShowAlphaChannelProperty, value);
//    }

//    public static readonly DependencyProperty ShowAlphaChannelProperty =
//        DependencyProperty.Register(nameof(ShowAlphaChannel), typeof(bool), typeof(ColorPicker),
//        new PropertyMetadata(true));

//    #endregion ShowAlphaChannel

//    #region Events

//    public event RoutedEventHandler ColorChanged;

//    #endregion Events

//    #region Commands

//    public ICommand SaveColorCommand
//    {
//        get => (ICommand)GetValue(SaveColorCommandProperty);
//        set => SetValue(SaveColorCommandProperty, value);
//    }

//    public static readonly DependencyProperty SaveColorCommandProperty =
//        DependencyProperty.Register(nameof(SaveColorCommand), typeof(ICommand), typeof(ColorPicker));

//    public ICommand PickColorCommand
//    {
//        get => (ICommand)GetValue(PickColorCommandProperty);
//        set => SetValue(PickColorCommandProperty, value);
//    }

//    public static readonly DependencyProperty PickColorCommandProperty =
//        DependencyProperty.Register(nameof(PickColorCommand), typeof(ICommand), typeof(ColorPicker));

//    #endregion Commands

//    public override void OnApplyTemplate()
//    {
//        base.OnApplyTemplate();

//        // 初始化命令
//        SaveColorCommand = new RelayCommand(_ => SaveColor());
//        PickColorCommand = new RelayCommand(_ => ToggleDropper());
//    }

//    protected override void OnMouseDown(MouseButtonEventArgs e)
//    {
//        base.OnMouseDown(e);

//        if (IsDropperActive && e.LeftButton == MouseButtonState.Pressed)
//        {
//            // 获取鼠标位置下的颜色
//            Point position = e.GetPosition(null);
//            Color pixelColor = ScreenColorPicker.GetColorAt(position);
//            SelectedColor = pixelColor;

//            // 停用吸管工具
//            IsDropperActive = false;
//        }
//    }

//    private void SaveColor()
//    {
//        // 将当前颜色保存到自定义调色板
//        // 实际实现可能需要更多逻辑
//    }

//    private void ToggleDropper()
//    {
//        IsDropperActive = !IsDropperActive;
//    }
//}

//// 辅助类
//internal class HslColor
//{
//    public double H { get; set; } // 0-360
//    public double S { get; set; } // 0-1
//    public double L { get; set; } // 0-1

//    public HslColor(double h, double s, double l)
//    {
//        H = h;
//        S = s;
//        L = l;
//    }
//}

//internal static class ColorHelper
//{
//    public static string ColorToHex(Color color)
//    {
//        return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
//    }

//    public static bool TryParseColor(string hexColor, out Color color)
//    {
//        color = Colors.Black;
//        try
//        {
//            if (hexColor.StartsWith("#"))
//            {
//                hexColor = hexColor.Substring(1);
//            }

//            if (hexColor.Length == 6)
//            {
//                // RGB格式
//                color = Color.FromRgb(
//                    Convert.ToByte(hexColor.Substring(0, 2), 16),
//                    Convert.ToByte(hexColor.Substring(2, 2), 16),
//                    Convert.ToByte(hexColor.Substring(4, 2), 16));
//                return true;
//            }
//            else if (hexColor.Length == 8)
//            {
//                // ARGB格式
//                color = Color.FromArgb(
//                    Convert.ToByte(hexColor.Substring(0, 2), 16),
//                    Convert.ToByte(hexColor.Substring(2, 2), 16),
//                    Convert.ToByte(hexColor.Substring(4, 2), 16),
//                    Convert.ToByte(hexColor.Substring(6, 2), 16));
//                return true;
//            }
//        }
//        catch
//        {
//            return false;
//        }
//        return false;
//    }

//    public static HslColor RgbToHsl(Color rgb)
//    {
//        double r = rgb.R / 255.0;
//        double g = rgb.G / 255.0;
//        double b = rgb.B / 255.0;

//        double max = Math.Max(Math.Max(r, g), b);
//        double min = Math.Min(Math.Min(r, g), b);
//        double delta = max - min;

//        double h = 0;
//        double s = 0;
//        double l = (max + min) / 2;

//        if (delta != 0)
//        {
//            s = l < 0.5 ? delta / (max + min) : delta / (2 - max - min);

//            if (r == max)
//            {
//                h = (g - b) / delta + (g < b ? 6 : 0);
//            }
//            else if (g == max)
//            {
//                h = (b - r) / delta + 2;
//            }
//            else
//            {
//                h = (r - g) / delta + 4;
//            }

//            h *= 60;
//        }

//        return new HslColor(h, s, l);
//    }

//    public static Color HslToRgb(HslColor hsl)
//    {
//        double h = hsl.H;
//        double s = hsl.S;
//        double l = hsl.L;

//        double r, g, b;

//        if (s == 0)
//        {
//            r = g = b = l;
//        }
//        else
//        {
//            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
//            double p = 2 * l - q;
//            r = HueToRgb(p, q, h / 360 + 1.0 / 3);
//            g = HueToRgb(p, q, h / 360);
//            b = HueToRgb(p, q, h / 360 - 1.0 / 3);
//        }

//        return Color.FromRgb(
//            (byte)(r * 255),
//            (byte)(g * 255),
//            (byte)(b * 255));
//    }

//    private static double HueToRgb(double p, double q, double t)
//    {
//        if (t < 0) t += 1;
//        if (t > 1) t -= 1;
//        if (t < 1.0 / 6) return p + (q - p) * 6 * t;
//        if (t < 1.0 / 2) return q;
//        if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
//        return p;
//    }
//}

//internal static class ScreenColorPicker
//{
//    [System.Runtime.InteropServices.DllImport("gdi32.dll")]
//    private static extern int GetPixel(IntPtr hdc, int nXPos, int nYPos);

//    [System.Runtime.InteropServices.DllImport("user32.dll")]
//    private static extern IntPtr GetDC(IntPtr hwnd);

//    [System.Runtime.InteropServices.DllImport("user32.dll")]
//    private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

//    public static Color GetColorAt(Point position)
//    {
//        IntPtr desk = GetDC(IntPtr.Zero);
//        int colorRef = GetPixel(desk, (int)position.X, (int)position.Y);
//        ReleaseDC(IntPtr.Zero, desk);

//        byte r = (byte)(colorRef & 0xFF);
//        byte g = (byte)((colorRef & 0xFF00) >> 8);
//        byte b = (byte)((colorRef & 0xFF0000) >> 16);

//        return Color.FromRgb(r, g, b);
//    }
//}

//// 简单的命令实现
//internal class RelayCommand : ICommand
//{
//    private readonly Action<object> _execute;
//    private readonly Func<object, bool> _canExecute;

//    public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
//    {
//        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
//        _canExecute = canExecute;
//    }

//    public event EventHandler CanExecuteChanged
//    {
//        add { CommandManager.RequerySuggested += value; }
//        remove { CommandManager.RequerySuggested -= value; }
//    }

//    public bool CanExecute(object parameter)
//    {
//        return _canExecute == null || _canExecute(parameter);
//    }

//    public void Execute(object parameter)
//    {
//        _execute(parameter);
//    }
//}