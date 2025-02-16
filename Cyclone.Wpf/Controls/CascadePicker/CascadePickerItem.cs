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
        _root = VisualTreeHelperExtension.TryFindLogicalParent<CascadePicker>(this);
    }

    protected override void OnHeaderChanged(object oldHeader, object newHeader)
    {
        base.OnHeaderChanged(oldHeader, newHeader);
        UpdateNodePath();
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        SetValue(IsPressedPropertyKey, true);
        CaptureMouse(); 
        RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        SetValue(IsPressedPropertyKey, false);
        ReleaseMouseCapture(); // 释放鼠标捕获
    }
  
    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        SetValue(IsPressedPropertyKey, false);
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
            new PropertyMetadata(default(string), OnNodePathChanged));

    private static void OnNodePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePickerItem item)
        {
            // 更新 NodePath 的值
            item.UpdateNodePath();
        }
    }

    private void UpdateNodePath()
    {
        if (DataContext is ICascadeNode node)
        {
            SetCurrentValue(NodePathProperty, node.NodePath);
        }
        else
        {
            SetCurrentValue(NodePathProperty, Header?.ToString() ?? string.Empty);
        }
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

    #region IsPressed
    public bool IsPressed
        {
            get => (bool)GetValue(IsPressedProperty);
            
        }


    private static readonly DependencyPropertyKey IsPressedPropertyKey=
        DependencyProperty.RegisterReadOnly(nameof(IsPressed), typeof(bool), typeof(CascadePickerItem), new PropertyMetadata(default(bool)));

    

    public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

    #endregion

}