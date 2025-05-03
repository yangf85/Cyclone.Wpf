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

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ColorPaletteItem))]
public class ColorPalette : ListBox
{
    static ColorPalette()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPalette), new FrameworkPropertyMetadata(typeof(ColorPalette)));
    }

    public ColorPalette()
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
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPalette),
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPalette palette)
        {
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
        DependencyProperty.Register(nameof(Rows), typeof(int), typeof(ColorPalette), new PropertyMetadata(12));

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(ColorPalette), new PropertyMetadata(9));

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
    }

    private void InitializeStandardColors()
    {
        // 第1行：红色系 - 从深红到浅红更平滑的过渡
        AddColorGroup("红色系", Color.FromRgb(102, 0, 0), Color.FromRgb(153, 0, 0),
                     Color.FromRgb(204, 0, 0), Color.FromRgb(255, 0, 0),
                     Color.FromRgb(255, 51, 51), Color.FromRgb(255, 102, 102),
                     Color.FromRgb(255, 153, 153), Color.FromRgb(255, 204, 204),
                     Color.FromRgb(255, 102, 102));

        // 第2行：橙色系 - 更均匀的橙色过渡
        AddColorGroup("橙色系", Color.FromRgb(153, 51, 0), Color.FromRgb(204, 76, 0),
                     Color.FromRgb(255, 102, 0), Color.FromRgb(255, 128, 0),
                     Color.FromRgb(255, 153, 51), Color.FromRgb(255, 178, 102),
                     Color.FromRgb(255, 204, 153), Color.FromRgb(255, 229, 204),
                     Color.FromRgb(204, 102, 51));

        // 第3行：黄色系 - 更纯净的黄色系列
        AddColorGroup("黄色系", Color.FromRgb(153, 153, 0), Color.FromRgb(204, 204, 0),
                     Color.FromRgb(255, 255, 0), Color.FromRgb(255, 255, 51),
                     Color.FromRgb(255, 255, 102), Color.FromRgb(255, 255, 153),
                     Color.FromRgb(255, 255, 204), Color.FromRgb(255, 250, 205),
                     Color.FromRgb(238, 232, 170));

        // 第4行：绿色系 - 暗绿到亮绿，更平滑的过渡
        AddColorGroup("绿色系", Color.FromRgb(0, 51, 0), Color.FromRgb(0, 102, 0),
                     Color.FromRgb(0, 153, 0), Color.FromRgb(0, 204, 0),
                     Color.FromRgb(0, 255, 0), Color.FromRgb(51, 255, 51),
                     Color.FromRgb(102, 255, 102), Color.FromRgb(153, 255, 153),
                     Color.FromRgb(204, 255, 204));

        // 第5行：青绿色系 - 绿到青的平滑过渡
        AddColorGroup("青绿色系", Color.FromRgb(0, 255, 0), Color.FromRgb(0, 255, 51),
                     Color.FromRgb(0, 255, 102), Color.FromRgb(0, 255, 153),
                     Color.FromRgb(0, 255, 204), Color.FromRgb(0, 255, 255),
                     Color.FromRgb(0, 204, 153), Color.FromRgb(0, 153, 102),
                     Color.FromRgb(0, 102, 51));

        // 第6行：蓝绿色系 - 青到蓝绿的平滑过渡
        AddColorGroup("蓝绿色系", Color.FromRgb(0, 102, 102), Color.FromRgb(0, 153, 153),
                     Color.FromRgb(0, 204, 204), Color.FromRgb(0, 255, 255),
                     Color.FromRgb(51, 255, 255), Color.FromRgb(102, 255, 255),
                     Color.FromRgb(153, 255, 255), Color.FromRgb(204, 255, 255),
                     Color.FromRgb(224, 255, 255));

        // 第7行：蓝色系 - 更均匀的蓝色渐变
        AddColorGroup("蓝色系", Color.FromRgb(0, 0, 102), Color.FromRgb(0, 0, 153),
                     Color.FromRgb(0, 0, 204), Color.FromRgb(0, 0, 255),
                     Color.FromRgb(51, 51, 255), Color.FromRgb(102, 102, 255),
                     Color.FromRgb(153, 153, 255), Color.FromRgb(204, 204, 255),
                     Color.FromRgb(230, 230, 250));

        // 第8行：紫蓝色系 - 蓝到紫的平滑过渡
        AddColorGroup("紫蓝色系", Color.FromRgb(25, 25, 112), Color.FromRgb(48, 25, 160),
                     Color.FromRgb(71, 60, 176), Color.FromRgb(72, 50, 204),
                     Color.FromRgb(75, 0, 255), Color.FromRgb(93, 39, 255),
                     Color.FromRgb(111, 78, 255), Color.FromRgb(129, 117, 255),
                     Color.FromRgb(147, 156, 255));

        // 第9行：紫色系 - 更均匀的紫色渐变
        AddColorGroup("紫色系", Color.FromRgb(76, 0, 153), Color.FromRgb(102, 0, 204),
                     Color.FromRgb(128, 0, 255), Color.FromRgb(147, 51, 255),
                     Color.FromRgb(160, 102, 255), Color.FromRgb(178, 153, 255),
                     Color.FromRgb(204, 204, 255), Color.FromRgb(221, 209, 255),
                     Color.FromRgb(238, 229, 255));

        // 第10行：粉紫色系 - 紫到粉红的平滑过渡
        AddColorGroup("粉紫色系", Color.FromRgb(128, 0, 128), Color.FromRgb(153, 0, 153),
                     Color.FromRgb(204, 0, 204), Color.FromRgb(255, 0, 255),
                     Color.FromRgb(255, 51, 255), Color.FromRgb(255, 102, 255),
                     Color.FromRgb(255, 153, 255), Color.FromRgb(255, 204, 255),
                     Color.FromRgb(255, 153, 204));

        // 第11行：棕色系 - 深棕到浅棕
        AddColorGroup("棕色系", Color.FromRgb(102, 51, 0), Color.FromRgb(153, 76, 0),
                     Color.FromRgb(153, 102, 51), Color.FromRgb(204, 102, 0),
                     Color.FromRgb(204, 153, 102), Color.FromRgb(210, 180, 140),
                     Color.FromRgb(222, 184, 135), Color.FromRgb(245, 222, 179),
                     Color.FromRgb(255, 228, 196));

        // 第12行：灰色系 - 黑到白的更均匀渐变
        AddColorGroup("灰色系", Color.FromRgb(0, 0, 0), Color.FromRgb(32, 32, 32),
                     Color.FromRgb(64, 64, 64), Color.FromRgb(96, 96, 96),
                     Color.FromRgb(128, 128, 128), Color.FromRgb(160, 160, 160),
                     Color.FromRgb(192, 192, 192), Color.FromRgb(224, 224, 224),
                     Color.FromRgb(255, 255, 255));
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

            Items.Add(colorItem);
        }
    }

    #region Override

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is ColorPaletteItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new ColorPaletteItem();
    }

    #endregion Override
}

public class ColorPaletteItem : ListBoxItem
{
}