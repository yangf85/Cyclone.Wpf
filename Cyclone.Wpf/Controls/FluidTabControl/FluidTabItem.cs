using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Cyclone.Wpf.Controls;

public class FluidTabItem : HeaderedContentControl
{
    static FluidTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FluidTabItem), new FrameworkPropertyMetadata(typeof(FluidTabItem)));
    }

    #region IsPressd

    private static readonly DependencyPropertyKey IsPressedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsPressedPropertyKey), typeof(bool), typeof(FluidTabItem), new PropertyMetadata(default(bool)));

    public static readonly DependencyProperty IsPressdProperty = IsPressedPropertyKey.DependencyProperty;

    public string IsPressd
    {
        get => (string)GetValue(IsPressdProperty);
    }

    #endregion IsPressd

    #region Override

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        SetValue(IsPressedPropertyKey, true);
        CaptureMouse();
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        SetValue(IsPressedPropertyKey, false);
        ReleaseMouseCapture();
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        SetValue(IsPressedPropertyKey, false);
    }

    #endregion Override
}