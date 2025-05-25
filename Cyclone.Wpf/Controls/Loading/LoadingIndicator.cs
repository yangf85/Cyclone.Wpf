using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 加载指示器基类，提供 IsActive 依赖属性的默认实现
/// </summary>
public abstract class LoadingIndicator : ContentControl, ILoadingIndicator
{
    #region IsActive

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(LoadingIndicator),
            new PropertyMetadata(false, OnIsActiveChanged));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var indicator = (LoadingIndicator)d;
        indicator.OnIsActiveChanged((bool)e.OldValue, (bool)e.NewValue);
    }

    #endregion IsActive

    /// <summary>
    /// 当 IsActive 属性改变时调用，子类可以重写此方法来响应激活状态的变化
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    protected virtual void OnIsActiveChanged(bool oldValue, bool newValue)
    {
        // 子类可以重写此方法来启动或停止动画
    }
}