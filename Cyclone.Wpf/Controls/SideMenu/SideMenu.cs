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
        DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(SideMenu),
        new PropertyMetadata(false, OnIsCompactChanged));

    private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.AnimateWidthChange((bool)e.NewValue);
        }
    }

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

    #region AnimationDuration

    public Duration AnimationDuration
    {
        get => (Duration)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(Duration), typeof(SideMenu),
        new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(250))));

    #endregion AnimationDuration

    #region Indent

    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

    public static readonly DependencyProperty IndentProperty =
        DependencyProperty.Register(nameof(Indent), typeof(double), typeof(SideMenu),
        new PropertyMetadata(20d, OnIndentChanged));

    private static void OnIndentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.UpdateChildrenIndent();
        }
    }

    #endregion Indent

    #region Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        var item = new SideMenuItem();
        item.Level = 0; // 顶级菜单项Level为0
        item.UpdateIndent(this.Indent);
        return item;
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SideMenuItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is SideMenuItem menuItem)
        {
            // 为顶级菜单项设置Level为0
            menuItem.Level = 0;
            menuItem.UpdateIndent(this.Indent);
        }
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

    // 动画宽度变化的方法
    private void AnimateWidthChange(bool isCompact)
    {
        if (!IsLoaded)
            return;

        Border rootBorder = GetTemplateChild("RootBorder") as Border;
        if (rootBorder == null)
            return;

        double targetWidth = isCompact ? CollapseWidth : ExpansionWidth;

        DoubleAnimation widthAnimation = new DoubleAnimation
        {
            To = targetWidth,
            Duration = AnimationDuration,
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        rootBorder.BeginAnimation(FrameworkElement.WidthProperty, widthAnimation);
    }

    private void UpdateChildrenIndent()
    {
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                menuItem.UpdateIndent(this.Indent);
            }
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }
}