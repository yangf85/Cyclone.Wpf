using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

public class FadeTransition : TransitionBase
{
    public double FromOpacity { get; set; } = 0;

    public double ToOpacity { get; set; } = 1;

    public override TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

    public override void Start(FrameworkElement element)
    {
        try
        {
            var animation = new DoubleAnimation
            {
                From = FromOpacity,
                To = ToOpacity,
                Duration = Duration,
                AutoReverse = false
            };

            animation.Completed += (s, e) => Completed?.Invoke(this, EventArgs.Empty);
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FadeTransition failed: {ex.Message}");
        }
    }

    public override void Stop(FrameworkElement element)
    {
        // 使用 HandoffBehavior.SnapshotAndReplace 停止当前动画
        element.BeginAnimation(UIElement.OpacityProperty, null, HandoffBehavior.SnapshotAndReplace);
    }

    public event EventHandler Completed;
}