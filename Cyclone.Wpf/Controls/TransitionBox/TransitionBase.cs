using System.Windows;
using System.Windows.Markup;

namespace Cyclone.Wpf.Controls;

public abstract class TransitionBase : MarkupExtension, ITransition
{
    // 实现 ITransition 接口的抽象方法
    public abstract void Start(FrameworkElement element);
    public abstract void Stop(FrameworkElement element);

    // 实现 ITransition 接口的 Duration 属性
    public abstract TimeSpan Duration { get; set; }

    // 实现 MarkupExtension 的 ProvideValue 方法
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}






