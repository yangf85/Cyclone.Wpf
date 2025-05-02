using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class StandardColorPalette : Selector
{
    static StandardColorPalette()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StandardColorPalette), new FrameworkPropertyMetadata(typeof(StandardColorPalette)));
    }

    public StandardColorPalette()
    {
        Loaded += (s, e) => Initialize();
    }

    #region SelectedColor

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(StandardColorPalette),
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StandardColorPalette palette)
        {
            // 当SelectedColor变更时，同步更新选中的项
            palette.UpdateSelectedItemFromColor((Color)e.NewValue);
        }
    }

    #endregion SelectedColor

    #region 行数和列数

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(nameof(Rows), typeof(int), typeof(StandardColorPalette), new PropertyMetadata(12));

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(StandardColorPalette), new PropertyMetadata(9));

    #endregion 行数和列数

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        // 当选择项改变时，更新SelectedColor
        if (SelectedItem is IColorRepresentation colorItem && colorItem.Color.HasValue)
        {
            SelectedColor = colorItem.Color.Value;
        }
    }

    /// <summary>
    /// 更新选中的颜色项
    /// </summary>
    private void UpdateSelectedItemFromColor(Color color)
    {
        foreach (IColorRepresentation item in Items)
        {
            if (item.Color.HasValue && AreColorsClose(item.Color.Value, color))
            {
                SelectedItem = item;
                return;
            }
        }
    }

    /// <summary>
    /// 判断两个颜色是否接近
    /// </summary>
    private bool AreColorsClose(Color a, Color b, int tolerance = 5)
    {
        return Math.Abs(a.R - b.R) <= tolerance &&
               Math.Abs(a.G - b.G) <= tolerance &&
               Math.Abs(a.B - b.B) <= tolerance &&
               Math.Abs(a.A - b.A) <= tolerance;
    }

    /// <summary>
    /// 初始化颜色集合
    /// </summary>
    void Initialize()
    {
        // 如果Items已有内容，不重复初始化
        if (Items.Count > 0)
            return;

        Items.Clear();

        // 添加标准颜色
        InitializeStandardColors();

        // 默认选中第一个颜色
        if (Items.Count > 0)
        {
            SelectedIndex = 0;
        }
    }

    /// <summary>
    /// 初始化标准颜色
    /// </summary>
    private void InitializeStandardColors()
    {
        // 第1行：红色系
        AddColorGroup("红色系", Colors.DarkRed, Colors.Red, Colors.IndianRed, Colors.LightCoral, Colors.Salmon,
                     Colors.LightSalmon, Colors.Tomato, Colors.OrangeRed, Colors.Crimson);

        // 第2行：橙色系
        AddColorGroup("橙色系", Colors.DarkOrange, Colors.Orange, Colors.Gold, Colors.Goldenrod, Colors.DarkGoldenrod,
                     Colors.LightGoldenrodYellow, Colors.PeachPuff, Colors.SandyBrown, Colors.Chocolate);

        // 第3行：黄色系
        AddColorGroup("黄色系", Colors.Yellow, Colors.LightYellow, Colors.LemonChiffon, Colors.Moccasin,
                     Colors.PaleGoldenrod, Colors.Khaki, Colors.DarkKhaki, Colors.Olive, Colors.YellowGreen);

        // 第4行：绿色系
        AddColorGroup("绿色系", Colors.DarkGreen, Colors.Green, Colors.ForestGreen, Colors.SeaGreen, Colors.MediumSeaGreen,
                     Colors.LightGreen, Colors.PaleGreen, Colors.SpringGreen, Colors.MediumSpringGreen);

        // 第5行：青绿色系
        AddColorGroup("青绿色系", Colors.Lime, Colors.LimeGreen, Colors.LawnGreen, Colors.Chartreuse, Colors.GreenYellow,
                     Colors.DarkOliveGreen, Colors.OliveDrab, Colors.DarkSeaGreen, Colors.MediumAquamarine);

        // 第6行：蓝绿色系
        AddColorGroup("蓝绿色系", Colors.Teal, Colors.DarkCyan, Colors.LightSeaGreen, Colors.CadetBlue, Colors.DarkTurquoise,
                     Colors.MediumTurquoise, Colors.Turquoise, Colors.Aquamarine, Colors.Aqua);

        // 第7行：蓝色系
        AddColorGroup("蓝色系", Colors.DarkBlue, Colors.Navy, Colors.MediumBlue, Colors.Blue, Colors.RoyalBlue,
                     Colors.SteelBlue, Colors.DeepSkyBlue, Colors.LightSkyBlue, Colors.LightBlue);

        // 第8行：紫蓝色系
        AddColorGroup("紫蓝色系", Colors.Indigo, Colors.SlateBlue, Colors.DarkSlateBlue, Colors.MediumSlateBlue,
                     Colors.MediumPurple, Colors.BlueViolet, Colors.DarkOrchid, Colors.DarkViolet, Colors.MediumOrchid);

        // 第9行：紫色系
        AddColorGroup("紫色系", Colors.Purple, Colors.DarkMagenta, Colors.Magenta, Colors.Violet, Colors.Plum,
                     Colors.Thistle, Colors.Orchid, Colors.HotPink, Colors.DeepPink);

        // 第10行：粉色系
        AddColorGroup("粉色系", Colors.MediumVioletRed, Colors.PaleVioletRed, Colors.Pink, Colors.LightPink,
                     Colors.RosyBrown, Colors.MistyRose, Colors.LavenderBlush, Colors.Lavender, Colors.Gainsboro);

        // 第11行：棕色系
        AddColorGroup("棕色系", Colors.Brown, Colors.Maroon, Colors.SaddleBrown, Colors.Sienna, Colors.Peru,
                     Colors.Tan, Colors.BurlyWood, Colors.Wheat, Colors.NavajoWhite);

        // 第12行：灰色系
        AddColorGroup("灰色系", Colors.Black, Colors.DimGray, Colors.Gray, Colors.DarkGray, Colors.Silver,
                     Colors.LightGray, Colors.Gainsboro, Colors.WhiteSmoke, Colors.White);
    }

    /// <summary>
    /// 添加一组颜色
    /// </summary>
    private void AddColorGroup(string category, params Color[] colors)
    {
        foreach (var color in colors)
        {
            var colorItem = new ColorRepresentation(color)
            {
                Category = category
            };
            var item = new ListBoxItem()
            {
                DataContext = colorItem,
            };
            Items.Add(item);
        }
    }
}