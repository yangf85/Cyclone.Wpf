using System;
using System.Collections.Generic;
using System.ComponentModel;
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

public class SideMenu : ItemsControl
{
    static SideMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenu), new FrameworkPropertyMetadata(typeof(SideMenu)));
    }

    // 存储当前实际宽度值，用于动画
    private double _currentWidth;

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
            // 当IsCompact属性改变时，启动动画
            menu.AnimateWidthChange((bool)e.NewValue);
        }
    }

    // 执行宽度变化动画
    private void AnimateWidthChange(bool isCompact)
    {
        // 获取目标宽度
        double targetWidth = isCompact ? CollapseWidth : ExpansionWidth;

        // 如果当前宽度为0（初始化状态），直接设置宽度而不使用动画
        if (_currentWidth == 0)
        {
            _currentWidth = targetWidth;
            this.Width = targetWidth;
            return;
        }

        // 创建动画
        DoubleAnimation animation = new DoubleAnimation
        {
            From = _currentWidth,
            To = targetWidth,
            Duration = AnimationDuration,
            FillBehavior = FillBehavior.HoldEnd,
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };

        // 更新当前宽度
        _currentWidth = targetWidth;

        // 开始动画
        this.BeginAnimation(FrameworkElement.WidthProperty, animation);
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
        DependencyProperty.Register(nameof(ExpansionWidth), typeof(double), typeof(SideMenu), new PropertyMetadata(150d));

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
        new PropertyMetadata(10d, OnIndentChanged));

    private static void OnIndentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.UpdateChildrenIndent();
        }
    }

    #endregion Indent

    #region DisplayMemberIcon

    public string DisplayMemberIcon
    {
        get => (string)GetValue(DisplayMemberIconProperty);
        set => SetValue(DisplayMemberIconProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberIconProperty =
        DependencyProperty.Register(nameof(DisplayMemberIcon), typeof(string), typeof(SideMenu),
        new PropertyMetadata(null, OnDisplayMemberIconChanged));

    private static void OnDisplayMemberIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.UpdateChildrenIconBinding();
        }
    }

    #endregion DisplayMemberIcon

    #region DisplayMemberIconTemplate

    public DataTemplate DisplayMemberIconTemplate
    {
        get => (DataTemplate)GetValue(DisplayMemberIconTemplateProperty);
        set => SetValue(DisplayMemberIconTemplateProperty, value);
    }

    public static readonly DependencyProperty DisplayMemberIconTemplateProperty =
        DependencyProperty.Register(nameof(DisplayMemberIconTemplate), typeof(DataTemplate), typeof(SideMenu),
        new PropertyMetadata(null, OnDisplayMemberIconTemplateChanged));

    private static void OnDisplayMemberIconTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            menu.UpdateChildrenIconTemplate();
        }
    }

    #endregion DisplayMemberIconTemplate

    #region ItemClick Command

    public ICommand ItemClickCommand
    {
        get => (ICommand)GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }

    public static readonly DependencyProperty ItemClickCommandProperty =
        DependencyProperty.Register(nameof(ItemClickCommand), typeof(ICommand), typeof(SideMenu), new PropertyMetadata(null));

    public object ItemClickCommandParameter
    {
        get => GetValue(ItemClickCommandParameterProperty);
        set => SetValue(ItemClickCommandParameterProperty, value);
    }

    public static readonly DependencyProperty ItemClickCommandParameterProperty =
        DependencyProperty.Register(nameof(ItemClickCommandParameter), typeof(object), typeof(SideMenu), new PropertyMetadata(null));

    #endregion ItemClick Command

    #region ItemClick Event

    public static readonly RoutedEvent ItemClickEvent =
        EventManager.RegisterRoutedEvent(nameof(ItemClick), RoutingStrategy.Bubble, typeof(ItemClickEventHandler), typeof(SideMenu));

    public event ItemClickEventHandler ItemClick
    {
        add { AddHandler(ItemClickEvent, value); }
        remove { RemoveHandler(ItemClickEvent, value); }
    }

    #endregion ItemClick Event

    #region Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        var item = new SideMenuItem();
        item.UpdateIndent(Indent);
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

            // 设置图标绑定和模板
            SetupIconBinding(menuItem, item);
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

    private void UpdateChildrenIconBinding()
    {
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                SetupIconBinding(menuItem, item);
            }
        }
    }

    private void UpdateChildrenIconTemplate()
    {
        foreach (var item in Items)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                // 更新当前菜单项的IconTemplate
                if (DisplayMemberIconTemplate != null)
                {
                    menuItem.IconTemplate = DisplayMemberIconTemplate;
                }

                // 更新子菜单项的IconTemplate
                UpdateChildIconTemplateRecursively(menuItem);
            }
        }
    }

    private void UpdateChildIconTemplateRecursively(ItemsControl itemsControl)
    {
        foreach (var item in itemsControl.Items)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(item) is SideMenuItem menuItem)
            {
                // 更新当前子菜单项的IconTemplate
                if (DisplayMemberIconTemplate != null)
                {
                    menuItem.IconTemplate = DisplayMemberIconTemplate;
                }

                // 如果有嵌套的子项，继续递归
                if (menuItem.HasItems)
                {
                    UpdateChildIconTemplateRecursively(menuItem);
                }
            }
        }
    }

    private void SetupIconBinding(SideMenuItem menuItem, object dataItem)
    {
        // 如果设置了DisplayMemberIcon，则创建绑定
        if (!string.IsNullOrEmpty(DisplayMemberIcon))
        {
            // 创建绑定到指定属性的Binding
            Binding iconBinding = new Binding(DisplayMemberIcon)
            {
                Source = dataItem,
                Mode = BindingMode.OneWay
            };

            // 将绑定应用到SideMenuItem的Icon属性
            menuItem.SetBinding(SideMenuItem.IconProperty, iconBinding);
        }

        // 如果设置了DisplayMemberIconTemplate，应用到menuItem
        if (DisplayMemberIconTemplate != null)
        {
            menuItem.IconTemplate = DisplayMemberIconTemplate;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 初始化宽度
        _currentWidth = IsCompact ? CollapseWidth : ExpansionWidth;
        this.Width = _currentWidth;
    }

    /// <summary>
    /// 内部方法，供SideMenuItem调用以触发点击命令
    /// </summary>
    /// <param name="menuItem">被点击的菜单项</param>
    internal void OnItemClicked(SideMenuItem menuItem)
    {
        // 触发路由事件
        ItemClickEventArgs args = new ItemClickEventArgs(menuItem);
        args.RoutedEvent = ItemClickEvent;
        RaiseEvent(args);

        // 执行命令
        if (ItemClickCommand != null && ItemClickCommand.CanExecute(null))
        {
            // 优先使用显式设置的命令参数
            object parameter = ItemClickCommandParameter;

            // 如果没有设置命令参数，则根据情况决定
            if (parameter == null)
            {
                // 如果菜单项有数据上下文，使用数据上下文作为参数
                if (menuItem.DataContext != null && menuItem.DataContext != this.DataContext)
                {
                    parameter = menuItem.DataContext;
                }
                // 否则使用菜单项本身作为参数
                else
                {
                    parameter = menuItem;
                }
            }

            // 执行命令，传递参数
            ItemClickCommand.Execute(parameter);
        }
    }
}