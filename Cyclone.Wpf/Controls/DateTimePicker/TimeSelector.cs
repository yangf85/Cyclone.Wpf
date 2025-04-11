using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    [TemplatePart(Name = "PART_ItemsPanel", Type = typeof(StackPanel))]
    public class TimeSelector : Control
    {
        #region Private Fields

        private StackPanel _itemsPanel;
        private Point _startDragPoint;
        private double _startVerticalOffset;
        private bool _isDragging;
        private ScrollViewer _scrollViewer;
        private List<TimeItem> _timeItems;
        private int _itemHeight = 30; // 默认项高度
        private const int VISIBLE_ITEMS = 5; // 可见项数

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
            _timeItems = new List<TimeItem>();
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

        #region Is24HourFormat

        public bool Is24HourFormat
        {
            get { return (bool)GetValue(Is24HourFormatProperty); }
            set { SetValue(Is24HourFormatProperty, value); }
        }

        public static readonly DependencyProperty Is24HourFormatProperty =
            DependencyProperty.Register("Is24HourFormat", typeof(bool), typeof(TimeSelector),
                new PropertyMetadata(true, OnIs24HourFormatChanged));

        private static void OnIs24HourFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = (TimeSelector)d;
            if (selector.SelectorType == TimeSelectorType.Hour)
            {
                selector.UpdateItems();
            }
        }

        #endregion Is24HourFormat

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

        #endregion Properties

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _itemsPanel = GetTemplateChild("PART_ItemsPanel") as StackPanel;
            _scrollViewer = FindVisualChild<ScrollViewer>(this);

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

        #endregion Overrides

        #region Event Handlers

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

            _scrollViewer.ScrollToVerticalOffset(newOffset);
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
                _scrollViewer.ScrollToVerticalOffset(newOffset);
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
                _scrollViewer.ScrollToVerticalOffset(targetOffset);

                e.Handled = true;
            }
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // 只在手动结束滚动或程序滚动结束时更新
            if (!_isDragging && e.ExtentHeightChange == 0)
            {
                int itemIndex = (int)Math.Round(e.VerticalOffset / _itemHeight);
                if (itemIndex >= 0 && itemIndex < _timeItems.Count)
                {
                    SelectedValue = _timeItems[itemIndex].Value;
                    ValueChanged?.Invoke(this, new TimeValueChangedEventArgs(SelectedValue));
                }
            }
        }

        #endregion Event Handlers

        #region Methods

        private void UpdateItems()
        {
            if (_itemsPanel == null)
                return;

            _itemsPanel.Children.Clear();
            _timeItems.Clear();

            int maxValue = 0;

            switch (SelectorType)
            {
                case TimeSelectorType.Hour:
                    maxValue = Is24HourFormat ? 23 : 12;
                    break;

                case TimeSelectorType.Minute:
                case TimeSelectorType.Second:
                    maxValue = 59;
                    break;
            }

            // 添加头尾空白项使选中项居中
            for (int i = 0; i < VISIBLE_ITEMS / 2; i++)
            {
                AddBlankItem();
            }

            // 添加实际时间项
            for (int i = 0; i <= maxValue; i++)
            {
                int displayValue = i;
                if (SelectorType == TimeSelectorType.Hour && !Is24HourFormat && i == 0)
                {
                    displayValue = 12; // 12小时制中0点显示为12
                }

                var timeItem = new TimeItem
                {
                    Value = i,
                    DisplayText = displayValue.ToString("D2")
                };

                _timeItems.Add(timeItem);

                var itemControl = new ContentControl
                {
                    Content = timeItem.DisplayText,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Width = 50,
                    Height = _itemHeight
                };

                _itemsPanel.Children.Add(itemControl);
            }

            // 添加头尾空白项使选中项居中
            for (int i = 0; i < VISIBLE_ITEMS / 2; i++)
            {
                AddBlankItem();
            }

            UpdateSelectedItem();
        }

        private void AddBlankItem()
        {
            var blankControl = new ContentControl
            {
                Content = string.Empty,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 50,
                Height = _itemHeight
            };

            _itemsPanel.Children.Add(blankControl);
        }

        private void UpdateSelectedItem()
        {
            if (_scrollViewer == null || _timeItems.Count == 0)
                return;

            int index = -1;
            for (int i = 0; i < _timeItems.Count; i++)
            {
                if (_timeItems[i].Value == SelectedValue)
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

        // 查找视觉树子元素的辅助方法
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        #endregion Methods
    }

    public class TimeItem
    {
        public int Value { get; set; }
        public string DisplayText { get; set; }
    }
}