using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 对话框窗口基础接口，抽象窗口操作的核心成员
/// 使服务层不依赖具体窗口类型（Window/自定义窗口控件）
/// </summary>
public interface IDialogWindow
{
    object DataContext { get; set; }

    bool? DialogResult { get; set; }

    Window Owner { get; set; }

    bool? ShowDialog();

    void Close();
}