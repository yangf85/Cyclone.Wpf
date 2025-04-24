using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cyclone.Wpf.Helpers;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 循环展示的面板，支持无限循环滚动，可配置为水平或垂直方向
/// 需配合ScrollViewer使用以实现滚动功能,设置ScrollViewer.CanContentScroll="True"
/// </summary>
public class CyclicPanel : Panel, IScrollInfo
{
    #region Private Fields

    private ScrollViewer _owner;
    private Size _extent = new Size(0, 0);
    private Size _viewport = new Size(0, 0);
    private Point _offset = new Point(0, 0);
    private bool _isInMeasure;
    private double _itemSize;
    private bool _isAnimating = false;

    #endregion Private Fields

    #region Dependency Properties

    public int VisibleItemCount
    {
        get { return (int)GetValue(VisibleItemCountProperty); }
        set { SetValue(VisibleItemCountProperty, value); }
    }

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register("VisibleItemCount", typeof(int), typeof(CyclicPanel),
            new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                             FrameworkPropertyMetadataOptions.AffectsArrange));

    public Orientation Orientation
    {
        get { return (Orientation)GetValue(OrientationProperty); }
        set { SetValue(OrientationProperty, value); }
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register("Orientation", typeof(Orientation), typeof(CyclicPanel),
            new FrameworkPropertyMetadata(Orientation.Vertical,
                FrameworkPropertyMetadataOptions.AffectsMeasure |
                FrameworkPropertyMetadataOptions.AffectsArrange));

    public IReadOnlyList<int> VisibleItemIndices
    {
        get { return (IReadOnlyList<int>)GetValue(VisibleItemIndicesProperty); }
        private set { SetValue(VisibleItemIndicesPropertyKey, value); }
    }

    private static readonly DependencyPropertyKey VisibleItemIndicesPropertyKey =
        DependencyProperty.RegisterReadOnly("VisibleItemIndices", typeof(IReadOnlyList<int>), typeof(CyclicPanel),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty VisibleItemIndicesProperty =
        VisibleItemIndicesPropertyKey.DependencyProperty;

    /// <summary>
    /// 动画持续时间
    /// </summary>
    public Duration AnimationDuration
    {
        get { return (Duration)GetValue(AnimationDurationProperty); }
        set { SetValue(AnimationDurationProperty, value); }
    }

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(CyclicPanel),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(75))));

    /// <summary>
    /// 缓动函数类型
    /// </summary>
    public EasingMode EasingMode
    {
        get { return (EasingMode)GetValue(EasingModeProperty); }
        set { SetValue(EasingModeProperty, value); }
    }

    public static readonly DependencyProperty EasingModeProperty =
        DependencyProperty.Register("EasingMode", typeof(EasingMode), typeof(CyclicPanel),
            new PropertyMetadata(EasingMode.EaseInOut));

    /// <summary>
    /// 是否启用动画效果
    /// </summary>
    public bool IsAnimationEnabled
    {
        get { return (bool)GetValue(IsAnimationEnabledProperty); }
        set { SetValue(IsAnimationEnabledProperty, value); }
    }

    public static readonly DependencyProperty IsAnimationEnabledProperty =
        DependencyProperty.Register("IsAnimationEnabled", typeof(bool), typeof(CyclicPanel),
            new PropertyMetadata(false));

    #endregion Dependency Properties

    #region Constructors

    public CyclicPanel()
    {
    }

    #endregion Constructors

    #region Layout Overrides

    protected override Size MeasureOverride(Size availableSize)
    {
        _isInMeasure = true;

        try
        {
            int itemCount = InternalChildren.Count;

            if (itemCount == 0)
            {
                // 清空可见项目索引列表
                VisibleItemIndices = new List<int>();
                return new Size(0, 0);
            }

            // 处理无限大小情况
            bool isVertical = Orientation == Orientation.Vertical;
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
                measureSize.Height = 300; // 默认高度
            }

            // 根据方向计算项目尺寸和滚动布局
            if (isVertical)
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
                double contentHeight = itemCount * _itemSize;

                // 设置滚动区域大小
                _extent = new Size(measureSize.Width, contentHeight);
                _viewport = measureSize;

                _owner?.InvalidateScrollInfo();

                // 测量所有子项
                Size childSize = new Size(measureSize.Width, _itemSize);
                foreach (UIElement child in InternalChildren)
                {
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
                double contentWidth = itemCount * _itemSize;

                // 设置滚动区域大小
                _extent = new Size(contentWidth, measureSize.Height);
                _viewport = measureSize;

                _owner?.InvalidateScrollInfo();

                // 测量所有子项
                Size childSize = new Size(_itemSize, measureSize.Height);
                foreach (UIElement child in InternalChildren)
                {
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

    protected override Size ArrangeOverride(Size finalSize)
    {
        int itemCount = InternalChildren.Count;

        if (itemCount == 0)
        {
            // 清空可见项目索引列表
            VisibleItemIndices = new List<int>();
            return finalSize;
        }

        // 更新视口大小
        _viewport = finalSize;
        _owner?.InvalidateScrollInfo();

        // 更新项目尺寸
        UpdateItemSize(finalSize);

        // 排列子元素
        ArrangeChildren(finalSize);

        // 非动画状态下更新可见项索引
        if (!_isAnimating)
            UpdateVisibleItemIndices();

        return finalSize;
    }

    #endregion Layout Overrides

    #region 辅助方法

    /// <summary>
    /// 根据面板尺寸更新项目尺寸
    /// </summary>
    private void UpdateItemSize(Size finalSize)
    {
        bool isVertical = Orientation == Orientation.Vertical;

        if (VisibleItemCount > 0)
        {
            if (isVertical && finalSize.Height > 0)
                _itemSize = finalSize.Height / VisibleItemCount;
            else if (!isVertical && finalSize.Width > 0)
                _itemSize = finalSize.Width / VisibleItemCount;
        }
    }

    /// <summary>
    /// 排列所有子元素
    /// </summary>
    private void ArrangeChildren(Size finalSize)
    {
        int itemCount = InternalChildren.Count;
        bool isVertical = Orientation == Orientation.Vertical;
        double totalLength = itemCount * _itemSize;
        double viewportLength = isVertical ? finalSize.Height : finalSize.Width;
        double currentOffset = isVertical ? VerticalOffset : HorizontalOffset;

        for (int i = 0; i < itemCount; i++)
        {
            UIElement child = InternalChildren[i];

            // 计算位置
            double position = (i * _itemSize) - currentOffset;

            // 循环滚动处理
            if (position >= viewportLength && itemCount > VisibleItemCount)
                position -= totalLength;
            else if (position < -_itemSize && itemCount > VisibleItemCount)
                position += totalLength;

            // 判断可见性
            bool isVisible = (position + _itemSize > 0) && (position < viewportLength);

            // 布局
            if (isVisible)
            {
                Rect bounds = isVertical
                    ? new Rect(0, position, finalSize.Width, _itemSize)
                    : new Rect(position, 0, _itemSize, finalSize.Height);

                child.Arrange(bounds);
                child.Visibility = Visibility.Visible;
            }
            else
            {
                // 不可见项
                Rect emptyBounds = isVertical
                    ? new Rect(0, position, 0, 0)
                    : new Rect(position, 0, 0, 0);

                child.Arrange(emptyBounds);
                child.Visibility = Visibility.Collapsed;
            }
        }
    }

    /// <summary>
    /// 更新可见项目索引列表，基于项目中心点是否在面板可视区域内判断
    /// </summary>
    private void UpdateVisibleItemIndices()
    {
        int itemCount = InternalChildren.Count;
        if (itemCount == 0)
        {
            // 清空可见项目索引列表
            VisibleItemIndices = new List<int>();
            return;
        }

        bool isVertical = Orientation == Orientation.Vertical;
        double viewportLength = isVertical ? _viewport.Height : _viewport.Width;
        double currentOffset = isVertical ? VerticalOffset : HorizontalOffset;
        double totalLength = itemCount * _itemSize;

        // 使用列表存储可见项及其位置
        var visibleItems = new List<KeyValuePair<int, double>>();

        // 检查所有项的可见性
        for (int i = 0; i < itemCount; i++)
        {
            // 计算项的位置
            double position = (i * _itemSize) - currentOffset;

            // 循环滚动处理
            if (position >= viewportLength && itemCount > VisibleItemCount)
                position -= totalLength;
            else if (position < -_itemSize && itemCount > VisibleItemCount)
                position += totalLength;

            // 计算项的中心点
            double itemCenter = position + (_itemSize / 2);

            // 判断中心点是否在可视区域内
            if (itemCenter >= 0 && itemCenter <= viewportLength)
                visibleItems.Add(new KeyValuePair<int, double>(i, position));
        }

        // 按显示位置排序
        visibleItems.Sort((a, b) => a.Value.CompareTo(b.Value));

        // 提取索引列表
        VisibleItemIndices = visibleItems.Select(item => item.Key).ToList();
    }

    /// <summary>
    /// 通用方法：设置偏移量
    /// </summary>
    private void SetOffset(double offset, bool isVertical)
    {
        if (_isInMeasure || InternalChildren.Count == 0)
            return;

        // 检查方向是否匹配当前面板设置
        if ((isVertical && Orientation != Orientation.Vertical) ||
            (!isVertical && Orientation != Orientation.Horizontal))
            return;

        // 获取总长度(高度或宽度)
        double totalLength = InternalChildren.Count * _itemSize;

        // 循环滚动处理
        if (totalLength > 0)
            offset = ((offset % totalLength) + totalLength) % totalLength;

        // 更新对应偏移量
        if (isVertical)
        {
            if (_offset.Y != offset)
            {
                _offset.Y = offset;
                _owner?.InvalidateScrollInfo();
                InvalidateArrange();
                UpdateVisibleItemIndices();
            }
        }
        else
        {
            if (_offset.X != offset)
            {
                _offset.X = offset;
                _owner?.InvalidateScrollInfo();
                InvalidateArrange();
                UpdateVisibleItemIndices();
            }
        }
    }

    /// <summary>
    /// 通用方法：设置偏移量并应用动画
    /// </summary>
    private void AnimateOffset(double offset, bool isVertical, bool forceUpdate = false)
    {
        if (_isInMeasure || InternalChildren.Count == 0 || (_isAnimating && !forceUpdate))
            return;

        // 检查方向是否匹配当前面板设置
        if ((isVertical && Orientation != Orientation.Vertical) ||
            (!isVertical && Orientation != Orientation.Horizontal))
            return;

        double totalLength = InternalChildren.Count * _itemSize;
        double currentOffset = isVertical ? _offset.Y : _offset.X;

        // 循环滚动处理
        if (totalLength > 0)
            offset = ((offset % totalLength) + totalLength) % totalLength;

        if (currentOffset == offset)
            return;

        if (_owner == null || !IsAnimationEnabled)
        {
            if (isVertical)
                SetVerticalOffset(offset);
            else
                SetHorizontalOffset(offset);
            return;
        }

        // 准备动画参数
        _isAnimating = true;
        double targetOffset = offset;
        double animationDistance = targetOffset - currentOffset;

        // 优化动画路径
        if (Math.Abs(animationDistance) > totalLength / 2 && totalLength > 0)
        {
            if (animationDistance > 0)
                targetOffset -= totalLength;
            else
                targetOffset += totalLength;
        }

        // 创建动画
        var animation = new DoubleAnimation
        {
            From = currentOffset,
            To = targetOffset,
            Duration = AnimationDuration,
            FillBehavior = FillBehavior.Stop
        };

        // 设置缓动函数
        switch (EasingMode)
        {
            case EasingMode.EaseIn:
                animation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn };
                break;

            case EasingMode.EaseOut:
                animation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut };
                break;

            case EasingMode.EaseInOut:
                animation.EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut };
                break;
        }

        // 设置附加属性
        if (isVertical)
            ScrollViewerHelper.SetVerticalOffset(_owner, currentOffset);
        else
            ScrollViewerHelper.SetHorizontalOffset(_owner, currentOffset);

        // 设置动画完成回调
        animation.Completed += (s, e) =>
        {
            // 计算最终偏移量
            double finalOffset = ((targetOffset % totalLength) + totalLength) % totalLength;

            // 更新偏移量
            if (isVertical)
            {
                _offset.Y = finalOffset;
                ScrollViewerHelper.SetVerticalOffset(_owner, finalOffset);
            }
            else
            {
                _offset.X = finalOffset;
                ScrollViewerHelper.SetHorizontalOffset(_owner, finalOffset);
            }

            _owner.InvalidateScrollInfo();
            InvalidateArrange();
            _isAnimating = false;

            // 更新可见项索引
            UpdateVisibleItemIndices();
        };

        // 开始动画
        if (isVertical)
            _owner.BeginAnimation(ScrollViewerHelper.VerticalOffsetProperty, animation);
        else
            _owner.BeginAnimation(ScrollViewerHelper.HorizontalOffsetProperty, animation);
    }

    #endregion 辅助方法

    #region 公共方法

    /// <summary>
    /// 滚动到指定索引位置，使该项目居中显示（带动画效果）
    /// </summary>
    /// <param name="index">要滚动到的项目索引</param>
    /// <param name="animated">是否使用动画效果，默认为 true</param>
    public void ScrollToIndex(int index, bool animated = true)
    {
        if (index < 0 || index >= InternalChildren.Count)
            throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围");

        bool isVertical = Orientation == Orientation.Vertical;
        double viewportLength = isVertical ? _viewport.Height : _viewport.Width;
        double itemPosition = index * _itemSize;

        // 计算居中位置的偏移量：项目顶部/左侧 - (视口高度/宽度 - 项目高度/宽度) / 2
        double centeredOffset = itemPosition - (viewportLength - _itemSize) / 2;

        // 确保偏移量在有效范围内
        double maxOffset = isVertical ?
            (_extent.Height - _viewport.Height) :
            (_extent.Width - _viewport.Width);

        centeredOffset = Math.Max(0, Math.Min(centeredOffset, maxOffset));

        // 设置偏移量（根据参数决定是否使用动画效果）
        if (animated && IsAnimationEnabled)
        {
            AnimateOffset(centeredOffset, isVertical);
        }
        else
        {
            SetOffset(centeredOffset, isVertical);
        }
    }

    #endregion 公共方法

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
        set
        {
            _owner = value;

            // 设置滚动条为隐藏，因为循环面板不需要显示滚动条
            if (_owner != null)
            {
                _owner.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
                _owner.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            }
        }
    }

    public void LineUp()
    {
        if (Orientation == Orientation.Vertical)
            SetVerticalOffset(VerticalOffset - _itemSize);
    }

    public void LineDown()
    {
        if (Orientation == Orientation.Vertical)
            SetVerticalOffset(VerticalOffset + _itemSize);
    }

    public void LineLeft()
    {
        if (Orientation == Orientation.Horizontal)
            SetHorizontalOffset(HorizontalOffset - _itemSize);
    }

    public void LineRight()
    {
        if (Orientation == Orientation.Horizontal)
            SetHorizontalOffset(HorizontalOffset + _itemSize);
    }

    public void PageUp()
    {
        if (Orientation == Orientation.Vertical)
            AnimateOffset(VerticalOffset - _viewport.Height, true);
    }

    public void PageDown()
    {
        if (Orientation == Orientation.Vertical)
            AnimateOffset(VerticalOffset + _viewport.Height, true);
    }

    public void PageLeft()
    {
        if (Orientation == Orientation.Horizontal)
            AnimateOffset(HorizontalOffset - _viewport.Width, false);
    }

    public void PageRight()
    {
        if (Orientation == Orientation.Horizontal)
            AnimateOffset(HorizontalOffset + _viewport.Width, false);
    }

    public void MouseWheelUp()
    {
        bool isVertical = Orientation == Orientation.Vertical;
        AnimateOffset(isVertical ? VerticalOffset - _itemSize : HorizontalOffset - _itemSize, isVertical);
    }

    public void MouseWheelDown()
    {
        bool isVertical = Orientation == Orientation.Vertical;
        AnimateOffset(isVertical ? VerticalOffset + _itemSize : HorizontalOffset + _itemSize, isVertical);
    }

    public void MouseWheelLeft()
    {
        if (Orientation == Orientation.Horizontal)
            AnimateOffset(HorizontalOffset - _itemSize, false);
    }

    public void MouseWheelRight()
    {
        if (Orientation == Orientation.Horizontal)
            AnimateOffset(HorizontalOffset + _itemSize, false);
    }

    public Rect MakeVisible(Visual visual, Rect rectangle)
    {
        if (visual == null || InternalChildren.Count == 0 || rectangle.IsEmpty)
            return Rect.Empty;

        // 找到视觉元素对应的子项索引
        int index = -1;
        for (int i = 0; i < InternalChildren.Count; i++)
        {
            if (InternalChildren[i] == visual)
            {
                index = i;
                break;
            }
        }

        if (index == -1)
            return Rect.Empty;

        bool isVertical = Orientation == Orientation.Vertical;
        double itemPosition = index * _itemSize;
        double itemEnd = itemPosition + _itemSize;
        double currentOffset = isVertical ? VerticalOffset : HorizontalOffset;
        double viewportLength = isVertical ? _viewport.Height : _viewport.Width;

        if (itemPosition < currentOffset)
        {
            // 项目在可视区域前（上/左），滚动使其可见
            AnimateOffset(itemPosition, isVertical);
        }
        else if (itemEnd > currentOffset + viewportLength)
        {
            // 项目在可视区域后（下/右），滚动使其可见
            AnimateOffset(itemEnd - viewportLength, isVertical);
        }

        // 返回项目在视口中的可见区域
        return isVertical
            ? new Rect(0, itemPosition - currentOffset, _viewport.Width, _itemSize)
            : new Rect(itemPosition - currentOffset, 0, _itemSize, _viewport.Height);
    }

    public void SetHorizontalOffset(double offset)
    {
        SetOffset(offset, false);
    }

    public void SetVerticalOffset(double offset)
    {
        SetOffset(offset, true);
    }

    #endregion IScrollInfo Implementation
}