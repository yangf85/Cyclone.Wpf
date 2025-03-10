using System;
using System.Collections.Generic;
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

public class HintBoxItem : ComboBoxItem, IHintable
{
    static HintBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HintBoxItem), new FrameworkPropertyMetadata(typeof(HintBoxItem)));
        
    }

    public HintBoxItem()
    {
       
    }

    #region HintText

    public static readonly DependencyProperty HintTextProperty =
        DependencyProperty.Register(nameof(HintText), typeof(string), typeof(HintBoxItem), new PropertyMetadata(default(string), OnHintTextChanged));

    private static void OnHintTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var item = (HintBoxItem)d;
        item.Content ??= e.NewValue;
    }

    public string HintText
    {
        get => (string)GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }

    #endregion HintText

    #region Clicked

    public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent("Clicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HintBoxItem));

    protected virtual void OnClicked(RoutedEventArgs e)
    {
        RaiseEvent(e);
    }

    public event RoutedEventHandler Clicked
    {
        add { AddHandler(ClickedEvent, value); }
        remove { RemoveHandler(ClickedEvent, value); }
    }

    #endregion Clicked


    #region Override

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
     
        base.OnMouseLeftButtonDown(e);
        OnClicked(new RoutedEventArgs(ClickedEvent));
    }

    

    #endregion Override

    internal void ChangeHighlightState(bool isHighlight)
    {
        IsHighlighted = isHighlight;
    }

    
}