using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 表示MultiComboBox控件中的可选项目
/// </summary>
public class MultiComboBoxItem : ContentControl
{
    #region 依赖属性

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(
            nameof(IsSelected),
            typeof(bool),
            typeof(MultiComboBoxItem),
            new PropertyMetadata(false, OnIsSelectedChanged));

    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register(
            nameof(IsHighlighted),
            typeof(bool),
            typeof(MultiComboBoxItem),
            new PropertyMetadata(false));

    #endregion 依赖属性

    #region 路由事件

    public static readonly RoutedEvent SelectedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Selected),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(MultiComboBoxItem));

    public static readonly RoutedEvent UnselectedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(Unselected),
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(MultiComboBoxItem));

    #endregion 路由事件

    #region 属性

    /// <summary>
    /// 获取或设置项目是否被选中
    /// </summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>
    /// 获取或设置项目是否被高亮显示（例如鼠标悬停或键盘导航）
    /// </summary>
    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    #endregion 属性

    #region 事件

    /// <summary>
    /// 当项目被选中时发生
    /// </summary>
    public event RoutedEventHandler Selected
    {
        add => AddHandler(SelectedEvent, value);
        remove => RemoveHandler(SelectedEvent, value);
    }

    /// <summary>
    /// 当项目被取消选中时发生
    /// </summary>
    public event RoutedEventHandler Unselected
    {
        add => AddHandler(UnselectedEvent, value);
        remove => RemoveHandler(UnselectedEvent, value);
    }

    #endregion 事件

    static MultiComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBoxItem), new FrameworkPropertyMetadata(typeof(MultiComboBoxItem)));
    }

    public MultiComboBoxItem()
    {
        // 注册鼠标事件来支持鼠标操作
        this.MouseEnter += (s, e) => IsHighlighted = true;
        this.MouseLeave += (s, e) => IsHighlighted = false;

        // 注册点击事件 - 使用MouseLeftButtonDown而不是MouseLeftButtonUp
        this.MouseLeftButtonDown += OnMouseLeftButtonDown;
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 切换选中状态
        IsSelected = !IsSelected;

        // 标记事件已处理，防止冒泡
        e.Handled = true;
    }

    #region 事件处理

    private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = (MultiComboBoxItem)d;
        bool isSelected = (bool)e.NewValue;

        // 触发相应的路由事件
        if (isSelected)
        {
            item.RaiseEvent(new RoutedEventArgs(SelectedEvent, item));
        }
        else
        {
            item.RaiseEvent(new RoutedEventArgs(UnselectedEvent, item));
        }
    }

    #endregion 事件处理
}