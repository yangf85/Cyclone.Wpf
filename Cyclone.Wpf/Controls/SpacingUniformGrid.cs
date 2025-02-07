using System.Collections.Generic;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls
{
public class SpacingUniformGrid : Panel
{
    private int _columns;

    private double[] _columnWidths;

    private double _horizontalSpacing;

    private int _rows;

    private StackMode _stackMode;

    private double _totalSpacingHeight;

    private double _totalSpacingWidth;

    private double _verticalSpacing;

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

            default:
                throw new ArgumentOutOfRangeException();
        }

        return arrangeSize;
    }

    protected override Size MeasureOverride(Size constraint)
    {
        UpdateComputedValues();

        switch (_stackMode)
        {
            case StackMode.Vertical:
                return MeasureStackVertical(constraint);

            case StackMode.Horizontal:
                return MeasureStackHorizontal(constraint);

            case StackMode.Both:
                return MeasureStackBoth(constraint);

            default:
                return MeasureStackNone(constraint);
        }
    }

    private void ArrangeStackBoth(Size arrangeSize, UIElementCollection children)
    {
        var childBounds = new Rect(0, 0, 0, 0);
        var xSteps = _columnWidths;
        var maxHeight = 0d;
        var xBound = arrangeSize.Width - 1.0;

        var firstColumn = FirstColumn;
        for (var i = 0; i < xSteps.Length && i < firstColumn; i++)
        {
            childBounds.X += xSteps[i] + _horizontalSpacing;
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

                if (childBounds.Height > maxHeight)
                {
                    maxHeight = childBounds.Height;
                }

                if (childBounds.X >= xBound)
                {
                    childBounds.Y += maxHeight + _verticalSpacing;
                    childBounds.X = 0;
                    maxHeight = 0d;
                }

                if (column == _columns)
                {
                    column = 0;
                }
            }
        }
    }

    private void ArrangeStackHorizontal(Size arrangeSize, UIElementCollection children)
    {
        var childBounds = new Rect(0, 0, 0, Math.Max(arrangeSize.Height - _totalSpacingHeight, 0) / _rows);
        var xSteps = _columnWidths;
        var yStep = childBounds.Height + _verticalSpacing;
        var xBound = arrangeSize.Width - 1.0;

        var firstColumn = FirstColumn;
        for (var i = 0; i < xSteps.Length && i < firstColumn; i++)
        {
            childBounds.X += xSteps[i] + _horizontalSpacing;
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

                if (childBounds.X >= xBound)
                {
                    childBounds.Y += yStep;
                    childBounds.X = 0;
                }

                if (column == _columns)
                {
                    column = 0;
                }
            }
        }
    }

    private void ArrangeStackNone(Size arrangeSize, UIElementCollection children)
    {
        var childBounds = new Rect(0, 0,
                Math.Max(arrangeSize.Width - _totalSpacingWidth, 0) / _columns,
                Math.Max(arrangeSize.Height - _totalSpacingHeight, 0) / _rows);
        var xStep = childBounds.Width + _horizontalSpacing;
        var yStep = childBounds.Height + _verticalSpacing;
        var xBound = arrangeSize.Width - 1.0;

        childBounds.X += xStep * FirstColumn;

        for (var i = 0; i < children.Count; i++)
        {
            var child = children[i];
            child.Arrange(childBounds);

            if (child.Visibility != Visibility.Collapsed)
            {
                childBounds.X += xStep;
                if (childBounds.X >= xBound)
                {
                    childBounds.Y += yStep;
                    childBounds.X = 0;
                }
            }
        }
    }

    private void ArrangeStackVertical(Size arrangeSize, UIElementCollection children)
    {
        var stackSizes = new List<double>();
        var childBounds = new Rect(0, 0, Math.Max(arrangeSize.Width - _totalSpacingWidth, 0) / _columns, 0);
        var xStep = childBounds.Width + _horizontalSpacing;
        var xBound = arrangeSize.Width - 1.0;

        childBounds.X += xStep * FirstColumn;
        var column = 0;
        for (var i = 0; i < children.Count; i++)
        {
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

        /*var childBounds = new Rect(0, 0, Math.Max(arrangeSize.Width - _totalSpacingWidth, 0) / _columns, 0);
        var xStep = childBounds.Width + _horizontalSpacing;
        var maxHeight = 0d;
        var xBound = arrangeSize.Width - 1.0;
        childBounds.X += xStep * FirstColumn;
        for (var i = 0; i < children.Count; i++) {
            var child = children[i];
            childBounds.Height = child.DesiredSize.Height;
            child.Arrange(childBounds);
            if (child.Visibility != Visibility.Collapsed) {
                childBounds.X += xStep;
                if (childBounds.Height > maxHeight) {
                    maxHeight = childBounds.Height;
                }
                if (childBounds.X >= xBound) {
                    childBounds.Y += maxHeight + _verticalSpacing;
                    childBounds.X = 0;
                    maxHeight = 0d;
                }
            }
        }*/
    }

    private Size MeasureStackBoth(Size constraint)
    {
        var childConstraint = new Size(
                double.PositiveInfinity,
                Math.Max(constraint.Height - _totalSpacingHeight, 0) / _rows);

        for (var i = 0; i < _columnWidths.Length; i++)
        {
            _columnWidths[i] = 0d;
        }

        var summaryChildrenHeight = 0d;
        var rowHeight = 0d;
        var itemsInRow = 0;

        //  Measure each child, keeping track of maximum desired width and height.
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];

            // Measure the child.
            child.Measure(childConstraint);
            var childDesiredSize = child.DesiredSize;

            if (childDesiredSize.Width > _columnWidths[itemsInRow])
            {
                _columnWidths[itemsInRow] = childDesiredSize.Width;
            }

            if (childDesiredSize.Height > rowHeight)
            {
                rowHeight = childDesiredSize.Height;
            }

            if (++itemsInRow == _columns)
            {
                itemsInRow = 0;
                summaryChildrenHeight += rowHeight;
                rowHeight = 0d;
            }
        }

        var sum = _totalSpacingWidth;
        for (var i = 0; i < _columnWidths.Length; i++)
        {
            sum += _columnWidths[i];
        }

        return new Size(sum, summaryChildrenHeight + _totalSpacingHeight);
    }

    private Size MeasureStackHorizontal(Size constraint)
    {
        var childConstraint = new Size(
                double.PositiveInfinity,
                Math.Max(constraint.Height - _totalSpacingHeight, 0) / _rows);

        for (var i = 0; i < _columnWidths.Length; i++)
        {
            _columnWidths[i] = 0d;
        }

        var maxChildDesiredHeight = 0d;
        var itemsInRow = 0;

        //  Measure each child, keeping track of maximum desired width and height.
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];

            // Measure the child.
            child.Measure(childConstraint);
            var childDesiredSize = child.DesiredSize;

            if (childDesiredSize.Width > _columnWidths[itemsInRow])
            {
                _columnWidths[itemsInRow] = childDesiredSize.Width;
            }

            if (maxChildDesiredHeight < childDesiredSize.Height)
            {
                maxChildDesiredHeight = childDesiredSize.Height;
            }

            if (++itemsInRow == _columns)
            {
                itemsInRow = 0;
            }
        }

        var sum = _totalSpacingWidth;
        for (var i = 0; i < _columnWidths.Length; i++)
        {
            sum += _columnWidths[i];
        }

        return new Size(sum, maxChildDesiredHeight * _rows + _totalSpacingHeight);
    }

    private Size MeasureStackNone(Size constraint)
    {
        var childConstraint = new Size(
                Math.Max(constraint.Width - _totalSpacingWidth, 0) / _columns,
                Math.Max(constraint.Height - _totalSpacingHeight, 0) / _rows);

        var maxChildDesiredWidth = 0d;
        var maxChildDesiredHeight = 0d;

        //  Measure each child, keeping track of maximum desired width and height.
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            var child = InternalChildren[i];

            // Measure the child.
            child.Measure(childConstraint);
            var childDesiredSize = child.DesiredSize;

            if (maxChildDesiredWidth < childDesiredSize.Width)
            {
                maxChildDesiredWidth = childDesiredSize.Width;
            }

            if (maxChildDesiredHeight < childDesiredSize.Height)
            {
                maxChildDesiredHeight = childDesiredSize.Height;
            }
        }

        return new Size(maxChildDesiredWidth * _columns + _totalSpacingWidth, maxChildDesiredHeight * _rows + _totalSpacingHeight);
    }

    private Size MeasureStackVertical(Size constraint)
    {
        // 根据constraint和_columns（可能是类中定义的字段）计算出每个子元素可以使用的最大宽度
        var childConstraint = new Size(
                Math.Max(constraint.Width - _totalSpacingWidth, 0) / _columns,
                double.PositiveInfinity);

        // 创建一个列表，用于存储每一列子元素的总高度
        var stackSizes = new List<double>();

        // 初始化最大子元素宽度为0
        var maxChildDesiredWidth = 0d;

        // 初始化最大列高度为0
        var maxColumnHeight = 0d;

        // 初始化当前行中子元素个数为0
        var itemsInRow = 0;

        // 遍历内部子元素集合
        for (int i = 0, count = InternalChildren.Count; i < count; ++i)
        {
            // 获取第i个子元素
            var child = InternalChildren[i];

            // 调用子元素的Measure方法，传入childConstraint作为参数，以确定其期望大小
            child.Measure(childConstraint);

            // 获取子元素返回的期望大小
            var childDesiredSize = child.DesiredSize;

            // 如果stackSizes列表中没有足够多的项来存储当前行中每一列的高度，则添加新项并初始化为0
            if (stackSizes.Count <= itemsInRow)
            {
                stackSizes.Add(0d);
            }

            // 将当前子元素的高度加上_verticalSpacing（可能是类中定义的字段）累加到对应列上，并更新最大列高度
            stackSizes[itemsInRow] += childDesiredSize.Height + _verticalSpacing;
            maxColumnHeight = Math.Max(maxColumnHeight, stackSizes[itemsInRow]);

            // 如果当前子元素的宽度大于之前记录的最大子元素宽度，则更新最大子元素宽度
            if (maxChildDesiredWidth < childDesiredSize.Width)
            {
                maxChildDesiredWidth = childDesiredSize.Width;
            }

            // 如果当前行已经放满了_columns个子元素，则换到下一行，并重置itemsInRow为0；否则将itemsInRow加1
            if (++itemsInRow == _columns)
            {
                itemsInRow = 0;
            }
        }

        // 返回一个新建的Size对象，其宽度等于最大子元素宽度乘以_columns再加上_totalSpacingWidth（可能是类中定义了字段），其高度等于最大列高度。
        return new Size(maxChildDesiredWidth * _columns + _totalSpacingWidth, maxColumnHeight);
    }

    private void UpdateComputedValues()
    {
        _stackMode = (VerticalStackMode ? StackMode.Vertical : StackMode.None) | (HorizontalStackMode ? StackMode.Horizontal : StackMode.None);
        _columns = Columns;
        _rows = Rows;

        if (FirstColumn >= _columns)
        {
            FirstColumn = 0;
        }

        if (_rows == 0 || _columns == 0)
        {
            var nonCollapsedCount = 0;

            for (int i = 0, count = InternalChildren.Count; i < count; ++i)
            {
                var child = InternalChildren[i];
                if (child.Visibility != Visibility.Collapsed)
                {
                    nonCollapsedCount++;
                }
            }

            if (nonCollapsedCount == 0)
            {
                nonCollapsedCount = 1;
            }

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

        _horizontalSpacing = HorizontalSpacing;
        _verticalSpacing = VerticalSpacing;

        _totalSpacingWidth = _columns == 0 ? 0 : _horizontalSpacing * (_columns - 1);
        _totalSpacingHeight = _rows == 0 ? 0 : _verticalSpacing * (_rows - 1);

        if ((_stackMode & StackMode.Horizontal) != 0)
        {
            _columnWidths = new double[_columns];
        }
    }

    [Flags]
    private enum StackMode
    {
        None = 0,

        Horizontal = 1,

        Vertical = 2,

        Both = 3
    }

    #region Columns

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int), typeof(SpacingUniformGrid),
                                                                                    new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int Columns
    {
        get => GetValue(ColumnsProperty) as int? ?? default;
        set => SetValue(ColumnsProperty, value);
    }

    #endregion Columns

    #region FirstColumn

    public static readonly DependencyProperty FirstColumnProperty = DependencyProperty.Register(nameof(FirstColumn), typeof(int), typeof(SpacingUniformGrid),
                    new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int FirstColumn
    {
        get => GetValue(FirstColumnProperty) as int? ?? default;
        set => SetValue(FirstColumnProperty, value);
    }

    #endregion FirstColumn

    #region HorizontalSpacing

    public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register(nameof(HorizontalSpacing), typeof(double),
            typeof(SpacingUniformGrid), new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty) as double? ?? default;
        set => SetValue(HorizontalSpacingProperty, value);
    }

    #endregion HorizontalSpacing

    #region HorizontalStackMode

    public static readonly DependencyProperty HorizontalStackModeProperty = DependencyProperty.Register(nameof(HorizontalStackMode), typeof(bool),
            typeof(SpacingUniformGrid));

    public bool HorizontalStackMode
    {
        get => GetValue(HorizontalStackModeProperty) as bool? ?? default;
        set => SetValue(HorizontalStackModeProperty, value);
    }

    #endregion HorizontalStackMode

    #region Rows

    public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(nameof(Rows), typeof(int), typeof(SpacingUniformGrid),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public int Rows
    {
        get => GetValue(RowsProperty) as int? ?? default;
        set => SetValue(RowsProperty, value);
    }

    #endregion Rows

    #region VerticalSpacing

    public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register(nameof(VerticalSpacing), typeof(double),
            typeof(SpacingUniformGrid), new FrameworkPropertyMetadata(10d, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty) as double? ?? default;
        set => SetValue(VerticalSpacingProperty, value);
    }

    #endregion VerticalSpacing

    #region VerticalStackMode

    public static readonly DependencyProperty VerticalStackModeProperty = DependencyProperty.Register(nameof(VerticalStackMode), typeof(bool),
            typeof(SpacingUniformGrid), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public bool VerticalStackMode
    {
        get => GetValue(VerticalStackModeProperty) as bool? ?? default;
        set => SetValue(VerticalStackModeProperty, value);
    }

    #endregion VerticalStackMode
}
}