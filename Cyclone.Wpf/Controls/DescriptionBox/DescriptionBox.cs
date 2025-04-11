// DescriptionBox.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// DescriptionBox控件 - 基于ItemsControl的容器，用于组织DescriptionItem项目
/// </summary>
[ContentProperty("Items")]
public class DescriptionBox : ItemsControl
{
    static DescriptionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DescriptionBox),
            new FrameworkPropertyMetadata(typeof(DescriptionBox)));
    }

    public DescriptionBox()
    {
        // 设置ItemsPanel为Grid
        var gridPanel = new ItemsPanelTemplate();
        var gridFactory = new FrameworkElementFactory(typeof(Grid));
        gridFactory.SetValue(Grid.IsSharedSizeScopeProperty, true);
        gridPanel.VisualTree = gridFactory;
        ItemsPanel = gridPanel;
    }

    #region 依赖属性

    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.Register("RowSpacing", typeof(double), typeof(DescriptionBox),
            new PropertyMetadata(10.0, OnLayoutPropertyChanged));

    public static readonly DependencyProperty ColumnSpacingProperty =
        DependencyProperty.Register("ColumnSpacing", typeof(double), typeof(DescriptionBox),
            new PropertyMetadata(10.0, OnLayoutPropertyChanged));

    #endregion 依赖属性

    #region 属性

    /// <summary>
    /// 行间距
    /// </summary>
    public double RowSpacing
    {
        get { return (double)GetValue(RowSpacingProperty); }
        set { SetValue(RowSpacingProperty, value); }
    }

    /// <summary>
    /// 列间距
    /// </summary>
    public double ColumnSpacing
    {
        get { return (double)GetValue(ColumnSpacingProperty); }
        set { SetValue(ColumnSpacingProperty, value); }
    }

    #endregion 属性

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = d as DescriptionBox;
        var panel = box?.GetItemsPanel() as Grid;

        if (panel != null)
        {
            //if (e.Property == RowSpacingProperty)
            //{
            //    panel.RowSpacing = (double)e.NewValue;
            //}
            //else if (e.Property == ColumnSpacingProperty)
            //{
            //    panel.ColumnSpacing = (double)e.NewValue;
            //}
        }
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        // 如果容器是DescriptionItem，确保它在Grid中的位置
        if (element is DescriptionItem descItem && item is DescriptionItem sourceItem)
        {
            // 拷贝布局属性
            Grid.SetRow(descItem, Grid.GetRow(sourceItem));
            Grid.SetColumn(descItem, Grid.GetColumn(sourceItem));
            Grid.SetRowSpan(descItem, Grid.GetRowSpan(sourceItem));
            Grid.SetColumnSpan(descItem, Grid.GetColumnSpan(sourceItem));

            // 确保Grid有足够的行列
            EnsureGridRowsAndColumns();
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DescriptionItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is DescriptionItem;
    }

    private void EnsureGridRowsAndColumns()
    {
        var panel = GetItemsPanel() as Grid;
        if (panel == null) return;

        int maxRow = 0;
        int maxColumn = 0;

        // 找出最大的行和列
        foreach (var item in Items)
        {
            if (item is DescriptionItem descItem)
            {
                int row = Grid.GetRow(descItem);
                int column = Grid.GetColumn(descItem);
                int rowSpan = Grid.GetRowSpan(descItem);
                int colSpan = Grid.GetColumnSpan(descItem);

                maxRow = Math.Max(maxRow, row + rowSpan);
                maxColumn = Math.Max(maxColumn, column + colSpan);
            }
        }

        // 确保有足够的行
        while (panel.RowDefinitions.Count < maxRow)
        {
            panel.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        // 确保有足够的列
        while (panel.ColumnDefinitions.Count < maxColumn)
        {
            panel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }
    }

    private Panel GetItemsPanel()
    {
        if (Template == null)
            return null;

        var itemsPresenter = Template.FindName("PART_ItemsPresenter", this) as ItemsPresenter;
        if (itemsPresenter == null)
            return null;

        return VisualTreeHelper.GetChild(itemsPresenter, 0) as Panel;
    }
}