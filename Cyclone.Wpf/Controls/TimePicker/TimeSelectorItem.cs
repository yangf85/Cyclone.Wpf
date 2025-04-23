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

    #region IsSelected

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(TimeSelectorItem), new PropertyMetadata(default(bool)));

    #endregion IsSelected
}