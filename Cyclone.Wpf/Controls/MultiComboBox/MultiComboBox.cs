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

[TemplatePart(Name = PART_Popup, Type = (typeof(Popup)))]
public class MultiSelectComboBox : ListBox
{
    private const string PART_Popup = nameof(PART_Popup);

    private const string PART_SelectionListBox = nameof(PART_SelectionListBox);

    private Popup _popup;

    static MultiSelectComboBox()
    {
        SelectionModeProperty.OverrideMetadata(typeof(MultiSelectComboBox), new FrameworkPropertyMetadata(SelectionMode.Multiple));

        CommandManager.RegisterClassCommandBinding(typeof(MultiSelectComboBox),
              new CommandBinding(DeselectItemCommand, OnDeselectItemCommand, OnCanDeselectItemCommand));
    }

    public MultiSelectComboBox()
    {
        SelectionChanged += MultiSelectComboBox_SelectionChanged;
    }

    private void MultiSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var comboBox = (MultiSelectComboBox)sender;
        foreach (var item in e.RemovedItems)
        {
            BindableSelectItems?.Remove(item);
        }
        foreach (var item in e.AddedItems)
        {
            BindableSelectItems?.Add(item);
        }

        //在呈现容器添加子项以后，刷新popup的位置
        if (_popup != null)
        {
            _popup.UpdateLayout();
            var method = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(_popup, null);
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
        IsDropDownOpen = false;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _popup = GetTemplateChild(PART_Popup) as Popup;
    }

    #endregion Override

    #region MaxColumns

    public static readonly DependencyProperty MaxColumnsProperty =
        DependencyProperty.Register(nameof(MaxColumns), typeof(int), typeof(MultiSelectComboBox), new PropertyMetadata(5));

    public int MaxColumns
    {
        get => (int)GetValue(MaxColumnsProperty);
        set => SetValue(MaxColumnsProperty, value);
    }

    #endregion MaxColumns

    #region IsSelectAll

    public static readonly DependencyProperty IsSelectAllProperty =
                DependencyProperty.Register(nameof(IsSelectAll), typeof(bool), typeof(MultiSelectComboBox), new PropertyMetadata(default(bool), OnIsSelectAllChanged));

    public bool IsSelectAll
    {
        get => (bool)GetValue(IsSelectAllProperty);
        set => SetValue(IsSelectAllProperty, value);
    }

    private static void OnIsSelectAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var box = d as MultiSelectComboBox;
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

    #region BindableSelectItems

    public static readonly DependencyProperty BindableSelectItemsProperty =
        DependencyProperty.Register(nameof(BindableSelectItems), typeof(IList), typeof(MultiSelectComboBox), new PropertyMetadata(default(IList)));

    public IList BindableSelectItems
    {
        get => (IList)GetValue(BindableSelectItemsProperty);
        set => SetValue(BindableSelectItemsProperty, value);
    }

    #endregion BindableSelectItems

    #region IsDropDownOpen

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(MultiSelectComboBox), new PropertyMetadata(default(bool)));

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    #endregion IsDropDownOpen

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(MultiSelectComboBox), new PropertyMetadata(320d));

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    #endregion MaxDropDownHeight

    #region PresenteItemStyle

    public static readonly DependencyProperty PresenteItemStyleProperty =
        DependencyProperty.Register(nameof(PresenteItemStyle), typeof(Style), typeof(MultiSelectComboBox), new PropertyMetadata(default(Style)));

    public Style PresenteItemStyle
    {
        get => (Style)GetValue(PresenteItemStyleProperty);
        set => SetValue(PresenteItemStyleProperty, value);
    }

    #endregion PresenteItemStyle

    #region DeselectItemCommand

    public static RoutedCommand DeselectItemCommand { get; private set; } = new RoutedCommand("DeselectItem", typeof(MultiSelectComboBox));

    private static void OnCanDeselectItemCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private static void OnDeselectItemCommand(object sender, ExecutedRoutedEventArgs e)
    {
        var multiComboBox = (MultiSelectComboBox)sender;
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
}