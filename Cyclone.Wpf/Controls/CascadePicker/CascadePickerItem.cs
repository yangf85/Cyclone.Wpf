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


public class CascadePickerItem : MenuItem,ICascadeNode
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


 

}