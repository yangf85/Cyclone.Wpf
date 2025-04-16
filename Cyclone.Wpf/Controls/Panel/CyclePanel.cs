using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 循环展示的面板，支持无限循环滚动，可配置为水平或垂直方向
/// 需配合ScrollViewer使用以实现滚动功能,设置ScrollViewer.CanContentScroll="True"
/// </summary>
public class CyclePanel : VirtualizingPanel, IScrollInfo
{
    #region Private Fields

    private ScrollViewer _owner;

    private Size _extent = new Size(0, 0);
    private Size _viewport = new Size(0, 0);
    private Point _offset = new Point(0, 0);
    private UIElementCollection _realizedChildren;
    private int _itemCount;
    private bool _isInMeasure;
    private double _itemSize; // 根据VisibleItemCount计算得到的项目尺寸（高度或宽度）

    /// <summary>
    /// 项目可见性阈值，项目可见部分必须超过此比例才算可见（默认为0.2，即20%）
    /// </summary>
    public double VisibilityThreshold
    {
        get { return (double)GetValue(VisibilityThresholdProperty); }
        set { SetValue(VisibilityThresholdProperty, value); }
    }

    public static readonly DependencyProperty VisibilityThresholdProperty =
        DependencyProperty.Register("VisibilityThreshold", typeof(double), typeof(CyclePanel),
            new FrameworkPropertyMetadata(0.2, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

    #endregion Private Fields

    #region Dependency Properties

    /// <summary>
    /// 可见项目数量，控件尺寸将均分给这些可见项目
    /// </summary>
    public int VisibleItemCount
    {
        get { return (int)GetValue(VisibleItemCountProperty); }
        set { SetValue(VisibleItemCountProperty, value); }
    }

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register("VisibleItemCount", typeof(int), typeof(CyclePanel),
            new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                             FrameworkPropertyMetadataOptions.AffectsArrange,
                                             OnVisibleItemCountChanged));

    private static void OnVisibleItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (CyclePanel)d;

        // 确保值为正数
        int value = (int)e.NewValue;
        if (value <= 0)
        {
            panel.SetCurrentValue(VisibleItemCountProperty, 1);
            return;
        }

        // 验证VisibleItemCount是否小于元素数量
        int itemCount = panel.InternalChildren.Count;
        if (itemCount > 0 && value >= itemCount)
        {
            throw new InvalidOperationException($"VisibleItemCount ({value}) 必须小于面板中的元素数量 ({itemCount}) 才能实现循环滚动。");
        }

        panel.InvalidateMeasure();
        panel.InvalidateArrange();
    }

    /// <summary>
    /// 滚动方向
    /// </summary>
    public Orientation Orientation
    {
        get { return (Orientation)GetValue(OrientationProperty); }
        set { SetValue(OrientationProperty, value); }
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register("Orientation", typeof(Orientation), typeof(CyclePanel),
            new FrameworkPropertyMetadata(Orientation.Vertical,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// 当前视口中可见的所有项目索引列表（只读）
    /// </summary>
    public IReadOnlyList<int> VisibleItemIndices
    {
        get { return (IReadOnlyList<int>)GetValue(VisibleItemIndicesProperty); }
        private set { SetValue(VisibleItemIndicesPropertyKey, value); }
    }

    private static readonly DependencyPropertyKey VisibleItemIndicesPropertyKey =
        DependencyProperty.RegisterReadOnly("VisibleItemIndices", typeof(IReadOnlyList<int>), typeof(CyclePanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

    public static readonly DependencyProperty VisibleItemIndicesProperty =
        VisibleItemIndicesPropertyKey.DependencyProperty;

    #endregion Dependency Properties

    #region Constructors

    public CyclePanel()
    {
        // 启用虚拟化和UI虚拟化
        VirtualizingPanel.SetIsVirtualizing(this, true);
        VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);

        // 启用容器回收
        VirtualizingPanel.SetCacheLength(this, new VirtualizationCacheLength(1, 1));
        VirtualizingPanel.SetCacheLengthUnit(this, VirtualizationCacheLengthUnit.Page);

        // 初始化可见项目索引列表
        VisibleItemIndices = [];

        // 设置默认可见性阈值
        VisibilityThreshold = 0.2;
    }

    #endregion Constructors

    #region Item Children Changed

    /// <summary>
    /// 项目添加或移除处理
    /// </summary>
    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);

        // 当添加或移除项目时，验证VisibleItemCount
        if (IsLoaded)
        {
            ValidateVisibleItemCount();
        }
    }

    /// <summary>
    /// 验证VisibleItemCount是否合法
    /// </summary>
    private void ValidateVisibleItemCount()
    {
        int itemCount = InternalChildren.Count;
        if (itemCount > 0 && VisibleItemCount >= itemCount)
        {
            throw new InvalidOperationException($"VisibleItemCount ({VisibleItemCount}) 必须小于面板中的元素数量 ({itemCount}) 才能实现循环滚动。");
        }
    }

    #endregion Item Children Changed

    #region Layout Overrides

    protected override Size MeasureOverride(Size availableSize)
    {
        _isInMeasure = true;

        try
        {
            _itemCount = InternalChildren.Count;

            if (_itemCount == 0)
            {
                // 清空可见项目索引列表
                SetValue(VisibleItemIndicesPropertyKey, new List<int>());
                return new Size(0, 0);
            }

            _realizedChildren = InternalChildren;

            // 处理无限大小情况
            bool widthIsInfinite = double.IsInfinity(availableSize.Width);
            bool heightIsInfinite = double.IsInfinity(availableSize.Height);

            Size measureSize = availableSize;

            // 如果宽度无限，设置一个默认值
            if (widthIsInfinite)
            {
                measureSize.Width = 300; // 默认宽度
            }

            // 如果高度无限，设置一个默认值
            if (heightIsInfinite)
            {
                measureSize.Height = _itemCount * 30; // 默认高度，每项30像素
            }

            // 根据方向计算项目尺寸和滚动布局
            if (Orientation == Orientation.Vertical)
            {
                // 计算每个项目的高度
                if (VisibleItemCount > 0 && !heightIsInfinite)
                {
                    _itemSize = measureSize.Height / VisibleItemCount;
                }
                else
                {
                    _itemSize = 30; // 默认值
                }

                // 计算内容总高度
                double contentHeight = _itemCount * _itemSize;

                // 设置滚动区域大小
                _extent = new Size(measureSize.Width, contentHeight);
                _viewport = measureSize;

                if (_owner != null)
                {
                    _owner.InvalidateScrollInfo();
                }

                // 计算可见项的索引范围
                int firstVisibleIndex = (int)Math.Floor(VerticalOffset / _itemSize);
                int lastVisibleIndex = (int)Math.Ceiling((VerticalOffset + _viewport.Height) / _itemSize);

                // 确保索引在有效范围内
                firstVisibleIndex = Math.Max(0, Math.Min(firstVisibleIndex, _itemCount - 1));
                lastVisibleIndex = Math.Max(0, Math.Min(lastVisibleIndex, _itemCount - 1));

                // 测量所有子项
                Size childSize = new Size(measureSize.Width, _itemSize);
                for (int i = 0; i < _itemCount; i++)
                {
                    UIElement child = _realizedChildren[i];
                    child.Measure(childSize);
                }

                // 返回期望的尺寸
                return heightIsInfinite ?
                    new Size(measureSize.Width, Math.Min(contentHeight, VisibleItemCount * _itemSize)) :
                    measureSize;
            }
            else // Orientation.Horizontal
            {
                // 计算每个项目的宽度
                if (VisibleItemCount > 0 && !widthIsInfinite)
                {
                    _itemSize = measureSize.Width / VisibleItemCount;
                }
                else
                {
                    _itemSize = 80; // 默认值
                }

                // 计算内容总宽度
                double contentWidth = _itemCount * _itemSize;

                // 设置滚动区域大小
                _extent = new Size(contentWidth, measureSize.Height);
                _viewport = measureSize;

                if (_owner != null)
                {
                    _owner.InvalidateScrollInfo();
                }

                // 计算可见项的索引范围
                int firstVisibleIndex = (int)Math.Floor(HorizontalOffset / _itemSize);
                int lastVisibleIndex = (int)Math.Ceiling((HorizontalOffset + _viewport.Width) / _itemSize);

                // 确保索引在有效范围内
                firstVisibleIndex = Math.Max(0, Math.Min(firstVisibleIndex, _itemCount - 1));
                lastVisibleIndex = Math.Max(0, Math.Min(lastVisibleIndex, _itemCount - 1));

                // 测量所有子项
                Size childSize = new Size(_itemSize, measureSize.Height);
                for (int i = 0; i < _itemCount; i++)
                {
                    UIElement child = _realizedChildren[i];
                    child.Measure(childSize);
                }

                // 返回期望的尺寸
                return widthIsInfinite ?
                    new Size(Math.Min(contentWidth, VisibleItemCount * _itemSize), measureSize.Height) :
                    measureSize;
            }
        }
        finally
        {
            _isInMeasure = false;
        }
    }

    protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
    {
        base.OnItemsChanged(sender, args);

        InvalidateMeasure();
        InvalidateArrange();

        _itemCount = InternalChildren.Count;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_itemCount == 0)
        {
            // 清空可见项目索引列表
            SetValue(VisibleItemIndicesPropertyKey, new List<int>());
            return finalSize;
        }

        // 更新视口大小
        _viewport = finalSize;

        if (_owner != null)
        {
            _owner.InvalidateScrollInfo();
        }

        // 用于收集实际可见项目的索引
        List<int> visibleIndices = new List<int>();

        // 根据方向排列子元素
        if (Orientation == Orientation.Vertical)
        {
            // 重新计算每个项目的高度
            if (VisibleItemCount > 0 && finalSize.Height > 0)
            {
                _itemSize = finalSize.Height / VisibleItemCount;
            }

            double totalHeight = _itemCount * _itemSize;

            // 排列所有子项
            for (int i = 0; i < _itemCount; i++)
            {
                UIElement child = _realizedChildren[i];

                // 计算项目的位置，但允许循环显示
                double itemTop = (i * _itemSize) - VerticalOffset;

                // 计算逻辑索引（考虑循环）
                int logicalIndex = i;
                bool isWrapped = false;

                // 当项目超出下方边界时，将其移到顶部循环显示
                if (itemTop > totalHeight - _itemSize && _itemCount > VisibleItemCount)
                {
                    itemTop -= totalHeight;
                    isWrapped = true;
                }
                // 当项目超出上方边界时，将其移到底部循环显示
                else if (itemTop < -_itemSize && _itemCount > VisibleItemCount)
                {
                    itemTop += totalHeight;
                    isWrapped = true;
                }

                // 计算项目的可见部分比例
                double visibleTop = Math.Max(0, itemTop);
                double visibleBottom = Math.Min(itemTop + _itemSize, _viewport.Height);
                double visibleHeight = visibleBottom - visibleTop;
                double visibilityRatio = visibleHeight / _itemSize;

                // 确定项目是否足够可见（超过阈值）
                bool isVisible = visibilityRatio >= VisibilityThreshold && visibleHeight > 0;

                if (isVisible)
                {
                    // 布局子项
                    child.Arrange(new Rect(0, itemTop, finalSize.Width, _itemSize));
                    child.Visibility = Visibility.Visible;

                    // 添加到可见项目索引列表（使用实际索引，而不是循环后的位置）
                    visibleIndices.Add(logicalIndex);
                }
                else
                {
                    // 不可见的项目
                    child.Arrange(new Rect(0, itemTop, 0, 0));
                    child.Visibility = Visibility.Collapsed;
                }
            }
        }
        else // Orientation.Horizontal
        {
            // 重新计算每个项目的宽度
            if (VisibleItemCount > 0 && finalSize.Width > 0)
            {
                _itemSize = finalSize.Width / VisibleItemCount;
            }

            double totalWidth = _itemCount * _itemSize;

            // 排列所有子项
            for (int i = 0; i < _itemCount; i++)
            {
                UIElement child = _realizedChildren[i];

                // 计算项目的位置，但允许循环显示
                double itemLeft = (i * _itemSize) - HorizontalOffset;

                // 计算逻辑索引（考虑循环）
                int logicalIndex = i;
                bool isWrapped = false;

                // 当项目超出右侧边界时，将其移到左侧循环显示
                if (itemLeft > totalWidth - _itemSize && _itemCount > VisibleItemCount)
                {
                    itemLeft -= totalWidth;
                    isWrapped = true;
                }
                // 当项目超出左侧边界时，将其移到右侧循环显示
                else if (itemLeft < -_itemSize && _itemCount > VisibleItemCount)
                {
                    itemLeft += totalWidth;
                    isWrapped = true;
                }

                // 计算项目的可见部分比例
                double visibleLeft = Math.Max(0, itemLeft);
                double visibleRight = Math.Min(itemLeft + _itemSize, _viewport.Width);
                double visibleWidth = visibleRight - visibleLeft;
                double visibilityRatio = visibleWidth / _itemSize;

                // 确定项目是否足够可见（超过阈值）
                bool isVisible = visibilityRatio >= VisibilityThreshold && visibleWidth > 0;

                if (isVisible)
                {
                    // 布局子项
                    child.Arrange(new Rect(itemLeft, 0, _itemSize, finalSize.Height));
                    child.Visibility = Visibility.Visible;

                    // 添加到可见项目索引列表（使用实际索引，而不是循环后的位置）
                    visibleIndices.Add(logicalIndex);
                }
                else
                {
                    // 不可见的项目
                    child.Arrange(new Rect(itemLeft, 0, 0, 0));
                    child.Visibility = Visibility.Collapsed;
                }
            }
        }

        // 限制可见项目数量不超过VisibleItemCount
        if (visibleIndices.Count > VisibleItemCount)
        {
            // 根据项目在视口中的位置排序
            SortVisibleIndicesByPosition(visibleIndices);

            // 只保留前VisibleItemCount个项目
            while (visibleIndices.Count > VisibleItemCount)
            {
                visibleIndices.RemoveAt(visibleIndices.Count - 1);
            }
        }
        else
        {
            // 根据项目在视口中的位置排序
            SortVisibleIndicesByPosition(visibleIndices);
        }

        // 更新可见项目索引列表依赖属性
        SetValue(VisibleItemIndicesPropertyKey, visibleIndices);

        return finalSize;
    }

    /// <summary>
    /// 根据项目在视口中的位置对可见索引进行排序
    /// </summary>
    private void SortVisibleIndicesByPosition(List<int> indices)
    {
        // 创建一个列表存储索引和它们的实际屏幕位置关系
        var itemPositions = new List<KeyValuePair<int, double>>();

        if (Orientation == Orientation.Vertical)
        {
            // 计算内容总高度
            double contentHeight = _itemCount * _itemSize;

            // 计算每个项目在视口中的实际显示位置
            foreach (int idx in indices)
            {
                double itemTop = (idx * _itemSize) - VerticalOffset;

                // 处理循环回绕的情况
                if (itemTop > contentHeight - _itemSize && _itemCount > VisibleItemCount)
                {
                    itemTop -= contentHeight;
                }
                else if (itemTop < -_itemSize && _itemCount > VisibleItemCount)
                {
                    itemTop += contentHeight;
                }

                // 保存索引和实际位置
                itemPositions.Add(new KeyValuePair<int, double>(idx, itemTop));
            }
        }
        else // Orientation.Horizontal
        {
            // 计算内容总宽度
            double contentWidth = _itemCount * _itemSize;

            // 计算每个项目在视口中的实际显示位置
            foreach (int idx in indices)
            {
                double itemLeft = (idx * _itemSize) - HorizontalOffset;

                // 处理循环回绕的情况
                if (itemLeft > contentWidth - _itemSize && _itemCount > VisibleItemCount)
                {
                    itemLeft -= contentWidth;
                }
                else if (itemLeft < -_itemSize && _itemCount > VisibleItemCount)
                {
                    itemLeft += contentWidth;
                }

                // 保存索引和实际位置
                itemPositions.Add(new KeyValuePair<int, double>(idx, itemLeft));
            }
        }

        // 按实际显示位置排序
        itemPositions.Sort((a, b) => a.Value.CompareTo(b.Value));

        // 更新索引列表
        indices.Clear();
        foreach (var item in itemPositions)
        {
            indices.Add(item.Key);
        }
    }

    #endregion Layout Overrides

    #region IScrollInfo Implementation

    public bool CanVerticallyScroll
    {
        get { return Orientation == Orientation.Vertical; }
        set { /* 由Orientation属性控制 */ }
    }

    public bool CanHorizontallyScroll
    {
        get { return Orientation == Orientation.Horizontal; }
        set { /* 由Orientation属性控制 */ }
    }

    public double ExtentWidth
    {
        get { return _extent.Width; }
    }

    public double ExtentHeight
    {
        get { return _extent.Height; }
    }

    public double ViewportWidth
    {
        get { return _viewport.Width; }
    }

    public double ViewportHeight
    {
        get { return _viewport.Height; }
    }

    public double HorizontalOffset
    {
        get { return _offset.X; }
    }

    public double VerticalOffset
    {
        get { return _offset.Y; }
    }

    public ScrollViewer ScrollOwner
    {
        get { return _owner; }
        set { _owner = value; }
    }

    public void LineUp()
    {
        if (Orientation == Orientation.Vertical)
        {
            SetVerticalOffset(VerticalOffset - _itemSize);
        }
    }

    public void LineDown()
    {
        if (Orientation == Orientation.Vertical)
        {
            SetVerticalOffset(VerticalOffset + _itemSize);
        }
    }

    public void LineLeft()
    {
        if (Orientation == Orientation.Horizontal)
        {
            SetHorizontalOffset(HorizontalOffset - _itemSize);
        }
    }

    public void LineRight()
    {
        if (Orientation == Orientation.Horizontal)
        {
            SetHorizontalOffset(HorizontalOffset + _itemSize);
        }
    }

    public void PageUp()
    {
        if (Orientation == Orientation.Vertical)
        {
            SetVerticalOffset(VerticalOffset - _viewport.Height);
        }
    }

    public void PageDown()
    {
        if (Orientation == Orientation.Vertical)
        {
            SetVerticalOffset(VerticalOffset + _viewport.Height);
        }
    }

    public void PageLeft()
    {
        if (Orientation == Orientation.Horizontal)
        {
            SetHorizontalOffset(HorizontalOffset - _viewport.Width);
        }
    }

    public void PageRight()
    {
        if (Orientation == Orientation.Horizontal)
        {
            SetHorizontalOffset(HorizontalOffset + _viewport.Width);
        }
    }

    public void MouseWheelUp()
    {
        if (Orientation == Orientation.Vertical)
        {
            SetVerticalOffset(VerticalOffset - _itemSize);
        }
        else
        {
            SetHorizontalOffset(HorizontalOffset - _itemSize);
        }
    }

    public void MouseWheelDown()
    {
        if (Orientation == Orientation.Vertical)
        {
            SetVerticalOffset(VerticalOffset + _itemSize);
        }
        else
        {
            SetHorizontalOffset(HorizontalOffset + _itemSize);
        }
    }

    public void MouseWheelLeft()
    {
        if (Orientation == Orientation.Horizontal)
        {
            SetHorizontalOffset(HorizontalOffset - _itemSize);
        }
    }

    public void MouseWheelRight()
    {
        if (Orientation == Orientation.Horizontal)
        {
            SetHorizontalOffset(HorizontalOffset + _itemSize);
        }
    }

    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        if (visual == null || _itemCount == 0 || rectangle.IsEmpty)
            return Rect.Empty;

        // 找到视觉元素对应的子项索引
        int index = -1;

        for (int i = 0; i < _itemCount; i++)
        {
            if (_realizedChildren[i] == visual)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
            return Rect.Empty;

        if (Orientation == Orientation.Vertical)
        {
            // 计算项目的顶部位置
            double itemTop = index * _itemSize;

            // 确保项目可见
            double itemBottom = itemTop + _itemSize;

            if (itemTop < VerticalOffset)
            {
                // 项目在可视区域上方，滚动到使其可见
                SetVerticalOffset(itemTop);
            }
            else if (itemBottom > VerticalOffset + _viewport.Height)
            {
                // 项目在可视区域下方，滚动到使其可见
                SetVerticalOffset(itemBottom - _viewport.Height);
            }

            // 返回项目在视口中的可见区域
            return new Rect(0, itemTop - VerticalOffset, _viewport.Width, _itemSize);
        }
        else // Orientation.Horizontal
        {
            // 计算项目的左侧位置
            double itemLeft = index * _itemSize;

            // 确保项目可见
            double itemRight = itemLeft + _itemSize;

            if (itemLeft < HorizontalOffset)
            {
                // 项目在可视区域左侧，滚动到使其可见
                SetHorizontalOffset(itemLeft);
            }
            else if (itemRight > HorizontalOffset + _viewport.Width)
            {
                // 项目在可视区域右侧，滚动到使其可见
                SetHorizontalOffset(itemRight - _viewport.Width);
            }

            // 返回项目在视口中的可见区域
            return new Rect(itemLeft - HorizontalOffset, 0, _itemSize, _viewport.Height);
        }
    }

    public void SetHorizontalOffset(double offset)
    {
        if (_isInMeasure || _itemCount == 0 || Orientation != Orientation.Horizontal)
            return;

        double totalWidth = _itemCount * _itemSize;

        // 循环滚动处理
        if (totalWidth > 0)
        {
            // 实现循环效果：当滚动超过内容宽度时，回到开始
            offset = ((offset % totalWidth) + totalWidth) % totalWidth;
        }

        if (_offset.X != offset)
        {
            _offset.X = offset;

            if (_owner != null)
            {
                _owner.InvalidateScrollInfo();
            }

            InvalidateArrange();
        }
    }

    public void SetVerticalOffset(double offset)
    {
        if (_isInMeasure || _itemCount == 0 || Orientation != Orientation.Vertical)
            return;

        double totalHeight = _itemCount * _itemSize;

        // 循环滚动处理
        if (totalHeight > 0)
        {
            // 实现循环效果：当滚动超过内容高度时，回到顶部
            offset = ((offset % totalHeight) + totalHeight) % totalHeight;
        }

        if (_offset.Y != offset)
        {
            _offset.Y = offset;

            if (_owner != null)
            {
                _owner.InvalidateScrollInfo();
            }

            InvalidateArrange();
        }
    }

    #endregion IScrollInfo Implementation
}