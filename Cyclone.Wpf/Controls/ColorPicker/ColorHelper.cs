using System;
using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 颜色处理辅助类
/// </summary>
public static class ColorHelper
{
    /// <summary>
    /// 计算给定色调的无饱和度颜色
    /// </summary>
    /// <param name="hue">色调 (0-360)</param>
    /// <returns>对应色调的灰色</returns>
    public static Color GetDesaturatedColor(double hue)
    {
        // 将HSV转RGB，使用饱和度为0，亮度为0.5
        double h = hue;
        double s = 0;
        double v = 0.5;

        HsvToRgb(h, s, v, out byte r, out byte g, out byte b);
        return Color.FromRgb(r, g, b);
    }

    /// <summary>
    /// 计算给定颜色的无饱和度版本
    /// </summary>
    /// <param name="color">原颜色</param>
    /// <returns>无饱和度颜色</returns>
    public static Color GetDesaturatedColor(Color color)
    {
        RgbToHsv(color.R, color.G, color.B, out double h, out _, out double v);
        return GetDesaturatedColor(h);
    }

    /// <summary>
    /// RGB转HSV
    /// </summary>
    public static void RgbToHsv(byte r, byte g, byte b, out double h, out double s, out double v)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        double max = Math.Max(Math.Max(red, green), blue);
        double min = Math.Min(Math.Min(red, green), blue);
        double delta = max - min;

        // 计算色调 (Hue)
        if (delta == 0)
        {
            h = 0; // 无色调 (灰色)
        }
        else if (max == red)
        {
            h = 60 * ((green - blue) / delta % 6);
        }
        else if (max == green)
        {
            h = 60 * ((blue - red) / delta + 2);
        }
        else // max == blue
        {
            h = 60 * ((red - green) / delta + 4);
        }

        if (h < 0)
            h += 360;

        // 计算饱和度 (Saturation)
        s = max == 0 ? 0 : delta / max;

        // 计算明度 (Value)
        v = max;
    }

    /// <summary>
    /// HSV转RGB
    /// </summary>
    public static void HsvToRgb(double h, double s, double v, out byte r, out byte g, out byte b)
    {
        double hh = h / 60.0;
        int i = (int)Math.Floor(hh);
        double ff = hh - i;

        double p = v * (1.0 - s);
        double q = v * (1.0 - (s * ff));
        double t = v * (1.0 - (s * (1.0 - ff)));

        double red, green, blue;

        switch (i)
        {
            case 0:
                red = v; green = t; blue = p;
                break;

            case 1:
                red = q; green = v; blue = p;
                break;

            case 2:
                red = p; green = v; blue = t;
                break;

            case 3:
                red = p; green = q; blue = v;
                break;

            case 4:
                red = t; green = p; blue = v;
                break;

            default: // case 5 or 6
                red = v; green = p; blue = q;
                break;
        }

        r = (byte)(red * 255);
        g = (byte)(green * 255);
        b = (byte)(blue * 255);
    }

    /// <summary>
    /// RGB转HSL
    /// </summary>
    public static void RgbToHsl(byte r, byte g, byte b, out double h, out double s, out double l)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        double max = Math.Max(Math.Max(red, green), blue);
        double min = Math.Min(Math.Min(red, green), blue);
        double delta = max - min;

        // 计算亮度 (Lightness)
        l = (max + min) / 2.0;

        // 计算色调 (Hue) 和 饱和度 (Saturation)
        if (delta == 0)
        {
            h = 0; // 无色调 (灰色)
            s = 0; // 无饱和度
        }
        else
        {
            s = l <= 0.5 ? delta / (max + min) : delta / (2.0 - max - min);

            if (max == red)
            {
                h = ((green - blue) / delta) % 6;
            }
            else if (max == green)
            {
                h = ((blue - red) / delta) + 2;
            }
            else // max == blue
            {
                h = ((red - green) / delta) + 4;
            }

            h *= 60;
            if (h < 0)
                h += 360;
        }
    }

    /// <summary>
    /// HSL转RGB
    /// </summary>
    public static void HslToRgb(double h, double s, double l, out byte r, out byte g, out byte b)
    {
        double red, green, blue;

        if (s == 0)
        {
            // 灰度
            red = green = blue = l;
        }
        else
        {
            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;

            red = HueToRgb(p, q, h / 360 + 1.0 / 3.0);
            green = HueToRgb(p, q, h / 360);
            blue = HueToRgb(p, q, h / 360 - 1.0 / 3.0);
        }

        r = (byte)(red * 255);
        g = (byte)(green * 255);
        b = (byte)(blue * 255);
    }

    /// <summary>
    /// 色调转RGB分量辅助方法
    /// </summary>
    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }

    /// <summary>
    /// RGB转CMYK
    /// </summary>
    public static void RgbToCmyk(byte r, byte g, byte b, out double c, out double m, out double y, out double k)
    {
        double red = r / 255.0;
        double green = g / 255.0;
        double blue = b / 255.0;

        k = 1 - Math.Max(Math.Max(red, green), blue);

        if (Math.Abs(k - 1.0) < 0.00001)
        {
            c = m = y = 0;
        }
        else
        {
            c = (1 - red - k) / (1 - k);
            m = (1 - green - k) / (1 - k);
            y = (1 - blue - k) / (1 - k);
        }
    }

    /// <summary>
    /// CMYK转RGB
    /// </summary>
    public static void CmykToRgb(double c, double m, double y, double k, out byte r, out byte g, out byte b)
    {
        r = (byte)((1 - c) * (1 - k) * 255);
        g = (byte)((1 - m) * (1 - k) * 255);
        b = (byte)((1 - y) * (1 - k) * 255);
    }

    /// <summary>
    /// 判断两个颜色是否接近
    /// </summary>
    public static bool AreColorsClose(Color a, Color b, int tolerance = 5)
    {
        return Math.Abs(a.R - b.R) <= tolerance &&
               Math.Abs(a.G - b.G) <= tolerance &&
               Math.Abs(a.B - b.B) <= tolerance &&
               Math.Abs(a.A - b.A) <= tolerance;
    }

    /// <summary>
    /// 获取颜色的相反色
    /// </summary>
    public static Color GetComplementaryColor(Color color)
    {
        return Color.FromRgb((byte)(255 - color.R), (byte)(255 - color.G), (byte)(255 - color.B));
    }

    /// <summary>
    /// 根据颜色亮度自动判断应使用黑色或白色文本
    /// </summary>
    public static Color GetContrastTextColor(Color background)
    {
        // 计算颜色亮度 (基于人眼对不同颜色的感知)
        double luminance = 0.299 * background.R + 0.587 * background.G + 0.114 * background.B;

        // 亮度高于128使用黑色文本，否则使用白色
        return luminance > 128 ? Colors.Black : Colors.White;
    }
}