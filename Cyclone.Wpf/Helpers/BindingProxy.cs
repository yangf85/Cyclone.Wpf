using System.Windows;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 【绑定上下文代理类】
/// 解决 WPF 中跨作用域绑定难题（如 DataGrid 列/ContextMenu 等无法直接访问上级 DataContext 的场景）。
/// 通过 Freezable 继承实现绑定支持，将外部数据上下文注入到无法直接访问的区域。
///
/// ▶ 典型使用场景：
///   1. DataGrid 列绑定外部 ViewModel 属性
///   2. 动态菜单项绑定全局状态
///   3. 样式/模板中访问外部数据源
///
/// ▶ XAML 使用示例：
/// <DataGrid.Resources>
///     <local:BindingProxy x:Key="proxy" Data="{Binding ExternalViewModel}" />
/// </DataGrid.Resources>
/// <DataGridTextColumn Binding="{Binding Source={StaticResource proxy}, Path=Data.UserName}" />
///
/// ▶ 注意事项：
///   - 需通过 Resources 声明并绑定 Data 属性
///   - 若代理对象需要跨线程使用，需调用 Freeze() 冻结实例
/// </summary>
public class BindingProxy : Freezable
{
    #region Overrides

    protected override Freezable CreateInstanceCore()
    {
        return new BindingProxy();
    }

    #endregion Overrides

    public object Data
    {
        get { return GetValue(DataProperty); }
        set { SetValue(DataProperty, value); }
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
}