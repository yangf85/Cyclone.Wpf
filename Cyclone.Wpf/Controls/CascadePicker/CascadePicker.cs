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
public class CascadePicker : Selector
{
    private const string PART_DisplayedTextBox = "PART_DisplayedTextBox";

    private const string PART_ItemsPopup = "PART_ItemsPopup";

    private TextBox _textBox;

    private Popup _popup;

    static CascadePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePicker), new FrameworkPropertyMetadata(typeof(CascadePicker)));
    }

    public CascadePicker()
    {
        Loaded += CascadePicker_Loaded;
        Unloaded += CascadePicker_Unloaded;
        LostFocus += CascadePicker_LostFocus;
    }

    private void CascadePicker_LostFocus(object sender, RoutedEventArgs e)
    {
        SetValue(IsOpenedProperty, false);
    }

    private void CascadePicker_Unloaded(object sender, RoutedEventArgs e)
    {
        RemoveHandler(CascadePickerItem.ItemClickEvent, new RoutedEventHandler(Item_Click));
    }

    private void CascadePicker_Loaded(object sender, RoutedEventArgs e)
    {
        AddHandler(CascadePickerItem.ItemClickEvent, new RoutedEventHandler(Item_Click));
    }

    #region IsReadOnly

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(CascadePicker), new PropertyMetadata(default(bool)));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion IsReadOnly

    #region Item_Click

    private void Item_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is CascadePickerItem item)
        {
            SetValue(TextProperty, GetSelectedPath(item));
            SetCurrentValue(SelectedItemProperty, item.DataContext);
            RaiseEvent(new RoutedEventArgs(CascadePicker.SelectedChangedEvent));
            if (!item.HasItems)
            {
                SetValue(IsOpenedProperty, false);
            }
        }
    }

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
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(CascadePicker), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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
        DependencyProperty.Register(nameof(IsOpened), typeof(bool), typeof(CascadePicker), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsOpened
    {
        get => (bool)GetValue(IsOpenedProperty);
        set => SetValue(IsOpenedProperty, value);
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
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _textBox = GetTemplateChild(PART_DisplayedTextBox) as TextBox;

        _popup = GetTemplateChild(PART_ItemsPopup) as Popup;
    }

    #endregion Override
}