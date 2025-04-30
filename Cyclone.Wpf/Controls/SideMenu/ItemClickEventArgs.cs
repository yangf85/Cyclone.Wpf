using System.Windows;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 定义ItemClick事件的委托
/// </summary>
/// <param name="sender">事件发送者</param>
/// <param name="e">事件参数</param>
public delegate void ItemClickEventHandler(object sender, ItemClickEventArgs e);

/// <summary>
/// ItemClick事件的参数类
/// </summary>
public class ItemClickEventArgs : RoutedEventArgs
{
    /// <summary>
    /// 被点击的菜单项
    /// </summary>
    public SideMenuItem ClickedItem { get; }

    public ItemClickEventArgs(SideMenuItem clickedItem)
    {
        ClickedItem = clickedItem;
    }
}