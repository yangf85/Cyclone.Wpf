using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 缩放过渡效果
/// </summary>
public class ScaleTransition : ITransition
{
    public void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration)
    {
        oldContent.Visibility = Visibility.Visible;
        oldContent.Opacity = 1.0;

        newContent.Visibility = Visibility.Hidden;
        newContent.Opacity = 1.0;

        oldContent.RenderTransformOrigin = new Point(0.5, 0.5);
        newContent.RenderTransformOrigin = new Point(0.5, 0.5);

        ScaleTransform oldScaleTransform = new ScaleTransform(1, 1);
        ScaleTransform newScaleTransform = new ScaleTransform(0, 0);

        oldContent.RenderTransform = oldScaleTransform;
        newContent.RenderTransform = newScaleTransform;

        transitionBox.UpdateLayout();

        Storyboard oldContentStoryboard = new Storyboard();
        Storyboard newContentStoryboard = new Storyboard();

        var easing = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut };

        DoubleAnimation oldScaleXAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration,
            EasingFunction = easing
        };

        DoubleAnimation oldScaleYAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration,
            EasingFunction = easing
        };

        Storyboard.SetTarget(oldScaleXAnimation, oldScaleTransform);
        Storyboard.SetTargetProperty(oldScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(oldScaleYAnimation, oldScaleTransform);
        Storyboard.SetTargetProperty(oldScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        oldContentStoryboard.Children.Add(oldScaleXAnimation);
        oldContentStoryboard.Children.Add(oldScaleYAnimation);

        DoubleAnimation newScaleXAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = duration,
            EasingFunction = easing
        };

        DoubleAnimation newScaleYAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            Duration = duration,
            EasingFunction = easing
        };

        Storyboard.SetTarget(newScaleXAnimation, newScaleTransform);
        Storyboard.SetTargetProperty(newScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(newScaleYAnimation, newScaleTransform);
        Storyboard.SetTargetProperty(newScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        newContentStoryboard.Children.Add(newScaleXAnimation);
        newContentStoryboard.Children.Add(newScaleYAnimation);

        oldContentStoryboard.Completed += (s, e) =>
        {
            oldContent.Visibility = Visibility.Hidden;

            newContent.Visibility = Visibility.Visible;

            newScaleTransform.ScaleX = 0;
            newScaleTransform.ScaleY = 0;

            transitionBox.UpdateLayout();

            newContentStoryboard.Begin();
        };

        newContentStoryboard.Completed += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("New content animation completed. Transition finished.");

            newContent.RenderTransform = null;
            newContent.RenderTransformOrigin = new Point(0, 0);
            newContent.Opacity = 1.0;
            newContent.Visibility = Visibility.Visible;

            oldContent.Visibility = Visibility.Collapsed;
            oldContent.Opacity = 0;
            oldContent.Content = null;
            oldContent.RenderTransform = null;
        };

        oldContentStoryboard.Begin(transitionBox);
    }
}