using Cyclone.Wpf.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;


public class CascadePickerItem : HeaderedItemsControl,ICascadeNode
{
    CascadePicker _root;
    static CascadePickerItem()
    {

        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePickerItem), new FrameworkPropertyMetadata(typeof(CascadePickerItem)));
    }

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = ElementHelper.TryFindLogicalParent<CascadePicker>(this);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
    }



    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is CascadePickerItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new CascadePickerItem();
    }
    #endregion


    #region NodePath
    public string NodePath
    {
        get => (string)GetValue(NodePathProperty);
        set => SetValue(NodePathProperty, value);
    }

    public static readonly DependencyProperty NodePathProperty =
        DependencyProperty.Register(nameof(NodePath), typeof(string), typeof(CascadePickerItem),
            new PropertyMetadata(default(string), null, CoerceNodePathValue));
    private static object CoerceNodePathValue(DependencyObject d, object baseValue)
    {
        if (d is not CascadePickerItem item) { return baseValue; }

        if (item.ItemTemplate != null && item.DataContext is ICascadeNode node)
        {
            return node.NodePath;
        }
        
        return item?.Header?.ToString() ?? baseValue;
    }

    #endregion

    #region ItemClickEvent

    public static readonly RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent("ItemClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CascadePickerItem));

    public event RoutedEventHandler ItemClick
    {
        add => AddHandler(ItemClickEvent, value);
        remove => RemoveHandler(ItemClickEvent, value);
    }

    private void OnItemClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
    }


    #endregion


    #region IsHighlighted
    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    static readonly  DependencyPropertyKey IsHighlightedPropertyKey=
        DependencyProperty.RegisterReadOnly(nameof(IsHighlighted), typeof(bool), typeof(CascadePickerItem), new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsHighlightedProperty = IsHighlightedPropertyKey.DependencyProperty;
        

    #endregion

}