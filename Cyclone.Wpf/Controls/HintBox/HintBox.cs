using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 继承自 ComboBox 的智能提示框控件
    /// </summary>
    [TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    public class HintBox : ComboBox
    {
        private TextBox _editableTextBox;
        private string _currentFilter = string.Empty;

        static HintBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HintBox),
                new FrameworkPropertyMetadata(typeof(HintBox)));

            // 重写这些属性以确保正确的默认行为
            IsEditableProperty.OverrideMetadata(typeof(HintBox),
                new FrameworkPropertyMetadata(true));

            IsTextSearchEnabledProperty.OverrideMetadata(typeof(HintBox),
                new FrameworkPropertyMetadata(false));

            StaysOpenOnEditProperty.OverrideMetadata(typeof(HintBox),
                new FrameworkPropertyMetadata(true));
        }

        public HintBox()
        {
            // 确保默认值
            IsEditable = true;
            IsTextSearchEnabled = false;
            StaysOpenOnEdit = true;
        }

        #region SearchMemberPath

        public static readonly DependencyProperty SearchMemberPathProperty =
            DependencyProperty.Register(nameof(SearchMemberPath), typeof(string), typeof(HintBox),
                new PropertyMetadata(null, OnSearchMemberPathChanged));

        /// <summary>
        /// 获取或设置用于搜索的属性路径。如果不设置，将使用 DisplayMemberPath
        /// </summary>
        public string SearchMemberPath
        {
            get => (string)GetValue(SearchMemberPathProperty);
            set => SetValue(SearchMemberPathProperty, value);
        }

        private static void OnSearchMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var hintBox = (HintBox)d;
            hintBox.RefreshFilter();
        }

        #endregion SearchMemberPath

        #region StringComparison

        public static readonly DependencyProperty StringComparisonProperty =
            DependencyProperty.Register(nameof(StringComparison), typeof(StringComparison), typeof(HintBox),
                new PropertyMetadata(StringComparison.OrdinalIgnoreCase, OnStringComparisonChanged));

        /// <summary>
        /// 获取或设置字符串比较方式
        /// </summary>
        public StringComparison StringComparison
        {
            get => (StringComparison)GetValue(StringComparisonProperty);
            set => SetValue(StringComparisonProperty, value);
        }

        private static void OnStringComparisonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var hintBox = (HintBox)d;
            hintBox.RefreshFilter();
        }

        #endregion StringComparison

        #region MaxDropDownHeight Override

        // ComboBox 已经有 MaxDropDownHeight 属性，我们可以直接使用
        // 不需要再定义 MaxContainerHeight

        #endregion MaxDropDownHeight Override

        #region Override Methods

        public override void OnApplyTemplate()
        {
            // 先解除旧的事件处理
            if (_editableTextBox != null)
            {
                _editableTextBox.TextChanged -= OnEditableTextBoxTextChanged;
            }

            base.OnApplyTemplate();

            // 获取模板部件
            _editableTextBox = GetTemplateChild("PART_EditableTextBox") as TextBox;

            if (_editableTextBox != null)
            {
                _editableTextBox.TextChanged += OnEditableTextBoxTextChanged;
            }

            // 处理清除按钮
            var clearButton = GetTemplateChild("PART_ClearButton") as Button;
            if (clearButton != null)
            {
                clearButton.Click += OnClearButtonClick;
            }
        }

        private void OnEditableTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            _currentFilter = _editableTextBox?.Text ?? string.Empty;

            // 应用过滤
            RefreshFilter();

            // 打开下拉框
            if (!IsDropDownOpen && Items.Count > 0)
            {
                IsDropDownOpen = true;
            }

            // 触发自定义的 TextChanged 事件
            RaiseEvent(new RoutedEventArgs(TextChangedEvent, this));
        }

        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            if (_editableTextBox != null)
            {
                _editableTextBox.Clear();
                _editableTextBox.Focus();
            }

            SelectedItem = null;
            _currentFilter = string.Empty;
            RefreshFilter();
        }

        protected override void OnDropDownOpened(EventArgs e)
        {
            base.OnDropDownOpened(e);

            // 确保有焦点
            _editableTextBox?.Focus();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            // 选择项改变时，更新文本框内容
            if (SelectedItem != null && _editableTextBox != null)
            {
                var displayText = GetItemText(SelectedItem);
                if (!string.IsNullOrEmpty(displayText))
                {
                    _editableTextBox.Text = displayText;
                }
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    // 如果有高亮项，选择它；否则选择第一项
                    if (IsDropDownOpen)
                    {
                        var highlightedItem = GetHighlightedItem();
                        if (highlightedItem != null)
                        {
                            SelectedItem = highlightedItem;
                        }
                        else if (Items.Count > 0)
                        {
                            SelectedItem = Items[0];
                        }

                        IsDropDownOpen = false;
                        e.Handled = true;
                    }
                    break;

                case Key.Escape:
                    if (IsDropDownOpen)
                    {
                        IsDropDownOpen = false;
                        e.Handled = true;
                    }
                    break;

                case Key.Down:
                case Key.Up:
                    // ComboBox 已经处理了上下键导航
                    if (!IsDropDownOpen && Items.Count > 0)
                    {
                        IsDropDownOpen = true;
                    }
                    break;
            }

            base.OnPreviewKeyDown(e);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new HintBoxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is HintBoxItem;
        }

        #endregion Override Methods

        #region Private Methods

        private void RefreshFilter()
        {
            if (Items.CanFilter)
            {
                Items.Filter = FilterPredicate;
            }
        }

        private bool FilterPredicate(object item)
        {
            if (string.IsNullOrEmpty(_currentFilter) || item == null)
            {
                return true;
            }

            var searchText = GetSearchText(item);
            return searchText.IndexOf(_currentFilter, StringComparison) >= 0;
        }

        private string GetItemText(object item)
        {
            if (item == null)
                return string.Empty;

            // 如果设置了 DisplayMemberPath，使用它
            if (!string.IsNullOrEmpty(DisplayMemberPath))
            {
                return GetPropertyValue(item, DisplayMemberPath)?.ToString() ?? string.Empty;
            }

            // 否则使用 ToString()
            return item.ToString();
        }

        private string GetSearchText(object item)
        {
            if (item == null)
                return string.Empty;

            // 如果设置了 SearchMemberPath，使用它
            if (!string.IsNullOrEmpty(SearchMemberPath))
            {
                return GetPropertyValue(item, SearchMemberPath)?.ToString() ?? string.Empty;
            }

            // 否则使用显示文本
            return GetItemText(item);
        }

        private object GetPropertyValue(object obj, string propertyPath)
        {
            if (obj == null || string.IsNullOrEmpty(propertyPath))
                return null;

            try
            {
                // 支持嵌套属性
                var properties = propertyPath.Split('.');
                object currentValue = obj;

                foreach (var propertyName in properties)
                {
                    if (currentValue == null)
                        return null;

                    var property = currentValue.GetType().GetProperty(propertyName);
                    if (property == null)
                        return null;

                    currentValue = property.GetValue(currentValue);
                }

                return currentValue;
            }
            catch
            {
                return null;
            }
        }

        private object GetHighlightedItem()
        {
            // 获取当前高亮的项
            foreach (var item in Items)
            {
                var container = ItemContainerGenerator.ContainerFromItem(item) as ComboBoxItem;
                if (container != null && container.IsHighlighted)
                {
                    return item;
                }
            }
            return null;
        }

        #endregion Private Methods

        #region TextChanged Event

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent(
            "TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HintBox));

        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        #endregion TextChanged Event
    }
}