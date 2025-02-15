using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

public class TransitionBox : ContentControl
{
    // 定义依赖属性，并提供默认的 FadeTransition 实例
    public static readonly DependencyProperty AnimationProperty =
        DependencyProperty.Register(
            nameof(Animation),
            typeof(ITransition),
            typeof(TransitionBox),
            new PropertyMetadata(new FadeTransition())); // 默认动画

    public ITransition Animation
    {
        get => (ITransition)GetValue(AnimationProperty);
        set => SetValue(AnimationProperty, value);
    }

    public TransitionBox()
    {
        Unloaded += OnUnloaded; // 挂接 Unloaded 事件
    }

    protected override async void OnContentChanged(object oldContent, object newContent)
    {
        // 停止旧内容的动画
        if (Animation != null && oldContent is FrameworkElement oldElement)
        {
            Animation.Stop(oldElement);
            await Task.Delay(Animation.Duration); // 等待动画完成
        }

        base.OnContentChanged(oldContent, newContent);

        // 启动新内容的动画
        if (Animation != null && newContent is FrameworkElement newElement)
        {
            Animation.Start(newElement);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // 清理资源，停止动画
        if (Animation != null && Content is FrameworkElement element)
        {
            Animation.Stop(element);
        }

        // 移除事件处理器
        Unloaded -= OnUnloaded;
    }
}






