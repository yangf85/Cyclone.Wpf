using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class Card : ContentControl
{
    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
    }

    #region Title

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(Card), new PropertyMetadata(default(string)));

    #endregion Title

    #region Icon

    public object Icon
    {
        get => (object)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(Card), new PropertyMetadata(default(object)));

    #endregion Icon

    #region HeaderBackground

    public Brush HeaderBackground
    {
        get => (Brush)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public static readonly DependencyProperty HeaderBackgroundProperty =
        DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(Card), new PropertyMetadata(default(Brush)));

    #endregion HeaderBackground

    #region HeaderForeground

    public Brush HeaderForeground
    {
        get => (Brush)GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }

    public static readonly DependencyProperty HeaderForegroundProperty =
        DependencyProperty.Register(nameof(HeaderForeground), typeof(Brush), typeof(Card), new PropertyMetadata(default(Brush)));

    #endregion HeaderForeground

    #region Footer

    public object Footer
    {
        get => (object)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(Card), new PropertyMetadata(default(object)));

    #endregion Footer

    #region FooterBackground

    public Brush FooterBackground
    {
        get => (Brush)GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
    }

    public static readonly DependencyProperty FooterBackgroundProperty =
        DependencyProperty.Register(nameof(FooterBackground), typeof(Brush), typeof(Card), new PropertyMetadata(default(Brush)));

    #endregion FooterBackground

    #region FooterForeground

    public Brush FooterForeground
    {
        get => (Brush)GetValue(FooterForegroundProperty);
        set => SetValue(FooterForegroundProperty, value);
    }

    public static readonly DependencyProperty FooterForegroundProperty =
        DependencyProperty.Register(nameof(FooterForeground), typeof(Brush), typeof(Card), new PropertyMetadata(default(Brush)));

    #endregion FooterForeground
}