using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

[ContentProperty(nameof(Title))]
public class FormSeperater : Control
{
    #region Title

    public object Title
    {
        get => (object)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(object), typeof(FormSeperater), new PropertyMetadata(default(object)));

    #endregion Title

    #region Description

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(FormSeperater), new PropertyMetadata(default(string)));

    #endregion Description

    #region DescriptionHorizontalAlignment

    public HorizontalAlignment DescriptionHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(DescriptionHorizontalAlignmentProperty);
        set => SetValue(DescriptionHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty DescriptionHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(DescriptionHorizontalAlignment), typeof(HorizontalAlignment), typeof(FormSeperater),
        new PropertyMetadata(default));

    #endregion DescriptionHorizontalAlignment

    #region SeperaterBrush

    public Brush SeperaterBrush
    {
        get => (Brush)GetValue(SeperaterBrushProperty);
        set => SetValue(SeperaterBrushProperty, value);
    }

    public static readonly DependencyProperty SeperaterBrushProperty =
        DependencyProperty.Register(nameof(SeperaterBrush), typeof(Brush), typeof(FormSeperater), new PropertyMetadata(default(Brush)));

    #endregion SeperaterBrush

    #region SeperaterThickness

    public double SeperaterThickness
    {
        get => (double)GetValue(SeperaterThicknessProperty);
        set => SetValue(SeperaterThicknessProperty, value);
    }

    public static readonly DependencyProperty SeperaterThicknessProperty =
        DependencyProperty.Register(nameof(SeperaterThickness), typeof(double), typeof(FormSeperater), new PropertyMetadata(default));

    #endregion SeperaterThickness
}