using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class TimeSelectorItem : Control
{
    static TimeSelectorItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSelectorItem), new FrameworkPropertyMetadata(typeof(TimeSelectorItem)));
    }

    public TimeSelectorItem()
    {
        // 添加鼠标左键点击事件处理
        MouseLeftButtonDown += TimeSelectorItem_MouseLeftButtonDown;
    }

    private void TimeSelectorItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // 触发点击事件
        OnItemClicked();

        // 标记事件已处理，防止事件继续冒泡
        e.Handled = true;
    }

    #region Value

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(TimeSelectorItem), new PropertyMetadata(default(int)));

    #endregion Value

    #region IsSelected

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(TimeSelectorItem), new PropertyMetadata(default(bool)));

    #endregion IsSelected

    #region 点击事件

    // 定义点击事件
    public static readonly RoutedEvent ItemClickedEvent = EventManager.RegisterRoutedEvent(
        "ItemClicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TimeSelectorItem));

    // 点击事件的CLR事件包装器
    public event RoutedEventHandler ItemClicked
    {
        add { AddHandler(ItemClickedEvent, value); }
        remove { RemoveHandler(ItemClickedEvent, value); }
    }

    // 触发点击事件的方法
    protected virtual void OnItemClicked()
    {
        RoutedEventArgs args = new RoutedEventArgs(ItemClickedEvent, this);
        RaiseEvent(args);
    }

    #endregion 点击事件
}