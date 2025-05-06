using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 淡入淡出过渡效果
/// </summary>
public class FadeTransition : ITransition
{
    public void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration)
    {
        // 创建一个Storyboard来协调动画
        Storyboard storyboard = new Storyboard();

        // 确保新内容初始状态正确
        newContent.Opacity = 0;

        // 为旧内容创建淡出动画
        DoubleAnimation oldAnimation = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = duration,
            FillBehavior = FillBehavior.HoldEnd,
            // 添加缓动函数使动画更平滑
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(oldAnimation, oldContent);
        Storyboard.SetTargetProperty(oldAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(oldAnimation);

        // 为新内容创建淡入动画，但使用短暂延迟确保旧内容开始淡出
        DoubleAnimation newAnimation = new DoubleAnimation
        {
            From = 0.0,
            To = 1.0,
            // 延迟一小段时间开始淡入，避免两个内容重叠时的视觉跳动
            BeginTime = TimeSpan.FromMilliseconds(10),
            Duration = duration,
            FillBehavior = FillBehavior.HoldEnd,
            // 添加缓动函数
            EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(newAnimation, newContent);
        Storyboard.SetTargetProperty(newAnimation, new PropertyPath(UIElement.OpacityProperty));
        storyboard.Children.Add(newAnimation);

        // 设置动画完成的处理
        storyboard.Completed += (s, e) =>
        {
            // 隐藏旧内容
            oldContent.Visibility = Visibility.Collapsed;
            oldContent.Content = null;
        };

        // 开始播放动画
        storyboard.Begin(transitionBox);
    }
}