using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.ViewModels;
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

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// SelectorView.xaml 的交互逻辑
/// </summary>
public partial class CascadePickerView : UserControl
{
    public CascadePickerView()
    {
        InitializeComponent();
        DataContext = new CascadePickerViewModel();
    }
}

public partial class CascadePickerViewModel : ObservableObject
{
    [ObservableProperty]
    public partial CascadeItemViewModel CascadePicker { get; set; } = new CascadeItemViewModel();

    [ObservableProperty]
    public partial CategoryViewModel CategoryPicker { get; set; } = new CategoryViewModel();

    [ObservableProperty]
    public partial DepartmentViewModel DepartmentPicker { get; set; } = new DepartmentViewModel();
}

// 示例1：保持兼容性的城市数据（但不再需要实现接口）
public partial class CascadeItemViewModel : ObservableObject
{
    public CascadeItemViewModel()
    {
        Cities =
        [
            new City
            {
                Name = "北京",
                Children =
                    [
                        new City {Name = "西城区"}, new City {Name = "东城区"}, new City {Name = "海淀区"},
                        new City {Name = "朝阳区"}, new City {Name = "丰台区"}, new City {Name = "石景山区"}
                    ]
            },
            new City
            {
                Name = "四川",
                Children =
                    [
                        new City {Name = "成都市"},
                        new City
                        {
                            Name = "巴中市", Children =
                            [
                                new City
                                {
                                    Name = "恩阳区"
                                },
                                new City
                                {
                                    Name = "南江县"
                                },
                                new City
                                {
                                    Name = "通江县"
                                }
                            ]
                        }
                    ]
            },
            new City
            {
                Name = "山东",
                Children =
                    [
                        new City {Name = "青岛市"},
                        new City {Name = "烟台市"},
                        new City {Name = "威海市"},
                        new City {Name = "枣庄市"},
                        new City
                        {
                            Name = "潍坊市",
                            Children =
                            [
                                new City
                                {
                                    Name = "青州市"
                                },
                                new City
                                {
                                    Name = "诸城市"
                                },
                                new City
                                {
                                    Name = "寿光市"
                                },
                                new City
                                {
                                    Name = "安丘市"
                                },
                                new City
                                {
                                    Name = "高密市"
                                },
                                new City
                                {
                                    Name = "昌邑市"
                                }
                            ]
                        }
                    ]
            },
        ];
    }

    [ObservableProperty]
    public partial City City { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<City> Cities { get; set; }

    [ObservableProperty]
    public partial string Text { get; set; } = "山东/潍坊市/青州市";

    partial void OnCityChanged(City value)
    {
        if (value != null)
            NotificationService.Instance.Show($"选择的城市为：{value.Name}");
    }

    [RelayCommand]
    private void GetCity()
    {
        if (City != null)
        {
            System.Windows.MessageBox.Show($"选择的城市为：{City.Name}");
        }
    }
}

// 示例2：使用 NodePathMemberPath 的方式
public partial class CategoryViewModel : ObservableObject
{
    public CategoryViewModel()
    {
        Categories =
        [
            new Category
            {
                Name = "电子产品",
                Description = "数码科技类",
                SubCategories =
                [
                    new Category
                    {
                        Name = "手机",
                        Description = "移动通讯设备",
                        SubCategories =
                        [
                            new Category { Name = "智能手机", Description = "Android/iOS" },
                            new Category { Name = "功能手机", Description = "基础通话" }
                        ]
                    },
                    new Category
                    {
                        Name = "电脑",
                        Description = "计算机设备",
                        SubCategories =
                        [
                            new Category { Name = "笔记本", Description = "便携式" },
                            new Category { Name = "台式机", Description = "高性能" },
                            new Category { Name = "平板电脑", Description = "触控设备" }
                        ]
                    }
                ]
            },
            new Category
            {
                Name = "服装",
                Description = "服饰鞋帽",
                SubCategories =
                [
                    new Category
                    {
                        Name = "男装",
                        Description = "男士服饰",
                        SubCategories =
                        [
                            new Category { Name = "上衣", Description = "T恤/衬衫" },
                            new Category { Name = "裤子", Description = "牛仔裤/休闲裤" }
                        ]
                    },
                    new Category
                    {
                        Name = "女装",
                        Description = "女士服饰",
                        SubCategories =
                        [
                            new Category { Name = "连衣裙", Description = "各式裙装" },
                            new Category { Name = "上衣", Description = "衬衫/T恤" },
                            new Category { Name = "裤子", Description = "裤装" }
                        ]
                    }
                ]
            }
        ];
    }

    [ObservableProperty]
    public partial Category SelectedCategory { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<Category> Categories { get; set; }

    partial void OnSelectedCategoryChanged(Category value)
    {
        if (value != null)
            NotificationService.Instance.Show($"选择的分类为：{value.Name}");
    }
}

// 示例3：使用复杂路径的方式
public partial class DepartmentViewModel : ObservableObject
{
    public DepartmentViewModel()
    {
        Departments =
        [
            new Department
            {
                Info = new DepartmentInfo { Code = "HR", Name = "人力资源部" },
                SubDepartments =
                [
                    new Department { Info = new DepartmentInfo { Code = "HR-REC", Name = "招聘组" }},
                    new Department { Info = new DepartmentInfo { Code = "HR-TRA", Name = "培训组" }}
                ]
            },
            new Department
            {
                Info = new DepartmentInfo { Code = "IT", Name = "信息技术部" },
                SubDepartments =
                [
                    new Department
                    {
                        Info = new DepartmentInfo { Code = "IT-DEV", Name = "开发组" },
                        SubDepartments =
                        [
                            new Department { Info = new DepartmentInfo { Code = "IT-DEV-FE", Name = "前端团队" }},
                            new Department { Info = new DepartmentInfo { Code = "IT-DEV-BE", Name = "后端团队" }}
                        ]
                    },
                    new Department { Info = new DepartmentInfo { Code = "IT-OPS", Name = "运维组" }}
                ]
            },
            new Department
            {
                Info = new DepartmentInfo { Code = "FIN", Name = "财务部" },
                SubDepartments =
                [
                    new Department { Info = new DepartmentInfo { Code = "FIN-ACC", Name = "会计组" }},
                    new Department { Info = new DepartmentInfo { Code = "FIN-AUD", Name = "审计组" }}
                ]
            }
        ];
    }

    [ObservableProperty]
    public partial Department SelectedDepartment { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<Department> Departments { get; set; }
}

// 简化的 City 类，不再需要实现接口
public class City
{
    public string Name { get; set; } = string.Empty;
    public List<City> Children { get; set; } = [];

    public override string ToString()
    {
        return Name;
    }
}

// Category 类
public class Category
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Category> SubCategories { get; set; } = [];
}

// 使用复杂对象路径的 Department 类
public class Department
{
    public DepartmentInfo Info { get; set; } = new DepartmentInfo();
    public List<Department> SubDepartments { get; set; } = [];
}

public class DepartmentInfo
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}