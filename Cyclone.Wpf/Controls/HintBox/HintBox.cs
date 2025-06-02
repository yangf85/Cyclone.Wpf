using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_ClearTextButton, Type = typeof(Button))]
[TemplatePart(Name = PART_HintTextButton, Type = typeof(Button))]
[TemplatePart(Name = PART_InputTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PART_DisplayPopup, Type = typeof(Popup))]
[TemplatePart(Name = PART_ContainerScrollViewer, Type = typeof(ScrollViewer))]
public class HintBox : Selector
{
    private const string PART_ContainerScrollViewer = nameof(PART_ContainerScrollViewer);
    private const string PART_ClearTextButton = nameof(PART_ClearTextButton);
    private const string PART_DisplayPopup = nameof(PART_DisplayPopup);
    private const string PART_InputTextBox = nameof(PART_InputTextBox);
    private const string PART_HintTextButton = nameof(PART_HintTextButton);

    private Popup _displayPopup;
    private TextBox _inputTextBox;
    public Button _clearTextButton;
    private ScrollViewer _scrollViewer;

    public HintBox()
    {
        Loaded += HintBox_Loaded;
        Unloaded += HintBox_Unloaded;
    }

    static HintBox()
    {
        InitializeCommands();
        // 重写 DisplayMemberPath 属性的元数据以添加回调
        DisplayMemberPathProperty.OverrideMetadata(typeof(HintBox),
            new FrameworkPropertyMetadata(string.Empty, OnDisplayMemberPathChanged));
    }

    private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var hintBox = (HintBox)d;
        // 当 DisplayMemberPath 改变时，刷新过滤器
        hintBox.Items?.Filter = hintBox.FilterPredicate;
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
        // 刷新过滤器
        hintBox.Items?.Filter = hintBox.FilterPredicate;
    }

    #endregion SearchMemberPath

    #region StringComparison

    public static readonly DependencyProperty StringComparisonProperty =
        DependencyProperty.Register(nameof(StringComparison), typeof(StringComparison), typeof(HintBox),
            new PropertyMetadata(StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// 获取或设置字符串比较方式
    /// </summary>
    public StringComparison StringComparison
    {
        get => (StringComparison)GetValue(StringComparisonProperty);
        set => SetValue(StringComparisonProperty, value);
    }

    #endregion StringComparison

    #region InputText

    public static readonly DependencyProperty InputTextProperty =
        DependencyProperty.Register(nameof(InputText), typeof(string), typeof(HintBox),
            new PropertyMetadata(default(string)));

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    #endregion InputText

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(HintBox),
            new PropertyMetadata(default(bool)));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    #endregion IsOpen

    #region MaxContainerHeight

    public double MaxContainerHeight
    {
        get => (double)GetValue(MaxContainerHeightProperty);
        set => SetValue(MaxContainerHeightProperty, value);
    }

    public static readonly DependencyProperty MaxContainerHeightProperty =
        DependencyProperty.Register(nameof(MaxContainerHeight), typeof(double), typeof(HintBox),
            new PropertyMetadata(300d));

    #endregion MaxContainerHeight

    #region Override Methods

    private void HintBox_Unloaded(object sender, RoutedEventArgs e)
    {
        RemoveHandler(HintBoxItem.ClickedEvent, new RoutedEventHandler(OnItemClicked));
    }

    private void OnItemClicked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is HintBoxItem clickedItem)
        {
            var index = ItemContainerGenerator.IndexFromContainer(clickedItem);
            SelectedItem = Items[index];
        }
        IsOpen = false;
    }

    private void HintBox_Loaded(object sender, RoutedEventArgs e)
    {
        AddHandler(HintBoxItem.ClickedEvent, new RoutedEventHandler(OnItemClicked), true);
    }

    private static void InitializeCommands()
    {
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(TextChangedEvent, this));

        if (Items != null)
        {
            if (Items.Filter == null || Items.Filter == FilterPredicate)
            {
                Items.Filter = FilterPredicate;
            }
        }

        IsOpen = true;
        _inputTextBox?.Focus();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        switch (e.Key)
        {
            case Key.Enter:
                HandleKeyEnter();
                break;

            case Key.Up:
                HandleKeyUpDown(true);
                break;

            case Key.Down:
                HandleKeyUpDown(false);
                break;

            case Key.Escape:
                HandleKeyCancel();
                break;

            default:
                break;
        }
    }

    private void HandleKeyCancel()
    {
        IsOpen = false;
    }

    private int GetHighlightedIndex()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            var item = ItemContainerGenerator.ContainerFromIndex(i);
            if (item is HintBoxItem hintBoxItem)
            {
                if (hintBoxItem.IsHighlighted)
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private void HandleKeyUpDown(bool isUp)
    {
        int itemCount = Items.Count;
        if (itemCount == 0)
        {
            return;
        }

        int currentIndex = GetHighlightedIndex();
        currentIndex = currentIndex == -1 ? 0 : currentIndex;

        // 计算新索引并处理循环
        int newIndex = (currentIndex + (isUp ? -1 : 1) + itemCount) % itemCount;

        // 取消当前高亮项
        if (ItemContainerGenerator.ContainerFromIndex(currentIndex) is HintBoxItem currentItem)
        {
            currentItem.ChangeHighlightState(false);
        }

        // 设置新高亮项
        if (ItemContainerGenerator.ContainerFromIndex(newIndex) is HintBoxItem newItem)
        {
            newItem.ChangeHighlightState(true);
            ScrollToHighlightedItem(newItem);
        }
    }

    private void ScrollToHighlightedItem(HintBoxItem item)
    {
        if (_scrollViewer == null)
            return;

        item.BringIntoView();

        var itemTransform = item.TransformToVisual(_scrollViewer);
        var itemTopPosition = itemTransform.Transform(new Point(0, 0)).Y;
        var itemBottomPosition = itemTopPosition + item.ActualHeight;

        var viewportTop = _scrollViewer.VerticalOffset;
        var viewportBottom = viewportTop + _scrollViewer.ViewportHeight;

        if (itemBottomPosition <= viewportTop || itemTopPosition >= viewportBottom)
        {
            item.BringIntoView();
        }
    }

    private void HandleKeyEnter()
    {
        var index = GetHighlightedIndex();

        if (index == -1)
        {
            IsOpen = false;
            return;
        }

        SelectedItem = Items[index];
        IsOpen = false;
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (SelectedItem != null)
        {
            // 获取显示文本
            string displayText = GetItemText(SelectedItem);
            if (!string.IsNullOrEmpty(displayText))
            {
                InputText = displayText;
                if (_inputTextBox != null)
                {
                    _inputTextBox.Text = InputText;
                }
            }
        }
    }

    /// <summary>
    /// 获取项的显示文本
    /// </summary>
    private string GetItemText(object item)
    {
        if (item == null)
            return string.Empty;

        // 如果设置了 DisplayMemberPath，使用它
        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            return GetPropertyValue(item, DisplayMemberPath)?.ToString() ?? string.Empty;
        }

        // 使用 ToString()
        return item.ToString();
    }

    /// <summary>
    /// 获取项的搜索文本
    /// </summary>
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

    /// <summary>
    /// 通过属性路径获取属性值
    /// </summary>
    private object GetPropertyValue(object obj, string propertyPath)
    {
        if (obj == null || string.IsNullOrEmpty(propertyPath))
            return null;

        try
        {
            // 支持嵌套属性，如 "Address.City"
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

    private bool FilterPredicate(object item)
    {
        if (string.IsNullOrEmpty(InputText) || item == null)
        {
            return true;
        }

        // 获取搜索文本
        var searchText = GetSearchText(item);

        // 检查是否包含输入文本
        return searchText.IndexOf(InputText, StringComparison) >= 0;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new HintBoxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is HintBoxItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is HintBoxItem container)
        {
            container.DataContext = item;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _inputTextBox = GetTemplateChild(PART_InputTextBox) as TextBox;
        if (_inputTextBox != null)
        {
            _inputTextBox.TextChanged -= InputTextBox_TextChanged;
            _inputTextBox.TextChanged += InputTextBox_TextChanged;
        }
        _displayPopup = GetTemplateChild(PART_DisplayPopup) as Popup;
        _scrollViewer = GetTemplateChild(PART_ContainerScrollViewer) as ScrollViewer;
    }

    #endregion Override Methods

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