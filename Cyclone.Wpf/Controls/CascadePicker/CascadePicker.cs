// CascadePicker.cs - 添加键盘支持的版本
using Cyclone.Wpf.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(CascadePickerItem))]
[TemplatePart(Name = PART_DisplayedTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PART_ItemsPopup, Type = typeof(Popup))]
[TemplatePart(Name = PART_ClearButton, Type = typeof(Button))]
[TemplatePart(Name = PART_OpenToggleButton, Type = typeof(ToggleButton))]
public class CascadePicker : ItemsControl
{
    private const string PART_DisplayedTextBox = "PART_DisplayedTextBox";

    private const string PART_ItemsPopup = "PART_ItemsPopup";

    private const string PART_ClearButton = "PART_ClearButton";

    private const string PART_OpenToggleButton = "PART_OpenToggleButton";

    private TextBox _textBox;

    private Popup _popup;

    private Button _clearButton;

    private ToggleButton _openToggleButton;

    private CascadePickerItem _currentFocusedItem;

    #region Commands

    public static readonly RoutedCommand ClearCommand = new RoutedCommand(
        "Clear",
        typeof(CascadePicker),
        new InputGestureCollection { new KeyGesture(Key.Delete) });

    #endregion Commands

    private void CascadePicker_LostFocus(object sender, RoutedEventArgs e)
    {
        // 检查焦点是否还在控件内部
        if (!IsKeyboardFocusWithin)
        {
            SetValue(IsOpenedProperty, false);
            SetFocusedItem(null);
        }
    }

    private void CascadePicker_Unloaded(object sender, RoutedEventArgs e)
    {
        RemoveHandler(CascadePickerItem.ItemClickEvent, new RoutedEventHandler(Item_Click));
    }

    private void CascadePicker_Loaded(object sender, RoutedEventArgs e)
    {
        AddHandler(CascadePickerItem.ItemClickEvent, new RoutedEventHandler(Item_Click));
    }

    static CascadePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePicker), new FrameworkPropertyMetadata(typeof(CascadePicker)));

        // 注册命令绑定
        CommandManager.RegisterClassCommandBinding(
            typeof(CascadePicker),
            new CommandBinding(
                ClearCommand,
                OnClearCommandExecuted,
                OnClearCommandCanExecute));
    }

    public CascadePicker()
    {
        Loaded += CascadePicker_Loaded;
        Unloaded += CascadePicker_Unloaded;
        LostFocus += CascadePicker_LostFocus;
    }

    #region Keyboard Navigation

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (IsReadOnly)
            return;

        switch (e.Key)
        {
            case Key.Down:
                if (!IsOpened)
                {
                    SetCurrentValue(IsOpenedProperty, true);
                    e.Handled = true;
                }
                else
                {
                    NavigateToNextItem();
                    e.Handled = true;
                }
                break;

            case Key.Up:
                if (IsOpened)
                {
                    NavigateToPreviousItem();
                    e.Handled = true;
                }
                break;

            case Key.Right:
                if (IsOpened && _currentFocusedItem?.HasItems == true)
                {
                    ExpandCurrentItem();
                    e.Handled = true;
                }
                break;

            case Key.Left:
                if (IsOpened)
                {
                    CollapseCurrentItem();
                    e.Handled = true;
                }
                break;

            case Key.Enter:
                if (IsOpened && _currentFocusedItem != null)
                {
                    SelectCurrentItem();
                    e.Handled = true;
                }
                break;

            case Key.Escape:
                if (IsOpened)
                {
                    SetCurrentValue(IsOpenedProperty, false);
                    _textBox?.Focus();
                    e.Handled = true;
                }
                break;

            case Key.Space:
                if (!IsOpened && _textBox?.IsFocused == true)
                {
                    SetCurrentValue(IsOpenedProperty, true);
                    e.Handled = true;
                }
                break;
        }
    }

    private void NavigateToNextItem()
    {
        var items = GetAllVisibleItems();
        if (items.Count == 0) return;

        if (_currentFocusedItem == null)
        {
            SetFocusedItem(items.First());
        }
        else
        {
            var currentIndex = items.IndexOf(_currentFocusedItem);
            if (currentIndex < items.Count - 1)
            {
                SetFocusedItem(items[currentIndex + 1]);
            }
        }
    }

    private void NavigateToPreviousItem()
    {
        var items = GetAllVisibleItems();
        if (items.Count == 0) return;

        if (_currentFocusedItem == null)
        {
            SetFocusedItem(items.Last());
        }
        else
        {
            var currentIndex = items.IndexOf(_currentFocusedItem);
            if (currentIndex > 0)
            {
                SetFocusedItem(items[currentIndex - 1]);
            }
        }
    }

    private void ExpandCurrentItem()
    {
        if (_currentFocusedItem?.HasItems == true)
        {
            _currentFocusedItem.IsExpanded = true;

            // 焦点移动到第一个子项
            var firstChild = _currentFocusedItem.ItemContainerGenerator.ContainerFromIndex(0) as CascadePickerItem;
            if (firstChild != null)
            {
                SetFocusedItem(firstChild);
            }
        }
    }

    private void CollapseCurrentItem()
    {
        if (_currentFocusedItem != null)
        {
            // 如果当前项是展开的，先折叠
            if (_currentFocusedItem.IsExpanded)
            {
                _currentFocusedItem.IsExpanded = false;
            }
            else
            {
                // 否则，导航到父项
                var parent = ItemsControl.ItemsControlFromItemContainer(_currentFocusedItem) as CascadePickerItem;
                if (parent != null)
                {
                    SetFocusedItem(parent);
                    parent.IsExpanded = false;
                }
            }
        }
    }

    private void SelectCurrentItem()
    {
        if (_currentFocusedItem != null)
        {
            SetValue(TextProperty, GetSelectedPath(_currentFocusedItem));
            SetCurrentValue(SelectedItemProperty, _currentFocusedItem.DataContext);

            // 如果没有子项，关闭弹出窗口
            if (!_currentFocusedItem.HasItems)
            {
                SetValue(IsOpenedProperty, false);
                _textBox?.Focus();
            }
        }
    }

    private List<CascadePickerItem> GetAllVisibleItems()
    {
        var items = new List<CascadePickerItem>();
        CollectVisibleItems(this, items);
        return items;
    }

    private void CollectVisibleItems(ItemsControl itemsControl, List<CascadePickerItem> items)
    {
        for (int i = 0; i < itemsControl.Items.Count; i++)
        {
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as CascadePickerItem;
            if (container != null && container.IsVisible)
            {
                items.Add(container);

                // 如果项是展开的，递归收集子项
                if (container.IsExpanded && container.HasItems)
                {
                    CollectVisibleItems(container, items);
                }
            }
        }
    }

    private void SetFocusedItem(CascadePickerItem item)
    {
        // 移除之前的高亮样式
        if (_currentFocusedItem != null)
        {
            _currentFocusedItem.IsHighlighted = false;
        }

        // 设置新的焦点项
        _currentFocusedItem = item;
        if (_currentFocusedItem != null)
        {
            _currentFocusedItem.IsHighlighted = true;
            _currentFocusedItem.BringIntoView();
        }
    }

    #endregion Keyboard Navigation

    #region Command Handlers

    public void Clear()
    {
        SetCurrentValue(TextProperty, string.Empty);
        SetCurrentValue(SelectedItemProperty, null);
        _textBox?.Focus();
    }

    private static void OnClearCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is CascadePicker picker && !picker.IsReadOnly)
        {
            picker.Clear();
        }
    }

    private static void OnClearCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is CascadePicker picker)
        {
            e.CanExecute = !picker.IsReadOnly && !string.IsNullOrEmpty(picker.Text);
        }
    }

    #endregion Command Handlers

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(CascadePicker),
            new PropertyMetadata(default(bool), OnIsReadOnlyChanged));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker picker)
        {
            picker.UpdateIsReadOnlyState();
            CommandManager.InvalidateRequerySuggested();
        }
    }

    private void UpdateIsReadOnlyState()
    {
        if (_clearButton != null)
        {
            _clearButton.IsEnabled = !IsReadOnly;
        }

        if (_openToggleButton != null)
        {
            _openToggleButton.IsEnabled = !IsReadOnly;
        }

        if (IsReadOnly && IsOpened)
        {
            SetValue(IsOpenedProperty, false);
        }
    }

    #endregion IsReadOnly

    #region NodeMemberPath

    public static readonly DependencyProperty NodeMemberPathProperty =
        DependencyProperty.Register(nameof(NodeMemberPath), typeof(string), typeof(CascadePicker),
            new PropertyMetadata(null, OnNodeMemberPathChanged));

    public string NodeMemberPath
    {
        get => (string)GetValue(NodeMemberPathProperty);
        set => SetValue(NodeMemberPathProperty, value);
    }

    private static void OnNodeMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker picker)
        {
            picker.UpdateAllItemsNodePath();
        }
    }

    private void UpdateAllItemsNodePath()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is CascadePickerItem container)
            {
                container.UpdateNodePath();
            }
        }
    }

    #endregion NodeMemberPath

    #region SelectedItem

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(CascadePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public object SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker picker)
        {
            picker.RaiseEvent(new RoutedEventArgs(SelectedChangedEvent));
            CommandManager.InvalidateRequerySuggested();
        }
    }

    #endregion SelectedItem

    #region Item_Click

    public string GetSelectedPath(CascadePickerItem item)
    {
        if (item == null) { return string.Empty; }

        if (IsShowFullPath)
        {
            var pathList = new List<string>();
            var currentItem = item;

            while (currentItem != null)
            {
                pathList.Insert(0, currentItem.NodePath);
                var parentContainer = ItemsControl.ItemsControlFromItemContainer(currentItem) as CascadePickerItem;
                currentItem = parentContainer;
            }
            return string.Join(Separator, pathList);
        }
        else
        {
            return item.NodePath;
        }
    }

    private void Item_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is CascadePickerItem item && !IsReadOnly)
        {
            SetValue(TextProperty, GetSelectedPath(item));
            SetCurrentValue(SelectedItemProperty, item.DataContext);
            if (!item.HasItems)
            {
                SetValue(IsOpenedProperty, false);
            }
        }
    }

    #endregion Item_Click

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(CascadePicker), new PropertyMetadata(string.Empty));

    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    #endregion Watermark

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CascadePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker picker)
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    #endregion Text

    #region SelectedChanged

    public static readonly RoutedEvent SelectedChangedEvent = EventManager.RegisterRoutedEvent("SelectedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CascadePicker));

    public event RoutedEventHandler SelectedChanged
    {
        add => AddHandler(SelectedChangedEvent, value);
        remove => RemoveHandler(SelectedChangedEvent, value);
    }

    #endregion SelectedChanged

    #region IsOpened

    public static readonly DependencyProperty IsOpenedProperty =
        DependencyProperty.Register(nameof(IsOpened), typeof(bool), typeof(CascadePicker),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenedChanged));

    public bool IsOpened
    {
        get => (bool)GetValue(IsOpenedProperty);
        set => SetValue(IsOpenedProperty, value);
    }

    private static void OnIsOpenedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePicker picker)
        {
            if (picker.IsReadOnly && (bool)e.NewValue)
            {
                picker.SetValue(IsOpenedProperty, false);
            }
            else if ((bool)e.NewValue)
            {
                // 当打开时，重置焦点项
                picker.SetFocusedItem(null);
            }
        }
    }

    #endregion IsOpened

    #region IsShowFullPath

    public static readonly DependencyProperty IsShowFullPathProperty =
        DependencyProperty.Register(nameof(IsShowFullPath), typeof(bool), typeof(CascadePicker), new PropertyMetadata(default(bool)));

    public bool IsShowFullPath
    {
        get => (bool)GetValue(IsShowFullPathProperty);
        set => SetValue(IsShowFullPathProperty, value);
    }

    #endregion IsShowFullPath

    #region Separator

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(nameof(Separator), typeof(string), typeof(CascadePicker), new PropertyMetadata("/"));

    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    #endregion Separator

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_clearButton != null)
        {
            _clearButton.Click -= ClearButton_Click;
        }

        _textBox = GetTemplateChild(PART_DisplayedTextBox) as TextBox;
        _popup = GetTemplateChild(PART_ItemsPopup) as Popup;
        _clearButton = GetTemplateChild(PART_ClearButton) as Button;
        _openToggleButton = GetTemplateChild(PART_OpenToggleButton) as ToggleButton;

        if (_clearButton != null)
        {
            _clearButton.Command = ClearCommand;
            _clearButton.CommandTarget = this;
            _clearButton.Click += ClearButton_Click;
        }

        UpdateIsReadOnlyState();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is CascadePickerItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new CascadePickerItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is CascadePickerItem container)
        {
            container.DataContext = item;
            container.NodeMemberPath = this.NodeMemberPath;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
    }

    #endregion Override
}