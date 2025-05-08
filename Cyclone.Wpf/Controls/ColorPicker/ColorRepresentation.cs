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
        _name = name ?? ColorHelper.GetColorName(color);
        _category = category ?? ColorHelper.GetColorCategory(color);
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

            return ColorHelper.ColorToRgbString(_color.Value);
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

            return ColorHelper.ColorToHslString(_color.Value);
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

            return ColorHelper.ColorToHsvString(_color.Value);
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

            return ColorHelper.ColorToHexString(_color.Value);
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

            return ColorHelper.ColorToCmykString(_color.Value);
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
}