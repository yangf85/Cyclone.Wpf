using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 自适应磁贴布局面板，用于创建类似Windows磁贴的UI布局。
/// 此面板将子元素以磁贴形式排列，支持不同大小的磁贴，并能够自动适应容器大小变化。
/// </summary>
/// <remarks>
/// <para>
/// <b>布局原理：</b>
/// TilePanel将布局区域划分为固定行列的网格，每个磁贴可以占据一个或多个网格单元。
/// 面板使用智能算法为每个子元素分配大小和位置，确保大小磁贴均匀分布，视觉效果美观。
/// 当容器大小变化时，所有磁贴会按比例缩放，但保持相对位置关系不变。
/// </para>
/// <para>
/// <b>使用方法：</b>
/// 通常作为ItemsControl的ItemsPanel使用，示例如下：
/// <code>
/// &lt;ItemsControl.ItemsPanel&gt;
///     &lt;ItemsPanelTemplate&gt;
///         &lt;cy:TilePanel Columns="10"
///                       Rows="12"
///                       MaxColumnSpan="4"
///                       MaxRowSpan="5"
///                       IsAutoFill="True"
///                       Spacing="5" /&gt;
///     &lt;/ItemsPanelTemplate&gt;
/// &lt;/ItemsControl.ItemsPanel&gt;
/// </code>
/// </para>
/// <para>
/// <b>主要属性：</b>
/// - Rows：布局的总行数，如果为0则自动计算。
/// - Columns：布局的总列数，如果为0则自动计算。
/// - Spacing：磁贴之间的间距。
/// - MaxRowSpan：单个磁贴可以跨越的最大行数。
/// - MaxColumnSpan：单个磁贴可以跨越的最大列数。
/// - IsAutoFill：是否自动填满可用空间，开启后会根据子元素数量优化布局并调整磁贴大小以填充空白区域。
/// </para>
/// <para>
/// <b>布局特性：</b>
/// - 自动生成不同大小的磁贴（如1x1, 2x1, 1x2, 2x2）。
/// - 大磁贴会均匀分布在布局中，避免聚集在某一区域。
/// - 特殊位置（如左上角、右上角等）会优先放置较大的磁贴。
/// - 支持完全自适应，磁贴会随容器大小变化而等比例缩放。
/// - 智能填充功能可以根据需要扩展磁贴大小，确保没有空白区域。
/// </para>
/// </remarks>
public class TilePanel : Panel
{
    #region 依赖属性

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register("Spacing", typeof(double), typeof(TilePanel),
            new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsArrange |
                                         FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double Spacing
    {
        get { return (double)GetValue(SpacingProperty); }
        set { SetValue(SpacingProperty, value); }
    }

    public static readonly DependencyProperty MaxRowSpanProperty =
       DependencyProperty.Register("MaxRowSpan", typeof(int), typeof(TilePanel),
           new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsArrange |
                                        FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int MaxRowSpan
    {
        get { return (int)GetValue(MaxRowSpanProperty); }
        set { SetValue(MaxRowSpanProperty, value); }
    }

    public static readonly DependencyProperty MaxColumnSpanProperty =
      DependencyProperty.Register("MaxColumnSpan", typeof(int), typeof(TilePanel),
          new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsArrange |
                                       FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int MaxColumnSpan
    {
        get { return (int)GetValue(MaxColumnSpanProperty); }
        set { SetValue(MaxColumnSpanProperty, value); }
    }

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register("Rows", typeof(int), typeof(TilePanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange |
                                         FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int Rows
    {
        get { return (int)GetValue(RowsProperty); }
        set { SetValue(RowsProperty, value); }
    }

    public int Columns
    {
        get { return (int)GetValue(ColumnsProperty); }
        set { SetValue(ColumnsProperty, value); }
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register("Columns", typeof(int), typeof(TilePanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsArrange |
                                         FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty IsAutoFillProperty =
        DependencyProperty.Register("IsAutoFill", typeof(bool), typeof(TilePanel),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange |
                                         FrameworkPropertyMetadataOptions.AffectsMeasure));

    public bool IsAutoFill
    {
        get { return (bool)GetValue(IsAutoFillProperty); }
        set { SetValue(IsAutoFillProperty, value); }
    }

    #endregion 依赖属性

    #region 布局数据

    // 保存每个子元素的行列跨度和位置
    private Dictionary<UIElement, TileInfo> _tileInfos;

    // 网格占位状态
    private bool[,] _occupiedCells;

    // 分配概率
    private static readonly int[] _sizeWeights = [70, 20, 10]; // 1x1, 2x1/1x2, 2x2的权重

    private class TileInfo
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
    }

    #endregion 布局数据

    public TilePanel()
    {
        _tileInfos = [];
    }

    /// <summary>
    /// 测量所有子元素并确定其大小
    /// </summary>

    protected override Size MeasureOverride(Size availableSize)
    {
        if (InternalChildren.Count == 0)
            return new Size(0, 0);

        // 如果启用了自动填充，根据子元素数量调整行列数
        if (IsAutoFill)
        {
            AdjustGridSizeForAutoFill();
        }

        // 初始化网格并分配磁贴位置（仅在必要时）
        if (_tileInfos.Count == 0 || _occupiedCells == null ||
            _occupiedCells.GetLength(0) != Rows || _occupiedCells.GetLength(1) != Columns)
        {
            InitializeGrid();
            AssignTileSizesAndPositions();
        }

        // 对于无限大小，返回0作为设计时的占位符，实际大小将在ArrangeOverride中处理
        if (double.IsPositiveInfinity(availableSize.Width) || double.IsNaN(availableSize.Width) ||
            double.IsPositiveInfinity(availableSize.Height) || double.IsNaN(availableSize.Height))
        {
            // 简单测量一下子元素以便它们能够计算自己的期望大小
            // 但这里不用设置固定的默认大小
            foreach (UIElement child in InternalChildren)
            {
                if (child != null && _tileInfos.TryGetValue(child, out TileInfo tileInfo))
                {
                    // 使用一个非常小的值来测量，之后会在Arrange阶段重新调整大小
                    child.Measure(new Size(10, 10));
                }
            }

            // 返回零大小，让布局系统知道我们可以使用任意大小
            return new Size(0, 0);
        }

        // 计算单元格尺寸 - 根据可用空间动态调整
        double cellWidth = (availableSize.Width - (Columns - 1) * Spacing) / Columns;
        double cellHeight = (availableSize.Height - (Rows - 1) * Spacing) / Rows;

        // 测量子元素
        foreach (UIElement child in InternalChildren)
        {
            if (child == null) continue;

            if (_tileInfos.TryGetValue(child, out TileInfo tileInfo))
            {
                double tileWidth = cellWidth * tileInfo.ColumnSpan + Spacing * (tileInfo.ColumnSpan - 1);
                double tileHeight = cellHeight * tileInfo.RowSpan + Spacing * (tileInfo.RowSpan - 1);
                child.Measure(new Size(tileWidth, tileHeight));
            }
        }

        // 返回所需大小
        return availableSize;
    }

    /// <summary>
    /// 布局所有子元素
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count == 0)
            return finalSize;

        // 动态计算单元格尺寸 - 随窗口大小变化而变化
        double cellWidth = (finalSize.Width - (Columns - 1) * Spacing) / Columns;
        double cellHeight = (finalSize.Height - (Rows - 1) * Spacing) / Rows;

        // 设置每个子元素的位置
        foreach (UIElement child in InternalChildren)
        {
            if (child == null) continue;

            if (_tileInfos.TryGetValue(child, out TileInfo tileInfo))
            {
                double x = tileInfo.Column * (cellWidth + Spacing);
                double y = tileInfo.Row * (cellHeight + Spacing);
                double width = cellWidth * tileInfo.ColumnSpan + Spacing * (tileInfo.ColumnSpan - 1);
                double height = cellHeight * tileInfo.RowSpan + Spacing * (tileInfo.RowSpan - 1);

                child.Arrange(new Rect(x, y, width, height));
            }
        }

        return finalSize;
    }

    /// <summary>
    /// 当面板子元素发生变化时重新计算布局
    /// </summary>
    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);

        // 子元素发生变化时，清除现有布局信息，强制重新计算
        _tileInfos.Clear();
        InvalidateMeasure();
    }

    /// <summary>
    /// 根据子元素数量调整网格大小，确保填满所有空间
    /// </summary>
    private void AdjustGridSizeForAutoFill()
    {
        int itemCount = InternalChildren.Count;

        if (itemCount == 0)
            return;

        // 如果行列数都已指定，但子元素不足以填满，则减少行列数
        if (Rows > 0 && Columns > 0)
        {
            int totalCells = Rows * Columns;
            int minRequiredCells = CalculateMinRequiredCells();

            if (minRequiredCells < totalCells)
            {
                // 计算更适合的行列数
                double aspectRatio = (double)Columns / Rows; // 保持原有宽高比
                int newTotalCells = Math.Max(minRequiredCells, itemCount);

                // 计算新的行列数，尽量保持原有比例
                Columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(newTotalCells * aspectRatio)));
                Rows = Math.Max(1, (int)Math.Ceiling((double)newTotalCells / Columns));
            }
        }
        else
        {
            // 如果行列数未指定，计算合适的行列数
            double aspectRatio = 1.5; // 默认宽高比，列数略多于行数

            if (Columns > 0 && Rows <= 0)
            {
                // 列数已指定，计算合适的行数
                Rows = Math.Max(1, (int)Math.Ceiling((double)itemCount / Columns));
            }
            else if (Rows > 0 && Columns <= 0)
            {
                // 行数已指定，计算合适的列数
                Columns = Math.Max(1, (int)Math.Ceiling((double)itemCount / Rows));
            }
            else
            {
                // 行列数都未指定，根据子元素数量计算合适的行列数
                Columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(itemCount * aspectRatio)));
                Rows = Math.Max(1, (int)Math.Ceiling((double)itemCount / Columns));
            }
        }
    }

    /// <summary>
    /// 计算至少需要多少个单元格才能容纳所有子元素
    /// 考虑到一些磁贴可能占用多个单元格
    /// </summary>
    private int CalculateMinRequiredCells()
    {
        int itemCount = InternalChildren.Count;

        // 估算大磁贴的数量
        int largeItemCount = (int)(itemCount * 0.3); // 假设30%的磁贴是大磁贴
        int regularItemCount = itemCount - largeItemCount;

        // 估算大磁贴平均占用的单元格
        double avgLargeItemCellCount = 2.5; // 假设平均每个大磁贴占用2.5个单元格

        // 计算总的单元格需求
        return regularItemCount + (int)(largeItemCount * avgLargeItemCellCount);
    }

    /// <summary>
    /// 初始化网格
    /// </summary>
    private void InitializeGrid()
    {
        _tileInfos.Clear();

        // 确保行列数有效
        if (Rows <= 0 || Columns <= 0)
        {
            int itemCount = InternalChildren.Count;
            double ratio = Math.Sqrt(itemCount) * 0.75; // 控制每行显示的磁贴数约为总数的0.75倍平方根

            if (Columns <= 0)
                Columns = Math.Max(1, (int)Math.Ceiling(ratio * 1.5)); // 列数略多

            if (Rows <= 0)
                Rows = Math.Max(1, (int)Math.Ceiling((double)itemCount / Columns));
        }

        // 初始化占位网格
        _occupiedCells = new bool[Rows, Columns];
        for (int i = 0; i < Rows; i++)
            for (int j = 0; j < Columns; j++)
                _occupiedCells[i, j] = false;
    }

    /// <summary>
    /// 分配磁贴大小和位置
    /// </summary>
    private void AssignTileSizesAndPositions()
    {
        Random random = new Random(InternalChildren.Count); // 使用固定种子保证布局一致性
        List<UIElement> children = new List<UIElement>();

        // 将子元素添加到列表中以便处理
        foreach (UIElement child in InternalChildren)
        {
            if (child != null)
                children.Add(child);
        }

        // 特殊位置列表 - 优先放置大磁贴的位置
        List<Point> specialPositions = GetSpecialPositions();
        int specialPosIndex = 0;

        // 先处理特殊位置的磁贴
        while (specialPosIndex < specialPositions.Count && children.Count > 0)
        {
            Point pos = specialPositions[specialPosIndex++];
            int row = (int)pos.Y;
            int col = (int)pos.X;

            // 检查该位置是否可以放置大磁贴
            if (IsAreaFree(row, col, 2, 2) && children.Count > 0)
            {
                // 在特殊位置放置2x2的磁贴
                UIElement child = children[0];
                children.RemoveAt(0);

                _tileInfos[child] = new TileInfo
                {
                    Row = row,
                    Column = col,
                    RowSpan = 2,
                    ColumnSpan = 2
                };

                // 标记占用
                MarkAreaOccupied(row, col, 2, 2);
            }
            else if (IsAreaFree(row, col, 2, 1) && children.Count > 0)
            {
                // 在特殊位置放置2x1的磁贴
                UIElement child = children[0];
                children.RemoveAt(0);

                _tileInfos[child] = new TileInfo
                {
                    Row = row,
                    Column = col,
                    RowSpan = 2,
                    ColumnSpan = 1
                };

                // 标记占用
                MarkAreaOccupied(row, col, 2, 1);
            }
            else if (IsAreaFree(row, col, 1, 2) && children.Count > 0)
            {
                // 在特殊位置放置1x2的磁贴
                UIElement child = children[0];
                children.RemoveAt(0);

                _tileInfos[child] = new TileInfo
                {
                    Row = row,
                    Column = col,
                    RowSpan = 1,
                    ColumnSpan = 2
                };

                // 标记占用
                MarkAreaOccupied(row, col, 1, 2);
            }
        }

        // 随机处理剩余的磁贴，确保均匀分布
        int remainingLargeCount = Math.Max(0, (int)(children.Count * 0.3)); // 大磁贴最多占总数的30%

        // 计算网格区域划分，用于均匀分布大磁贴
        int rowSections = Math.Max(1, Rows / 3);
        int colSections = Math.Max(1, Columns / 3);

        // 先分配一些大磁贴，确保它们均匀分布
        if (remainingLargeCount > 0)
        {
            for (int rs = 0; rs < rowSections && remainingLargeCount > 0 && children.Count > 0; rs++)
            {
                for (int cs = 0; cs < colSections && remainingLargeCount > 0 && children.Count > 0; cs++)
                {
                    int startRow = rs * (Rows / rowSections);
                    int startCol = cs * (Columns / colSections);

                    // 在区域中尝试找一个位置放置大磁贴
                    bool placedInSection = false;

                    for (int r = startRow; r < startRow + Rows / rowSections && r < Rows - 1 && !placedInSection; r++)
                    {
                        for (int c = startCol; c < startCol + Columns / colSections && c < Columns - 1 && !placedInSection; c++)
                        {
                            if (IsAreaFree(r, c, 2, 2) && random.Next(100) < 50) // 50%概率放2x2
                            {
                                UIElement child = children[0];
                                children.RemoveAt(0);

                                _tileInfos[child] = new TileInfo
                                {
                                    Row = r,
                                    Column = c,
                                    RowSpan = 2,
                                    ColumnSpan = 2
                                };

                                MarkAreaOccupied(r, c, 2, 2);
                                remainingLargeCount--;
                                placedInSection = true;
                            }
                            else if (IsAreaFree(r, c, 2, 1) && random.Next(100) < 30 && !placedInSection) // 30%概率放2x1
                            {
                                UIElement child = children[0];
                                children.RemoveAt(0);

                                _tileInfos[child] = new TileInfo
                                {
                                    Row = r,
                                    Column = c,
                                    RowSpan = 2,
                                    ColumnSpan = 1
                                };

                                MarkAreaOccupied(r, c, 2, 1);
                                remainingLargeCount--;
                                placedInSection = true;
                            }
                            else if (IsAreaFree(r, c, 1, 2) && random.Next(100) < 30 && !placedInSection) // 30%概率放1x2
                            {
                                UIElement child = children[0];
                                children.RemoveAt(0);

                                _tileInfos[child] = new TileInfo
                                {
                                    Row = r,
                                    Column = c,
                                    RowSpan = 1,
                                    ColumnSpan = 2
                                };

                                MarkAreaOccupied(r, c, 1, 2);
                                remainingLargeCount--;
                                placedInSection = true;
                            }
                        }
                    }
                }
            }
        }

        // 处理剩余的所有磁贴，填充网格
        foreach (UIElement child in children)
        {
            // 根据权重和剩余大磁贴数随机决定磁贴大小
            int size = GetRandomTileSize(random, remainingLargeCount > 0);
            if (size > 0) remainingLargeCount--;

            // 分配大小
            int rowSpan = 1;
            int colSpan = 1;

            switch (size)
            {
                case 1: // 2x1
                    rowSpan = 2;
                    break;

                case 2: // 1x2
                    colSpan = 2;
                    break;

                case 3: // 2x2
                    rowSpan = 2;
                    colSpan = 2;
                    break;
            }

            // 限制跨度不超过最大值
            rowSpan = Math.Min(rowSpan, MaxRowSpan);
            colSpan = Math.Min(colSpan, MaxColumnSpan);

            // 查找可用位置
            bool found = FindFreePosition(out int row, out int col, rowSpan, colSpan);

            // 如果找不到位置，尝试降级磁贴大小
            if (!found && (rowSpan > 1 || colSpan > 1))
            {
                if (rowSpan > 1 && colSpan > 1)
                {
                    // 先尝试2x1
                    if (FindFreePosition(out row, out col, 2, 1))
                    {
                        rowSpan = 2;
                        colSpan = 1;
                        found = true;
                    }
                    // 再尝试1x2
                    else if (FindFreePosition(out row, out col, 1, 2))
                    {
                        rowSpan = 1;
                        colSpan = 2;
                        found = true;
                    }
                }
                else if (rowSpan > 1)
                {
                    // 尝试降为1x1
                    if (FindFreePosition(out row, out col, 1, 1))
                    {
                        rowSpan = 1;
                        found = true;
                    }
                }
                else if (colSpan > 1)
                {
                    // 尝试降为1x1
                    if (FindFreePosition(out row, out col, 1, 1))
                    {
                        colSpan = 1;
                        found = true;
                    }
                }
            }

            // 如果还是找不到位置，创建新行/列
            if (!found && IsAutoFill)
            {
                // 扩展网格
                ExpandGridIfNeeded(rowSpan, colSpan);

                // 重新尝试查找位置
                found = FindFreePosition(out row, out col, rowSpan, colSpan);

                // 如果还是找不到，最后尝试1x1
                if (!found && (rowSpan > 1 || colSpan > 1))
                {
                    if (FindFreePosition(out row, out col, 1, 1))
                    {
                        rowSpan = 1;
                        colSpan = 1;
                        found = true;
                    }
                }
            }

            // 如果仍然找不到，放弃处理该元素
            if (!found) continue;

            // 保存磁贴信息
            _tileInfos[child] = new TileInfo
            {
                Row = row,
                Column = col,
                RowSpan = rowSpan,
                ColumnSpan = colSpan
            };

            // 标记占用
            MarkAreaOccupied(row, col, rowSpan, colSpan);
        }

        // 如果开启了自动填充，尝试填充所有空白区域
        if (IsAutoFill)
        {
            FillEmptyAreas();
        }
    }

    /// <summary>
    /// 如果需要，扩展网格大小
    /// </summary>
    private void ExpandGridIfNeeded(int requiredRowSpan, int requiredColSpan)
    {
        // 检查网格是否已满
        bool gridFull = true;

        for (int r = 0; r <= Rows - requiredRowSpan; r++)
        {
            for (int c = 0; c <= Columns - requiredColSpan; c++)
            {
                if (IsAreaFree(r, c, requiredRowSpan, requiredColSpan))
                {
                    gridFull = false;
                    break;
                }
            }
            if (!gridFull) break;
        }

        // 如果网格已满，扩展网格
        if (gridFull)
        {
            // 创建更大的网格
            int newRows = Rows;
            int newColumns = Columns;

            // 按需增加行或列
            if (requiredRowSpan > 1 && requiredColSpan > 1)
            {
                // 增加行和列，保持比例
                newRows += 1;
                newColumns += 1;
            }
            else if (requiredRowSpan > 1)
            {
                // 只增加行
                newRows += 1;
            }
            else if (requiredColSpan > 1)
            {
                // 只增加列
                newColumns += 1;
            }
            else
            {
                // 都是1x1，取决于当前行列比例
                if (Rows <= Columns)
                    newRows += 1;
                else
                    newColumns += 1;
            }

            // 创建新的占位网格
            bool[,] newOccupiedCells = new bool[newRows, newColumns];

            // 复制旧数据
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    newOccupiedCells[r, c] = _occupiedCells[r, c];
                }
            }

            // 新增的单元格默认为未占用
            for (int r = Rows; r < newRows; r++)
            {
                for (int c = 0; c < newColumns; c++)
                {
                    newOccupiedCells[r, c] = false;
                }
            }

            for (int r = 0; r < Rows; r++)
            {
                for (int c = Columns; c < newColumns; c++)
                {
                    newOccupiedCells[r, c] = false;
                }
            }

            // 更新网格
            _occupiedCells = newOccupiedCells;
            Rows = newRows;
            Columns = newColumns;
        }
    }

    /// <summary>
    /// 尝试填充所有空白区域，调整现有磁贴大小
    /// </summary>
    private void FillEmptyAreas()
    {
        // 找出所有已分配磁贴
        List<KeyValuePair<UIElement, TileInfo>> assignedTiles = new List<KeyValuePair<UIElement, TileInfo>>();

        foreach (var kvp in _tileInfos)
        {
            assignedTiles.Add(kvp);
        }

        // 尝试扩展现有磁贴来填充空白
        foreach (var tile in assignedTiles)
        {
            TileInfo info = tile.Value;

            // 尝试向右扩展
            if (info.ColumnSpan < MaxColumnSpan)
            {
                int newColSpan = info.ColumnSpan;
                while (newColSpan < MaxColumnSpan && info.Column + newColSpan < Columns)
                {
                    bool canExpand = true;
                    for (int r = info.Row; r < info.Row + info.RowSpan; r++)
                    {
                        if (r < Rows && info.Column + newColSpan < Columns)
                        {
                            if (_occupiedCells[r, info.Column + newColSpan])
                            {
                                canExpand = false;
                                break;
                            }
                        }
                        else
                        {
                            canExpand = false;
                            break;
                        }
                    }

                    if (canExpand)
                    {
                        newColSpan++;
                    }
                    else
                    {
                        break;
                    }
                }

                // 扩展并标记占用
                if (newColSpan > info.ColumnSpan)
                {
                    for (int r = info.Row; r < info.Row + info.RowSpan; r++)
                    {
                        for (int c = info.Column + info.ColumnSpan; c < info.Column + newColSpan; c++)
                        {
                            _occupiedCells[r, c] = true;
                        }
                    }
                    info.ColumnSpan = newColSpan;
                }
            }

            // 尝试向下扩展
            if (info.RowSpan < MaxRowSpan)
            {
                int newRowSpan = info.RowSpan;
                while (newRowSpan < MaxRowSpan && info.Row + newRowSpan < Rows)
                {
                    bool canExpand = true;
                    for (int c = info.Column; c < info.Column + info.ColumnSpan; c++)
                    {
                        if (c < Columns && info.Row + newRowSpan < Rows)
                        {
                            if (_occupiedCells[info.Row + newRowSpan, c])
                            {
                                canExpand = false;
                                break;
                            }
                        }
                        else
                        {
                            canExpand = false;
                            break;
                        }
                    }

                    if (canExpand)
                    {
                        newRowSpan++;
                    }
                    else
                    {
                        break;
                    }
                }

                // 扩展并标记占用
                if (newRowSpan > info.RowSpan)
                {
                    for (int r = info.Row + info.RowSpan; r < info.Row + newRowSpan; r++)
                    {
                        for (int c = info.Column; c < info.Column + info.ColumnSpan; c++)
                        {
                            _occupiedCells[r, c] = true;
                        }
                    }
                    info.RowSpan = newRowSpan;
                }
            }
        }
    }

    /// <summary>
    /// 获取特殊位置列表（优先放置大磁贴的位置）
    /// </summary>
    private List<Point> GetSpecialPositions()
    {
        var positions = new List<Point>();

        // 左上角
        positions.Add(new Point(0, 0));

        // 右上角
        if (Columns >= 3)
            positions.Add(new Point(Columns - 2, 0));

        // 左下角
        if (Rows >= 3)
            positions.Add(new Point(0, Rows - 2));

        // 中间位置
        if (Rows >= 4 && Columns >= 4)
            positions.Add(new Point(Columns / 2 - 1, Rows / 2 - 1));

        return positions;
    }

    /// <summary>
    /// 检查指定区域是否空闲
    /// </summary>
    private bool IsAreaFree(int startRow, int startCol, int rowSpan, int colSpan)
    {
        // 检查边界
        if (startRow < 0 || startCol < 0 || startRow + rowSpan > Rows || startCol + colSpan > Columns)
            return false;

        // 检查每个单元格
        for (int r = startRow; r < startRow + rowSpan; r++)
        {
            for (int c = startCol; c < startCol + colSpan; c++)
            {
                if (_occupiedCells[r, c])
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 标记指定区域为已占用
    /// </summary>
    private void MarkAreaOccupied(int startRow, int startCol, int rowSpan, int colSpan)
    {
        for (int r = startRow; r < startRow + rowSpan; r++)
        {
            for (int c = startCol; c < startCol + colSpan; c++)
            {
                _occupiedCells[r, c] = true;
            }
        }
    }

    /// <summary>
    /// 查找可用于放置磁贴的位置
    /// </summary>
    private bool FindFreePosition(out int row, out int col, int rowSpan, int colSpan)
    {
        // 初始化输出参数
        row = -1;
        col = -1;

        // 首先尝试"蛇形"搜索 - 从左上到右下，但跳过一些位置以增加分散性
        for (int r = 0; r < Rows; r += 2)
        {
            // 偶数行从左到右
            for (int c = 0; c < Columns; c += 2)
            {
                if (IsAreaFree(r, c, rowSpan, colSpan))
                {
                    row = r;
                    col = c;
                    return true;
                }
            }
        }

        for (int r = 1; r < Rows; r += 2)
        {
            // 奇数行从右到左
            for (int c = Columns - 1; c >= 0; c -= 2)
            {
                if (IsAreaFree(r, c, rowSpan, colSpan))
                {
                    row = r;
                    col = c;
                    return true;
                }
            }
        }

        // 如果上面没找到，进行完整搜索
        for (int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (IsAreaFree(r, c, rowSpan, colSpan))
                {
                    row = r;
                    col = c;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// 根据权重随机生成磁贴大小
    /// 0: 1x1, 1: 2x1, 2: 1x2, 3: 2x2
    /// </summary>
    private int GetRandomTileSize(Random random, bool allowLarge)
    {
        if (!allowLarge)
            return 0; // 只返回1x1

        int totalWeight = 0;
        for (int i = 0; i < _sizeWeights.Length; i++)
            totalWeight += _sizeWeights[i];

        int value = random.Next(totalWeight);
        int sum = 0;

        for (int i = 0; i < _sizeWeights.Length; i++)
        {
            sum += _sizeWeights[i];
            if (value < sum)
            {
                // 将索引转换为实际大小
                return i switch
                {
                    0 => 0,// 1x1
                    1 => random.Next(2) == 0 ? 1 : 2,// 随机返回2x1或1x2
                    2 => 3,// 2x2
                    _ => 0,
                };
            }
        }

        return 0;
    }
}