using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

public enum StepStatus
{
    Completed,
    Current,
    Pending
}

public class StepperItem : ContentControl
{
    static StepperItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepperItem),
            new FrameworkPropertyMetadata(typeof(StepperItem)));
    }

    #region Description

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(StepperItem),
            new PropertyMetadata(string.Empty));

    #endregion Description

    #region Status

    public StepStatus Status
    {
        get => (StepStatus)GetValue(StatusProperty);
        private set => SetValue(StatusPropertyKey, value);
    }

    private static readonly DependencyPropertyKey StatusPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Status), typeof(StepStatus), typeof(StepperItem),
            new PropertyMetadata(StepStatus.Pending, OnStatusChanged));

    public static readonly DependencyProperty StatusProperty = StatusPropertyKey.DependencyProperty;

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StepperItem item)
        {
            item.OnStatusChanged((StepStatus)e.OldValue, (StepStatus)e.NewValue);
        }
    }

    protected virtual void OnStatusChanged(StepStatus oldStatus, StepStatus newStatus)
    {
        // 状态变化时可以执行自定义逻辑
        // 这个方法可以被子类重写以添加自定义行为

        // 触发StatusChanged事件
        StatusChanged?.Invoke(this, new RoutedEventArgs());
    }

    /// <summary>
    /// 状态变化事件
    /// </summary>
    public event RoutedEventHandler StatusChanged;

    /// <summary>
    /// 内部方法，根据当前步骤索引更新状态
    /// </summary>
    internal void UpdateStatus(int currentIndex)
    {
        if (Index < currentIndex)
            SetValue(StatusPropertyKey, StepStatus.Completed);
        else if (Index == currentIndex)
            SetValue(StatusPropertyKey, StepStatus.Current);
        else
            SetValue(StatusPropertyKey, StepStatus.Pending);
    }

    #endregion Status

    #region IsLast

    public bool IsLast
    {
        get => (bool)GetValue(IsLastProperty);
        private set => SetValue(IsLastPropertyKey, value);
    }

    internal static readonly DependencyPropertyKey IsLastPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLast), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(default(bool), OnIsLastChanged));

    public static readonly DependencyProperty IsLastProperty = IsLastPropertyKey.DependencyProperty;

    private static void OnIsLastChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // 不需要特殊处理，连接器可见性由样式控制
    }

    /// <summary>
    /// 内部方法，设置是否是最后一项
    /// </summary>
    internal void SetIsLast(bool value)
    {
        SetValue(IsLastPropertyKey, value);
    }

    #endregion IsLast

    #region IsFirst

    public bool IsFirst
    {
        get => (bool)GetValue(IsFirstProperty);
        private set => SetValue(IsFirstPropertyKey, value);
    }

    internal static readonly DependencyPropertyKey IsFirstPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsFirst), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsFirstProperty = IsFirstPropertyKey.DependencyProperty;

    /// <summary>
    /// 内部方法，设置是否是第一项
    /// </summary>
    internal void SetIsFirst(bool value)
    {
        SetValue(IsFirstPropertyKey, value);
    }

    #endregion IsFirst

    #region Index

    /// <summary>
    /// 获取该步骤在Stepper中的索引
    /// </summary>
    public int Index
    {
        get => (int)GetValue(IndexProperty);
        private set => SetValue(IndexPropertyKey, value);
    }

    internal static readonly DependencyPropertyKey IndexPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Index), typeof(int), typeof(StepperItem),
            new PropertyMetadata(-1));

    public static readonly DependencyProperty IndexProperty = IndexPropertyKey.DependencyProperty;

    /// <summary>
    /// 内部方法，设置项目索引
    /// </summary>
    internal void SetIndex(int value)
    {
        SetValue(IndexPropertyKey, value);
    }

    #endregion Index

    #region CanNavigate

    /// <summary>
    /// 指示是否可以导航到此步骤
    /// </summary>
    public bool CanNavigate
    {
        get => (bool)GetValue(CanNavigateProperty);
        set => SetValue(CanNavigateProperty, value);
    }

    public static readonly DependencyProperty CanNavigateProperty =
        DependencyProperty.Register(nameof(CanNavigate), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(true));

    #endregion CanNavigate

    public StepperItem()
    {
        // 无需特殊处理连接器可见性，由样式控制
    }

    /// <summary>
    /// 导航到此步骤
    /// </summary>
    /// <returns>导航是否成功</returns>
    public bool NavigateTo()
    {
        if (!CanNavigate) return false;

        var parent = ItemsControl.ItemsControlFromItemContainer(this) as Stepper;
        if (parent != null && Index >= 0)
        {
            parent.CurrentIndex = Index;
            return true;
        }
        return false;
    }
}