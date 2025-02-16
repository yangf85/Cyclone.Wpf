using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

public class SlideTransition : TransitionBase
{
    public double FromX { get; set; } = -100;

    public double ToX { get; set; } = 0;

    public override TimeSpan Duration { get; set; } = TimeSpan.FromSeconds(1);

    public override void Start(FrameworkElement element)
    {
        try
        {
            var translateAnimation = new DoubleAnimation
            {
                From = FromX,
                To = ToX,
                Duration = Duration,
                AutoReverse = false
            };

            translateAnimation.Completed += (s, e) => Completed?.Invoke(this, EventArgs.Empty);
            element.RenderTransform = new TranslateTransform(); // 确保 Transform 存在
            element.RenderTransformOrigin = new Point(0.5, 0.5); // 设置变换中心点
            element.BeginAnimation(TranslateTransform.XProperty, translateAnimation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SlideTransition failed: {ex.Message}");
        }
    }

    public override void Stop(FrameworkElement element)
    {
        element.BeginAnimation(TranslateTransform.XProperty, null, HandoffBehavior.SnapshotAndReplace);
    }

    public event EventHandler Completed;
}