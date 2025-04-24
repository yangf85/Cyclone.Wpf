using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 可配置行列间距的均匀网格面板控件，提供比UniformGrid更丰富的布局选项。
/// 支持水平和垂直方向的不同堆叠模式，以及可自定义的项目间距。
/// </summary>
/// <remarks>
/// <para>实现思路：</para>
/// <para>1. 支持四种堆叠模式：None(默认)、Horizontal、Vertical和Both(两者结合)</para>
/// <para>2. 通过HorizontalStackMode和VerticalStackMode属性组合决定使用哪种堆叠模式</para>
/// <para>3. 每种模式有不同的测量和排列策略：</para>
/// <para>   - None模式：所有元素大小相同，均匀分布</para>
/// <para>   - Horizontal模式：每列宽度根据其中最宽的元素确定，行高统一</para>
/// <para>   - Vertical模式：列宽统一，但每列中的元素可以有不同高度，垂直堆叠</para>
/// <para>   - Both模式：结合水平和垂直特性，列宽和行高都可变</para>
/// <para>4. 使用HorizontalSpacing和VerticalSpacing控制元素之间的间距</para>
/// <para>5. 可通过FirstColumn属性指定起始列，实现部分偏移效果</para>
/// </remarks>
/// <example>
/// <code>
/// &lt;SpacingUniformGrid Rows="3" Columns="4"
///                      HorizontalSpacing="10"
///                      VerticalSpacing="15"
///                      HorizontalStackMode="True"&gt;
///     &lt;Button Content="1"/&gt;
///     &lt;Button Content="2"/&gt;
///     &lt;TextBlock Text="3" Background="LightBlue"/&gt;
///     &lt;!-- 更多子元素 --&gt;
/// &lt;/SpacingUniformGrid&gt;
/// </code>
/// </example>
public class SpacingUniformGrid : Panel
{
    #region 私有字段

    private int _columns;
    private double[] _columnWidths;
    private double _horizontalSpacing;
    private int _rows;
    private StackMode _stackMode;
    private double _totalSpacingHeight;
    private double _totalSpacingWidth;
    private double _verticalSpacing;

    #endregion 私有字段

    #region 依赖属性

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty FirstColumnProperty =
        DependencyProperty.Register(nameof(FirstColumn), typeof(int), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int FirstColumn
    {
        get => (int)GetValue(FirstColumnProperty);
        set => SetValue(FirstColumnProperty, value);
    }

    public static readonly DependencyProperty HorizontalSpacingProperty =
        DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double HorizontalSpacing
    {
        get => (double)GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    public static readonly DependencyProperty HorizontalStackModeProperty =
        DependencyProperty.Register(nameof(HorizontalStackMode), typeof(bool), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public bool HorizontalStackMode
    {
        get => (bool)GetValue(HorizontalStackModeProperty);
        set => SetValue(HorizontalStackModeProperty, value);
    }

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(nameof(Rows), typeof(int), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public static readonly DependencyProperty VerticalSpacingProperty =
        DependencyProperty.Register(nameof(VerticalSpacing), typeof(double), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(5d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double VerticalSpacing
    {
        get => (double)GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    public static readonly DependencyProperty VerticalStackModeProperty =
        DependencyProperty.Register(nameof(VerticalStackMode), typeof(bool), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public bool VerticalStackMode
    {
        get => (bool)GetValue(VerticalStackModeProperty);
        set => SetValue(VerticalStackModeProperty, value);
    }

    #endregion 依赖属性

    #region 枚举定义

    [Flags]
    private enum StackMode
    {
        None = 0,
        Horizontal = 1,
        Vertical = 2,
        Both = Horizontal | Vertical
    }

    #endregion 枚举定义

    #region 测量与布局

    protected override Size MeasureOverride(Size constraint)
    {
        UpdateComputedValues();

        return _stackMode switch
        {
            StackMode.Vertical => MeasureStackVertical(constraint),
            StackMode.Horizontal => MeasureStackHorizontal(constraint),
            StackMode.Both => MeasureStackBoth(constraint),
            _ => MeasureStackNone(constraint)
        };
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        var children = InternalChildren;

        switch (_stackMode)
        {
            case StackMode.Vertical:
                ArrangeStackVertical(arrangeSize, children);
                break;

            case StackMode.Horizontal:
                ArrangeStackHorizontal(arrangeSize, children);
                break;

            case StackMode.Both:
                ArrangeStackBoth(arrangeSize, children);
                break;

            case StackMode.None:
                ArrangeStackNone(arrangeSize, children);
                break;
        }

        return arrangeSize;
    }

    #endregion 测量与布局

    #region 私有辅助方法

    private void UpdateComputedValues()
    {
        // 根据标志位计算堆叠模式
        _stackMode = (VerticalStackMode ? StackMode.Vertical : StackMode.None) |
                    (HorizontalStackMode ? StackMode.Horizontal : StackMode.None);

        _columns = Columns;
        _rows = Rows;

        // 确保FirstColumn不超过列数
        if (FirstColumn >= _columns)
        {
            FirstColumn = 0;
        }

        // 计算行列数
        CalculateRowsAndColumns();

        // 设置间距
        _horizontalSpacing = HorizontalSpacing;
        _verticalSpacing = VerticalSpacing;

        // 计算总间距
        _totalSpacingWidth = _columns <= 1 ? 0 : _horizontalSpacing * (_columns - 1);
        _totalSpacingHeight = _rows <= 1 ? 0 : _verticalSpacing * (_rows - 1);

        // 如果需要水平堆叠，初始化列宽数组
        if ((_stackMode & StackMode.Horizontal) != 0)
        {
            _columnWidths = new double[_columns];
        }
    }

    private void CalculateRowsAndColumns()
    {
        if (_rows != 0 && _columns != 0) return;

        // 计算非折叠元素数量
        var nonCollapsedCount = CountNonCollapsedChildren();

        if (nonCollapsedCount == 0)
        {
            nonCollapsedCount = 1;
        }

        // 计算行数和列数
        if (_rows == 0)
        {
            if (_columns > 0)
            {
                _rows = (nonCollapsedCount + FirstColumn + (_columns - 1)) / _columns;
            }
            else
            {
                _rows = (int)Math.Sqrt(nonCollapsedCount);
                if (_rows * _rows < nonCollapsedCount)
                {
                    _rows++;
                }
                _columns = _rows;
            }
        }
        else if (_columns == 0)
        {
            _columns = (nonCollapsedCount + (_rows - 1)) / _rows;
        }
    }

    private int CountNonCollapsedChildren()
    {
        var count = 0;
        for (int i = 0, total = InternalChildren.Count; i < total; ++i)
        {
            if (InternalChildren[i].Visibility != Visibility.Collapsed)
            {
                count++;
            }
        }
        return count;
    }

    #region 测量方法

    private Size MeasureStackNone(Size constraint)
    {
        // 计算子元素约束
        var childConstraint = CalculateChildConstraint(constraint, false, false);
        var maxSize = MeasureChildren(childConstraint);

        // 计算总尺寸
        return new Size(
            maxSize.Width * _columns + _totalSpacingWidth,
            maxSize.Height * _rows + _totalSpacingHeight);
    }

    private Size MeasureStackVertical(Size constraint)
    {
        var childConstraint = CalculateChildConstraint(constraint, false, true);
        var stackSizes = new List<double>();
        var maxSize = new Size();
        var itemsInRow = 0;

        // 遍历测量每个子元素
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];
            child.Measure(childConstraint);
            var childSize = child.DesiredSize;

            // 确保列表有足够的空间
            if (stackSizes.Count <= itemsInRow)
            {
                stackSizes.Add(0d);
            }

            // 累加高度并更新最大值
            stackSizes[itemsInRow] += childSize.Height + _verticalSpacing;
            maxSize.Height = Math.Max(maxSize.Height, stackSizes[itemsInRow]);
            maxSize.Width = Math.Max(maxSize.Width, childSize.Width);

            // 切换到下一列
            if (++itemsInRow == _columns)
            {
                itemsInRow = 0;
            }
        }

        // 返回总尺寸
        return new Size(
            maxSize.Width * _columns + _totalSpacingWidth,
            maxSize.Height);
    }

    private Size MeasureStackHorizontal(Size constraint)
    {
        var childConstraint = CalculateChildConstraint(constraint, true, false);
        InitializeColumnWidths();

        var maxHeight = 0d;
        var itemsInRow = 0;

        // 测量每个子元素
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];
            child.Measure(childConstraint);
            var childSize = child.DesiredSize;

            // 更新列宽和最大高度
            _columnWidths[itemsInRow] = Math.Max(_columnWidths[itemsInRow], childSize.Width);
            maxHeight = Math.Max(maxHeight, childSize.Height);

            // 切换到下一列
            if (++itemsInRow == _columns)
            {
                itemsInRow = 0;
            }
        }

        // 计算总宽度
        var totalWidth = CalculateTotalColumnWidth();

        // 返回总尺寸
        return new Size(totalWidth, maxHeight * _rows + _totalSpacingHeight);
    }

    private Size MeasureStackBoth(Size constraint)
    {
        var childConstraint = CalculateChildConstraint(constraint, true, false);
        InitializeColumnWidths();

        var totalHeight = 0d;
        var rowHeight = 0d;
        var itemsInRow = 0;

        // 测量每个子元素
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];
            child.Measure(childConstraint);
            var childSize = child.DesiredSize;

            // 更新列宽和行高
            _columnWidths[itemsInRow] = Math.Max(_columnWidths[itemsInRow], childSize.Width);
            rowHeight = Math.Max(rowHeight, childSize.Height);

            // 切换到下一行
            if (++itemsInRow == _columns)
            {
                itemsInRow = 0;
                totalHeight += rowHeight;
                rowHeight = 0d;
            }
        }

        // 计算总宽度
        var totalWidth = CalculateTotalColumnWidth();

        // 返回总尺寸
        return new Size(totalWidth, totalHeight + _totalSpacingHeight);
    }

    private void InitializeColumnWidths()
    {
        for (var i = 0; i < _columnWidths.Length; i++)
        {
            _columnWidths[i] = 0d;
        }
    }

    private double CalculateTotalColumnWidth()
    {
        var sum = _totalSpacingWidth;
        for (var i = 0; i < _columnWidths.Length; i++)
        {
            sum += _columnWidths[i];
        }
        return sum;
    }

    private Size CalculateChildConstraint(Size constraint, bool useFixedHeight, bool useFixedWidth)
    {
        var width = useFixedWidth
            ? Math.Max(constraint.Width - _totalSpacingWidth, 0) / _columns
            : double.PositiveInfinity;

        var height = useFixedHeight
            ? Math.Max(constraint.Height - _totalSpacingHeight, 0) / _rows
            : double.PositiveInfinity;

        return new Size(width, height);
    }

    private Size MeasureChildren(Size childConstraint)
    {
        var maxSize = new Size();

        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];
            child.Measure(childConstraint);
            var childSize = child.DesiredSize;

            maxSize.Width = Math.Max(maxSize.Width, childSize.Width);
            maxSize.Height = Math.Max(maxSize.Height, childSize.Height);
        }

        return maxSize;
    }

    #endregion 测量方法

    #region 布局方法

    private void ArrangeStackNone(Size finalSize, UIElementCollection children)
    {
        var childBounds = new Rect(
            0, 0,
            Math.Max(finalSize.Width - _totalSpacingWidth, 0) / _columns,
            Math.Max(finalSize.Height - _totalSpacingHeight, 0) / _rows);

        var xStep = childBounds.Width + _horizontalSpacing;
        var yStep = childBounds.Height + _verticalSpacing;
        var xBound = finalSize.Width - 1.0;

        // 应用FirstColumn偏移
        childBounds.X += xStep * FirstColumn;

        ArrangeChildrenWithSimpleStep(children, childBounds, xStep, yStep, xBound);
    }

    private void ArrangeStackVertical(Size finalSize, UIElementCollection children)
    {
        var stackSizes = new List<double>();
        var childBounds = new Rect(
            0, 0,
            Math.Max(finalSize.Width - _totalSpacingWidth, 0) / _columns,
            0);

        var xStep = childBounds.Width + _horizontalSpacing;
        var xBound = finalSize.Width - 1.0;

        // 应用FirstColumn偏移
        childBounds.X += xStep * FirstColumn;
        var column = 0;

        for (var i = 0; i < children.Count; i++)
        {
            // 确保列表有足够空间
            if (column >= stackSizes.Count)
            {
                stackSizes.Add(0d);
            }

            var child = children[i];
            childBounds.Y = stackSizes[column];
            childBounds.Height = child.DesiredSize.Height;
            child.Arrange(childBounds);

            if (child.Visibility != Visibility.Collapsed)
            {
                childBounds.X += xStep;
                stackSizes[column] += childBounds.Height + _verticalSpacing;

                // 处理行溢出
                if (childBounds.X >= xBound)
                {
                    childBounds.X = 0;
                    column = 0;
                }
                else
                {
                    ++column;
                }
            }
        }
    }

    private void ArrangeStackHorizontal(Size finalSize, UIElementCollection children)
    {
        var childBounds = new Rect(
            0, 0,
            0,
            Math.Max(finalSize.Height - _totalSpacingHeight, 0) / _rows);

        var yStep = childBounds.Height + _verticalSpacing;
        var xBound = finalSize.Width - 1.0;

        // 应用FirstColumn偏移
        var firstColumn = FirstColumn;
        for (var i = 0; i < _columnWidths.Length && i < firstColumn; i++)
        {
            childBounds.X += _columnWidths[i] + _horizontalSpacing;
        }

        var column = 0;
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            childBounds.Width = child.DesiredSize.Width;
            child.Arrange(childBounds);

            if (child.Visibility != Visibility.Collapsed)
            {
                childBounds.X += _columnWidths[column++] + _horizontalSpacing;

                // 处理行溢出
                if (childBounds.X >= xBound)
                {
                    childBounds.Y += yStep;
                    childBounds.X = 0;
                }

                // 重置列计数
                if (column == _columns)
                {
                    column = 0;
                }
            }
        }
    }

    private void ArrangeStackBoth(Size finalSize, UIElementCollection children)
    {
        var childBounds = new Rect(0, 0, 0, 0);
        var xBound = finalSize.Width - 1.0;
        var maxHeight = 0d;

        // 应用FirstColumn偏移
        var firstColumn = FirstColumn;
        for (var i = 0; i < _columnWidths.Length && i < firstColumn; i++)
        {
            childBounds.X += _columnWidths[i] + _horizontalSpacing;
        }

        var column = 0;
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            childBounds.Height = child.DesiredSize.Height;
            childBounds.Width = child.DesiredSize.Width;
            child.Arrange(childBounds);

            if (child.Visibility != Visibility.Collapsed)
            {
                childBounds.X += _columnWidths[column++] + _horizontalSpacing;
                maxHeight = Math.Max(maxHeight, childBounds.Height);

                // 处理行溢出
                if (childBounds.X >= xBound)
                {
                    childBounds.Y += maxHeight + _verticalSpacing;
                    childBounds.X = 0;
                    maxHeight = 0d;
                }

                // 重置列计数
                if (column == _columns)
                {
                    column = 0;
                }
            }
        }
    }

    private void ArrangeChildrenWithSimpleStep(UIElementCollection children, Rect childBounds, double xStep, double yStep, double xBound)
    {
        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            child.Arrange(childBounds);

            if (child.Visibility != Visibility.Collapsed)
            {
                childBounds.X += xStep;

                // 处理行溢出
                if (childBounds.X >= xBound)
                {
                    childBounds.Y += yStep;
                    childBounds.X = 0;
                }
            }
        }
    }

    #endregion 布局方法

    #endregion 私有辅助方法
}