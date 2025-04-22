using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
public class TimeSelector : Selector
{
    #region 私有字段

    private ScrollViewer _scrollViewer;

    #endregion 私有字段

    #region 事件

    public event EventHandler<TimeValueChangedEventArgs> ValueChanged;

    #endregion 事件

    #region 构造函数

    static TimeSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSelector), new FrameworkPropertyMetadata(typeof(TimeSelector)));
    }

    public TimeSelector()
    {
        // 设置默认值
        VisibleItemCount = 5;
        GenerateItems();
    }

    #endregion 构造函数

    #region 依赖属性

    #region VisibleItemCount

    public int VisibleItemCount
    {
        get => (int)GetValue(VisibleItemCountProperty);
        set => SetValue(VisibleItemCountProperty, value);
    }

    public static readonly DependencyProperty VisibleItemCountProperty =
        DependencyProperty.Register(nameof(VisibleItemCount), typeof(int), typeof(TimeSelector),
            new PropertyMetadata(5, OnVisibleItemCountChanged));

    private static void OnVisibleItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;
        int value = (int)e.NewValue;

        selector.Dispatcher.BeginInvoke(new Action(() =>
        {
            selector.UpdateItemHeight();
        }), DispatcherPriority.Loaded);
    }

    void UpdateItemHeight()
    {
        var height = ActualHeight / VisibleItemCount;
        foreach (var obj in Items)
        {
            if (obj is TimeSelectorItem item)
            {
                item.Height = height;
            }
        }
    }

    #endregion VisibleItemCount

    #region SelectedTimeValue

    /// <summary>
    /// 获取或设置所选时间值
    /// </summary>
    public int SelectedTimeValue
    {
        get { return (int)GetValue(SelectedTimeValueProperty); }
        set { SetValue(SelectedTimeValueProperty, value); }
    }

    public static readonly DependencyProperty SelectedTimeValueProperty =
        DependencyProperty.Register("SelectedTimeValue", typeof(int), typeof(TimeSelector),
            new PropertyMetadata(0, OnSelectedTimeValueChanged));

    private static void OnSelectedTimeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;
        int newValue = (int)e.NewValue;
    }

    #endregion SelectedTimeValue

    #region SelectorType

    public TimeSelectorType SelectorType
    {
        get { return (TimeSelectorType)GetValue(SelectorTypeProperty); }
        set { SetValue(SelectorTypeProperty, value); }
    }

    public static readonly DependencyProperty SelectorTypeProperty =
        DependencyProperty.Register("SelectorType", typeof(TimeSelectorType), typeof(TimeSelector),
            new PropertyMetadata(TimeSelectorType.Hour, OnSelectorTypeChanged));

    private static void OnSelectorTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;
        selector.GenerateItems();
    }

    void GenerateItems()
    {
        Items.Clear();
        var values = GetValues(SelectorType);
        foreach (var value in values)
        {
            var item = new TimeSelectorItem
            {
                Value = value,
            };
            Items.Add(item);
        }
    }

    IEnumerable<int> GetValues(TimeSelectorType type)
    {
        var times = 10;
        if (type == TimeSelectorType.Hour)
        {
            return Enumerable.Repeat(Enumerable.Range(0, 24), times).SelectMany(x => x);
        }
        else if (type == TimeSelectorType.Minute)
        {
            return Enumerable.Repeat(Enumerable.Range(0, 60), times).SelectMany(x => x);
        }
        else
        {
            return Enumerable.Repeat(Enumerable.Range(0, 60), times).SelectMany(x => x);
        }
    }

    #endregion SelectorType

    #endregion 依赖属性

    #region 重写方法

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 获取ScrollViewer
        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

        if (_scrollViewer != null)
        {
            // 禁用自动滚动到选中项
            ScrollViewer.SetCanContentScroll(this, true);

            // 监听滚动事件
            //_scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TimeSelectorItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TimeSelectorItem;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateItemHeight();
    }

    #endregion 重写方法
}