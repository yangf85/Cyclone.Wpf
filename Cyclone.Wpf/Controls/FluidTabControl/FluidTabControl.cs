using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using Cyclone.Wpf.Helpers;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public enum FluidTabPlacement
{
    Left,

    Right,
}

[TemplatePart(Name = "PART_ItemsPanel", Type = typeof(Panel))]
[TemplatePart(Name = "PART_Container", Type = typeof(ScrollViewer))]
public class FluidTabControl : Selector
{
    private ScrollViewer _container;
    private Panel _itemsPanel;
    private bool _isSelecting;
    private bool _isScrolling;
    private Storyboard _currentStoryboard;

    #region AnimationDuration

    public static readonly DependencyProperty AnimationDurationProperty =
    DependencyProperty.Register("AnimationDuration", typeof(TimeSpan), typeof(FluidTabControl),
        new FrameworkPropertyMetadata(TimeSpan.FromSeconds(1d)));

    public TimeSpan AnimationDuration
    {
        get => (TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    #endregion AnimationDuration

    #region FluidTabPlacement

    public FluidTabPlacement FluidTabPlacement
    {
        get => (FluidTabPlacement)GetValue(FluidTabPlacementProperty);
        set => SetValue(FluidTabPlacementProperty, value);
    }

    public static readonly DependencyProperty FluidTabPlacementProperty =
        DependencyProperty.Register(nameof(FluidTabPlacement), typeof(FluidTabPlacement), typeof(FluidTabControl), new PropertyMetadata(default(FluidTabPlacement)));

    #endregion FluidTabPlacement

    #region ItemHeaderHorizontal

    public HorizontalAlignment ItemHeaderHorizontal
    {
        get => (HorizontalAlignment)GetValue(ItemHeaderHorizontalProperty);
        set => SetValue(ItemHeaderHorizontalProperty, value);
    }

    public static readonly DependencyProperty ItemHeaderHorizontalProperty =
        DependencyProperty.Register(nameof(ItemHeaderHorizontal), typeof(HorizontalAlignment), typeof(FluidTabControl), new PropertyMetadata(default(HorizontalAlignment)));

    #endregion ItemHeaderHorizontal

    static FluidTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabControl),
            new FrameworkPropertyMetadata(typeof(FluidTabControl)));
    }

    #region Private

    private static FluidTabItem FindContentOwner(DependencyObject element)
    {
        while (element != null)
        {
            if (element is Border border && border.Child != null)
            {
                return FluidTabItem.GetContentOwner(border.Child);
            }

            if (element is FluidTabItem tabItem)
            {
                return tabItem;
            }

            element = VisualTreeHelper.GetParent(element);
        }
        return null;
    }

    private void UpdateItemsContent()
    {
        if (_container == null || _itemsPanel == null) return;

        _itemsPanel.Children.Clear();

        foreach (var item in GetValidItems())
        {
            var content = new Border
            {
                Background = Brushes.Transparent,
                Child = GetItemContent(item)
            };

            _itemsPanel.Children.Add(content);
        }
    }

    private IEnumerable<FluidTabItem> GetValidItems() =>
        Items.OfType<object>()
            .Select(GetFluidTabItem)
            .Where(item => item != null);

    private UIElement GetItemContent(FluidTabItem item) =>
        item?.Content as UIElement ?? new FrameworkElement();

    private void HandleScrollPosition(FluidTabItem targetItem, double verticalOffset)
    {
        if (targetItem == null) return;

        bool atBottom = verticalOffset > _container.ScrollableHeight - 1;
        var finalItem = atBottom ? GetLastItem() : targetItem;

        var rawItem = finalItem != null
            ? ItemContainerGenerator.ItemFromContainer(finalItem)
            : null;

        if (rawItem != null && !Equals(SelectedItem, rawItem))
        {
            _isScrolling = true;
            SelectedItem = rawItem;
            _isScrolling = false;
        }
    }

    private FluidTabItem GetLastItem()
    {
        if (Items.Count == 0) { return null; }
        var lastItem = Items[Items.Count - 1];
        return GetFluidTabItem(lastItem);
    }

    private void ScrollToSelectedItem()
    {
        var selectedItem = GetSelectedContainer();
        if (selectedItem?.Content is not FrameworkElement content) return;

        if (VisualTreeHelper.GetParent(content) is Border border)
        {
            var position = border.TranslatePoint(new Point(), _itemsPanel);
            ScrollToOffset(position.Y);
        }
    }

    private FluidTabItem GetSelectedContainer()
    {
        if (SelectedItem == null) return null;

        return ItemContainerGenerator.ContainerFromItem(SelectedItem) as FluidTabItem
            ?? GetFluidTabItem(SelectedItem);
    }

    private FluidTabItem GetFluidTabItem(object item)
    {
        if (item is FluidTabItem tabItem) return tabItem;

        if (item != null)
        {
            int index = Items.IndexOf(item);
            if (index >= 0)
            {
                return ItemContainerGenerator.ContainerFromIndex(index) as FluidTabItem;
            }
        }
        return default;
    }

    private void ScrollToOffset(double targetOffset)
    {
        if (_container == null) return;

        // 停止正在进行的动画
        _currentStoryboard?.Stop();

        if (AnimationDuration == TimeSpan.Zero || _container.VerticalOffset == targetOffset)
        {
            _container.ScrollToVerticalOffset(targetOffset);
            return;
        }

        var animation = new DoubleAnimation
        {
            From = _container.VerticalOffset,
            To = targetOffset,
            Duration = AnimationDuration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        _currentStoryboard = new Storyboard();
        _currentStoryboard.Children.Add(animation);

        Storyboard.SetTarget(animation, _container);
        Storyboard.SetTargetProperty(animation, new PropertyPath(ScrollViewerBehavior.VerticalOffsetProperty));

        _currentStoryboard.Completed += (s, e) => _currentStoryboard = null;
        _currentStoryboard.Begin();
    }

    private void OnContainerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isSelecting || _container == null || _currentStoryboard != null) return;

        var hitTestPoint = new Point(1, e.VerticalOffset);
        var hitTestResult = _itemsPanel.InputHitTest(hitTestPoint);

        if (hitTestResult is DependencyObject element)
        {
            var targetItem = FindContentOwner(element);
            HandleScrollPosition(targetItem, e.VerticalOffset);
        }
    }

    #endregion Private

    #region Override

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateItemsContent();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (!_isScrolling && _container != null)
        {
            _isSelecting = true;
            ScrollToSelectedItem();
            _isSelecting = false;
        }
    }

    protected override DependencyObject GetContainerForItemOverride() => new FluidTabItem();

    protected override bool IsItemItsOwnContainerOverride(object item) => item is FluidTabItem;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_container != null)
        {
            _container.ScrollChanged -= OnContainerScrollChanged;
        }

        _container = GetTemplateChild("PART_Container") as ScrollViewer;
        _itemsPanel = new VirtualizingStackPanel();
        VirtualizingPanel.SetVirtualizationMode(_itemsPanel, VirtualizationMode.Recycling);

        if (_container != null)
        {
            _container.Content = _itemsPanel;
            _container.ScrollChanged += OnContainerScrollChanged;
        }

        UpdateItemsContent();

        PreviewMouseWheel += FluidTabControl_PreviewMouseWheel;
    }

    // 修复在Item区域滚动时，鼠标滚轮事件被拦截的问题
    private void FluidTabControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (_container != null && !_isScrolling)
        {
            // 计算新的垂直偏移
            double newOffset = _container.VerticalOffset - (e.Delta / 3.0);
            newOffset = Math.Max(0, Math.Min(newOffset, _container.ScrollableHeight));

            // 滚动到新位置
            _container.ScrollToVerticalOffset(newOffset);

            // 标记事件为已处理
            e.Handled = true;
        }
    }

    #endregion Override

    private static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior),
                new FrameworkPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static double GetVerticalOffset(ScrollViewer obj) => obj.VerticalOffset;

        public static void SetVerticalOffset(ScrollViewer obj, double value) => obj.ScrollToVerticalOffset(value);

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }
    }
}