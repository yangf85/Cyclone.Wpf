using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 瀑布流布局面板，支持横向和纵向两种布局模式，自动调整元素尺寸。
/// </summary>
/// <remarks>
/// <para>
/// WaterfallPanel提供了一个灵活的瀑布流布局实现，支持横向和纵向两种布局模式。
/// 主要设计思路是在主方向上（横向布局的行高或纵向布局的列宽）均分可用空间，
/// 而在次方向上（横向布局的宽度或纵向布局的高度）允许内容自由扩展或使用随机尺寸。
/// </para>
/// <para>
/// 使用方法:
/// <code>
/// &lt;WaterfallPanel Orientation="Vertical" Columns="3" Spacing="10" MinItemSize="50" MaxItemSize="200"&gt;
///     &lt;!-- 子元素内容 --&gt;
/// &lt;/WaterfallPanel&gt;
/// </code>
/// </para>
/// <para>
/// 主要属性:
/// - Orientation: 布局方向，可选Vertical（垂直）或Horizontal（水平）
/// - Columns: 纵向布局时的列数
/// - Rows: 横向布局时的行数
/// - Spacing: 元素之间的间距
/// - MinItemSize: 次方向上元素的最小尺寸
/// - MaxItemSize: 次方向上元素的最大尺寸
/// - FillLastItem: 是否拉伸每行或每列的最后一个元素以填充剩余空间
/// </para>
/// <para>
/// 布局算法:
/// 1. 在主方向上均分可用空间，不受MinItemSize和MaxItemSize影响 但是最小的高度*行数+间隙的高度需要小于可用高度
/// 2. 次方向上的尺寸在MinItemSize和MaxItemSize之间随机生成，或根据内容自适应
/// 3. 新元素总是添加到当前最短的行或列中，以保持整体布局平衡
/// 4. 支持可见性变化和动态添加/删除元素
/// </para>
/// </remarks>
public class WaterfallPanel : Panel
{
    #region 依赖属性

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WaterfallPanel),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsArrange));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(WaterfallPanel),
            new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsArrange));

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, Math.Max(1, value));
    }

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(nameof(Rows), typeof(int), typeof(WaterfallPanel),
            new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsArrange));

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, Math.Max(1, value));
    }

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(WaterfallPanel),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, Math.Max(0, value));
    }

    public static readonly DependencyProperty MinItemSizeProperty =
        DependencyProperty.Register(nameof(MinItemSize), typeof(double), typeof(WaterfallPanel),
            new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double MinItemSize
    {
        get => (double)GetValue(MinItemSizeProperty);
        set => SetValue(MinItemSizeProperty, Math.Max(0, value));
    }

    public static readonly DependencyProperty MaxItemSizeProperty =
        DependencyProperty.Register(nameof(MaxItemSize), typeof(double), typeof(WaterfallPanel),
            new FrameworkPropertyMetadata(200.0, FrameworkPropertyMetadataOptions.AffectsArrange));

    public double MaxItemSize
    {
        get => (double)GetValue(MaxItemSizeProperty);
        set => SetValue(MaxItemSizeProperty, value);
    }

    public static readonly DependencyProperty FillLastItemProperty =
        DependencyProperty.Register(nameof(FillLastItem), typeof(bool), typeof(WaterfallPanel),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange));

    public bool FillLastItem
    {
        get => (bool)GetValue(FillLastItemProperty);
        set => SetValue(FillLastItemProperty, value);
    }

    #endregion 依赖属性

    #region 私有字段

    private static readonly Random Random = new Random();
    private readonly List<double> _itemSizes = new List<double>();

    #endregion 私有字段

    #region 布局重写

    protected override Size MeasureOverride(Size availableSize)
    {
        if (InternalChildren.Count == 0)
            return new Size(0, 0);

        bool isVertical = Orientation == Orientation.Vertical;

        // 为每个元素生成次方向的尺寸
        _itemSizes.Clear();
        foreach (UIElement child in InternalChildren)
        {
            if (child.Visibility == Visibility.Collapsed)
            {
                _itemSizes.Add(0);
                continue;
            }

            _itemSizes.Add(GenerateRandomSize());
        }

        // 返回一个合理的期望尺寸
        int count = isVertical ? Columns : Rows;
        count = Math.Max(1, count);
        double spacing = Spacing;
        double totalSpacing = (count - 1) * spacing;

        if (isVertical)
        {
            double width = double.IsInfinity(availableSize.Width) ?
                count * MinItemSize + totalSpacing :
                availableSize.Width;

            double avgHeight = _itemSizes.Count > 0 ? _itemSizes.Average() : MinItemSize;
            double height = double.IsInfinity(availableSize.Height) ?
                avgHeight * Math.Ceiling((double)_itemSizes.Count / count) :
                availableSize.Height;

            return new Size(width, height);
        }
        else
        {
            double height = double.IsInfinity(availableSize.Height) ?
                count * MinItemSize + totalSpacing :
                availableSize.Height;

            double avgWidth = _itemSizes.Count > 0 ? _itemSizes.Average() : MinItemSize;
            double width = double.IsInfinity(availableSize.Width) ?
                avgWidth * Math.Ceiling((double)_itemSizes.Count / count) :
                availableSize.Width;

            return new Size(width, height);
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (InternalChildren.Count == 0 || _itemSizes.Count == 0)
            return finalSize;

        bool isVertical = Orientation == Orientation.Vertical;
        int count = isVertical ? Columns : Rows;
        count = Math.Max(1, count);

        double spacing = Spacing;
        double totalSpacing = (count - 1) * spacing;

        // 计算主方向上的尺寸（列宽或行高）
        // 始终均分可用空间，不受MinItemSize和MaxItemSize的影响
        double primarySize = (isVertical ?
            finalSize.Width - totalSpacing :
            finalSize.Height - totalSpacing) / count;

        // 确保主方向尺寸至少为1像素
        primarySize = Math.Max(1, primarySize);

        // 跟踪每条线的位置和最后一个元素
        double[] positions = new double[count];
        int[] lastItemIndices = new int[count];
        for (int i = 0; i < count; i++)
        {
            positions[i] = 0;
            lastItemIndices[i] = -1;
        }

        // 计算每个元素的位置
        List<Rect> arrangeRects = new List<Rect>(InternalChildren.Count);
        int visibleItemCount = 0;

        // 确保元素不会超出边界
        double maxSecondarySize = isVertical ? finalSize.Height : finalSize.Width;

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];
            if (child.Visibility == Visibility.Collapsed)
            {
                arrangeRects.Add(Rect.Empty);
                continue;
            }

            visibleItemCount++;

            // 找出当前位置最小的线
            int shortestIndex = FindShortestLine(positions);
            lastItemIndices[shortestIndex] = i;

            // 获取次方向尺寸
            double secondarySize = i < _itemSizes.Count ? _itemSizes[i] : GenerateRandomSize();

            // 约束次方向尺寸以确保在可见区域内
            if (!isVertical)
            {
                // 横向布局时约束宽度
                double maxWidth = finalSize.Width - positions[shortestIndex];
                if (positions[shortestIndex] > 0) maxWidth -= spacing;
                secondarySize = Math.Min(secondarySize, maxWidth);
                secondarySize = Math.Max(1, secondarySize);
            }
            else
            {
                // 纵向布局时约束高度
                double remainingHeight = maxSecondarySize - positions[shortestIndex];
                if (positions[shortestIndex] > 0) remainingHeight -= spacing;
                if (remainingHeight > 0 && secondarySize > remainingHeight)
                {
                    secondarySize = remainingHeight;
                }
                secondarySize = Math.Max(1, secondarySize);
            }

            // 计算元素位置
            double x, y;
            if (isVertical)
            {
                // 竖向布局：固定列宽，元素位于最短列
                x = shortestIndex * (primarySize + spacing);
                y = positions[shortestIndex];
                if (y > 0) y += spacing;
            }
            else
            {
                // 横向布局：固定行高，元素位于最短行
                x = positions[shortestIndex];
                if (x > 0) x += spacing;
                y = shortestIndex * (primarySize + spacing);
            }

            // 创建布局矩形
            Rect rect = isVertical
                ? new Rect(x, y, primarySize, secondarySize)
                : new Rect(x, y, secondarySize, primarySize);

            arrangeRects.Add(rect);

            // 更新位置
            positions[shortestIndex] = isVertical ? rect.Bottom : rect.Right;
        }

        // 处理FillLastItem，只有当每列/行都有元素时才执行
        if (FillLastItem && visibleItemCount >= count)
        {
            // 找出所有行/列的最后一个元素并填充
            for (int lineIndex = 0; lineIndex < count; lineIndex++)
            {
                int lastItemIndex = lastItemIndices[lineIndex];
                if (lastItemIndex >= 0 && lastItemIndex < arrangeRects.Count)
                {
                    Rect rect = arrangeRects[lastItemIndex];
                    if (isVertical)
                    {
                        double remainingHeight = finalSize.Height - rect.Top;
                        if (remainingHeight > 0)
                            rect.Height = remainingHeight;
                    }
                    else
                    {
                        double remainingWidth = finalSize.Width - rect.Left;
                        if (remainingWidth > 0)
                            rect.Width = remainingWidth;
                    }
                    arrangeRects[lastItemIndex] = rect;
                }
            }
        }

        // 测量和排列每个子元素
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];
            if (child.Visibility == Visibility.Collapsed)
                continue;

            if (i < arrangeRects.Count)
            {
                Rect rect = arrangeRects[i];

                // 确保不超出面板边界
                if (rect.Right > finalSize.Width)
                    rect.Width = Math.Max(1, finalSize.Width - rect.X);

                if (rect.Bottom > finalSize.Height)
                    rect.Height = Math.Max(1, finalSize.Height - rect.Y);

                // 测量和排列子元素
                child.Measure(new Size(rect.Width, rect.Height));
                child.Arrange(rect);
            }
        }

        return finalSize;
    }

    #endregion 布局重写

    #region 辅助方法

    /// <summary>
    /// 生成随机尺寸（在最小值和最大值之间）
    /// </summary>
    private double GenerateRandomSize()
    {
        double min = MinItemSize;
        double max = Math.Max(min, MaxItemSize);

        return min + Random.NextDouble() * (max - min);
    }

    /// <summary>
    /// 找出当前位置最小的线
    /// </summary>
    private static int FindShortestLine(double[] positions)
    {
        int shortestIndex = 0;
        double minPosition = positions[0];

        for (int i = 1; i < positions.Length; i++)
        {
            if (positions[i] < minPosition)
            {
                minPosition = positions[i];
                shortestIndex = i;
            }
        }

        return shortestIndex;
    }

    #endregion 辅助方法
}