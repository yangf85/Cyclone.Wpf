using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class SideMenu:ItemsControl
{

    static SideMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenu), new FrameworkPropertyMetadata(typeof(SideMenu)));
    }
   

    #region Indent
    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

    public static readonly DependencyProperty IndentProperty =
        DependencyProperty.Register(nameof(Indent), typeof(double), typeof(SideMenu), new PropertyMetadata(20d));

    #endregion

    #region IsExpanded
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(SideMenu), new PropertyMetadata(default(bool)));

    #endregion

    #region CollapseWidth
    public double CollapseWidth
    {
        get => (double)GetValue(CollapseWidthProperty);
        set => SetValue(CollapseWidthProperty, value);
    }

    public static readonly DependencyProperty CollapseWidthProperty =
        DependencyProperty.Register(nameof(CollapseWidth), typeof(double), typeof(SideMenu), new PropertyMetadata(default(double)));

    #endregion


    #region ExpansionWidth
    public double ExpansionWidth
    {
        get => (double)GetValue(ExpansionWidthProperty);
        set => SetValue(ExpansionWidthProperty, value);
    }

    public static readonly DependencyProperty ExpansionWidthProperty =
        DependencyProperty.Register(nameof(ExpansionWidth), typeof(double), typeof(SideMenu), new PropertyMetadata(default(double)));

    #endregion

    #region Header
    public object Header
    {
        get => (object)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SideMenu), new PropertyMetadata(default(object)));

    #endregion


    #region Footer
    public object Footer
    {
        get => (object)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(SideMenu), new PropertyMetadata(default(object)));

    #endregion
    #region Override

   
    protected override DependencyObject GetContainerForItemOverride()
    {
        return new SideMenuItem();
         
    }
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SideMenuItem;
    }
    #endregion
}
