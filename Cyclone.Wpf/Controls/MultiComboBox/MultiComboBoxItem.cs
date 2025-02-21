using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Cyclone.Wpf.Controls;

public class MultiComboBoxItem : ListBoxItem
{
    #region HasCheckBox

    public static readonly DependencyProperty HasCheckBoxProperty =
        DependencyProperty.Register(nameof(HasCheckBox), typeof(bool), typeof(MultiComboBoxItem), new PropertyMetadata(true));

    public bool HasCheckBox
    {
        get => (bool)GetValue(HasCheckBoxProperty);
        set => SetValue(HasCheckBoxProperty, value);
    }

    #endregion HasCheckBox

    #region CheckBoxStyle

    public static readonly DependencyProperty CheckBoxStyleProperty =
        DependencyProperty.Register(nameof(CheckBoxStyle), typeof(Style), typeof(MultiComboBoxItem), new PropertyMetadata(default(Style)));

    public Style CheckBoxStyle
    {
        get => (Style)GetValue(CheckBoxStyleProperty);
        set => SetValue(CheckBoxStyleProperty, value);
    }

    #endregion CheckBoxStyle

    public MultiComboBoxItem()
    {
    }
}
