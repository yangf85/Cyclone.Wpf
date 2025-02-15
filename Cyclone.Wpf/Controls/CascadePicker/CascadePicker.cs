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
[TemplatePart(Name = "PART_DisplayedTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class CascadePicker : Selector
{
    private const string PART_DisplayedTextBox = "PART_DisplayedTextBox";
    private const string PART_Popup = "PART_Popup";

    private TextBox _textBox;
    private Popup _popup;
    static CascadePicker()
    {
        
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePicker), new FrameworkPropertyMetadata(typeof(CascadePicker)));
    }

    public CascadePicker()
    {
        AddHandler(CascadePickerItem.ItemClickEvent, new RoutedEventHandler(Item_Click));

    }

    #region Item_Click
    private void Item_Click(object sender, RoutedEventArgs e)
    {
        if (e.Source is CascadePickerItem item)
        {
            SetValue(DisplayedNodePathProperty, GetDisplayedPath(item));
          
            SetValue(SelectedItemProperty, item);
            RaiseEvent(new RoutedEventArgs(CascadePicker.SelectedChangedEvent));
            if (!item.HasItems)
            {
                SetValue(IsOpenedProperty, false);
            }
        }
        
    }

    private string GetDisplayedPath(CascadePickerItem item)
    {
        return IsShowFullPath ? GetFullPath(item) : item.Header?.ToString() ?? string.Empty;
    }

    private string GetFullPath(CascadePickerItem item)
    {
        var pathList = new List<string>();
        var currentItem = item;


        while (currentItem != null)
        {
            pathList.Insert(0, currentItem.NodePath);
            currentItem = currentItem.Parent as CascadePickerItem;
        }

        return string.Join(Separator.ToString(), pathList);
    }
    #endregion



    #region Watermark
    public string Watermark
    {
        get => (string)GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }

    public static readonly DependencyProperty WatermarkProperty =
        DependencyProperty.Register(nameof(Watermark), typeof(string), typeof(CascadePicker), new PropertyMetadata(string.Empty));

    #endregion



    #region DisplayedNodePath
    public string DisplayedNodePath
    {
        get => (string)GetValue(DisplayedNodePathProperty);
        set => SetValue(DisplayedNodePathProperty, value);
    }

    public static readonly DependencyProperty DisplayedNodePathProperty =
        DependencyProperty.Register(nameof(DisplayedNodePath), typeof(string), typeof(CascadePicker), new FrameworkPropertyMetadata(string.Empty,FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    #endregion


    #region SelectedItem
    public object SelectedItem
    {
        get => (object)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(CascadePicker), new PropertyMetadata(default(object)));

    #endregion

    #region SelectedChanged
    public event RoutedEventHandler SelectedChanged
    {
        add => AddHandler(SelectedChangedEvent, value);
        remove => RemoveHandler(SelectedChangedEvent, value);
    }


    public static readonly RoutedEvent SelectedChangedEvent = EventManager.RegisterRoutedEvent("SelectedChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CascadePicker));
    #endregion

    #region IsOpened
    public bool IsOpened
    {
        get => (bool)GetValue(IsOpenedProperty);
        set => SetValue(IsOpenedProperty, value);
    }

    public static readonly DependencyProperty IsOpenedProperty =
        DependencyProperty.Register(nameof(IsOpened), typeof(bool), typeof(CascadePicker), new PropertyMetadata(default(bool)));
    #endregion

    #region IsShowFullPath
    public bool IsShowFullPath
    {
        get => (bool)GetValue(IsShowFullPathProperty);
        set => SetValue(IsShowFullPathProperty, value);
    }

    public static readonly DependencyProperty IsShowFullPathProperty =
        DependencyProperty.Register(nameof(IsShowFullPath), typeof(bool), typeof(CascadePicker), new PropertyMetadata(default(bool)));
    #endregion


    #region Separator
    public object Separator
    {
        get => (object)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(nameof(Separator), typeof(object), typeof(CascadePicker), new PropertyMetadata('/'));

    #endregion



    #region SeparatorTemplate
    public DataTemplate SeparatorTemplate
    {
        get => (DataTemplate)GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }

    public static readonly DependencyProperty SeparatorTemplateProperty =
        DependencyProperty.Register(nameof(SeparatorTemplate), typeof(DataTemplate), typeof(CascadePicker), new PropertyMetadata(default(DataTemplate)));

    #endregion

    #region Override
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is CascadePickerItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new CascadePickerItem();
    }


    #endregion


}
