using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 简化的水平翻转过渡效果
/// </summary>
public class FlipTransition : ITransition
{
    public void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration)
    {
        System.Diagnostics.Debug.WriteLine("=== 开始水平翻转过渡 ===");

        // 记录控件尺寸和状态
        System.Diagnostics.Debug.WriteLine($"转换框尺寸: 宽={transitionBox.ActualWidth}, 高={transitionBox.ActualHeight}");
        System.Diagnostics.Debug.WriteLine($"旧内容尺寸: 宽={oldContent.ActualWidth}, 高={oldContent.ActualHeight}");
        System.Diagnostics.Debug.WriteLine($"新内容尺寸: 宽={newContent.ActualWidth}, 高={newContent.ActualHeight}");

        // 设置内容状态
        oldContent.Visibility = Visibility.Visible;
        oldContent.Opacity = 1.0;

        // 初始隐藏新内容
        newContent.Visibility = Visibility.Hidden;

        // 创建变换
        ScaleTransform oldScaleTransform = new ScaleTransform(1, 1);
        oldContent.RenderTransformOrigin = new Point(0.5, 0.5);
        oldContent.RenderTransform = oldScaleTransform;

        ScaleTransform newScaleTransform = new ScaleTransform(0, 1);
        newContent.RenderTransformOrigin = new Point(0.5, 0.5);
        newContent.RenderTransform = newScaleTransform;

        // 强制布局更新
        transitionBox.UpdateLayout();

        // 使用Task延迟一小段时间后再开始动画，确保UI已正确渲染
        Task.Delay(50).ContinueWith(_ =>
        {
            transitionBox.Dispatcher.Invoke(() =>
            {
                // 确保变换已应用
                System.Diagnostics.Debug.WriteLine($"开始动画前 - 旧内容缩放: {oldScaleTransform.ScaleX}");

                // 创建旧内容的动画 (水平缩放到0)
                DoubleAnimation oldScaleXAnimation = new DoubleAnimation
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = duration
                };

                // 设置动画完成回调
                oldScaleXAnimation.Completed += (s, e) =>
                {
                    System.Diagnostics.Debug.WriteLine("旧内容动画完成，开始新内容动画");

                    // 隐藏旧内容
                    oldContent.Visibility = Visibility.Hidden;

                    // 显示新内容
                    newContent.Visibility = Visibility.Visible;

                    // 创建新内容的动画 (水平缩放从0到1)
                    DoubleAnimation newScaleXAnimation = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = duration
                    };

                    newScaleXAnimation.Completed += (s2, e2) =>
                    {
                        System.Diagnostics.Debug.WriteLine("新内容动画完成");

                        // 清理变换
                        newContent.RenderTransform = null;
                        oldContent.Visibility = Visibility.Collapsed;
                        oldContent.Content = null;
                        oldContent.RenderTransform = null;
                    };

                    // 开始新内容动画
                    newScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, newScaleXAnimation);
                };

                // 开始旧内容动画
                System.Diagnostics.Debug.WriteLine("开始旧内容动画");
                oldScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, oldScaleXAnimation);
            });
        });
    }
}