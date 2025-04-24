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
using System.Diagnostics;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
[TemplatePart(Name = "PART_CyclicPanel", Type = typeof(CyclicPanel))]
public class TimeSelector : Selector
{
    #region 私有字段

    private ScrollViewer _scrollViewer;
    private CyclicPanel _cyclicPanel;
    private DependencyPropertyDescriptor _dpd;

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
        GenerateItems();
        Loaded += TimeSelector_Loaded;
        Unloaded += TimeSelector_Unloaded;
    }

    private void TimeSelector_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_cyclicPanel != null && _dpd != null)
        {
            _dpd.RemoveValueChanged(_cyclicPanel, HandlePanelVisbleIndicesChanged);
        }
        if (Items.Count > VisibleItemCount && _cyclicPanel.VisibleItemIndices.Count > 0)
        {
            SelectedIndex = _cyclicPanel.VisibleItemIndices[VisibleItemCount / 2];
        }
    }

    void UpdateSelectedIndex()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is TimeSelectorItem item)
            {
                item.IsSelected = i == SelectedIndex;
            }
        }
    }

    private void HandlePanelVisbleIndicesChanged(object? sender, EventArgs e)
    {
        SelectedIndex = _cyclicPanel.VisibleItemIndices[(int)Math.Floor(VisibleItemCount / 2d)];
        UpdateSelectedIndex();
    }

    private void TimeSelector_Loaded(object sender, RoutedEventArgs e)
    {
        if (_cyclicPanel == null) { return; }
        SelectedIndex = VisibleItemCount / 2;
        UpdateSelectedIndex();
        _dpd = DependencyPropertyDescriptor.FromProperty(CyclicPanel.VisibleItemIndicesProperty, typeof(CyclicPanel));
        _dpd.AddValueChanged(_cyclicPanel, HandlePanelVisbleIndicesChanged);
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
            new PropertyMetadata(5, OnVisibleItemCountChanged, OnCoerceVisibleItemCount));

    private static object OnCoerceVisibleItemCount(DependencyObject d, object baseValue)
    {
        var value = (int)baseValue;
        var selector = (TimeSelector)d;
        if (value >= selector.Items.Count) { throw new ArgumentException("VisibleItemCount must be <= Items.Count."); }
        if (value <= 0) { throw new ArgumentException("VisibleItemCount must be > 0."); }
        if (value % 2 == 0) { throw new ArgumentException("VisibleItemCount must be an odd number."); }

        return baseValue;
    }

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
        int value = (int)e.NewValue;
        if (selector.Items.Count > 0)
        {
            selector.SelectedIndex = selector.Items.OfType<TimeSelectorItem>().First(i => i.Value == value).Value;
        }
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
        if (type == TimeSelectorType.Hour)
        {
            return Enumerable.Range(0, 24);
        }
        else
        {
            return Enumerable.Range(0, 60);
        }
    }

    #endregion SelectorType

    #endregion 依赖属性

    #region 重写方法

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);
        if (SelectedIndex >= 0)
        {
            SelectedTimeValue = Items.OfType<TimeSelectorItem>().First(i => i.Value == SelectedIndex).Value;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 获取ScrollViewer
        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        _cyclicPanel = GetTemplateChild("PART_CyclicPanel") as CyclicPanel;
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