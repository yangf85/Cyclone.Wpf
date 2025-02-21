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
    static MultiComboBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBoxItem), new FrameworkPropertyMetadata(typeof(MultiComboBoxItem)));
    }
    public MultiComboBoxItem()
    {
    }
}
