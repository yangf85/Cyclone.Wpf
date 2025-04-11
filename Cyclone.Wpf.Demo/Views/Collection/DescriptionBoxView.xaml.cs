using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// DescriptionView.xaml 的交互逻辑
    /// </summary>
    public partial class DescriptionBoxView : UserControl
    {
        /// <summary>
        /// 构造函数，初始化控件并设置数据上下文
        /// </summary>
        public DescriptionBoxView()
        {
            InitializeComponent();
            // 创建并设置视图模型作为数据上下文
            DataContext = new DescriptionBoxViewModel();
        }
    }

    /// <summary>
    /// DescriptionBox 的视图模型类
    /// 用于提供数据绑定支持
    /// </summary>
    public partial class DescriptionBoxViewModel : ObservableObject
    {
        /// <summary>
        /// 项目集合，使用 ObservableProperty 特性自动实现 INotifyPropertyChanged
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<DescriptionBoxItemViewModel> _items;

        /// <summary>
        /// 构造函数，初始化示例数据
        /// </summary>
        public DescriptionBoxViewModel()
        {
            // 初始化测试数据
            Items = new ObservableCollection<DescriptionBoxItemViewModel>
            {
                new DescriptionBoxItemViewModel
                {
                    Label = "用户ID:",
                    Value = "12345",
                    Description = "用户的唯一标识符"
                },
                new DescriptionBoxItemViewModel
                {
                    Label = "用户名:",
                    Value = "johnsmith",
                    Description = "系统登录名"
                },
                new DescriptionBoxItemViewModel
                {
                    Label = "创建日期:",
                    Value = "2023-04-01",
                    Description = "账户创建日期"
                },
                new DescriptionBoxItemViewModel
                {
                    Label = "状态:",
                    Value = "激活",
                    Description = "当前账户状态"
                }
            };
        }
    }

    /// <summary>
    /// DescriptionBoxItem 的视图模型类
    /// 表示单个描述项的数据
    /// </summary>
    public partial class DescriptionBoxItemViewModel : ObservableObject
    {
        /// <summary>
        /// 标签文本
        /// </summary>
        [ObservableProperty]
        private string _label;

        /// <summary>
        /// 值文本
        /// </summary>
        [ObservableProperty]
        private string _value;

        /// <summary>
        /// 描述文本
        /// </summary>
        [ObservableProperty]
        private string _description;

        // 可选的网格定位属性

        /// <summary>
        /// 项目在网格中的行索引
        /// </summary>
        [ObservableProperty]
        private int _row;

        /// <summary>
        /// 项目在网格中的列索引
        /// </summary>
        [ObservableProperty]
        private int _column;

        /// <summary>
        /// 项目在网格中的行跨度
        /// </summary>
        [ObservableProperty]
        private int _rowSpan = 1;

        /// <summary>
        /// 项目在网格中的列跨度
        /// </summary>
        [ObservableProperty]
        private int _columnSpan = 1;
    }
}