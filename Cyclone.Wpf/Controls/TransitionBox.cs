using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

public interface ITransitionAnimation
{
    void BeginAnimation(UIElement element);
    void EndAnimation(UIElement element);
}


public class TransitionBox:ContentControl
{

    #region TransitionAnimation
    public ITransitionAnimation TransitionAnimation
    {
        get => (ITransitionAnimation)GetValue(TransitionAnimationProperty);
        set => SetValue(TransitionAnimationProperty, value);
    }

    public static readonly DependencyProperty TransitionAnimationProperty =
        DependencyProperty.Register(nameof(TransitionAnimation), typeof(ITransitionAnimation), typeof(TransitionBox), new PropertyMetadata(default(ITransitionAnimation)));

    #endregion
}
