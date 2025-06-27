using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// EnumSelector 控件使用案例演示
    /// </summary>
    public partial class EnumSelectorView : UserControl
    {
        public EnumSelectorView()
        {
            InitializeComponent();
            DataContext = new EnumSelectorDemoViewModel();
        }
    }

    public partial class EnumSelectorDemoViewModel : ObservableObject
    {
        #region 属性

        [ObservableProperty]
        public partial UserRole UserRole { get; set; } = UserRole.User;

        [ObservableProperty]
        public partial Priority Priority { get; set; } = Priority.Medium;

        [ObservableProperty]
        public partial FilePermissions FilePermissions { get; set; } = FilePermissions.Read | FilePermissions.Write;

        [ObservableProperty]
        public partial WorkDays WorkDays { get; set; } = WorkDays.Monday;

        #endregion 属性

        #region 命令

        [RelayCommand]
        private void SetDefaults()
        {
            UserRole = UserRole.Admin;
            Priority = Priority.High;
            FilePermissions = FilePermissions.ReadWrite;
            WorkDays = WorkDays.Weekdays;
        }

        [RelayCommand]
        private void ResetAll()
        {
            UserRole = UserRole.Guest;
            Priority = Priority.Low;
            FilePermissions = FilePermissions.None;
            WorkDays = WorkDays.None;
        }

        #endregion 命令
    }

    #region 枚举定义

    /// <summary>
    /// 用户角色 - 普通枚举
    /// </summary>
    public enum UserRole
    {
        [Description("访客")]
        Guest = 0,

        [Description("普通用户")]
        User = 1,

        [Description("管理员")]
        Admin = 2,

        [Description("超级管理员")]
        SuperAdmin = 3
    }

    /// <summary>
    /// 优先级 - 普通枚举
    /// </summary>
    public enum Priority
    {
        [Description("低")]
        Low = 1,

        [Description("中")]
        Medium = 2,

        [Description("高")]
        High = 3,

        [Description("紧急")]
        Critical = 4
    }

    /// <summary>
    /// 文件权限 - Flags枚举
    /// </summary>
    [Flags]
    public enum FilePermissions
    {
        [Description("无权限")]
        None = 0,

        [Description("读取")]
        Read = 1,

        [Description("写入")]
        Write = 2,

        [Description("执行")]
        Execute = 4,

        [Description("删除")]
        Delete = 8,

        [Description("读写")]
        ReadWrite = Read | Write,

        [Description("完全控制")]
        FullControl = Read | Write | Execute | Delete
    }

    /// <summary>
    /// 工作日 - Flags枚举
    /// </summary>
    [Flags]
    public enum WorkDays
    {
        [Description("无")]
        None = 0,

        [Description("周一")]
        Monday = 1,

        [Description("周二")]
        Tuesday = 2,

        [Description("周三")]
        Wednesday = 4,

        [Description("周四")]
        Thursday = 8,

        [Description("周五")]
        Friday = 16,

        [Description("工作日")]
        Weekdays = Monday | Tuesday | Wednesday | Thursday | Friday,

        [Description("周末")]
        Weekend = 32 | 64, // Saturday | Sunday

        [Description("全周")]
        AllDays = Weekdays | Weekend
    }

    #endregion 枚举定义
}