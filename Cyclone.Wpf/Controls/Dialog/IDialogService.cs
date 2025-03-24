using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Cyclone.Wpf.Controls;

/// <summary>
/// 对话框服务接口，定义对话框管理核心功能：
/// 1. 注册ViewModel与对话框窗口的关联
/// 2. 显示模态对话框并返回操作结果
/// </summary>
public interface IDialogService
{
    void Register<TViewModel, TDialog>() where TViewModel : IDialogRequestClose where TDialog : IDialogWindow, new();

    bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;
}

/// <summary>
/// 对话框服务具体实现类，功能包括：
/// - 维护ViewModel与窗口类型的映射表
/// - 处理对话框事件订阅/取消
/// - 控制对话框的创建和显示流程
/// </summary>
public class BasicDialogService : IDialogService
{
    private IDictionary<Type, Type> _dialogs = new Dictionary<Type, Type>();

    private Window _owner;

    public BasicDialogService(Window owner)
    {
        _owner = owner;
    }

    public void Register<TViewModel, TDialog>()
                where TViewModel : IDialogRequestClose
        where TDialog : IDialogWindow, new()
    {
        _dialogs[typeof(TViewModel)] = typeof(TDialog);
    }

    public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose
    {
        var dialogType = _dialogs[typeof(TViewModel)];
        var dialog = (IDialogWindow)Activator.CreateInstance(dialogType);

        dialog.Owner = _owner;
        dialog.DataContext = viewModel;

        viewModel.RequestClosed += CreateHandle(viewModel, dialog);//挂载事件方法

        return dialog.ShowDialog();//显示模态对话框，不可使用Window.Show方法
    }

    private static EventHandler<DialogRequestCloseEventArgs> CreateHandle<TViewModel>(TViewModel viewModel, IDialogWindow dialog) where TViewModel : IDialogRequestClose
    {
        void Handle(object sender, DialogRequestCloseEventArgs e)
        {
            viewModel.RequestClosed -= Handle;//移除事件方法

            if (e.DialogResult.HasValue)
            {
                dialog.DialogResult = e.DialogResult;//关闭对话框 返回结果
            }
            else
            {
                dialog.Close();//直接关闭对话框
            }
        }

        return Handle;
    }
}