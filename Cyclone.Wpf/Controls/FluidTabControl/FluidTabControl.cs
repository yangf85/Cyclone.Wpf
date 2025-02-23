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

namespace Cyclone.Wpf.Controls;

public enum FluidTabPlacement
{
    Left,

    Right,
}

[TemplatePart(Name = "PART_Container", Type = typeof(ScrollViewer))]
public class FluidTabControl : Selector
{
    private ScrollViewer _container;

    private Panel _itemsPanel;

    private bool _isSelecting;

    private bool _isScrolling;

    static FluidTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabControl),
            new FrameworkPropertyMetadata(typeof(FluidTabControl)));
    }

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

    private void OnContainerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (_isSelecting || _container == null) return;

        var hitTestPoint = new Point(1, e.VerticalOffset);
        var hitTestResult = _itemsPanel.InputHitTest(hitTestPoint);

        if (hitTestResult is DependencyObject element)
        {
            var targetItem = FindContentOwner(element);
            HandleScrollPosition(targetItem, e.VerticalOffset);
        }
    }

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
        if (Items.Count == 0) return null;
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
            _container?.ScrollToVerticalOffset(position.Y);
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
        return null;
    }

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
    }
}