using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
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
                new FrameworkPropertyMetadata(Orientation.Horizontal,
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
                    double contentHeight = itemCount * _itemSize;

                    // 设置滚动区域大小
                    _extent = new Size(measureSize.Width, contentHeight);
                    _viewport = measureSize;

                    if (_owner != null)
                    {
                        _owner.InvalidateScrollInfo();
                    }

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

                    if (_owner != null)
                    {
                        _owner.InvalidateScrollInfo();
                    }

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

            if (_owner != null)
            {
                _owner.InvalidateScrollInfo();
            }

            // 用于收集可见项目的索引和位置信息
            List<KeyValuePair<int, double>> visibleItemsWithPosition = new List<KeyValuePair<int, double>>();

            // 根据方向排列子元素
            if (Orientation == Orientation.Vertical)
            {
                // 重新计算每个项目的高度
                if (VisibleItemCount > 0 && finalSize.Height > 0)
                {
                    _itemSize = finalSize.Height / VisibleItemCount;
                }

                double totalHeight = itemCount * _itemSize;

                // 排列所有子项
                for (int i = 0; i < itemCount; i++)
                {
                    UIElement child = InternalChildren[i];

                    // 计算项目的位置，但允许循环显示
                    double itemTop = (i * _itemSize) - VerticalOffset;

                    // 当项目超出下方边界时，将其移到顶部循环显示
                    if (itemTop >= finalSize.Height && itemCount > VisibleItemCount)
                    {
                        itemTop -= totalHeight;
                    }
                    // 当项目超出上方边界时，将其移到底部循环显示
                    else if (itemTop < -_itemSize && itemCount > VisibleItemCount)
                    {
                        itemTop += totalHeight;
                    }

                    // 确定项目是否可见
                    bool isVisible = (itemTop + _itemSize > 0) && (itemTop < finalSize.Height);

                    if (isVisible)
                    {
                        // 布局子项
                        child.Arrange(new Rect(0, itemTop, finalSize.Width, _itemSize));
                        child.Visibility = Visibility.Visible;

                        // 添加到可见项目索引和位置列表，用位置排序
                        visibleItemsWithPosition.Add(new KeyValuePair<int, double>(i, itemTop));
                    }
                    else
                    {
                        // 不可见的项目
                        child.Arrange(new Rect(0, itemTop, 0, 0));
                        child.Visibility = Visibility.Collapsed;
                    }
                }

                // 按照垂直位置（从上到下）排序可见项目
                visibleItemsWithPosition.Sort((a, b) => a.Value.CompareTo(b.Value));
            }
            else // Orientation.Horizontal
            {
                // 重新计算每个项目的宽度
                if (VisibleItemCount > 0 && finalSize.Width > 0)
                {
                    _itemSize = finalSize.Width / VisibleItemCount;
                }

                double totalWidth = itemCount * _itemSize;

                // 排列所有子项
                for (int i = 0; i < itemCount; i++)
                {
                    UIElement child = InternalChildren[i];

                    // 计算项目的位置，但允许循环显示
                    double itemLeft = (i * _itemSize) - HorizontalOffset;

                    // 当项目超出右侧边界时，将其移到左侧循环显示
                    if (itemLeft >= finalSize.Width && itemCount > VisibleItemCount)
                    {
                        itemLeft -= totalWidth;
                    }
                    // 当项目超出左侧边界时，将其移到右侧循环显示
                    else if (itemLeft < -_itemSize && itemCount > VisibleItemCount)
                    {
                        itemLeft += totalWidth;
                    }

                    // 确定项目是否可见
                    bool isVisible = (itemLeft + _itemSize > 0) && (itemLeft < finalSize.Width);

                    if (isVisible)
                    {
                        // 布局子项
                        child.Arrange(new Rect(itemLeft, 0, _itemSize, finalSize.Height));
                        child.Visibility = Visibility.Visible;

                        // 添加到可见项目索引和位置列表，用位置排序
                        visibleItemsWithPosition.Add(new KeyValuePair<int, double>(i, itemLeft));
                    }
                    else
                    {
                        // 不可见的项目
                        child.Arrange(new Rect(itemLeft, 0, 0, 0));
                        child.Visibility = Visibility.Collapsed;
                    }
                }

                // 按照水平位置（从左到右）排序可见项目
                visibleItemsWithPosition.Sort((a, b) => a.Value.CompareTo(b.Value));
            }

            // 提取排序后的可见项目索引列表
            List<int> visibleIndices = visibleItemsWithPosition.Select(kv => kv.Key).ToList();

            // 更新可见项目索引列表依赖属性
            VisibleItemIndices = visibleIndices;

            return finalSize;
        }

        #endregion Layout Overrides

        /// <summary>
        /// 滚动到指定索引位置，使该项目居中显示
        /// </summary>
        /// <param name="index">要滚动到的项目索引</param>
        public void ScrollToIndex(int index)
        {
            if (index < 0 || index >= InternalChildren.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "索引超出范围");

            if (Orientation == Orientation.Vertical)
            {
                // 计算项目的顶部位置
                double itemTop = index * _itemSize;

                // 计算居中位置的偏移量：项目顶部 - (视口高度 - 项目高度) / 2
                double centeredOffset = itemTop - (_viewport.Height - _itemSize) / 2;

                // 确保偏移量在有效范围内
                centeredOffset = Math.Max(0, Math.Min(centeredOffset, _extent.Height - _viewport.Height));

                // 设置垂直偏移量
                SetVerticalOffset(centeredOffset);
            }
            else // Orientation.Horizontal
            {
                // 计算项目的左侧位置
                double itemLeft = index * _itemSize;

                // 计算居中位置的偏移量：项目左侧 - (视口宽度 - 项目宽度) / 2
                double centeredOffset = itemLeft - (_viewport.Width - _itemSize) / 2;

                // 确保偏移量在有效范围内
                centeredOffset = Math.Max(0, Math.Min(centeredOffset, _extent.Width - _viewport.Width));

                // 设置水平偏移量
                SetHorizontalOffset(centeredOffset);
            }
        }

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
            if (_isInMeasure || InternalChildren.Count == 0 || Orientation != Orientation.Horizontal)
                return;

            double totalWidth = InternalChildren.Count * _itemSize;

            // 循环滚动处理
            if (totalWidth > 0)
            {
                // 实现循环效果：当滚动超过内容宽度时，回到开始
                offset = ((offset % totalWidth) + totalWidth) % totalWidth;
            }

            if (_offset.X != offset)
            {
                _offset.X = offset;

                _owner?.InvalidateScrollInfo();

                InvalidateArrange();
            }
        }

        public void SetVerticalOffset(double offset)
        {
            if (_isInMeasure || InternalChildren.Count == 0 || Orientation != Orientation.Vertical)
                return;

            double totalHeight = InternalChildren.Count * _itemSize;

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
}