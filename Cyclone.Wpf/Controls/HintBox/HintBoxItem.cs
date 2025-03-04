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

public class HintBoxItem : ComboBoxItem,IHintText
{
    private HintBox _ParentHintBox => ItemsControl.ItemsControlFromItemContainer(this) as HintBox;


    #region Override

  
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
      
    }
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        _ParentHintBox?.NotifyHintBoxItemMouseLeftButtonDown(this);
        RaiseEvent(new RoutedEventArgs(ClickedEvent, this));
        e.Handled = true;
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        _ParentHintBox?.NotifyHintBoxItemMouseEnter(this);
        e.Handled = true;
    }

    internal void SetHighlight(bool highlight)
    {
        IsHighlighted = highlight;
    }
    #endregion

    #region HintText
    public string HintText
    {
        get => (string)GetValue(HintTextProperty);
        set => SetValue(HintTextProperty, value);
    }
  

    public static readonly DependencyProperty HintTextProperty =
        DependencyProperty.Register(nameof(HintText), typeof(string), typeof(HintBoxItem), new PropertyMetadata(default(string)));

    #endregion

    #region Clicked
    public static readonly RoutedEvent ClickedEvent = EventManager.RegisterRoutedEvent("Clicked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HintBoxItem));
    public event RoutedEventHandler Clicked
    {
        add { AddHandler(ClickedEvent, value); }
        remove { RemoveHandler(ClickedEvent, value); }
    }

    protected virtual void OnClicked(RoutedEventArgs e)
    {
        RaiseEvent(e);
    }

    #endregion
}