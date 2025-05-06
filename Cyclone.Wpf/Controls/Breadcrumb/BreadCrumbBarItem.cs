using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class BreadCrumbBarItem : ListBoxItem
{
    static BreadCrumbBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadCrumbBarItem), new FrameworkPropertyMetadata(typeof(BreadCrumbBarItem)));
    }

    #region Icon

    public object Icon
    {
        get => (object)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(BreadCrumbBarItem), new PropertyMetadata(default(object)));

    #endregion Icon

    #region Indicator

    public object Indicator
    {
        get => (object)GetValue(IndicatorProperty);
        set => SetValue(IndicatorProperty, value);
    }

    public static readonly DependencyProperty IndicatorProperty =
        DependencyProperty.Register(nameof(Indicator), typeof(object), typeof(BreadCrumbBarItem), new PropertyMetadata(default(object)));

    #endregion Indicator

    #region IndicatorTemplate

    public DataTemplate IndicatorTemplate
    {
        get => (DataTemplate)GetValue(IndicatorTemplateProperty);
        set => SetValue(IndicatorTemplateProperty, value);
    }

    public static readonly DependencyProperty IndicatorTemplateProperty =
        DependencyProperty.Register(nameof(IndicatorTemplate), typeof(DataTemplate), typeof(BreadCrumbBarItem), new PropertyMetadata(default(DataTemplate)));

    #endregion IndicatorTemplate

    #region IsFirst

    public bool IsFirst
    {
        get => (bool)GetValue(IsFirstProperty);
        internal set => SetValue(IsFirstPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsFirstPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsFirst), typeof(bool), typeof(BreadCrumbBarItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsFirstProperty = IsFirstPropertyKey.DependencyProperty;

    #endregion IsFirst

    #region IsLast

    public bool IsLast
    {
        get => (bool)GetValue(IsLastProperty);
        internal set => SetValue(IsLastPropertyKey, value);
    }

    private static readonly DependencyPropertyKey IsLastPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLast), typeof(bool), typeof(BreadCrumbBarItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsLastProperty = IsLastPropertyKey.DependencyProperty;

    #endregion IsLast
}