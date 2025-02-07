using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Helpers;

public class PanelHelper : DependencyObject
{

    #region ElementMargin

    public static Thickness GetElementMargin(DependencyObject obj) => (Thickness)obj.GetValue(ElementMarginProperty);

    public static void SetElementMargin(DependencyObject obj, Thickness value) => obj.SetValue(ElementMarginProperty, value);

    public static readonly DependencyProperty ElementMarginProperty =
                DependencyProperty.RegisterAttached("ElementMargin", typeof(Thickness), typeof(PanelHelper), new PropertyMetadata(0d, OnElementMarginChanged));

    private static void OnElementMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Panel panel)
        {
            double newValue = (double)e.NewValue;
            double oldValue = (double)e.OldValue;
            if (panel.IsLoaded)
            {
                UpdateChildrenMargin(panel, newValue);
            }
            else
            {
                panel.Loaded -= OnPanelLoaded;
                panel.Loaded += OnPanelLoaded;
                panel.SetValue(ElementMarginProperty, newValue);
            }
        }
    }

    private static void OnPanelLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is Panel panel)
        {
            double margin = (double)panel.GetValue(ElementMarginProperty);
            UpdateChildrenMargin(panel, margin);
            panel.Loaded -= OnPanelLoaded;
        }
    }

    private static void UpdateChildrenMargin(Panel panel, double margin)
    {
        foreach (UIElement child in panel.Children)
        {
            if (child is FrameworkElement frameworkElement)
            {
                frameworkElement.Margin = new Thickness(margin);
            }
        }
    }
    #endregion


   
   
}
