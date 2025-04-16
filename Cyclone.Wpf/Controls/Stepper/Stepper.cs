using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

public enum StepChangeDirection
{
    Forward,
    Backward
}

public class StepChangedEventArgs : RoutedEventArgs
{
    public int Current { get; set; }
    public StepChangeDirection Direction { get; set; }

    public StepChangedEventArgs(RoutedEvent routedEvent, object source) : base(routedEvent, source)
    {
    }
}

public class Stepper : ItemsControl
{
    static Stepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Stepper),
            new FrameworkPropertyMetadata(typeof(Stepper)));
    }

    #region CurrentIndex

    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    public static readonly DependencyProperty CurrentIndexProperty =
        DependencyProperty.Register(nameof(CurrentIndex), typeof(int), typeof(Stepper),
            new PropertyMetadata(default(int), OnCurrentIndexChanged));

    private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Stepper stepper)
        {
            int oldIndex = (int)e.OldValue;
            int newIndex = (int)e.NewValue;

            if (oldIndex == newIndex) return;

            // 更新所有StepperItem的状态
            stepper.UpdateStepperItemsStatus();

            // 触发StepChanged事件
            var direction = newIndex > oldIndex ? StepChangeDirection.Forward : StepChangeDirection.Backward;
            var args = new StepChangedEventArgs(StepChangedEvent, stepper)
            {
                Current = newIndex,
                Direction = direction
            };

            stepper.RaiseEvent(args);
        }
    }

    private void UpdateStepperItemsStatus()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StepperItem item)
            {
                // 设置索引
                item.SetIndex(i);

                // 更新步骤项的状态、是否第一项或最后一项
                item.UpdateStatus(CurrentIndex);
                item.SetIsFirst(i == 0);
                item.SetIsLast(i == Items.Count - 1);
            }
        }
    }

    #endregion CurrentIndex

    #region Orientation

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Stepper),
            new PropertyMetadata(Orientation.Horizontal));

    #endregion Orientation

    #region StepChangedEvent

    public static readonly RoutedEvent StepChangedEvent = EventManager.RegisterRoutedEvent(
        "StepChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Stepper));

    public event RoutedEventHandler StepChanged
    {
        add { AddHandler(StepChangedEvent, value); }
        remove { RemoveHandler(StepChangedEvent, value); }
    }

    #endregion StepChangedEvent

    #region 公共方法

    /// <summary>
    /// 前进到下一步
    /// </summary>
    /// <returns>如果成功前进则返回true，否则返回false</returns>
    public bool MoveNext()
    {
        if (CurrentIndex < Items.Count - 1)
        {
            CurrentIndex++;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 后退到上一步
    /// </summary>
    /// <returns>如果成功后退则返回true，否则返回false</returns>
    public bool MovePrevious()
    {
        if (CurrentIndex > 0)
        {
            CurrentIndex--;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 跳转到指定步骤
    /// </summary>
    /// <param name="index">步骤索引</param>
    /// <returns>如果跳转成功则返回true，否则返回false</returns>
    public bool JumpTo(int index)
    {
        if (index >= 0 && index < Items.Count && index != CurrentIndex)
        {
            CurrentIndex = index;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 重置到第一步
    /// </summary>
    public void Reset()
    {
        CurrentIndex = 0;
    }

    #endregion 公共方法

    public Stepper()
    {
        // 注册事件，以便在加载后初始化所有StepperItem
        Loaded += Stepper_Loaded;
    }

    private void Stepper_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateStepperItemsStatus();
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new StepperItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is StepperItem;
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateStepperItemsStatus();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StepperItem stepperItem)
        {
            // 获取项的索引
            int index = ItemContainerGenerator.IndexFromContainer(element);

            // 设置索引
            stepperItem.SetIndex(index);

            // 更新状态和位置
            stepperItem.UpdateStatus(CurrentIndex);
            stepperItem.SetIsFirst(index == 0);
            stepperItem.SetIsLast(index == Items.Count - 1);
        }
    }
}