namespace Cyclone.Wpf.Controls;

/// <summary>
/// 弹出警告框服务接口
/// </summary>
public interface IAlertService : IDisposable
{
    /// <summary>
    /// 显示普通警告框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="title">窗口标题</param>
    /// <returns>对话框结果</returns>
    bool? Show(object content, string title = null);

    /// <summary>
    /// 显示带验证回调的警告框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="validation">验证回调函数，返回 true 允许关闭，false 阻止关闭</param>
    /// <param name="title">窗口标题</param>
    void ShowWithValidation(object content, Func<bool> validation, string title = null);

    /// <summary>
    /// 配置选项
    /// </summary>
    AlertOption Option { get; }
}
