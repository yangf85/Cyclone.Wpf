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

[ContentProperty("Content")]//不能继承ContentControl 会导致视觉元素连接错误
public class FluidTabItem : Control
{
    public FluidTabItem()
    {
        // 监听 DataContext 变化
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        // 触发父控件更新内容
        if (Parent is FluidTabControl tabControl)
        {
            tabControl.UpdateItemsContent();
        }
    }

    static FluidTabItem()
    {
        FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabItem), new FrameworkPropertyMetadata(typeof(FluidTabItem)));
    }

    #region Content

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register("Content", typeof(object), typeof(FluidTabItem), new PropertyMetadata(ContentPropertyChangedCallback));

    public object Content
    {
        get => GetValue(ContentProperty);

        set => SetValue(ContentProperty, value);
    }

    public static void ContentPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        SetContentOwner(e.NewValue as DependencyObject, d as FluidTabItem);
    }

    #endregion Content

    #region Header

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register("Header", typeof(object), typeof(FluidTabItem), new PropertyMetadata(default));

    public object Header
    {
        get => GetValue(HeaderProperty);

        set => SetValue(HeaderProperty, value);
    }

    #endregion Header

    #region ContentOwner

    internal static readonly DependencyProperty ContentOwnerProperty =
        DependencyProperty.RegisterAttached("ContentOwner", typeof(FluidTabItem), typeof(FluidTabItem));

    internal static FluidTabItem GetContentOwner(DependencyObject obj)
    {
        return (FluidTabItem)obj.GetValue(ContentOwnerProperty);
    }

    internal static void SetContentOwner(DependencyObject obj, FluidTabItem value)
    {
        obj.SetValue(ContentOwnerProperty, value);
    }

    #endregion ContentOwner

    #region IsSelected

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);

        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        Selector.IsSelectedProperty.AddOwner(typeof(FluidTabItem), new FrameworkPropertyMetadata(OnIsSelectedChanged));

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

    #endregion IsSelected

    #region Override

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (ItemsControl.ItemsControlFromItemContainer(this) is FluidTabControl tabControl)
        {
            // 获取当前 FluidTabItem 对应的数据项
            var dataItem = tabControl.ItemContainerGenerator.ItemFromContainer(this);

            if (dataItem != null)
            {
                // 如果找到了数据项，设置它为选中项
                tabControl.SelectedItem = dataItem;
            }
            else
            {
                // 如果没有找到数据项（可能是直接添加的 FluidTabItem），则设置控件本身
                tabControl.SelectedItem = this;
            }
        }
    }

    #endregion Override
}