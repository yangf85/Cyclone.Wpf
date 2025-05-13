using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;


public class LcdDisplayer : ItemsControl
{
    static LcdDisplayer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LcdDisplayer),
            new FrameworkPropertyMetadata(typeof(LcdDisplayer)));
    }


    #region Text
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(LcdDisplayer), new PropertyMetadata(default(string)));

    #endregion


    #region ActiveColor

    public static readonly DependencyProperty ActiveColorProperty =
        DependencyProperty.Register("ActiveColor", typeof(Brush), typeof(LcdDisplayer),
            new PropertyMetadata(null));

    public Brush ActiveColor
    {
        get { return (Brush)GetValue(ActiveColorProperty); }
        set { SetValue(ActiveColorProperty, value); }
    }

    #endregion
    #region InactiveColor
    public static readonly DependencyProperty InactiveColorProperty =
        DependencyProperty.Register("InactiveColor", typeof(Brush), typeof(LcdDisplayer),
            new PropertyMetadata(null));

    public Brush InactiveColor
    {
        get { return (Brush)GetValue(InactiveColorProperty); }
        set { SetValue(InactiveColorProperty, value); }
    }

    #endregion
  

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateDigits();
    }

    private void UpdateDigits()
    {
        
    }
}