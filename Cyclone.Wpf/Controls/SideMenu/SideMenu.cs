using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

public class SideMenu : ItemsControl
{
    static SideMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenu), new FrameworkPropertyMetadata(typeof(SideMenu)));
    }

    #region IsCompact

    public bool IsCompact
    {
        get => (bool)GetValue(IsCompactProperty);
        set => SetValue(IsCompactProperty, value);
    }

    public static readonly DependencyProperty IsCompactProperty =
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(SideMenu), new PropertyMetadata(false));

    #endregion IsCompact

    #region IsShowOpenButton

    public bool IsShowOpenButton
    {
        get => (bool)GetValue(IsShowOpenButtonProperty);
        set => SetValue(IsShowOpenButtonProperty, value);
    }

    public static readonly DependencyProperty IsShowOpenButtonProperty =
        DependencyProperty.Register(nameof(IsShowOpenButton), typeof(bool), typeof(SideMenu), new PropertyMetadata(true));

    #endregion IsShowOpenButton

    #region CollapseWidth

    public double CollapseWidth
    {
        get => (double)GetValue(CollapseWidthProperty);
        set => SetValue(CollapseWidthProperty, value);
    }

    public static readonly DependencyProperty CollapseWidthProperty =
        DependencyProperty.Register(nameof(CollapseWidth), typeof(double), typeof(SideMenu), new PropertyMetadata(60d));

    #endregion CollapseWidth

    #region ExpansionWidth

    public double ExpansionWidth
    {
        get => (double)GetValue(ExpansionWidthProperty);
        set => SetValue(ExpansionWidthProperty, value);
    }

    public static readonly DependencyProperty ExpansionWidthProperty =
        DependencyProperty.Register(nameof(ExpansionWidth), typeof(double), typeof(SideMenu), new PropertyMetadata(180d));

    #endregion ExpansionWidth

    #region Header

    public object Header
    {
        get => (object)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SideMenu), new PropertyMetadata(default(object)));

    #endregion Header

    #region Footer

    public object Footer
    {
        get => (object)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(SideMenu), new PropertyMetadata(default(object)));

    #endregion Footer

    #region Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new SideMenuItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SideMenuItem;
    }

    #endregion Override

    internal void DeactivateItems()
    {
        // 从顶层开始递归取消所有子项的激活状态
        DeactivateItemsRecursively(this);
    }

    /// <summary>
    /// 递归取消传入 ItemsControl 内所有子项的激活状态
    /// </summary>
    /// <param name="itemsControl">当前遍历的 ItemsControl</param>
    private void DeactivateItemsRecursively(ItemsControl itemsControl)
    {
        foreach (var item in itemsControl.Items)
        {
            // 获取该项对应的容器
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                // 取消当前菜单项的激活状态
                menuItem.SetInactive();

                // 如果当前菜单项内还有嵌套的子项，则递归调用
                if (menuItem.HasItems)
                {
                    DeactivateItemsRecursively(menuItem);
                }
            }
        }
    }
}