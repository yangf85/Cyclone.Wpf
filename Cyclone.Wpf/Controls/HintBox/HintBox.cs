using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public interface IHintable
{
    string HintText { get; }
}

[TemplatePart(Name = PART_ClearTextButton, Type = typeof(Button))]
[TemplatePart(Name = PART_HintTextButton, Type = typeof(Button))]
[TemplatePart(Name = PART_InputTextBox, Type = typeof(TextBox))]
[TemplatePart(Name = PART_DisplayPopup, Type = typeof(Popup))]
public class HintBox : Selector
{
    private const string PART_ClearTextButton = nameof(PART_ClearTextButton);

    private const string PART_DisplayPopup = nameof(PART_DisplayPopup);

    private const string PART_InputTextBox = nameof(PART_InputTextBox);

    private const string PART_HintTextButton = nameof(PART_HintTextButton);

    private Popup _displayPopup;

    private TextBox _inputTextBox;

    public Button _clearTextButton;

    

    public HintBox()
    {
        Loaded += HintBox_Loaded;
        Unloaded+= HintBox_Unloaded;
        
    }

    private void HintBox_Unloaded(object sender, RoutedEventArgs e)
    {
        RemoveHandler(HintBoxItem.MouseLeftButtonDownEvent,new RoutedEventHandler(OnItemMouseLeftButtonDown));
        
    }

    private void HintBox_Loaded(object sender, RoutedEventArgs e)
    {
        AddHandler(HintBoxItem.MouseLeftButtonDownEvent,new RoutedEventHandler(OnItemMouseLeftButtonDown), true);
    }

    private void OnItemMouseLeftButtonDown(object sender, RoutedEventArgs e)
    {
        
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

    #region ClearTextCommand

    public static RoutedCommand ClearTextCommand { get; private set; } =
         new RoutedCommand("ClearText", typeof(HintBox));

    private static void OnCanClearTextCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var searchBox = (HintBox)sender;

        if (searchBox._inputTextBox != null)
        {
            e.CanExecute = !string.IsNullOrEmpty(searchBox._inputTextBox.Text);
        }
    }

    private static void OnClearTextCommand(object sender, ExecutedRoutedEventArgs e)
    {
        var searchBox = (HintBox)sender;
        if (searchBox._inputTextBox != null)
        {
            searchBox._inputTextBox.Clear();
            searchBox._inputTextBox.Focus();
        }
    }

    #endregion ClearTextCommand

    #region HintCommand

    public static RoutedCommand HintCommand { get; private set; } =
        new RoutedCommand("Hint", typeof(HintBox));

    private static void OnCanHintCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var hintBox = (HintBox)sender;
        e.CanExecute = !hintBox.HasItems;
    }

    private static void OnHintCommand(object sender, ExecutedRoutedEventArgs e)
    {
        var hintBox = (HintBox)sender;
        if (hintBox._displayPopup != null)
        {
            hintBox.IsOpen = true;
        }
    }

    #endregion HintCommand

    #region MaxContainerHeight
    public double MaxContainerHeight
    {
        get => (double)GetValue(MaxContainerHeightProperty);
        set => SetValue(MaxContainerHeightProperty, value);
    }

    public static readonly DependencyProperty MaxContainerHeightProperty =
        DependencyProperty.Register(nameof(MaxContainerHeight), typeof(double), typeof(HintBox), new PropertyMetadata(300d));

    #endregion

    private static void InitializeCommands()
    {
        CommandManager.RegisterClassCommandBinding(typeof(HintBox),
            new CommandBinding(HintCommand, OnHintCommand, OnCanHintCommand));

        CommandManager.RegisterClassCommandBinding(typeof(HintBox),
           new CommandBinding(ClearTextCommand, OnClearTextCommand, OnCanClearTextCommand));
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        //过滤项目，当默认的过滤器为null时候，按照类型的ToString方法来过滤
        if (Items != null)
        {
            if (Items.Filter == null || Items.Filter == FilterPredicate)
            {
                Items.Filter = FilterPredicate;
            }
        }
        IsOpen = true;
        _inputTextBox?.Focus();
        RaiseEvent(new RoutedEventArgs(TextChangedEvent, this));
    }

    private void SetText(string text)
    {
        if (_inputTextBox != null)
        {
            _inputTextBox.Text = text;
            _inputTextBox.CaretIndex = _inputTextBox.Text.Length;
        }
    }

    internal void NotifyHintBoxItemMouseEnter(HintBoxItem hintBoxItem)
    {
        for (int i = 0; i < ItemContainerGenerator.Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is HintBoxItem item)
            {
                item.SetHighlight(false);
            }
        }
        hintBoxItem.SetHighlight(true);
    }

    internal void NotifyHintBoxItemMouseLeftButtonDown(HintBoxItem hintBoxItem)
    {
        SetText(hintBoxItem.HintText);
        IsOpen = false;
    }

    #region Override

    private int _highlightIndex = -1;

    private int _HighlightIndex
    {
        get => _highlightIndex;

        set
        {
            if (value <= 0)
            {
                value = 0;
            }
            if (value >= Items.Count)
            {
                value = Items.Count - 1;
            }
            _highlightIndex = value;
        }
    }

    private bool FilterPredicate(object item)
    {
        if (string.IsNullOrEmpty(InputText) || item == null)
        {
            return true;
        }

        string text = string.Empty;
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

    private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Items.Count == 0) return;

        switch (e.Key)
        {
            case Key.Down:
            case Key.Tab:
                _HighlightIndex += 1;
                MoveSelectionTo(_HighlightIndex);
                e.Handled = true;
                break;

            case Key.Up:
                _HighlightIndex -= 1;
                MoveSelectionTo(_HighlightIndex);
                e.Handled = true;
                break;

            case Key.Enter:
                SelectHighlightedItem();
                break;
        }
    }

    private void MoveSelectionTo(int position)
    {
        for (int i = 0; i < ItemContainerGenerator.Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is HintBoxItem item)
            {
                item.SetHighlight(false);
            }
        }

        if (ItemContainerGenerator.ContainerFromIndex(position) is HintBoxItem current)
        {
            current.SetHighlight(true);
        };
    }

    private void SelectHighlightedItem()
    {
        for (int i = 0; i < ItemContainerGenerator.Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is HintBoxItem item)
            {
                item.IsSelected = false;
                if (item.IsHighlighted)
                {
                    item.IsSelected = true;
                    NotifyHintBoxItemMouseLeftButtonDown(item);
                    return;
                }
            }
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
            _inputTextBox.PreviewKeyDown -= InputTextBox_PreviewKeyDown;
            _inputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
        }
        _displayPopup = GetTemplateChild(PART_DisplayPopup) as Popup;
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