using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class TimeSelectorItem : Control
{
    static TimeSelectorItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimeSelectorItem), new FrameworkPropertyMetadata(typeof(TimeSelectorItem)));
    }

    #region Value

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(int), typeof(TimeSelectorItem), new PropertyMetadata(default(int)));

    #endregion Value
}