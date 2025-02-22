using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(SplitButtonItem))]
[TemplatePart(Name = PART_OpenButton, Type = typeof(ToggleButton))]
[TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
public class SplitButton : Selector
{
    private const string PART_OpenButton = nameof(PART_OpenButton);

    private const string PART_Popup = nameof(PART_Popup);

    private ToggleButton _openButton;

    private Popup _popup;

    #region Label

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(SplitButton), new PropertyMetadata(default(object)));

    public object Label
    {
        get => (object)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion Label

    #region LabelTemplate

    public static readonly DependencyProperty LabelTemplateProperty =
        DependencyProperty.Register(nameof(LabelTemplate), typeof(DataTemplate), typeof(SplitButton), new PropertyMetadata(default(DataTemplate)));

    public DataTemplate LabelTemplate
    {
        get => (DataTemplate)GetValue(LabelTemplateProperty);
        set => SetValue(LabelTemplateProperty, value);
    }

    #endregion LabelTemplate

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(SplitButton), new PropertyMetadata(default(bool)));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    #endregion IsOpen

    #region ItemClickCommand

    public static readonly DependencyProperty ItemClickCommandProperty =
        DependencyProperty.Register(
            nameof(ItemClickCommand),
            typeof(ICommand),
            typeof(SplitButton),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ItemClickCommandParameterProperty =
        DependencyProperty.Register(
            nameof(ItemClickCommandParameter),
            typeof(object),
            typeof(SplitButton),
            new FrameworkPropertyMetadata(null));

    public ICommand ItemClickCommand
    {
        get => (ICommand)GetValue(ItemClickCommandProperty);
        set => SetValue(ItemClickCommandProperty, value);
    }

    public object ItemClickCommandParameter
    {
        get => GetValue(ItemClickCommandParameterProperty);
        set => SetValue(ItemClickCommandParameterProperty, value);
    }

    #endregion ItemClickCommand

    #region Override方法

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new SplitButtonItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SplitButtonItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is ContentControl container && !string.IsNullOrEmpty(DisplayMemberPath))
        {
            var binding = new Binding(DisplayMemberPath);
            container.SetBinding(ContentControl.ContentProperty, binding);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _openButton = GetTemplateChild(PART_OpenButton) as ToggleButton;

        _popup = GetTemplateChild(PART_Popup) as Popup;

        if (_openButton != null && _popup != null)
        {
            _popup.Closed += (s, e) =>
            {
                Focus();
                _openButton.IsChecked = false;
            };
        }
    }

    #endregion Override方法
}