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

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(TreeSelectItem))]
[TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
[TemplatePart(Name = "PART_Menu", Type = typeof(Menu))]
public class TreeSelect : Selector
{
    private const string PART_EditableTextBox = "PART_EditableTextBox";

    private const string PART_Popup = "PART_Popup";

    private const string PART_Menu = "PART_Menu";

    private TextBox _textBox;

    private Popup _popup;

    private Menu _menu;

    #region Separator

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(nameof(Separator), typeof(string), typeof(TreeSelect), new PropertyMetadata("/"));

    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    #endregion Separator

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _textBox = GetTemplateChild(PART_EditableTextBox) as TextBox;

        _popup = GetTemplateChild(PART_Popup) as Popup;
        _menu = GetTemplateChild(PART_Menu) as Menu;
    }

    #endregion Override
}