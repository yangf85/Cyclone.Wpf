using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

public class RotateTransition : TransitionBase
{
    public double FromAngle { get; set; } = 0;

    public double ToAngle { get; set; } = 360;

    public override TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

    public override void Start(FrameworkElement element)
    {
        try
        {
            var rotateAnimation = new DoubleAnimation
            {
                From = FromAngle,
                To = ToAngle,
                Duration = Duration,
                AutoReverse = false
            };

            rotateAnimation.Completed += (s, e) => Completed?.Invoke(this, EventArgs.Empty);
            element.RenderTransform = new RotateTransform();
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"RotateTransition failed: {ex.Message}");
        }
    }

    public override void Stop(FrameworkElement element)
    {
        element.BeginAnimation(RotateTransform.AngleProperty, null, HandoffBehavior.SnapshotAndReplace);
    }

    public event EventHandler Completed;
}