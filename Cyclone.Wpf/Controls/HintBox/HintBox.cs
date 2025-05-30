using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public interface IHintable
{
    string HintText { get; }
}

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

    static HintBox()
    {
        InitializeCommands();
    }

    #region InputText

    public static readonly DependencyProperty InputTextProperty =
        DependencyProperty.Register(nameof(InputText), typeof(string), typeof(HintBox), new PropertyMetadata(default(string)));

    public string InputText
    {
        get => (string)GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    #endregion InputText

    #region IsIgnoreCase

    public static readonly DependencyProperty IsIgnoreCaseProperty =
        DependencyProperty.Register(nameof(IsIgnoreCase), typeof(bool), typeof(HintBox), new PropertyMetadata(true));

    public bool IsIgnoreCase
    {
        get => (bool)GetValue(IsIgnoreCaseProperty);
        set => SetValue(IsIgnoreCaseProperty, value);
    }

    #endregion IsIgnoreCase

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(HintBox), new PropertyMetadata(default(bool)));

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
        DependencyProperty.Register(nameof(MaxContainerHeight), typeof(double), typeof(HintBox), new PropertyMetadata(300d));

    #endregion MaxContainerHeight

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

    #region Override

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

        // 确保元素已生成（禁用虚拟化时无需此步骤）
        item.BringIntoView();

        // 获取元素相对于ScrollViewer的坐标
        var itemTransform = item.TransformToVisual(_scrollViewer);
        var itemTopPosition = itemTransform.Transform(new Point(0, 0)).Y;
        var itemBottomPosition = itemTopPosition + item.ActualHeight;

        // 获取滚动视口的垂直范围
        var viewportTop = _scrollViewer.VerticalOffset;
        var viewportBottom = viewportTop + _scrollViewer.ViewportHeight;

        // 检查元素是否完全在视口外
        if (itemBottomPosition <= viewportTop || itemTopPosition >= viewportBottom)
        {
            // 元素完全不可见，滚动到可见位置
            // 使用BringIntoView确保元素完全可见
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

        if (SelectedItem is IHintable hintable)
        {
            InputText = hintable.HintText;
            if (_inputTextBox != null)
            {
                _inputTextBox.Text = InputText;
            }
        }
    }

    private bool FilterPredicate(object item)
    {
        if (string.IsNullOrEmpty(InputText) || item == null)
        {
            return true;
        }

        string text;
        if (item is IHintable hintable)
        {
            text = hintable.HintText;
        }
        else
        {
            text = item.ToString();
        }
        if (IsIgnoreCase)
        {
            return text.ToUpper().Contains(InputText.ToUpper());
        }
        else
        {
            return text.Contains(InputText);
        }
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

    #endregion Override

    #region TextChanged

    public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HintBox));

    public event RoutedEventHandler TextChanged
    {
        add { AddHandler(TextChangedEvent, value); }
        remove { RemoveHandler(TextChangedEvent, value); }
    }

    #endregion TextChanged
}