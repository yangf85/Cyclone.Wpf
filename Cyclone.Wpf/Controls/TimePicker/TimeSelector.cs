using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(ScrollViewer))]
    public class TimeSelector : ItemsControl
    {
        #region Private Fields

        private ScrollViewer _scrollViewer;
        private Point _startDragPoint;
        private double _startVerticalOffset;
        private bool _isDragging;
        private int _itemHeight = 30;
        private const int VISIBLE_ITEMS = 5;
        private int _maxValue; // 存储最大值用于循环计算

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
        }

        private void TimeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateItems();
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
            selector.UpdateSelectedItem();
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
            selector.UpdateItems();
        }

        #endregion SelectorType

        #region SelectedItemBackground

        public Brush SelectedItemBackground
        {
            get { return (Brush)GetValue(SelectedItemBackgroundProperty); }
            set { SetValue(SelectedItemBackgroundProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemBackgroundProperty =
            DependencyProperty.Register("SelectedItemBackground", typeof(Brush), typeof(TimeSelector),
                new PropertyMetadata(null));

        #endregion SelectedItemBackground

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

            // 取消现有事件绑定
            if (_scrollViewer != null)
            {
                _scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
                _scrollViewer.PreviewMouseDown -= ScrollViewer_PreviewMouseDown;
                _scrollViewer.PreviewMouseMove -= ScrollViewer_PreviewMouseMove;
                _scrollViewer.PreviewMouseUp -= ScrollViewer_PreviewMouseUp;
                _scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            }

            // 获取模板部件
            _scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;

            // 绑定事件
            if (_scrollViewer != null)
            {
                _scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                _scrollViewer.PreviewMouseDown += ScrollViewer_PreviewMouseDown;
                _scrollViewer.PreviewMouseMove += ScrollViewer_PreviewMouseMove;
                _scrollViewer.PreviewMouseUp += ScrollViewer_PreviewMouseUp;
                _scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }

            UpdateItems();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is ContentPresenter container && item is TimeItem timeItem)
            {
                container.MouseLeftButtonDown += Container_MouseLeftButtonDown;
                container.Tag = timeItem.Value; // 存储值用于点击处理
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is ContentPresenter container)
            {
                container.MouseLeftButtonDown -= Container_MouseLeftButtonDown;
            }

            base.ClearContainerForItemOverride(element, item);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ContentPresenter();
        }

        #endregion Overrides

        #region Event Handlers

        private void Container_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ContentPresenter container && container.Tag is int value)
            {
                SelectValue(value);
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            double newOffset;

            if (e.Delta > 0) // 向上滚动
            {
                newOffset = _scrollViewer.VerticalOffset - _itemHeight;
            }
            else // 向下滚动
            {
                newOffset = _scrollViewer.VerticalOffset + _itemHeight;
            }

            // 连续循环滚动处理
            if (IsCyclical)
            {
                // 计算显示区域的位置
                int realItemsCount = _maxValue + 1; // 实际项目数量
                double minOffset = 0;
                double maxOffset = (realItemsCount + VISIBLE_ITEMS * 2) * _itemHeight;
                double realAreaStart = VISIBLE_ITEMS * _itemHeight;
                double realAreaEnd = (VISIBLE_ITEMS + realItemsCount) * _itemHeight;

                _scrollViewer.ScrollToVerticalOffset(newOffset);

                // 检查是否达到边界区域
                if (newOffset <= realAreaStart && newOffset >= minOffset)
                {
                    // 计算位于实际区域中的相对位置
                    double relativePos = newOffset - minOffset;
                    double wrappedOffset = realAreaEnd - (realAreaStart - relativePos);
                    if (wrappedOffset < maxOffset)
                    {
                        // 在下一帧更新，避免闪烁
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _scrollViewer.ScrollToVerticalOffset(wrappedOffset);
                        }), System.Windows.Threading.DispatcherPriority.Render);
                    }
                }
                else if (newOffset >= realAreaEnd && newOffset <= maxOffset)
                {
                    // 计算位于实际区域中的相对位置
                    double relativePos = newOffset - realAreaEnd;
                    double wrappedOffset = realAreaStart + relativePos;
                    if (wrappedOffset >= minOffset)
                    {
                        // 在下一帧更新，避免闪烁
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _scrollViewer.ScrollToVerticalOffset(wrappedOffset);
                        }), System.Windows.Threading.DispatcherPriority.Render);
                    }
                }
            }
            else
            {
                _scrollViewer.ScrollToVerticalOffset(newOffset);
            }
        }

        private void ScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _startDragPoint = e.GetPosition(_scrollViewer);
                _startVerticalOffset = _scrollViewer.VerticalOffset;
                _isDragging = true;
                _scrollViewer.CaptureMouse();
                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(_scrollViewer);
                double yDelta = _startDragPoint.Y - currentPoint.Y;
                double newOffset = _startVerticalOffset + yDelta;

                // 连续循环滚动处理
                if (IsCyclical)
                {
                    // 计算实际项目区域的位置
                    int realItemsCount = _maxValue + 1; // 实际项目数量
                    double realAreaStart = VISIBLE_ITEMS * _itemHeight;
                    double realAreaEnd = (VISIBLE_ITEMS + realItemsCount) * _itemHeight;

                    _scrollViewer.ScrollToVerticalOffset(newOffset);

                    // 检查是否拖动到了边界区域
                    if (newOffset < realAreaStart)
                    {
                        // 计算相对位置，使其映射到下半部分
                        double relativePos = realAreaStart - newOffset;
                        double wrappedOffset = realAreaEnd - relativePos;

                        // 更新拖拽起始点，保持平滑滚动
                        _startDragPoint = currentPoint;
                        _startVerticalOffset = wrappedOffset;

                        // 在下一帧更新，避免闪烁
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _scrollViewer.ScrollToVerticalOffset(wrappedOffset);
                        }), System.Windows.Threading.DispatcherPriority.Render);
                    }
                    else if (newOffset > realAreaEnd)
                    {
                        // 计算相对位置，使其映射到上半部分
                        double relativePos = newOffset - realAreaEnd;
                        double wrappedOffset = realAreaStart + relativePos;

                        // 更新拖拽起始点，保持平滑滚动
                        _startDragPoint = currentPoint;
                        _startVerticalOffset = wrappedOffset;

                        // 在下一帧更新，避免闪烁
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            _scrollViewer.ScrollToVerticalOffset(wrappedOffset);
                        }), System.Windows.Threading.DispatcherPriority.Render);
                    }
                }
                else
                {
                    _scrollViewer.ScrollToVerticalOffset(newOffset);
                }

                e.Handled = true;
            }
        }

        private void ScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                _scrollViewer.ReleaseMouseCapture();

                // 滚动到最近的项
                int itemIndex = (int)Math.Round(_scrollViewer.VerticalOffset / _itemHeight);
                double targetOffset = itemIndex * _itemHeight;

                // 检查是否需要调整到实际项区域
                if (IsCyclical)
                {
                    int realItemsCount = _maxValue + 1;
                    double realAreaStart = VISIBLE_ITEMS * _itemHeight;
                    double realAreaEnd = (VISIBLE_ITEMS + realItemsCount) * _itemHeight;

                    // 如果在前面的虚拟区域
                    if (targetOffset < realAreaStart)
                    {
                        // 计算对应的真实位置
                        double relativePos = targetOffset;
                        double equivalentOffset = realAreaEnd - (realAreaStart - relativePos);

                        // 确保在有效范围内
                        if (equivalentOffset < (realItemsCount + VISIBLE_ITEMS * 2) * _itemHeight)
                        {
                            targetOffset = equivalentOffset;
                        }
                    }
                    // 如果在后面的虚拟区域
                    else if (targetOffset >= realAreaEnd)
                    {
                        // 计算对应的真实位置
                        double relativePos = targetOffset - realAreaEnd;
                        double equivalentOffset = realAreaStart + relativePos;

                        // 确保在有效范围内
                        if (equivalentOffset >= 0)
                        {
                            targetOffset = equivalentOffset;
                        }
                    }
                }

                _scrollViewer.ScrollToVerticalOffset(targetOffset);

                // 更新选中值
                UpdateSelectedValueFromOffset(targetOffset);

                e.Handled = true;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 只在手动结束滚动或程序滚动结束时更新
            if (!_isDragging && e.ExtentHeightChange == 0)
            {
                UpdateSelectedValueFromOffset(e.VerticalOffset);
            }
        }

        #endregion Event Handlers

        #region Methods

        private void UpdateSelectedValueFromOffset(double offset)
        {
            int itemIndex = (int)Math.Round(offset / _itemHeight);

            // 有效索引范围检查
            if (itemIndex >= VISIBLE_ITEMS / 2 && itemIndex < Items.Count - VISIBLE_ITEMS / 2)
            {
                if (Items[itemIndex] is TimeItem timeItem && timeItem.Value >= 0)
                {
                    SelectValue(timeItem.Value);
                }
            }
        }

        private void SelectValue(int value)
        {
            if (value < 0) return; // 忽略无效值（空白项）

            // 如果启用循环，确保值在有效范围内
            if (IsCyclical && _maxValue > 0)
            {
                // 循环调整值到有效范围
                while (value < 0)
                {
                    value += (_maxValue + 1);
                }

                value = value % (_maxValue + 1);
            }

            if (SelectedValue != value)
            {
                SelectedValue = value;
                ValueChanged?.Invoke(this, new TimeValueChangedEventArgs(value));
            }
        }

        private void UpdateItems()
        {
            Items.Clear();

            _maxValue = 0;

            switch (SelectorType)
            {
                case TimeSelectorType.Hour:
                    _maxValue = 23; // 只使用24小时制
                    break;

                case TimeSelectorType.Minute:
                case TimeSelectorType.Second:
                    _maxValue = 59;
                    break;
            }

            // 连续循环实现：创建三倍的项目集

            // 添加前面的虚拟项（底部项的副本）
            for (int i = 0; i < VISIBLE_ITEMS; i++)
            {
                // 从尾部开始，依次添加 VISIBLE_ITEMS 个项目
                int valueIndex = _maxValue - VISIBLE_ITEMS + i + 1;
                // 处理越界情况
                if (valueIndex < 0) valueIndex += (_maxValue + 1);

                int value = IsCyclical ? valueIndex : -1;
                string displayText = value >= 0 ? value.ToString("D2") : string.Empty;

                Items.Add(new TimeItem { Value = value, DisplayText = displayText });
            }

            // 添加实际时间项（主要项）
            for (int i = 0; i <= _maxValue; i++)
            {
                var timeItem = new TimeItem
                {
                    Value = i,
                    DisplayText = i.ToString("D2")
                };

                Items.Add(timeItem);
            }

            // 添加后面的虚拟项（顶部项的副本）
            for (int i = 0; i < VISIBLE_ITEMS; i++)
            {
                // 添加头部的值作为后缀
                int value = IsCyclical ? i : -1;
                string displayText = value >= 0 ? value.ToString("D2") : string.Empty;

                Items.Add(new TimeItem { Value = value, DisplayText = displayText });
            }

            UpdateSelectedItem();
        }

        private void UpdateSelectedItem()
        {
            if (_scrollViewer == null || Items.Count == 0)
                return;

            int index = -1;

            // 查找中间区域的匹配项（非虚拟项）
            for (int i = VISIBLE_ITEMS; i < VISIBLE_ITEMS + _maxValue + 1; i++)
            {
                if (Items[i] is TimeItem timeItem && timeItem.Value == SelectedValue)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                _scrollViewer.ScrollToVerticalOffset(index * _itemHeight);
            }
        }

        #endregion Methods
    }

    public class TimeItem
    {
        public int Value { get; set; }
        public string DisplayText { get; set; }
    }
}