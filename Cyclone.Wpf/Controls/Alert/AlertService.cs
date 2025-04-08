using System;
using System.Windows;
using System.Threading;

namespace Cyclone.Wpf.Controls;

public interface IAlertService
{
    void Show(object content, DataTemplate template, string title = null);
}

public class AlertService : IAlertService
{
    private readonly AlertOption _option;

    // 静态单例实例
    private static readonly Lazy<AlertService> _lazyInstance =
        new Lazy<AlertService>(() => new AlertService(), LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>
    /// 获取AlertService的单例实例
    /// </summary>
    public static AlertService Instance => _lazyInstance.Value;

    /// <summary>
    /// 使用默认选项初始化AlertService的新实例
    /// </summary>
    public AlertService() : this(new AlertOption())
    {
    }

    /// <summary>
    /// 使用自定义选项初始化AlertService的新实例
    /// </summary>
    public AlertService(AlertOption option)
    {
        _option = option ?? throw new ArgumentNullException(nameof(option));
    }

    public void Show(object content, DataTemplate template, string title = null)
    {
    }
}