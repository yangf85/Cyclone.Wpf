using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

public class ScaleTransition : TransitionBase
{
    public double FromScale { get; set; } = 0.5;

    public double ToScale { get; set; } = 1;

    public override TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

    public override void Start(FrameworkElement element)
    {
        try
        {
            var scaleXAnimation = new DoubleAnimation
            {
                From = FromScale,
                To = ToScale,
                Duration = Duration,
                AutoReverse = false
            };

            var scaleYAnimation = new DoubleAnimation
            {
                From = FromScale,
                To = ToScale,
                Duration = Duration,
                AutoReverse = false
            };

            scaleXAnimation.Completed += (s, e) => Completed?.Invoke(this, EventArgs.Empty);
            element.RenderTransform = new ScaleTransform();
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            element.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ScaleTransition failed: {ex.Message}");
        }
    }

    public override void Stop(FrameworkElement element)
    {
        element.BeginAnimation(ScaleTransform.ScaleXProperty, null, HandoffBehavior.SnapshotAndReplace);
        element.BeginAnimation(ScaleTransform.ScaleYProperty, null, HandoffBehavior.SnapshotAndReplace);
    }

    public event EventHandler Completed;
}