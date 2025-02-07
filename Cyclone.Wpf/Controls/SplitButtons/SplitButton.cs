using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 不要使用数据模板
/// </summary>
public class SplitButton : ItemsControl
{
    #region ItemClick
    public static readonly RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent("ItemClick",
        RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<object>), typeof(SplitButton));

    public event RoutedPropertyChangedEventHandler<object> ItemClick
    {
        add
        {
            AddHandler(ItemClickEvent, value);
        }
        remove
        {
            RemoveHandler(ItemClickEvent, value);
        }
    }

    public virtual void OnItemClick(object oldValue, object newValue)
    {
        RoutedPropertyChangedEventArgs<object> arg = new RoutedPropertyChangedEventArgs<object>(oldValue, newValue, ItemClickEvent);
        RaiseEvent(arg);
    }

    #endregion


    #region Label
    public object Label
    {
        get => (object)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(SplitButton), new PropertyMetadata(default(object)));

    #endregion

    #region IsDropDownOpen
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(SplitButton), new PropertyMetadata(default(bool)));

    #endregion

    #region Override方法
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new SplitButtonItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SplitButtonItem;
    }

    #endregion


}
