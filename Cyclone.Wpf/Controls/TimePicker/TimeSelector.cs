using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_ItemsContainer", Type = typeof(UniformGrid))]
public class TimeSelector : ListBox
{
    #region Private Fields

    private UniformGrid _container;
    private bool _isInternalUpdate;
    private bool _isDragging;
    private Point _lastDragPoint;
    private double _dragAccumulator;
    private const double DRAG_THRESHOLD = 15.0; // 拖动阈值，超过这个值才会触发值变化
    private int _maxValue; // 存储最大值
    private const int VISIBLE_ITEM_COUNT = 5; // 固定显示的项数（确保是奇数）

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
        Loaded += TimeSelector_Loaded;
        SelectionChanged += TimeSelector_SelectionChanged;
        PreviewMouseWheel += TimeSelector_PreviewMouseWheel;
        PreviewMouseDown += TimeSelector_PreviewMouseDown;
        PreviewMouseMove += TimeSelector_PreviewMouseMove;
        PreviewMouseUp += TimeSelector_PreviewMouseUp;
    }

    private void TimeSelector_Loaded(object sender, RoutedEventArgs e)
    {
        InitializeItems();

        // 确保在UI完全加载后设置选中值
        Dispatcher.BeginInvoke(new Action(() =>
        {
            UpdateSelection(SelectedValue);
        }), DispatcherPriority.Loaded);
    }

    private void TimeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && !_isInternalUpdate)
        {
            int value = -1;

            if (e.AddedItems[0] is TimeSelectorItem timeItem)
            {
                value = timeItem.Value;
            }
            else if (e.AddedItems[0] is int intValue)
            {
                value = intValue;
            }

            if (value >= 0 && SelectedValue != value)
            {
                _isInternalUpdate = true;
                try
                {
                    SelectedValue = value;
                    ValueChanged?.Invoke(this, new TimeValueChangedEventArgs(value));
                }
                finally
                {
                    _isInternalUpdate = false;
                }
            }
        }
    }

    #endregion Construction

    #region Properties

    #region SelectedValue

    public int SelectedValue
    {
        get { return (int)GetValue(SelectedValueProperty); }
        set { SetValue(SelectedValueProperty, value); }
    }

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register("SelectedValue", typeof(int), typeof(TimeSelector),
            new PropertyMetadata(0, OnSelectedValueChanged));

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (TimeSelector)d;

        // 避免在内部更新时重复处理
        if (!selector._isInternalUpdate)
        {
            selector.UpdateSelection((int)e.NewValue);
        }
    }

    #endregion SelectedValue

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

    #region IsCyclical

    public bool IsCyclical
    {
        get { return (bool)GetValue(IsCyclicalProperty); }
        set { SetValue(IsCyclicalProperty, value); }
    }

    public static readonly DependencyProperty IsCyclicalProperty =
        DependencyProperty.Register("IsCyclical", typeof(bool), typeof(TimeSelector),
            new PropertyMetadata(true));

    #endregion IsCyclical

    #endregion Properties

    #region Overrides

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _container = GetTemplateChild("PART_Container") as UniformGrid;
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
                int actualValue = GetNormalizedValue(value);
                container.Value = actualValue;
                container.DisplayText = actualValue.ToString("D2");
            }
        }
    }

    #endregion Overrides

    #region Event Handlers

    private void TimeSelector_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;

        // 向上滚动为负值，向下滚动为正值
        int delta = e.Delta > 0 ? -1 : 1;

        // 改变选中值
        ChangeValue(delta);
    }

    private void TimeSelector_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _lastDragPoint = e.GetPosition(this);
            _isDragging = true;
            _dragAccumulator = 0;
            CaptureMouse();
            e.Handled = true;
        }
    }

    private void TimeSelector_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
        {
            Point currentPoint = e.GetPosition(this);
            double deltaY = currentPoint.Y - _lastDragPoint.Y;
            _lastDragPoint = currentPoint;

            // 累积拖动距离
            _dragAccumulator += deltaY;

            // 如果累积的拖动距离超过阈值，改变选中值
            if (Math.Abs(_dragAccumulator) >= DRAG_THRESHOLD)
            {
                int change = _dragAccumulator > 0 ? 1 : -1; // 向下拖动为正，向上拖动为负
                ChangeValue(change);
                _dragAccumulator = 0; // 重置累积值
            }

            e.Handled = true;
        }
    }

    private void TimeSelector_PreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    #endregion Event Handlers

    #region Methods

    private void InitializeItems()
    {
        Items.Clear();

        _maxValue = GetMaxValueForType();

        // 使用固定的显示项数量
        int halfCount = VISIBLE_ITEM_COUNT / 2;

        // 添加所有项
        for (int i = -halfCount; i <= halfCount; i++)
        {
            // 计算实际值（考虑循环）
            int value = GetValueWithOffset(SelectedValue, i);
            Items.Add(value);
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

    // 获取规范化的值（确保在有效范围内）
    private int GetNormalizedValue(int value)
    {
        int totalValues = _maxValue + 1;
        return ((value % totalValues) + totalValues) % totalValues;
    }

    // 获取指定偏移量的值
    private int GetValueWithOffset(int baseValue, int offset)
    {
        if (!IsCyclical)
        {
            // 非循环模式，限制在有效范围内
            int result = baseValue + offset;
            return Math.Max(0, Math.Min(result, _maxValue));
        }
        else
        {
            // 循环模式，循环计算
            return GetNormalizedValue(baseValue + offset);
        }
    }

    // 改变当前值（增加或减少）
    private void ChangeValue(int delta)
    {
        if (!IsCyclical)
        {
            // 非循环模式，检查边界
            int newValue = SelectedValue + delta;
            if (newValue < 0 || newValue > _maxValue)
                return;

            UpdateSelection(newValue);
        }
        else
        {
            // 循环模式，循环变化
            int newValue = GetNormalizedValue(SelectedValue + delta);
            UpdateSelection(newValue);
        }
    }

    // 更新选中项
    private void UpdateSelection(int value)
    {
        if (Items.Count == 0) return;

        // 调整值确保在有效范围内
        int normalizedValue = GetNormalizedValue(value);

        _isInternalUpdate = true;
        try
        {
            // 更新选中值
            SelectedValue = normalizedValue;

            // 使用固定的可见项数量
            int halfCount = VISIBLE_ITEM_COUNT / 2;

            // 更新所有项的值
            Items.Clear();
            for (int i = -halfCount; i <= halfCount; i++)
            {
                int itemValue = GetValueWithOffset(normalizedValue, i);
                Items.Add(itemValue);
            }

            // 设置中间项为选中项
            SelectedIndex = halfCount;

            // 触发值改变事件
            ValueChanged?.Invoke(this, new TimeValueChangedEventArgs(normalizedValue));
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
        }), DispatcherPriority.Render);
    }

    #endregion Methods
}