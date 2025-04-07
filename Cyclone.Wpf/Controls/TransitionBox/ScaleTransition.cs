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
        System.Diagnostics.Debug.WriteLine("Starting scale transition with sequential animation");

        // Make sure we can see the old content first
        oldContent.Visibility = Visibility.Visible;
        oldContent.Opacity = 1.0;

        // Keep new content hidden until we're ready
        newContent.Visibility = Visibility.Hidden;
        newContent.Opacity = 1.0;

        // Set transform centers
        oldContent.RenderTransformOrigin = new Point(0.5, 0.5);
        newContent.RenderTransformOrigin = new Point(0.5, 0.5);

        // Create scale transforms with initial values
        ScaleTransform oldScaleTransform = new ScaleTransform(1, 1);
        ScaleTransform newScaleTransform = new ScaleTransform(0, 0);

        // Important: Replace any existing transforms
        oldContent.RenderTransform = oldScaleTransform;
        newContent.RenderTransform = newScaleTransform;

        // Force layout update to ensure transforms are applied
        transitionBox.UpdateLayout();

        System.Diagnostics.Debug.WriteLine($"Pre-animation state - Old content: Visible={oldContent.Visibility}, Scale={oldScaleTransform.ScaleX}");
        System.Diagnostics.Debug.WriteLine($"Pre-animation state - New content: Visible={newContent.Visibility}, Scale={newScaleTransform.ScaleX}");

        // Create two separate storyboards - one for each phase
        Storyboard oldContentStoryboard = new Storyboard();
        Storyboard newContentStoryboard = new Storyboard();

        // Create easing function
        var easing = new PowerEase { Power = 2, EasingMode = EasingMode.EaseInOut };

        // First phase: Animate old content scaling down
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

        // Set animation targets for old content
        Storyboard.SetTarget(oldScaleXAnimation, oldScaleTransform);
        Storyboard.SetTargetProperty(oldScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(oldScaleYAnimation, oldScaleTransform);
        Storyboard.SetTargetProperty(oldScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        // Add animations to old content storyboard
        oldContentStoryboard.Children.Add(oldScaleXAnimation);
        oldContentStoryboard.Children.Add(oldScaleYAnimation);

        // Create animations for new content (these will be run after old content completes)
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

        // Set animation targets for new content
        Storyboard.SetTarget(newScaleXAnimation, newScaleTransform);
        Storyboard.SetTargetProperty(newScaleXAnimation, new PropertyPath(ScaleTransform.ScaleXProperty));

        Storyboard.SetTarget(newScaleYAnimation, newScaleTransform);
        Storyboard.SetTargetProperty(newScaleYAnimation, new PropertyPath(ScaleTransform.ScaleYProperty));

        // Add animations to new content storyboard
        newContentStoryboard.Children.Add(newScaleXAnimation);
        newContentStoryboard.Children.Add(newScaleYAnimation);

        // Handle first phase completion
        oldContentStoryboard.Completed += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("Old content animation completed. Starting new content animation.");

            // Ensure old content is hidden
            oldContent.Visibility = Visibility.Hidden;

            // Make new content visible before starting its animation
            newContent.Visibility = Visibility.Visible;

            // Ensure new content is at the start scale
            newScaleTransform.ScaleX = 0;
            newScaleTransform.ScaleY = 0;

            // Force layout update before starting second animation
            transitionBox.UpdateLayout();

            // Start the second phase animation
            newContentStoryboard.Begin();
        };

        // Handle second phase completion
        newContentStoryboard.Completed += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("New content animation completed. Transition finished.");

            // Clean up and restore normal state
            newContent.RenderTransform = null;
            newContent.RenderTransformOrigin = new Point(0, 0);
            newContent.Opacity = 1.0;
            newContent.Visibility = Visibility.Visible;

            // Clean up old content
            oldContent.Visibility = Visibility.Collapsed;
            oldContent.Opacity = 0;
            oldContent.Content = null;
            oldContent.RenderTransform = null;
        };

        // Start the first phase animation
        System.Diagnostics.Debug.WriteLine("Beginning first phase animation (old content scale down)");
        oldContentStoryboard.Begin(transitionBox);
    }
}