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

/// <summary>
/// 定义 ColorPalette 的工作模式
/// </summary>
public enum ColorPaletteMode
{
    /// <summary>
    /// 预设颜色模式 - 显示标准颜色集合
    /// </summary>
    Preset,

    /// <summary>
    /// 自定义模式 - 通过 ItemsSource 绑定外部颜色集合
    /// </summary>
    Custom
}

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

    #region Mode 属性

    /// <summary>
    /// 调色板的模式 - 预设或自定义
    /// </summary>
    public ColorPaletteMode Mode
    {
        get => (ColorPaletteMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(ColorPaletteMode), typeof(ColorPalette),
            new PropertyMetadata(ColorPaletteMode.Preset));

    #endregion Mode 属性

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

    #region Rows and Columns

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

    #endregion Rows and Columns

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        // Update SelectedColor when selection changes
        if (SelectedItem is IColorRepresentation colorItem && colorItem.Color.HasValue)
        {
            SelectedColor = colorItem.Color.Value;
        }
    }

    /// <summary>
    /// Update the selected color item
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
    /// Determine if two colors are close
    /// </summary>
    private bool AreColorsClose(Color a, Color b, int tolerance = 5)
    {
        return Math.Abs(a.R - b.R) <= tolerance &&
               Math.Abs(a.G - b.G) <= tolerance &&
               Math.Abs(a.B - b.B) <= tolerance &&
               Math.Abs(a.A - b.A) <= tolerance;
    }

    /// <summary>
    /// Initialize color collection
    /// </summary>
    void Initialize()
    {
        // 检查是否是自定义模式
        if (Mode == ColorPaletteMode.Custom)
        {
            // 自定义模式下，不初始化预设颜色
            return;
        }

        // 检查是否是从 XAML 中通过绑定设置了 ItemsSource
        if (ItemsSource != null)
        {
            // 当存在外部 ItemsSource 时，不要初始化标准颜色
            return;
        }

        // 仅当没有内容时才初始化
        if (Items.Count > 0)
            return;

        // 添加标准颜色
        InitializeStandardColors();
    }

    private void InitializeStandardColors()
    {
        // 基于图片中的色板设计
        // 第5个为基准色，向左变浅，向右变深

        // Row 1: 红色系列 - 第5个为标准红色
        AddColorGroup("红色", Color.FromRgb(179, 0, 0), Color.FromRgb(204, 0, 0),
                     Color.FromRgb(229, 0, 0), Color.FromRgb(242, 0, 0),
                     Color.FromRgb(255, 0, 0), Color.FromRgb(255, 77, 77),
                     Color.FromRgb(255, 153, 153), Color.FromRgb(255, 204, 204),
                     Color.FromRgb(255, 240, 240));

        // Row 2: 橙色系列 - 第5个为标准橙色
        AddColorGroup("橙色", Color.FromRgb(153, 77, 0), Color.FromRgb(179, 90, 0),
                     Color.FromRgb(204, 102, 0), Color.FromRgb(230, 115, 0),
                     Color.FromRgb(255, 128, 0), Color.FromRgb(255, 153, 51),
                     Color.FromRgb(255, 178, 102), Color.FromRgb(255, 204, 153),
                     Color.FromRgb(255, 235, 204));

        // Row 3: 黄色系列 - 第5个为标准黄色
        AddColorGroup("黄色", Color.FromRgb(153, 153, 0), Color.FromRgb(179, 179, 0),
                     Color.FromRgb(204, 204, 0), Color.FromRgb(230, 230, 0),
                     Color.FromRgb(255, 255, 0), Color.FromRgb(255, 255, 77),
                     Color.FromRgb(255, 255, 153), Color.FromRgb(255, 255, 204),
                     Color.FromRgb(255, 255, 230));

        // Row 4: 绿色系列 - 第5个为标准绿色
        AddColorGroup("绿色", Color.FromRgb(0, 102, 0), Color.FromRgb(0, 128, 0),
                     Color.FromRgb(0, 153, 0), Color.FromRgb(0, 179, 0),
                     Color.FromRgb(0, 255, 0), Color.FromRgb(77, 255, 77),
                     Color.FromRgb(153, 255, 153), Color.FromRgb(204, 255, 204),
                     Color.FromRgb(230, 255, 230));

        // Row 5: 青色系列 - 第5个为标准青色
        AddColorGroup("青色", Color.FromRgb(0, 102, 102), Color.FromRgb(0, 128, 128),
                     Color.FromRgb(0, 153, 153), Color.FromRgb(0, 204, 204),
                     Color.FromRgb(0, 255, 255), Color.FromRgb(77, 255, 255),
                     Color.FromRgb(153, 255, 255), Color.FromRgb(204, 255, 255),
                     Color.FromRgb(230, 255, 255));

        // Row 6: 蓝色系列 - 第5个为标准蓝色
        AddColorGroup("蓝色", Color.FromRgb(0, 0, 153), Color.FromRgb(0, 0, 179),
                     Color.FromRgb(0, 0, 204), Color.FromRgb(0, 0, 230),
                     Color.FromRgb(0, 0, 255), Color.FromRgb(77, 77, 255),
                     Color.FromRgb(153, 153, 255), Color.FromRgb(204, 204, 255),
                     Color.FromRgb(230, 230, 255));

        // Row 7: 紫色系列 - 第5个为标准紫色
        AddColorGroup("紫色", Color.FromRgb(51, 0, 102), Color.FromRgb(64, 0, 128),
                     Color.FromRgb(77, 0, 153), Color.FromRgb(89, 0, 179),
                     Color.FromRgb(102, 0, 204), Color.FromRgb(128, 51, 255),
                     Color.FromRgb(178, 102, 255), Color.FromRgb(204, 153, 255),
                     Color.FromRgb(230, 204, 255));

        // Row 8: 洋红/品红系列 - 第5个为标准洋红色
        AddColorGroup("洋红", Color.FromRgb(102, 0, 102), Color.FromRgb(128, 0, 128),
                     Color.FromRgb(153, 0, 153), Color.FromRgb(204, 0, 204),
                     Color.FromRgb(255, 0, 255), Color.FromRgb(255, 77, 255),
                     Color.FromRgb(255, 153, 255), Color.FromRgb(255, 204, 255),
                     Color.FromRgb(255, 230, 255));

        // Row 9: 粉色系列 - 第5个为标准粉红色
        AddColorGroup("粉色", Color.FromRgb(153, 0, 76), Color.FromRgb(179, 0, 89),
                     Color.FromRgb(204, 0, 102), Color.FromRgb(230, 0, 115),
                     Color.FromRgb(255, 0, 128), Color.FromRgb(255, 77, 166),
                     Color.FromRgb(255, 153, 204), Color.FromRgb(255, 204, 230),
                     Color.FromRgb(255, 230, 242));

        // Row 10: 棕色系列 - 第5个为标准棕色
        AddColorGroup("棕色", Color.FromRgb(102, 51, 51), Color.FromRgb(128, 64, 64),
                     Color.FromRgb(153, 76, 76), Color.FromRgb(166, 83, 83),
                     Color.FromRgb(153, 76, 0), Color.FromRgb(179, 90, 0),
                     Color.FromRgb(204, 102, 0), Color.FromRgb(204, 153, 102),
                     Color.FromRgb(222, 184, 135));

        // Row 11: 灰色系列 - 第5个为中灰色
        AddColorGroup("灰色", Color.FromRgb(32, 32, 32), Color.FromRgb(64, 64, 64),
                     Color.FromRgb(96, 96, 96), Color.FromRgb(112, 112, 112),
                     Color.FromRgb(128, 128, 128), Color.FromRgb(160, 160, 160),
                     Color.FromRgb(192, 192, 192), Color.FromRgb(224, 224, 224),
                     Color.FromRgb(255, 255, 255));

        // Row 12: 黑色系列 - 全黑到深灰
        AddColorGroup("黑色", Color.FromRgb(0, 0, 0), Color.FromRgb(5, 5, 5),
                     Color.FromRgb(10, 10, 10), Color.FromRgb(15, 15, 15),
                     Color.FromRgb(20, 20, 20), Color.FromRgb(25, 25, 25),
                     Color.FromRgb(30, 30, 30), Color.FromRgb(40, 40, 40),
                     Color.FromRgb(50, 50, 50));
    }

    /// <summary>
    /// Add a group of colors
    /// </summary>
    private void AddColorGroup(string category, params Color[] colors)
    {
        // 如果是自定义模式或有外部 ItemsSource 绑定，则不直接操作 Items 集合
        if (Mode == ColorPaletteMode.Custom || ItemsSource != null)
            return;

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