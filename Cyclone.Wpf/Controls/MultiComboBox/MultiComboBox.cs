using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_DisplayedTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PART_Watermark, Type = typeof(TextBlock))]
[TemplatePart(Name = PART_ToggleButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_ClearButton, Type = typeof(Button))]
[TemplatePart(Name = PART_ItemsContainer, Type = typeof(Popup))]
[TemplatePart(Name = PART_SelectAllCheckBox, Type = typeof(CheckBox))]
public class MultiComboBox : ItemsControl
{
    private const string PART_DisplayedTextBox = "PART_DisplayedTextBox";
    private const string PART_Watermark = "PART_Watermark";
    private const string PART_ToggleButton = "PART_ToggleButton";
    private const string PART_ClearButton = "PART_ClearButton";
    private const string PART_ItemsContainer = "PART_ItemsContainer";
    private const string PART_SelectAllCheckBox = "PART_SelectAllCheckBox";

    private TextBox _textBox;
    private TextBlock _watermark;
    private ToggleButton _toggleButton;
    private Button _clearButton;
    private bool _isInternalUpdate = false;
    private Popup _itemsContainer;
    private CheckBox _selectAllCheckBox;

    #region 路由命令

    public static readonly RoutedCommand ClearCommand = new RoutedCommand(
        "Clear", typeof(MultiComboBox));

    #endregion 路由命令

    #region SelectedItems

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.Register(nameof(SelectedItems), typeof(IList), typeof(MultiComboBox),
            new FrameworkPropertyMetadata(new List<object>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged));

    public IList SelectedItems
    {
        get => (IList)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiComboBox control)
        {
            // 当SelectedItems改变时，要重新关联Collection Changed事件
            if (e.OldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= control.SelectedItems_CollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += control.SelectedItems_CollectionChanged;
            }

            control.UpdateItems();
            control.UpdateDisplayText();
            control.UpdateSelectAllState();
        }
    }

    #endregion SelectedItems

    #region SelectedValuePath

    public static readonly DependencyProperty SelectedValuePathProperty =
      DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(MultiComboBox),
          new PropertyMetadata(string.Empty));

    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    #endregion SelectedValuePath

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
      DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(MultiComboBox),
          new PropertyMetadata(false));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    #endregion IsOpen

    #region Separator

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(nameof(Separator), typeof(string), typeof(MultiComboBox),
            new PropertyMetadata(", "));

    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    #endregion Separator

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
       DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(MultiComboBox),
           new PropertyMetadata("请选择...", OnWatermarkTextChanged));

    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    #endregion Watermark

    #region MaxContainerHeight

    public static readonly DependencyProperty MaxContainerHeightProperty =
        DependencyProperty.Register(nameof(MaxContainerHeight), typeof(double), typeof(MultiComboBox),
            new PropertyMetadata(200.0));

    public double MaxContainerHeight
    {
        get => (double)GetValue(MaxContainerHeightProperty);
        set => SetValue(MaxContainerHeightProperty, value);
    }

    #endregion MaxContainerHeight

    #region IsShowSelectAll

    public static readonly DependencyProperty IsShowSelectAllProperty =
        DependencyProperty.Register(nameof(IsShowSelectAll), typeof(bool), typeof(MultiComboBox),
            new PropertyMetadata(false));

    public bool IsShowSelectAll
    {
        get => (bool)GetValue(IsShowSelectAllProperty);
        set => SetValue(IsShowSelectAllProperty, value);
    }

    #endregion IsShowSelectAll

    #region SelectAllText

    public static readonly DependencyProperty SelectAllTextProperty =
        DependencyProperty.Register(nameof(SelectAllText), typeof(string), typeof(MultiComboBox),
            new PropertyMetadata("全选"));

    public string SelectAllText
    {
        get => (string)GetValue(SelectAllTextProperty);
        set => SetValue(SelectAllTextProperty, value);
    }

    #endregion SelectAllText

    #region IsShowClearButton

    public static readonly DependencyProperty IsShowClearButtonProperty =
        DependencyProperty.Register(nameof(IsShowClearButton), typeof(bool), typeof(MultiComboBox),
            new PropertyMetadata(true));

    public bool IsShowClearButton
    {
        get => (bool)GetValue(IsShowClearButtonProperty);
        set => SetValue(IsShowClearButtonProperty, value);
    }

    #endregion IsShowClearButton

    #region HasSelectedItems

    public static readonly DependencyProperty HasSelectedItemsProperty =
        DependencyProperty.Register(nameof(HasSelectedItems), typeof(bool), typeof(MultiComboBox),
            new PropertyMetadata(false));

    public bool HasSelectedItems
    {
        get => (bool)GetValue(HasSelectedItemsProperty);
        private set => SetValue(HasSelectedItemsProperty, value);
    }

    #endregion HasSelectedItems

    #region 路由事件

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(SelectionChangedEventHandler), typeof(MultiComboBox));

    #endregion 路由事件

    #region 事件

    public event SelectionChangedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    #endregion 事件

    static MultiComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBox), new FrameworkPropertyMetadata(typeof(MultiComboBox)));

        // 注册命令绑定
        CommandManager.RegisterClassCommandBinding(typeof(MultiComboBox),
            new CommandBinding(ClearCommand, OnClearCommandExecuted, OnClearCommandCanExecute));
    }

    public MultiComboBox()
    {
        Keyboard.AddKeyDownHandler(this, OnKeyDown);
        Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);

        // 确保在加载后更新显示
        Loaded += (s, e) =>
        {
            UpdateDisplayText();

            // 如果SelectedItems是可观察集合，添加集合变更通知
            if (SelectedItems is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += SelectedItems_CollectionChanged;
            }
        };

        // 卸载时移除事件
        Unloaded += (s, e) =>
        {
            if (SelectedItems is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged -= SelectedItems_CollectionChanged;
            }
        };
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new MultiComboBoxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MultiComboBoxItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is MultiComboBoxItem multiComboBoxItem)
        {
            // 处理 DisplayMemberPath
            if (!string.IsNullOrEmpty(DisplayMemberPath) && !(item is string))
            {
                var binding = new Binding(DisplayMemberPath) { Source = item };
                var textBlock = new TextBlock();
                textBlock.SetBinding(TextBlock.TextProperty, binding);
                multiComboBoxItem.Content = textBlock;
            }
            else
            {
                multiComboBoxItem.Content = item;
            }

            // 移除任何现有的事件处理器，避免重复订阅
            multiComboBoxItem.Selected -= Item_Selected;
            multiComboBoxItem.Unselected -= Item_Unselected;

            // 检查项目是否被选中
            bool isSelected = SelectedItems.Contains(item);

            // 防止在设置IsSelected时触发不必要的事件
            _isInternalUpdate = true;
            multiComboBoxItem.IsSelected = isSelected;
            _isInternalUpdate = false;

            // 添加事件处理
            multiComboBoxItem.Selected += Item_Selected;
            multiComboBoxItem.Unselected += Item_Unselected;
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        if (element is MultiComboBoxItem multiComboBoxItem)
        {
            // 移除事件处理器
            multiComboBoxItem.Selected -= Item_Selected;
            multiComboBoxItem.Unselected -= Item_Unselected;
        }

        base.ClearContainerForItemOverride(element, item);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _textBox = GetTemplateChild(PART_DisplayedTextBox) as TextBox;
        _watermark = GetTemplateChild(PART_Watermark) as TextBlock;
        _toggleButton = GetTemplateChild(PART_ToggleButton) as ToggleButton;
        _clearButton = GetTemplateChild(PART_ClearButton) as Button;
        _itemsContainer = GetTemplateChild(PART_ItemsContainer) as Popup;
        _selectAllCheckBox = GetTemplateChild(PART_SelectAllCheckBox) as CheckBox;

        if (_selectAllCheckBox != null)
        {
            _selectAllCheckBox.Checked -= SelectAllCheckBox_Checked;
            _selectAllCheckBox.Unchecked -= SelectAllCheckBox_Unchecked;

            _selectAllCheckBox.Checked += SelectAllCheckBox_Checked;
            _selectAllCheckBox.Unchecked += SelectAllCheckBox_Unchecked;
        }

        UpdateItems();
        UpdateDisplayText();
        UpdateSelectAllState();
    }

    #region 命令处理

    private static void OnClearCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is MultiComboBox multiComboBox)
        {
            multiComboBox.UnselectAll();
        }
    }

    private static void OnClearCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is MultiComboBox multiComboBox)
        {
            e.CanExecute = multiComboBox.SelectedItems != null && multiComboBox.SelectedItems.Count > 0;
        }
    }

    #endregion 命令处理

    #region 事件处理器

    private static void OnWatermarkTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (MultiComboBox)d;
        if (control._watermark != null)
        {
            control._watermark.Text = (string)e.NewValue;
            control.UpdateDisplayText();
        }
    }

    private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isInternalUpdate)
            return;

        UpdateItems();
        UpdateDisplayText();
        UpdateSelectAllState();

        // 触发SelectionChanged事件
        List<object> removedItems = new List<object>();
        List<object> addedItems = new List<object>();

        if (e.OldItems != null)
        {
            removedItems.AddRange(e.OldItems.Cast<object>());
        }

        if (e.NewItems != null)
        {
            addedItems.AddRange(e.NewItems.Cast<object>());
        }

        var args = new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems);
        RaiseEvent(args);
    }

    private void Item_Selected(object sender, RoutedEventArgs e)
    {
        if (_isInternalUpdate)
            return;

        if (sender is MultiComboBoxItem item)
        {
            var itemData = GetItemFromContainer(item);
            if (itemData != null && !SelectedItems.Contains(itemData))
            {
                _isInternalUpdate = true;
                SelectedItems.Add(itemData);
                _isInternalUpdate = false;

                // 立即更新显示文本
                UpdateDisplayText();

                // 更新全选状态
                UpdateSelectAllState();
            }
        }
    }

    private void Item_Unselected(object sender, RoutedEventArgs e)
    {
        if (_isInternalUpdate)
            return;

        if (sender is MultiComboBoxItem item)
        {
            var itemData = GetItemFromContainer(item);
            if (itemData != null && SelectedItems.Contains(itemData))
            {
                _isInternalUpdate = true;
                SelectedItems.Remove(itemData);
                _isInternalUpdate = false;

                // 立即更新显示文本
                UpdateDisplayText();

                // 更新全选状态
                UpdateSelectAllState();
            }
        }
    }

    private void SelectAllCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        if (_isInternalUpdate)
            return;

        // 选择所有项
        SelectAll();
    }

    private void SelectAllCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        if (_isInternalUpdate)
            return;

        // 取消选择所有项
        UnselectAll();
    }

    // 从容器获取数据项
    private object GetItemFromContainer(MultiComboBoxItem container)
    {
        if (ItemContainerGenerator.ItemFromContainer(container) is object item)
        {
            return item;
        }

        // 如果ItemContainerGenerator未找到，尝试使用DataContext
        return container.DataContext;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen)
        {
            IsOpen = false;
            e.Handled = true;
        }
    }

    private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
    {
        // 确保不会关闭打开的下拉菜单，除非点击是真正在控件外部
        if (IsOpen)
        {
            Point pt = e.GetPosition(this);
            bool isOutside = (pt.X < 0 || pt.Y < 0 || pt.X > ActualWidth || pt.Y > ActualHeight + MaxContainerHeight);

            if (isOutside)
            {
                IsOpen = false;
            }
            else
            {
                e.Handled = true; // 阻止事件冒泡，避免关闭下拉菜单
            }
        }
    }

    #endregion 事件处理器

    #region 公共方法

    public void SelectAll()
    {
        if (Items.Count == 0)
            return;

        _isInternalUpdate = true;
        try
        {
            // 先清空当前选择
            SelectedItems.Clear();

            // 添加所有项到选中集合
            foreach (var item in Items)
            {
                if (!SelectedItems.Contains(item))
                {
                    SelectedItems.Add(item);
                }
            }

            // 更新所有容器的选中状态
            for (int i = 0; i < Items.Count; i++)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(i) as MultiComboBoxItem;
                if (container != null)
                {
                    container.IsSelected = true;
                }
            }

            // 设置全选项的选中状态
            if (_selectAllCheckBox != null)
            {
                _selectAllCheckBox.IsChecked = true;
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }

        // 更新显示文本
        UpdateDisplayText();

        // 触发SelectionChanged事件
        var args = new SelectionChangedEventArgs(SelectionChangedEvent, new List<object>(), Items.Cast<object>().ToList());
        RaiseEvent(args);
    }

    public void UnselectAll()
    {
        if (SelectedItems.Count == 0)
            return;

        _isInternalUpdate = true;
        try
        {
            // 获取当前选中项的副本
            List<object> oldSelectedItems = new List<object>();
            foreach (var item in SelectedItems)
            {
                oldSelectedItems.Add(item);
            }

            // 清空选择
            SelectedItems.Clear();

            // 更新所有容器的选中状态
            for (int i = 0; i < Items.Count; i++)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(i) as MultiComboBoxItem;
                if (container != null)
                {
                    container.IsSelected = false;
                }
            }

            // 设置全选项的选中状态
            if (_selectAllCheckBox != null)
            {
                _selectAllCheckBox.IsChecked = false;
            }

            // 更新显示文本
            UpdateDisplayText();

            // 触发SelectionChanged事件
            var args = new SelectionChangedEventArgs(SelectionChangedEvent, oldSelectedItems, new List<object>());
            RaiseEvent(args);
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    #endregion 公共方法

    #region 辅助方法

    private void UpdateItems()
    {
        if (_isInternalUpdate)
            return;

        _isInternalUpdate = true;
        try
        {
            // 更新所有可见的MultiComboBoxItem的选中状态
            for (int i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var container = ItemContainerGenerator.ContainerFromIndex(i) as MultiComboBoxItem;
                if (container != null)
                {
                    container.IsSelected = SelectedItems.Contains(item);
                }
            }
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    private void UpdateDisplayText()
    {
        if (_textBox == null || _watermark == null)
            return;

        if (SelectedItems == null || SelectedItems.Count == 0)
        {
            _textBox.Text = string.Empty;
            _watermark.Visibility = Visibility.Visible;
        }
        else
        {
            var displayTexts = new List<string>();
            foreach (var item in SelectedItems)
            {
                string displayText;
                if (!string.IsNullOrEmpty(DisplayMemberPath) && !(item is string))
                {
                    var property = item.GetType().GetProperty(DisplayMemberPath);
                    displayText = property?.GetValue(item)?.ToString() ?? item.ToString();
                }
                else
                {
                    displayText = item.ToString();
                }
                displayTexts.Add(displayText);
            }

            _textBox.Text = string.Join(Separator, displayTexts);
            _watermark.Visibility = Visibility.Collapsed;
        }
    }

    private void UpdateSelectAllState()
    {
        if (_selectAllCheckBox == null || !IsShowSelectAll || Items.Count == 0)
            return;

        _isInternalUpdate = true;
        try
        {
            // 判断是否所有项都被选中
            bool allSelected = true;
            foreach (var item in Items)
            {
                if (!SelectedItems.Contains(item))
                {
                    allSelected = false;
                    break;
                }
            }

            // 更新全选项的状态
            _selectAllCheckBox.IsChecked = allSelected;
        }
        finally
        {
            _isInternalUpdate = false;
        }
    }

    #endregion 辅助方法
}