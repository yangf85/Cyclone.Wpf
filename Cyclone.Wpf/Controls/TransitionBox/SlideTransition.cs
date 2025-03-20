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
            // 确保 RenderTransform 已初始化
            if (element.RenderTransform == null)
            {
                element.RenderTransform = new TranslateTransform();
            }

            element.RenderTransformOrigin = new Point(0.5, 0.5); // 可选：设置变换中心点

            var animation = new DoubleAnimation
            {
                From = FromX,
                To = ToX,
                Duration = Duration,
                AutoReverse = false
            };

            animation.Completed += (s, e) => Completed?.Invoke(this, EventArgs.Empty);
            element.BeginAnimation(TranslateTransform.XProperty, animation);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SlideTransition failed: {ex.Message}");
        }
    }

    public override void Stop(FrameworkElement element)
    {
        element.BeginAnimation(TranslateTransform.XProperty, null);
        if (element.RenderTransform is TranslateTransform transform)
        {
            transform.X = ToX; // 恢复最终位置
        }
    }

    public event EventHandler Completed;
}