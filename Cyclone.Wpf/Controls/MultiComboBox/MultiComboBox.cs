using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_SelectedItemsControl, Type = (typeof(ItemsControl)))]
[TemplatePart(Name = PART_OpenButton, Type = (typeof(ToggleButton)))]
[TemplatePart(Name = PART_Popup, Type = (typeof(Popup)))]
public class MultiComboBox : MultiSelector
{
    private const string PART_OpenButton = nameof(PART_OpenButton);

    private const string PART_Popup = nameof(PART_Popup);

    private const string PART_SelectedItemsControl = nameof(PART_SelectedItemsControl);

    private Popup _popup;

    ItemsControl _itemsControl;

    static MultiComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBox), new FrameworkPropertyMetadata(typeof(MultiComboBox)));

        CommandManager.RegisterClassCommandBinding(typeof(MultiComboBox),
              new CommandBinding(DeselectItemCommand, OnDeselectItemCommand, OnCanDeselectItemCommand));
    }

    public MultiComboBox()
    {
        SelectionChanged += MultiSelectComboBox_SelectionChanged;
    }

    private void MultiSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        foreach (var item in e.RemovedItems)
        {
            BindableSelectItems?.Remove(item);
        }
        foreach (var item in e.AddedItems)
        {
            BindableSelectItems?.Add(item);
        }
    }

    #region Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new MultiComboBoxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is MultiComboBoxItem;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        IsOpened = false;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _popup = GetTemplateChild(PART_Popup) as Popup;
    }

    #endregion Override

    #region MaxColumns

    public static readonly DependencyProperty MaxColumnsProperty =
        DependencyProperty.Register(nameof(MaxColumns), typeof(int), typeof(MultiComboBox), new PropertyMetadata(5));

    public int MaxColumns
    {
        get => (int)GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
    }

    #endregion MaxColumns

    #region MaxRows

    public static readonly DependencyProperty MaxRowsProperty =
        DependencyProperty.Register(nameof(MaxRows), typeof(int), typeof(MultiComboBox), new PropertyMetadata(default(int)));

    public int MaxRows
    {
        get => (int)GetValue(MaxRowsProperty);
        set => SetValue(MaxRowsProperty, value);
    }

    #endregion MaxRows

    #region IsSelectAll

    public static readonly DependencyProperty IsSelectAllProperty =
                DependencyProperty.Register(nameof(IsSelectAll), typeof(bool), typeof(MultiComboBox), new PropertyMetadata(default(bool), OnIsSelectAllChanged));

    public bool IsSelectAll
    {
        get => (bool)GetValue(IsSelectAllProperty);
        set => SetValue(IsSelectAllProperty, value);
    }

    private static void OnIsSelectAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = d as MultiComboBox;
        if ((bool)e.NewValue)
        {
            box.SelectAll();
        }
        else
        {
            box.UnselectAll();
        }
    }

    #endregion IsSelectAll

    #region SelectAllText

    public static readonly DependencyProperty SelectAllTextProperty =
        DependencyProperty.Register(nameof(SelectAllText), typeof(string), typeof(MultiComboBox), new PropertyMetadata("SelectAll"));

    public string SelectAllText
    {
        get => (string)GetValue(SelectAllTextProperty);
        set => SetValue(SelectAllTextProperty, value);
    }

    #endregion SelectAllText

    #region BindableSelectItems

    public static readonly DependencyProperty BindableSelectItemsProperty =
        DependencyProperty.Register(nameof(BindableSelectItems), typeof(IList), typeof(MultiComboBox), new PropertyMetadata(default(IList)));

    public IList BindableSelectItems
    {
        get => (IList)GetValue(BindableSelectItemsProperty);
        set => SetValue(BindableSelectItemsProperty, value);
    }

    #endregion BindableSelectItems

    #region IsOpened

    public static readonly DependencyProperty IsOpenedProperty =
        DependencyProperty.Register(nameof(IsOpened), typeof(bool), typeof(MultiComboBox), new PropertyMetadata(default(bool)));

    public bool IsOpened
    {
        get => (bool)GetValue(IsOpenedProperty);
        set => SetValue(IsOpenedProperty, value);
    }

    #endregion IsOpened

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(MultiComboBox), new PropertyMetadata(320d));

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    #endregion MaxDropDownHeight

    #region PresenteItemStyle

    public static readonly DependencyProperty PresenteItemStyleProperty =
        DependencyProperty.Register(nameof(PresenteItemStyle), typeof(Style), typeof(MultiComboBox), new PropertyMetadata(default(Style)));

    public Style PresenteItemStyle
    {
        get => (Style)GetValue(PresenteItemStyleProperty);
        set => SetValue(PresenteItemStyleProperty, value);
    }

    #endregion PresenteItemStyle

    #region DeselectItemCommand

    public static RoutedCommand DeselectItemCommand { get; private set; } = new RoutedCommand("DeselectItem", typeof(MultiComboBox));

    private static void OnCanDeselectItemCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private static void OnDeselectItemCommand(object sender, ExecutedRoutedEventArgs e)
    {
        var multiComboBox = (MultiComboBox)sender;
        foreach (var item in multiComboBox.SelectedItems)
        {
            if (item == e.Parameter)
            {
                multiComboBox.SelectedItems.Remove(item);
                break;
            }
        }
    }

    #endregion DeselectItemCommand

    #region IsShowCheckBox

    public static readonly DependencyProperty IsShowCheckBoxProperty =
        DependencyProperty.Register(nameof(IsShowCheckBox), typeof(bool), typeof(MultiComboBox), new PropertyMetadata(default(bool)));

    public bool IsShowCheckBox
    {
        get => (bool)GetValue(IsShowCheckBoxProperty);
        set => SetValue(IsShowCheckBoxProperty, value);
    }

    #endregion IsShowCheckBox
}