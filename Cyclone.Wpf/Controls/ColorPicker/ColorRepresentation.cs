using System.ComponentModel;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 颜色表示接口
/// </summary>
public interface IColorRepresentation
{
    string Name { get; }
    Color? Color { get; }
    string RGB { get; }
    string HSL { get; }
    string HSV { get; }
    string HEX { get; }
    string CYMK { get; }
    string Category { get; }
}

/// <summary>
/// 颜色表示实现类
/// </summary>
public class ColorRepresentation : IColorRepresentation, INotifyPropertyChanged
{
    private Color? _color = Colors.Transparent;
    private string _name = string.Empty;
    private string _category = string.Empty;
    private bool _isSelected;

    /// <summary>
    /// 创建一个新的颜色表示
    /// </summary>
    public ColorRepresentation()
    {
    }

    /// <summary>
    /// 使用指定颜色创建颜色表示
    /// </summary>
    /// <param name="color">颜色</param>
    /// <param name="name">颜色名称</param>
    /// <param name="category">颜色分类</param>
    public ColorRepresentation(Color color, string name = null, string category = null)
    {
        _color = color;
        _name = name ?? GetColorName(color);
        _category = category ?? GetDefaultCategory(color);
    }

    /// <summary>
    /// 颜色
    /// </summary>
    public Color? Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                // 所有颜色表示格式都是基于颜色计算的，所以需要通知它们全部更新
                OnPropertyChanged(nameof(Color));
                OnPropertyChanged(nameof(RGB));
                OnPropertyChanged(nameof(HSL));
                OnPropertyChanged(nameof(HSV));
                OnPropertyChanged(nameof(HEX));
                OnPropertyChanged(nameof(CYMK));
            }
        }
    }

    /// <summary>
    /// 颜色名称
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    /// <summary>
    /// 颜色分类
    /// </summary>
    public string Category
    {
        get => _category;
        set
        {
            if (_category != value)
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }
    }

    /// <summary>
    /// 是否选中
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    /// <summary>
    /// RGB格式表示
    /// </summary>
    public string RGB
    {
        get
        {
            if (_color == null || _color == Colors.Transparent)
                return string.Empty;

            return $"RGB: {_color.Value.R}, {_color.Value.G}, {_color.Value.B}";
        }
    }

    /// <summary>
    /// HSL格式表示
    /// </summary>
    public string HSL
    {
        get
        {
            if (_color == null || _color == Colors.Transparent)
                return string.Empty;

            ColorHelper.RgbToHsl(_color.Value.R, _color.Value.G, _color.Value.B, out double h, out double s, out double l);
            return $"HSL: {h:F0}°, {s:P0}, {l:P0}";
        }
    }

    /// <summary>
    /// HSV格式表示
    /// </summary>
    public string HSV
    {
        get
        {
            if (_color == null || _color == Colors.Transparent)
                return string.Empty;

            ColorHelper.RgbToHsv(_color.Value.R, _color.Value.G, _color.Value.B, out double h, out double s, out double v);
            return $"HSV: {h:F0}°, {s:P0}, {v:P0}";
        }
    }

    /// <summary>
    /// 十六进制格式表示
    /// </summary>
    public string HEX
    {
        get
        {
            if (_color == null || _color == Colors.Transparent)
                return string.Empty;

            return $"#{_color.Value.R:X2}{_color.Value.G:X2}{_color.Value.B:X2}";
        }
    }

    /// <summary>
    /// CMYK格式表示
    /// </summary>
    public string CYMK
    {
        get
        {
            if (_color == null || _color == Colors.Transparent)
                return string.Empty;

            ColorHelper.RgbToCmyk(_color.Value.R, _color.Value.G, _color.Value.B, out double c, out double m, out double y, out double k);
            return $"CMYK: {c:P0}, {m:P0}, {y:P0}, {k:P0}";
        }
    }

    /// <summary>
    /// 属性变更事件
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// 触发属性变更事件
    /// </summary>
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #region 辅助方法

    /// <summary>
    /// 根据颜色获取默认分类
    /// </summary>
    private string GetDefaultCategory(Color color)
    {
        ColorHelper.RgbToHsv(color.R, color.G, color.B, out double h, out double s, out double v);

        // 灰度色
        if (s < 0.15)
        {
            if (v < 0.1) return "黑色";
            if (v > 0.9) return "白色";
            return "灰色";
        }

        // 按色相分类
        if (h < 30) return "红色";
        if (h < 60) return "橙色";
        if (h < 90) return "黄色";
        if (h < 150) return "绿色";
        if (h < 210) return "青色";
        if (h < 270) return "蓝色";
        if (h < 330) return "紫色";
        return "红色";
    }

    /// <summary>
    /// 获取颜色名称
    /// </summary>
    private string GetColorName(Color color)
    {
        // 基本颜色命名
        if (color == Colors.Red) return "红色";
        if (color == Colors.Green) return "绿色";
        if (color == Colors.Blue) return "蓝色";
        if (color == Colors.Yellow) return "黄色";
        if (color == Colors.Purple) return "紫色";
        if (color == Colors.Cyan) return "青色";
        if (color == Colors.Magenta) return "品红";
        if (color == Colors.White) return "白色";
        if (color == Colors.Black) return "黑色";
        if (color == Colors.Gray) return "灰色";
        if (color == Colors.Orange) return "橙色";

        // 如果不是标准颜色，返回十六进制值作为名称
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    #endregion 辅助方法
}