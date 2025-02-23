using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;

namespace Cyclone.Wpf.Controls;

[ContentProperty("Content")]
public class FluidTabItem : Control
{
    internal static readonly DependencyProperty ContentOwnerProperty;

    public static readonly DependencyProperty HeaderProperty;

    public static readonly DependencyProperty ContentProperty;

    public static readonly DependencyProperty IsSelectedProperty;

    public object Header
    {
        get
        {
            return GetValue(HeaderProperty);
        }
        set
        {
            SetValue(HeaderProperty, value);
        }
    }

    public object Content
    {
        get
        {
            return GetValue(ContentProperty);
        }
        set
        {
            SetValue(ContentProperty, value);
        }
    }

    public bool IsSelected
    {
        get
        {
            return (bool)GetValue(IsSelectedProperty);
        }
        set
        {
            SetValue(IsSelectedProperty, value);
        }
    }

    static FluidTabItem()
    {
        HeaderProperty = DependencyProperty.Register("Header", typeof(object), typeof(FluidTabItem), new PropertyMetadata(default));
        ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(FluidTabItem), new PropertyMetadata(ContentPropertyChangedCallback));
        ContentOwnerProperty = DependencyProperty.RegisterAttached("ContentOwner", typeof(FluidTabItem), typeof(FluidTabItem));
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabItem), new FrameworkPropertyMetadata(typeof(FluidTabItem)));
        IsSelectedProperty = Selector.IsSelectedProperty.AddOwner(typeof(FluidTabItem), new FrameworkPropertyMetadata(OnIsSelectedChanged));
    }

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        FluidTabItem fluidTabItem = d as FluidTabItem;
        if ((bool)e.NewValue)
        {
            fluidTabItem.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, fluidTabItem));
        }
        else
        {
            fluidTabItem.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, fluidTabItem));
        }
    }

    private void HandleIsSelectedChanged(bool newValue, RoutedEventArgs e)
    {
        RaiseEvent(e);
    }

    protected virtual void OnSelected(RoutedEventArgs e)
    {
        HandleIsSelectedChanged(newValue: true, e);
    }

    protected virtual void OnUnselected(RoutedEventArgs e)
    {
        HandleIsSelectedChanged(newValue: false, e);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (ItemsControl.ItemsControlFromItemContainer(this) is Selector selector)
        {
            selector.SelectedItem = this;
        }
    }

    internal static FluidTabItem GetContentOwner(DependencyObject obj)
    {
        return (FluidTabItem)obj.GetValue(ContentOwnerProperty);
    }

    internal static void SetContentOwner(DependencyObject obj, FluidTabItem value)
    {
        obj.SetValue(ContentOwnerProperty, value);
    }

    public static void ContentPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SetContentOwner(e.NewValue as DependencyObject, d as FluidTabItem);
    }
}