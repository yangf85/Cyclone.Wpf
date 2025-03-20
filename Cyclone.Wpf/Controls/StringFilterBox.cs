using Cyclone.Wpf.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

[TypeConverter(typeof(EnumAttributeTypeConverter<DescriptionAttribute>))]
public enum StringOperator
{
    [Description("Equal")]
    Equal,

    [Description("NotEqual")]
    NotEqual,

    [Description("Contains")]
    Contains,

    [Description("NotContains")]
    NotContains,

    [Description("StartsWith")]
    StartsWith,

    [Description("EndsWith")]
    EndsWith,

    [Description("Regex")]
    Regex
}

/// <summary>
/// 一个用于文本过滤的控件 ，比如包含，不包含等等
/// </summary>
[TemplatePart(Name = PART_ActivedCheckBox, Type = typeof(CheckBox))]
[TemplatePart(Name = PART_OperatorComboBox, Type = typeof(ComboBox))]
[TemplatePart(Name = PART_InputTextBox, Type = typeof(TextBox))]
public class StringFilterBox : Control
{
    private const string PART_ActivedCheckBox = nameof(PART_ActivedCheckBox);
    private const string PART_InputTextBox = nameof(PART_InputTextBox);
    private const string PART_OperatorComboBox = nameof(PART_OperatorComboBox);

    #region Label

    public static readonly DependencyProperty LabelProperty =
               FormItem.LabelProperty.AddOwner(typeof(StringFilterBox), new PropertyMetadata(default, OnLabelChanged));

    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion Label

    #region IsActived

    public static readonly DependencyProperty IsActivedProperty =
        DependencyProperty.Register(nameof(IsActived), typeof(bool), typeof(StringFilterBox), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsActived
    {
        get => (bool)GetValue(IsActivedProperty);
        set => SetValue(IsActivedProperty, value);
    }

    #endregion IsActived

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StringFilterBox), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion Text

    #region SharedName

    public static readonly DependencyProperty SharedNameProperty =
                       FormItem.SharedNameProperty.AddOwner(typeof(StringFilterBox), new PropertyMetadata(default(string), OnSharedNameChanged));

    public string SharedName
    {
        get => (string)GetValue(SharedNameProperty);
        set => SetValue(SharedNameProperty, value);
    }

    private static void OnSharedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var filterBox = d as StringFilterBox;
        if (filterBox != null)
        {
            if (!string.IsNullOrEmpty(e.NewValue?.ToString()))
            {
                filterBox.Loaded += StringFilterBox_Loaded;
            }
            else
            {
                filterBox.Loaded -= StringFilterBox_Loaded;
            }
        }
    }

    private static void StringFilterBox_Loaded(object sender, RoutedEventArgs e)
    {
        var filterBox = sender as StringFilterBox;
        if (filterBox != null && filterBox.Parent is Panel panel)
        {
            panel.SetValue(Grid.IsSharedSizeScopeProperty, true);
        }
    }

    #endregion SharedName

    #region Operator

    public static readonly DependencyProperty OperatorProperty =
                        DependencyProperty.Register(nameof(Operator), typeof(StringOperator), typeof(StringFilterBox), new FrameworkPropertyMetadata(default(StringOperator), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public StringOperator Operator
    {
        get => (StringOperator)GetValue(OperatorProperty);
        set => SetValue(OperatorProperty, value);
    }

    #endregion Operator

    #region ExtendObject

    public static readonly DependencyProperty ExtendObjectProperty =
        DependencyProperty.Register(nameof(ExtendObject), typeof(object), typeof(StringFilterBox), new PropertyMetadata(default(object)));

    public object ExtendObject
    {
        get => GetValue(ExtendObjectProperty);
        set => SetValue(ExtendObjectProperty, value);
    }

    #endregion ExtendObject
}