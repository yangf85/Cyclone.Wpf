using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Data;
using System.Collections.Generic;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 时间选择器控件，支持小时、分钟、秒的选择
/// </summary>
[TemplatePart(Name = "PART_ItemsPanel", Type = typeof(CyclePanel))]
[TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
public class TimeSelector : ListBox
{
    #region Private Fields

    private CyclePanel _itemsPanel;
    private bool _isInternalUpdate;
    private int _maxValue; // 存储最大值
    private ScrollViewer _scrollViewer;

    #endregion Private Fields

    #region Events

    public event EventHandler<TimeValueChangedEventArgs> ValueChanged;

    #endregion Events

    #region Construction

    static TimeSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSelector), new FrameworkPropertyMetadata(typeof(TimeSelector)));
    }

    public TimeSelector()
    {
        SelectionChanged += TimeSelector_SelectionChanged;
        Loaded += TimeSelector_Loaded;
    }

    private void TimeSelector_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeItems();

        // 确保在UI完全加载后设置选中值
        Dispatcher.BeginInvoke(new Action(() =>
        {
            UpdateSelection(SelectedTimeValue);
        }), DispatcherPriority.Loaded);
    }

    private void TimeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isInternalUpdate || e.AddedItems.Count == 0)
            return;

        int value = -1;

        if (e.AddedItems[0] is TimeSelectorItem timeItem)
        {
            value = timeItem.Value;
        }
        else if (e.AddedItems[0] is int intValue)
        {
            value = intValue;
        }

        if (value >= 0 && SelectedTimeValue != value)
        {
            _isInternalUpdate = true;
            try
            {
                SelectedTimeValue = value;
                ValueChanged?.Invoke(this, new TimeValueChangedEventArgs(value));
            }
            finally
            {
                _isInternalUpdate = false;
            }
        }
    }

    #endregion Construction

    #region Properties

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

        // 避免在内部更新时重复处理
        if (!selector._isInternalUpdate)
        {
            selector.UpdateSelection((int)e.NewValue);
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
        selector.InitializeItems();
    }

    #endregion SelectorType

    #endregion Properties

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // 查找CyclePanel控件
        _itemsPanel = GetTemplateChild("PART_ItemsPanel") as CyclePanel;
        _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        //_scrollViewer.CanContentScroll = true;
        //_itemsPanel.ScrollOwner = _scrollViewer;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new TimeSelectorItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is TimeSelectorItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is TimeSelectorItem container && item is not TimeSelectorItem)
        {
            if (item is int value)
            {
                // 设置实际值和显示文本
                container.Value = value;
                container.DisplayText = value.ToString("D2");
            }
        }
    }

    #endregion Overrides

    #region Additional Properties for Binding

    /// <summary>
    /// 用于接收CyclePanel的VisibleItemIndices属性变化
    /// </summary>
    public IReadOnlyList<int> VisibleItemIndices
    {
        get { return (IReadOnlyList<int>)GetValue(VisibleItemIndicesProperty); }
        set { SetValue(VisibleItemIndicesProperty, value); }
    }

    public static readonly DependencyProperty VisibleItemIndicesProperty =
        DependencyProperty.Register("VisibleItemIndices", typeof(IReadOnlyList<int>), typeof(TimeSelector),
            new PropertyMetadata(null, OnVisibleItemIndicesChanged));

    private static void OnVisibleItemIndicesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;
        selector.UpdateFromVisibleIndices();
    }

    private void UpdateFromVisibleIndices()
    {
        if (VisibleItemIndices == null || VisibleItemIndices.Count == 0 || _isInternalUpdate)
            return;

        // 获取当前可见项中的中间项索引
        int middleIndex = VisibleItemIndices.Count / 2;

        if (middleIndex < VisibleItemIndices.Count)
        {
            int visibleItemIndex = VisibleItemIndices[middleIndex];
            if (visibleItemIndex >= 0 && visibleItemIndex < Items.Count)
            {
                // 设置选中项
                _isInternalUpdate = true;
                try
                {
                    SelectedIndex = visibleItemIndex;

                    // 获取选中值
                    if (Items[visibleItemIndex] is TimeSelectorItem item)
                    {
                        SelectedTimeValue = item.Value;
                    }
                    else if (Items[visibleItemIndex] is int value)
                    {
                        SelectedTimeValue = value;
                    }
                }
                finally
                {
                    _isInternalUpdate = false;
                }
            }
        }
    }

    #endregion Additional Properties for Binding

    #region Methods

    private void InitializeItems()
    {
        Items.Clear();

        _maxValue = GetMaxValueForType();

        // 添加所有可能的值
        for (int i = 0; i <= _maxValue; i++)
        {
            Items.Add(i);
        }
    }

    private int GetMaxValueForType()
    {
        return SelectorType switch
        {
            TimeSelectorType.Hour => 23,
            TimeSelectorType.Minute => 59,
            TimeSelectorType.Second => 59,
            _ => 59
        };
    }

    // 更新选中项
    private void UpdateSelection(int value)
    {
        if (Items.Count == 0) return;

        // 确保值在有效范围内
        int normalizedValue = Math.Max(0, Math.Min(value, _maxValue));

        _isInternalUpdate = true;
        try
        {
            // 更新选中值
            SelectedTimeValue = normalizedValue;

            // 寻找匹配的项
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] is TimeSelectorItem item)
                {
                    if (item.Value == normalizedValue)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
                else if (Items[i] is int itemValue)
                {
                    if (itemValue == normalizedValue)
                    {
                        SelectedIndex = i;
                        break;
                    }
                }
            }

            // 触发值改变事件
            ValueChanged?.Invoke(this, new TimeValueChangedEventArgs(normalizedValue));

            // 确保选中的项在可视区域中间
            if (_itemsPanel != null && SelectedIndex >= 0)
            {
                ScrollIntoView(Items[SelectedIndex]);
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    // 为TimePicker提供的公共方法
    public void ForceScrollToValueAndSelect(int value)
    {
        // 确保在UI线程中执行
        Dispatcher.BeginInvoke(new Action(() =>
        {
            UpdateSelection(value);
            // 强制更新布局以确保选择器能立即显示正确值
            UpdateLayout();
        }), DispatcherPriority.Render);
    }

    #endregion Methods
}