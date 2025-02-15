using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

public interface ILoadingAnimation
{
    void Start(UIElement parent);
    void Stop(UIElement parent);
}

[TemplatePart(Name = PART_Overlay, Type = typeof(Rectangle))]
public class LoadingBox:ContentControl
{
    const string PART_Overlay=nameof(PART_Overlay);
    #region OverlayBrush
    public Brush OverlayBrush
    {
        get => (Brush)GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    public static readonly DependencyProperty OverlayBrushProperty =
        DependencyProperty.Register(nameof(OverlayBrush), typeof(Brush), typeof(LoadingBox), new PropertyMetadata(default(Brush)));

    #endregion

    #region IsLoading
    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingBox), new PropertyMetadata(default(bool)));

    #endregion

    //应该使用一个集合列表
    #region LaodingAnimation
    public ILoadingAnimation LaodingAnimation
    {
        get => (ILoadingAnimation)GetValue(LaodingAnimationProperty);
        set => SetValue(LaodingAnimationProperty, value);
    }

    public static readonly DependencyProperty LaodingAnimationProperty =
        DependencyProperty.Register(nameof(LaodingAnimation), typeof(ILoadingAnimation), typeof(LoadingBox), new PropertyMetadata(default(ILoadingAnimation)));

    #endregion

}
