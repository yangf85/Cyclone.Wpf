namespace Cyclone.Wpf.Controls;

/// <summary>
/// 对话框关闭请求事件参数，用于传递对话框操作结果
/// 封装对话框关闭时的状态（确认/取消/无结果关闭）
/// </summary>
public class DialogRequestCloseEventArgs
{
    public bool? DialogResult { get; private set; }

    public DialogRequestCloseEventArgs(bool? dialogResult)
    {
        DialogResult = dialogResult;
    }
}