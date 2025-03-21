using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class SeparatedUniformPanel : Panel
{
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(SeparatedUniformPanel),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public Orientation Orientation
    {
        get { return (Orientation)GetValue(OrientationProperty); }
        set { SetValue(OrientationProperty, value); }
    }

    public static readonly DependencyProperty SeparatorTemplateProperty =
        DependencyProperty.Register(nameof(SeparatorTemplate), typeof(DataTemplate), typeof(SeparatedUniformPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnSeparatorTemplateChanged));

    public DataTemplate SeparatorTemplate
    {
        get { return (DataTemplate)GetValue(SeparatorTemplateProperty); }
        set { SetValue(SeparatorTemplateProperty, value); }
    }

    private static void OnSeparatorTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SeparatedUniformPanel panel)
        {
            panel.InvalidateMeasure();
            panel.InvalidateVisual();
        }
    }

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(nameof(Separator), typeof(object), typeof(SeparatedUniformPanel),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnSeparatorChanged));

    public object Separator
    {
        get { return GetValue(SeparatorProperty); }
        set { SetValue(SeparatorProperty, value); }
    }

    private static void OnSeparatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SeparatedUniformPanel panel)
        {
            panel.InvalidateMeasure();
            panel.InvalidateVisual();
        }
    }

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(SeparatedUniformPanel),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double Spacing
    {
        get { return (double)GetValue(SpacingProperty); }
        set { SetValue(SpacingProperty, value); }
    }

    private readonly List<UIElement> _separators = new List<UIElement>();

    protected override Size MeasureOverride(Size availableSize)
    {
        Size totalSize = new Size();

        // 清除旧的分隔符以避免重复显示
        RemoveVisualSeparators();
        _separators.Clear();

        int itemCount = InternalChildren.Count;
        if (itemCount == 0)
            return totalSize;

        double maxItemHeight = 0;

        for (int i = 0; i < itemCount; i++)
        {
            UIElement child = InternalChildren[i];
            child.Measure(availableSize);

            if (Orientation == Orientation.Horizontal)
            {
                maxItemHeight = Math.Max(maxItemHeight, child.DesiredSize.Height);
            }
            else
            {
                totalSize.Width = Math.Max(totalSize.Width, child.DesiredSize.Width);
            }

            if (i < itemCount - 1) // Only add separators between items
            {
                FrameworkElement separator = CreateSeparator();
                if (separator != null)
                {
                    separator.Measure(availableSize);
                    _separators.Add(separator);
                    AddVisualChild(separator);

                    if (Orientation == Orientation.Horizontal)
                    {
                        maxItemHeight = Math.Max(maxItemHeight, separator.DesiredSize.Height);
                    }
                    else
                    {
                        totalSize.Width = Math.Max(totalSize.Width, separator.DesiredSize.Width);
                    }
                }
            }
        }

        if (Orientation == Orientation.Horizontal)
        {
            totalSize.Width = availableSize.Width;
            totalSize.Height = maxItemHeight;
        }
        else
        {
            totalSize.Height = availableSize.Height;
        }

        return totalSize;
    }


    protected override Size ArrangeOverride(Size finalSize)
    {
        int itemCount = InternalChildren.Count;
        if (itemCount == 0)
            return finalSize;

        double totalSeparatorWidth = _separators.Sum(s => s.DesiredSize.Width);
        double availableWidthForItems = finalSize.Width - totalSeparatorWidth - (Spacing * (itemCount - 1));
        double itemWidth = availableWidthForItems / itemCount;
        double offset = 0;

        for (int i = 0; i < itemCount; i++)
        {
            UIElement child = InternalChildren[i];

            if (Orientation == Orientation.Horizontal)
            {
                child.Arrange(new Rect(offset, 0, itemWidth, finalSize.Height));
                offset += itemWidth;
            }
            else
            {
                child.Arrange(new Rect(0, offset, finalSize.Width, child.DesiredSize.Height));
                offset += child.DesiredSize.Height;
            }

            if (i < _separators.Count)
            {
                UIElement separator = _separators[i];
                double separatorWidth = separator.DesiredSize.Width;

                if (Orientation == Orientation.Horizontal)
                {
                    separator.Arrange(new Rect(offset, 0, separatorWidth, finalSize.Height));
                    offset += separatorWidth + Spacing;
                }
                else
                {
                    separator.Arrange(new Rect(0, offset, finalSize.Width, separator.DesiredSize.Height));
                    offset += separator.DesiredSize.Height + Spacing;
                }
            }
        }

        return finalSize;
    }



    protected override int VisualChildrenCount => base.VisualChildrenCount + _separators.Count;

    protected override Visual GetVisualChild(int index)
    {
        if (index < base.VisualChildrenCount)
            return base.GetVisualChild(index);
        return _separators[index - base.VisualChildrenCount];
    }

    private FrameworkElement CreateSeparator()
    {
        FrameworkElement separator = null;
        if (SeparatorTemplate != null)
        {
            separator = SeparatorTemplate.LoadContent() as FrameworkElement;
            if (separator != null)
            {
                separator.DataContext = Separator;
            }
        }
        else if (Separator is string separatorText)
        {
            separator = new TextBlock
            {
                Text = separatorText,
            };
        }
        return separator;
    }

    private void RemoveVisualSeparators()
    {
        foreach (var separator in _separators)
        {
            RemoveVisualChild(separator);
        }
    }
}




