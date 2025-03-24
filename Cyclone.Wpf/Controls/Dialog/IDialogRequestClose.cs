using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 对话框关闭请求接口，ViewModel需实现此接口
/// 用于通过事件机制通知视图层关闭对话框
/// </summary>
public interface IDialogRequestClose
{
    event EventHandler<DialogRequestCloseEventArgs> RequestClosed;
}