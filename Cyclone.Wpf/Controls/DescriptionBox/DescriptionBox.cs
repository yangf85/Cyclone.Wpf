// DescriptionBox.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// DescriptionBox 控件 - 一个用于显示描述项目的容器控件
/// 允许将多个 DescriptionItem 控件排列在网格中
/// </summary>
[TemplatePart(Name = "PART_ItemsContainer", Type = typeof(Grid))]
public class DescriptionBox : ItemsControl
{
    // 内部网格容器，用于放置子项目
    private Grid _container;

    // 标记是否已经设置过初始行和列
    private bool _isGridInitialized = false;

    /// <summary>
    /// 静态构造函数，重写默认样式
    /// </summary>
    static DescriptionBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DescriptionBox),
            new FrameworkPropertyMetadata(typeof(DescriptionBox)));
    }

    /// <summary>
    /// 为项目准备容器时的处理
    /// 设置每个 DescriptionItem 的网格位置属性
    /// </summary>
    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (_container == null || !_isGridInitialized)
        {
            // 延迟到模板应用后再初始化网格
            return;
        }

        // 设置项目的网格属性
        if (element is DescriptionItem descItem)
        {
            // 确保索引在有效范围内
            int row = Math.Min(descItem.Row, _container.RowDefinitions.Count - 1);
            int col = Math.Min(descItem.Column, _container.ColumnDefinitions.Count - 1);
            row = Math.Max(0, row);
            col = Math.Max(0, col);

            // 应用 Grid.Row, Grid.Column 等附加属性
            Grid.SetRow(descItem, row);
            Grid.SetColumn(descItem, col);
            Grid.SetRowSpan(descItem, descItem.RowSpan > 0 ? descItem.RowSpan : 1);
            Grid.SetColumnSpan(descItem, descItem.ColumnSpan > 0 ? descItem.ColumnSpan : 1);
        }
    }

    #region 重写方法

    /// <summary>
    /// 获取项目的容器
    /// </summary>
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new DescriptionItem();
    }

    /// <summary>
    /// 判断项目是否自己就是容器
    /// </summary>
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is DescriptionItem;
    }

    /// <summary>
    /// 应用模板时的处理
    /// 初始化网格并设置事件处理程序
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _container = GetTemplateChild("PART_ItemsContainer") as Grid;

        // 初始化网格
        if (_container != null)
        {
            // 启用共享尺寸组功能
            Grid.SetIsSharedSizeScope(_container, true);

            // 移除调试网格线
            _container.ShowGridLines = false;

            // 预先初始化网格
            InitializeGrid();
        }

        // 当加载完成后重新布局项目
        Loaded += (s, e) => LayoutItemsAfterLoad();

        // 当项目集合变化时
        ItemContainerGenerator.ItemsChanged += (s, e) =>
        {
            if (_container != null && IsLoaded)
            {
                LayoutItemsAfterLoad();
            }
        };
    }

    #endregion 重写方法

    /// <summary>
    /// 初始化网格 - 创建一组默认的行和列
    /// </summary>
    private void InitializeGrid()
    {
        if (_container == null) return;

        _container.RowDefinitions.Clear();
        _container.ColumnDefinitions.Clear();

        // 首先添加一个默认行和列
        _container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _isGridInitialized = true;
    }

    /// <summary>
    /// 控件加载完成后执行布局
    /// </summary>
    private void LayoutItemsAfterLoad()
    {
        if (_container == null) return;

        // 计算需要的最大行数和列数
        int maxRow = 0;
        int maxColumn = 0;

        // 遍历所有子项目
        for (int i = 0; i < Items.Count; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as DescriptionItem;
            if (container != null)
            {
                maxRow = Math.Max(maxRow, container.Row + (container.RowSpan > 0 ? container.RowSpan - 1 : 0));
                maxColumn = Math.Max(maxColumn, container.Column + (container.ColumnSpan > 0 ? container.ColumnSpan - 1 : 0));
            }
        }

        // 确保行数足够
        while (_container.RowDefinitions.Count <= maxRow)
        {
            _container.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        // 确保列数足够
        while (_container.ColumnDefinitions.Count <= maxColumn)
        {
            _container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        }

        // 重新设置每个子项目的网格位置
        for (int i = 0; i < Items.Count; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as DescriptionItem;
            if (container != null)
            {
                Grid.SetRow(container, container.Row);
                Grid.SetColumn(container, container.Column);
                Grid.SetRowSpan(container, container.RowSpan > 0 ? container.RowSpan : 1);
                Grid.SetColumnSpan(container, container.ColumnSpan > 0 ? container.ColumnSpan : 1);
            }
        }
    }
}