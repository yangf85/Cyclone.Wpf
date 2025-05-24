namespace Cyclone.Wpf.Controls;

/// <summary>
/// 加载指示器接口，用于统一管理各种加载动画控件
/// </summary>
public interface ILoadingIndicator
{
    /// <summary>
    /// 获取或设置加载动画是否激活
    /// </summary>
    bool IsActive { get; set; }
}