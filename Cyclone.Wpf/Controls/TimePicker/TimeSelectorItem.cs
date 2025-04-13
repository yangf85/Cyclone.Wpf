using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class TimeSelectorItem : ListBoxItem
{
    static TimeSelectorItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSelectorItem), new FrameworkPropertyMetadata(typeof(TimeSelectorItem)));
    }

    #region Value Property

    public int Value
    {
        get { return (int)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(int), typeof(TimeSelectorItem),
            new PropertyMetadata(-1));

    #endregion Value Property

    #region DisplayText Property

    public string DisplayText
    {
        get { return (string)GetValue(DisplayTextProperty); }
        set { SetValue(DisplayTextProperty, value); }
    }

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register("DisplayText", typeof(string), typeof(TimeSelectorItem),
            new PropertyMetadata(string.Empty));

    #endregion DisplayText Property
}