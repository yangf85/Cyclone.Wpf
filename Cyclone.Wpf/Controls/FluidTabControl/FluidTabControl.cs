// 完整的 FluidTabControl.cs 修复

using Cyclone.Wpf.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

    public static readonly DependencyProperty FluidTabPlacementProperty =
        DependencyProperty.Register(nameof(FluidTabPlacement), typeof(FluidTabPlacement), typeof(FluidTabControl), new PropertyMetadata(default(FluidTabPlacement)));

    public FluidTabPlacement FluidTabPlacement
    {
        get => (FluidTabPlacement)GetValue(FluidTabPlacementProperty);
        set => SetValue(FluidTabPlacementProperty, value);
    }

    #endregion FluidTabPlacement

    #region ItemHeaderHorizontal

    public static readonly DependencyProperty ItemHeaderHorizontalProperty =
        DependencyProperty.Register(nameof(ItemHeaderHorizontal), typeof(HorizontalAlignment), typeof(FluidTabControl), new PropertyMetadata(default(HorizontalAlignment)));

    public HorizontalAlignment ItemHeaderHorizontal
    {
        get => (HorizontalAlignment)GetValue(ItemHeaderHorizontalProperty);
        set => SetValue(ItemHeaderHorizontalProperty, value);
    }

    #endregion ItemHeaderHorizontal

    static FluidTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabControl),
            new FrameworkPropertyMetadata(typeof(FluidTabControl)));
    }

    #region Private

    private void ScrollToSelectedItem()
    {
        if (SelectedItem == null) return;

        // 获取选中项的索引
        int selectedIndex = Items.IndexOf(SelectedItem);
        if (selectedIndex < 0) return;

        // 查找对应索引的内容元素
        FrameworkElement targetElement = null;

        foreach (var child in _itemsPanel.Children)
        {
            if (child is Border border && border.Tag is int index && index == selectedIndex)
            {
                targetElement = border;
                break;
            }
        }

        if (targetElement != null)
        {
            var position = targetElement.TranslatePoint(new Point(), _itemsPanel);
            ScrollToOffset(position.Y);
        }
    }

    private FluidTabItem GetFluidTabItem(object item)
    {
        if (item is FluidTabItem tabItem) return tabItem;

        // 当使用 ItemsSource 时，ItemContainerGenerator 会为每个数据项生成容器
        var container = ItemContainerGenerator.ContainerFromItem(item) as FluidTabItem;
        if (container != null) return container;

        // 如果还没有生成容器，通过索引查找
        int index = Items.IndexOf(item);
        if (index >= 0)
        {
            return ItemContainerGenerator.ContainerFromIndex(index) as FluidTabItem;
        }

        return null;
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

        // 找到当前可见的内容项
        var hitTestPoint = new Point(_container.ActualWidth / 2, e.VerticalOffset + _container.ActualHeight / 2);
        var hitTestResult = VisualTreeHelper.HitTest(_itemsPanel, hitTestPoint);

        if (hitTestResult?.VisualHit != null)
        {
            // 向上查找 Border
            DependencyObject current = hitTestResult.VisualHit;
            while (current != null && !(current is Border border && border.Tag is int))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            if (current is Border targetBorder && targetBorder.Tag is int index)
            {
                // 通过索引获取对应的项
                if (index >= 0 && index < Items.Count)
                {
                    var item = Items[index];
                    if (!Equals(SelectedItem, item))
                    {
                        _isScrolling = true;
                        SelectedItem = item;
                        _isScrolling = false;
                    }
                }
            }
        }
    }

    internal void UpdateItemsContent()
    {
        if (_container == null || _itemsPanel == null) return;

        _itemsPanel.Children.Clear();

        for (int i = 0; i < Items.Count; i++)
        {
            var item = Items[i];
            var container = GetFluidTabItem(item);

            // 创建内容包装器
            var contentWrapper = new Border
            {
                Background = Brushes.Transparent,
                Tag = i  // 使用索引作为标识
            };

            // 使用 ContentPresenter 来显示内容，避免直接使用已有父元素的内容
            ContentPresenter contentPresenter = null;

            if (item is FluidTabItem tabItem)
            {
                // 对于 FluidTabItem，创建 ContentPresenter 并绑定到其 Content
                contentPresenter = new ContentPresenter();
                contentPresenter.SetBinding(ContentPresenter.ContentProperty, new Binding("Content") { Source = tabItem });

                // 绑定 DataContext
                var dataContextBinding = new Binding("DataContext")
                {
                    Source = tabItem,
                    Mode = BindingMode.OneWay
                };
                contentWrapper.SetBinding(Border.DataContextProperty, dataContextBinding);
            }
            else
            {
                // 对于通过 ItemsSource 绑定的项，使用 ItemTemplate
                contentPresenter = new ContentPresenter
                {
                    ContentTemplate = ItemTemplate,
                    Content = item
                };
                contentWrapper.DataContext = item;
            }

            // 设置 ContentOwner，建立关联
            if (container != null && contentPresenter != null)
            {
                FluidTabItem.SetContentOwner(contentPresenter, container);
                // 同时在 wrapper 上也设置，以便查找
                FluidTabItem.SetContentOwner(contentWrapper, container);
            }

            contentWrapper.Child = contentPresenter;
            _itemsPanel.Children.Add(contentWrapper);
        }
    }

    #endregion Private

    #region Override

    // 使用普通的 MouseWheel 事件，只有当其他控件没有处理时才会到达这里
    private void FluidTabControl_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        // 如果事件已经被处理，则不再处理
        if (e.Handled || _container == null || _isScrolling) return;

        // 检查鼠标是否在内容区域内
        var containerPosition = e.GetPosition(_container);
        var contentBounds = new Rect(0, 0, _container.ActualWidth, _container.ActualHeight);

        if (contentBounds.Contains(containerPosition))
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

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);

        // 延迟更新内容，确保容器已经生成
        Dispatcher.BeginInvoke(new Action(() => UpdateItemsContent()),
            System.Windows.Threading.DispatcherPriority.Loaded);
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

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is FluidTabItem container && !(item is FluidTabItem))
        {
            var itemType = item.GetType();
            var headerProperty = itemType.GetProperty("Header");
            if (headerProperty != null)
            {
                var headerBinding = new Binding("Header")
                {
                    Source = item,
                    Mode = BindingMode.OneWay
                };
                container.SetBinding(FluidTabItem.HeaderProperty, headerBinding);
            }

            container.DataContext = item;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _container = GetTemplateChild("PART_Container") as ScrollViewer;
        _itemsPanel = new VirtualizingStackPanel();
        VirtualizingPanel.SetVirtualizationMode(_itemsPanel, VirtualizationMode.Recycling);

        if (_container != null)
        {
            _container.ScrollChanged -= OnContainerScrollChanged;
            _container.ScrollChanged += OnContainerScrollChanged;
            _container.Content = _itemsPanel;
        }

        UpdateItemsContent();

        // 移除旧的事件处理器（如果存在）
        MouseWheel -= FluidTabControl_MouseWheel;
        // 使用普通的 MouseWheel 事件而不是 PreviewMouseWheel
        MouseWheel += FluidTabControl_MouseWheel;
    }

    #endregion Override

    private static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset", typeof(double), typeof(ScrollViewerBehavior),
                new FrameworkPropertyMetadata(0.0, OnVerticalOffsetChanged));

        private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        public static double GetVerticalOffset(ScrollViewer obj) => obj.VerticalOffset;

        public static void SetVerticalOffset(ScrollViewer obj, double value) => obj.ScrollToVerticalOffset(value);
    }
}