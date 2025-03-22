using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = "PART_TextBlock", Type = typeof(TextBlock))]
public class CopyableTextBlock : Control
{
    static CopyableTextBlock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CopyableTextBlock), new FrameworkPropertyMetadata(typeof(CopyableTextBlock)));
    }

    #region Text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(CopyableTextBlock),
            new FrameworkPropertyMetadata(string.Empty)
        );

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    #endregion Text

    #region IsCopyed

    public bool IsCopyed
    {
        get => (bool)GetValue(IsCopyedProperty);
    }

    private static readonly DependencyPropertyKey IsCopyedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCopyed), typeof(bool), typeof(CopyableTextBlock), new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsCopyedProperty = IsCopyedPropertyKey.DependencyProperty;

    #endregion IsCopyed

    #region Override

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        System.Windows.Clipboard.SetText(Text);
        SetValue(IsCopyedPropertyKey, true);
        Focus();
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
        base.OnLostFocus(e);
        SetValue(IsCopyedPropertyKey, false);
    }

    #endregion Override
}