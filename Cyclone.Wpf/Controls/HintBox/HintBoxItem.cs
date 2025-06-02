using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class HintBoxItem : ComboBoxItem
{
    static HintBoxItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HintBoxItem), new FrameworkPropertyMetadata(typeof(HintBoxItem)));
    }

    public HintBoxItem()
    {
    }

    #region Clicked

    public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent(
        "Clicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HintBoxItem));

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