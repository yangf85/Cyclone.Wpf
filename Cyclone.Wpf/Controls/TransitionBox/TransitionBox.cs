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
    public static readonly DependencyProperty TransitionProperty =
        DependencyProperty.Register(
            nameof(Transition),
            typeof(ITransition),
            typeof(TransitionBox),
            new PropertyMetadata(new FadeTransition())); // 默认动画

    public ITransition Transition
    {
        get => (ITransition)GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    public TransitionBox()
    {
        Unloaded += OnUnloaded; // 挂接 Unloaded 事件
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        // 停止旧内容的动画
        if (Transition != null && oldContent is FrameworkElement oldElement)
        {
            Transition.Stop(oldElement);
        }

        base.OnContentChanged(oldContent, newContent);

        // 启动新内容的动画
        if (Transition != null && newContent is FrameworkElement newElement)
        {
            Transition.Start(newElement);
        }
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        // 清理资源，停止动画
        if (Transition != null && Content is FrameworkElement element)
        {
            Transition.Stop(element);
        }

        // 移除事件处理器
        Unloaded -= OnUnloaded;
    }
}