using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 滑动过渡效果
/// </summary>
public class SlideTransition : ITransition
{
    /// <summary>
    /// 滑动方向
    /// </summary>
    public enum SlideDirection
    {
        /// <summary>从左向右滑动</summary>
        LeftToRight,

        /// <summary>从右向左滑动</summary>
        RightToLeft,

        /// <summary>从上向下滑动</summary>
        TopToBottom,

        /// <summary>从下向上滑动</summary>
        BottomToTop
    }

    /// <summary>
    /// 获取或设置滑动方向
    /// </summary>
    public SlideDirection Direction { get; set; } = SlideDirection.RightToLeft;

    public void StartTransition(TransitionBox transitionBox, ContentPresenter oldContent, ContentPresenter newContent, Duration duration)
    {
        // 设置变换
        oldContent.RenderTransform = new TranslateTransform();
        newContent.RenderTransform = new TranslateTransform();

        // 根据方向确定动画的起始值和结束值
        double fromX = 0, toX = 0, fromY = 0, toY = 0;

        switch (Direction)
        {
            case SlideDirection.LeftToRight:
                fromX = -transitionBox.ActualWidth;
                toX = transitionBox.ActualWidth;
                break;

            case SlideDirection.RightToLeft:
                fromX = transitionBox.ActualWidth;
                toX = -transitionBox.ActualWidth;
                break;

            case SlideDirection.TopToBottom:
                fromY = -transitionBox.ActualHeight;
                toY = transitionBox.ActualHeight;
                break;

            case SlideDirection.BottomToTop:
                fromY = transitionBox.ActualHeight;
                toY = -transitionBox.ActualHeight;
                break;
        }

        // 创建旧内容的滑出动画
        var oldXAnimation = new DoubleAnimation
        {
            To = toX,
            Duration = duration
        };

        var oldYAnimation = new DoubleAnimation
        {
            To = toY,
            Duration = duration
        };

        // 创建新内容的滑入动画
        var newXAnimation = new DoubleAnimation
        {
            From = fromX,
            To = 0,
            Duration = duration
        };

        var newYAnimation = new DoubleAnimation
        {
            From = fromY,
            To = 0,
            Duration = duration
        };

        // 设置新内容初始位置
        ((TranslateTransform)newContent.RenderTransform).X = fromX;
        ((TranslateTransform)newContent.RenderTransform).Y = fromY;
        newContent.Opacity = 1;

        // 创建完成时操作的Storyboard
        Storyboard storyboard = new Storyboard();
        storyboard.Completed += (s, e) =>
        {
            // 动画完成后，清空旧内容
            oldContent.Visibility = Visibility.Collapsed;
            oldContent.Content = null;
        };

        // 应用动画
        ((TranslateTransform)oldContent.RenderTransform).BeginAnimation(TranslateTransform.XProperty, oldXAnimation);
        ((TranslateTransform)oldContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, oldYAnimation);
        ((TranslateTransform)newContent.RenderTransform).BeginAnimation(TranslateTransform.XProperty, newXAnimation);
        ((TranslateTransform)newContent.RenderTransform).BeginAnimation(TranslateTransform.YProperty, newYAnimation);

        // 启动Storyboard以确保动画完成事件触发
        storyboard.Duration = duration;
        storyboard.Begin();
    }
}