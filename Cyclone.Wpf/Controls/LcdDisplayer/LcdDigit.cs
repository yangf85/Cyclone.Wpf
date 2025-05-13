using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;



public class LcdDigit : Control
{
    
    private Path _topSegment;
    private Path _topRightSegment;
    private Path _bottomRightSegment;
    private Path _bottomSegment;
    private Path _bottomLeftSegment;
    private Path _topLeftSegment;
    private Path _middleSegment;
    private Path dotSegment;
    static LcdDigit()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LcdDigit),
            new FrameworkPropertyMetadata(typeof(LcdDigit)));
    }


    #region character
    public char character
    {
        get => (char)GetValue(characterProperty);
        set => SetValue(characterProperty, value);
    }

    public static readonly DependencyProperty characterProperty =
        DependencyProperty.Register(nameof(character), typeof(char), typeof(LcdDigit), new PropertyMetadata(default(char)));

    #endregion

    #region ActiveColor
    public static readonly DependencyProperty ActiveColorProperty =
        DependencyProperty.Register("ActiveColor", typeof(Brush), typeof(LcdDigit),
            new PropertyMetadata(Brushes.Red));

    public Brush ActiveColor
    {
        get { return (Brush)GetValue(ActiveColorProperty); }
        set { SetValue(ActiveColorProperty, value); }
    }
    #endregion
    #region InactiveColor
    public static readonly DependencyProperty InactiveColorProperty =
        DependencyProperty.Register("InactiveColor", typeof(Brush), typeof(LcdDigit),
            new PropertyMetadata(Brushes.DarkGray));

    public Brush InactiveColor
    {
        get { return (Brush)GetValue(InactiveColorProperty); }
        set { SetValue(InactiveColorProperty, value); }
    }
    #endregion

    #region SegmentThickness
  
    public static readonly DependencyProperty SegmentThicknessProperty =
        DependencyProperty.Register("SegmentThickness", typeof(double), typeof(LcdDigit),
            new PropertyMetadata(1.0));

    public double SegmentThickness
    {
        get { return (double)GetValue(SegmentThicknessProperty); }
        set { SetValue(SegmentThicknessProperty, value); }
    }

    #endregion



   

    

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

      
       
    }

   
   



    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

      
    }
}