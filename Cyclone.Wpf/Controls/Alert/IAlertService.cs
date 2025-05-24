using System;
using System.Threading.Tasks;

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
    /// 显示带同步验证回调的警告框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="validation">验证回调函数，返回 true 允许关闭，false 阻止关闭</param>
    /// <param name="title">窗口标题</param>
    /// <returns>对话框结果</returns>
    bool? Show(object content, Func<bool> validation, string title = null);

    /// <summary>
    /// 显示带异步验证回调的警告框
    /// </summary>
    /// <param name="content">窗口内容</param>
    /// <param name="asyncValidation">异步验证回调函数，返回 true 允许关闭，false 阻止关闭</param>
    /// <param name="title">窗口标题</param>
    /// <returns>返回一个Task，当对话框关闭时完成</returns>
    Task ShowAsync(object content, Func<Task<bool>> asyncValidation, string title = null);

    /// <summary>
    /// 显示带泛型异步验证回调的警告框
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    /// <param name="content">窗口内容</param>
    /// <param name="asyncValidation">异步验证回调函数，接收类型为T的参数</param>
    /// <param name="validationParameter">传递给验证函数的参数</param>
    /// <param name="title">窗口标题</param>
    /// <returns>返回一个Task，当对话框关闭时完成</returns>
    Task ShowAsync<T>(object content, Func<T, Task<bool>> asyncValidation, T validationParameter, string title = null);

    /// <summary>
    /// 配置选项
    /// </summary>
    AlertOption Option { get; }
}