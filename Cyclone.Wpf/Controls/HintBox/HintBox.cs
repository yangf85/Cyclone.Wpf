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
        RemoveHandler(HintBoxItem.ClickedEvent,new RoutedEventHandler(OnItemClickedButtonDown));
        
    }

    private void OnItemClickedButtonDown(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is HintBoxItem clickedItem)
        {
            var itemData = ItemContainerGenerator.ItemFromContainer(clickedItem);
          
            SelectedItem = itemData;

            if (itemData is IHintable hintable)
            {
                InputText = hintable.HintText;
            }
                
        }
        IsOpen = false;
    }

   

    private void HintBox_Loaded(object sender, RoutedEventArgs e)
    {
        AddHandler(HintBoxItem.ClickedEvent, new RoutedEventHandler(OnItemClickedButtonDown), true);
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

    #endregion

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

    private void SetText(string text)
    {
        if (_inputTextBox != null)
        {
            _inputTextBox.Text = text;
            _inputTextBox.CaretIndex = _inputTextBox.Text.Length;
        }
    }






    #region Override

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Enter)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var item=ItemContainerGenerator.ContainerFromIndex(i);
                if (item is HintBoxItem hintBoxItem)
                {
                    if (hintBoxItem.IsMouseOver)
                    {
                        SetText(hintBoxItem.HintText);
                        IsOpen = false;
                        break;
                    }
                }
            }

        }
    }
    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        var s = e.AddedItems;
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