using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

public class Switch:ToggleButton
{
    static Switch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Switch), new FrameworkPropertyMetadata(typeof(Switch)));
    }


    #region TrackWidth
    public double TrackWidth
    {
        get => (double)GetValue(TrackWidthProperty);
        set => SetValue(TrackWidthProperty, value);
    }

    public static readonly DependencyProperty TrackWidthProperty =
        DependencyProperty.Register(nameof(TrackWidth), typeof(double), typeof(Switch), new PropertyMetadata(50d));

    #endregion
}
