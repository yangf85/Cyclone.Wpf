using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Cyclone.Wpf.Helpers;

public static class ScrollViewerHelper
{
    public static readonly DependencyProperty VerticalOffsetProperty =
        DependencyProperty.RegisterAttached("VerticalOffset", typeof(double),
            typeof(ScrollViewerHelper), new PropertyMetadata(0.0, OnVerticalOffsetChanged));

    private static void OnVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ScrollViewer scrollViewer)
            scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
    }

    public static double GetVerticalOffset(DependencyObject obj)
            => (double)obj.GetValue(VerticalOffsetProperty);

    public static void SetVerticalOffset(DependencyObject obj, double value)
        => obj.SetValue(VerticalOffsetProperty, value);
}