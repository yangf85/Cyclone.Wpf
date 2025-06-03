using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

public class HintBoxItem : ComboBoxItem
{
    static HintBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HintBoxItem),
            new FrameworkPropertyMetadata(typeof(HintBoxItem)));
    }
}