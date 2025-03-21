using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Cyclone.Wpf.Controls;

public class RadioButtonGroup : ItemsControl
{
    static RadioButtonGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RadioButtonGroup), new FrameworkPropertyMetadata(typeof(RadioButtonGroup)));
    }

    public RadioButtonGroup()
    {
        Loaded += RadioButtonGroup_Loaded;
        Unloaded += RadioButtonGroup_Unloaded;
    }

    #region Private

    private void RadioButtonGroup_Unloaded(object sender, RoutedEventArgs e)
    {
        RemoveHandler(RadioButton.CheckedEvent, new RoutedEventHandler(RadioButton_Clicked));
    }

    private void RadioButtonGroup_Loaded(object sender, RoutedEventArgs e)
    {
        AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(RadioButton_Clicked));
    }

    private void RadioButton_Clicked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is RadioButton radioButton)
        {
            SelectedItem = radioButton.DataContext ?? radioButton.Content;
        }
    }

    #endregion Private

    #region SelectedItem

    public object SelectedItem
    {
        get => (object)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(RadioButtonGroup),
            new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion SelectedItem

    #region GroupName

    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(RadioButtonGroup), new PropertyMetadata(nameof(RadioButtonGroup)));

    #endregion GroupName

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is RadioButton;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new RadioButton() { GroupName = GroupName };
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
    }
}