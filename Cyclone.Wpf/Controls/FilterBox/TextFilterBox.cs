using Cyclone.Wpf.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

[TypeConverter(typeof(EnumDescriptionTypeConverter))]
public enum TextOperator
{
    [Description("=")]           // Equal
    Equal,

    [Description("≠")]           // NotEqual
    NotEqual,

    [Description("∋")]           // Contains
    Contains,

    [Description("∌")]          // NotContains
    NotContains,

    [Description("^")]           // StartsWith
    StartsWith,

    [Description("$")]           // EndsWith
    EndsWith,

    [Description("*")]      // Regex（无法用单符号，但可缩短为"R"）
    Regex
}

/// <summary>
/// 一个用于文本过滤的控件 ，比如包含，不包含等等
/// </summary>
[TemplatePart(Name = PART_ActivedCheckBox, Type = typeof(CheckBox))]
[TemplatePart(Name = PART_OperatorComboBox, Type = typeof(ComboBox))]
[TemplatePart(Name = PART_InputTextBox, Type = typeof(TextBox))]
public class TextFilterBox : Control
{
    private const string PART_ActivedCheckBox = nameof(PART_ActivedCheckBox);
    private const string PART_InputTextBox = nameof(PART_InputTextBox);
    private const string PART_OperatorComboBox = nameof(PART_OperatorComboBox);

    #region Label

    public static readonly DependencyProperty LabelProperty =
               FormItem.LabelProperty.AddOwner(typeof(TextFilterBox), new PropertyMetadata(default, OnLabelChanged));

    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion Label

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(TextFilterBox), new PropertyMetadata(default(object)));

    public object Description
    {
        get => (object)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion Description

    #region IsActived

    public static readonly DependencyProperty IsActivedProperty =
        DependencyProperty.Register(nameof(IsActived), typeof(bool), typeof(TextFilterBox), new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsActived
    {
        get => (bool)GetValue(IsActivedProperty);
        set => SetValue(IsActivedProperty, value);
    }

    #endregion IsActived

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextFilterBox), new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion Text

    #region SharedName

    public static readonly DependencyProperty SharedNameProperty =
                       FormItem.SharedNameProperty.AddOwner(typeof(TextFilterBox), new PropertyMetadata(default(string)));

    public string SharedName
    {
        get => (string)GetValue(SharedNameProperty);
        set => SetValue(SharedNameProperty, value);
    }

    #endregion SharedName

    #region Operator

    public static readonly DependencyProperty OperatorProperty =
                        DependencyProperty.Register(nameof(Operator), typeof(TextOperator), typeof(TextFilterBox), new FrameworkPropertyMetadata(default(TextOperator), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public TextOperator Operator
    {
        get => (TextOperator)GetValue(OperatorProperty);
        set => SetValue(OperatorProperty, value);
    }

    #endregion Operator

    #region ExtendObject

    public static readonly DependencyProperty ExtendObjectProperty =
        DependencyProperty.Register(nameof(ExtendObject), typeof(object), typeof(TextFilterBox), new PropertyMetadata(default(object)));

    public object ExtendObject
    {
        get => GetValue(ExtendObjectProperty);
        set => SetValue(ExtendObjectProperty, value);
    }

    #endregion ExtendObject
}