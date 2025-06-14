// CascadePickerView.xaml.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// CascadePickerView.xaml 的交互逻辑
/// </summary>
public partial class CascadePickerView : UserControl
{
    public CascadePickerView()
    {
        InitializeComponent();
        DataContext = new CascadePickerViewModel();
    }
}

/// <summary>
/// CascadePicker 示例的 ViewModel
/// </summary>
public partial class CascadePickerViewModel : ObservableObject
{
    public CascadePickerViewModel()
    {
        InitializeCities();
        InitializeDepartments();
    }

    #region 城市数据（示例2和示例4使用）

    [ObservableProperty]
    private ObservableCollection<City> cities;

    [ObservableProperty]
    private City selectedCity;

    private void InitializeCities()
    {
        Cities = new ObservableCollection<City>
        {
            new City
            {
                Name = "北京市",
                Children = new List<City>
                {
                    new City { Name = "东城区" },
                    new City { Name = "西城区" },
                    new City { Name = "朝阳区" },
                    new City { Name = "海淀区" }
                }
            },
            new City
            {
                Name = "上海市",
                Children = new List<City>
                {
                    new City { Name = "黄浦区" },
                    new City { Name = "徐汇区" },
                    new City { Name = "浦东新区" }
                }
            },
            new City
            {
                Name = "广东省",
                Children = new List<City>
                {
                    new City
                    {
                        Name = "广州市",
                        Children = new List<City>
                        {
                            new City { Name = "天河区" },
                            new City { Name = "越秀区" },
                            new City { Name = "白云区" }
                        }
                    },
                    new City
                    {
                        Name = "深圳市",
                        Children = new List<City>
                        {
                            new City { Name = "福田区" },
                            new City { Name = "南山区" },
                            new City { Name = "宝安区" }
                        }
                    }
                }
            }
        };
    }

    partial void OnSelectedCityChanged(City value)
    {
        if (value != null)
        {
            // 可以在这里处理选中城市变化的逻辑
            System.Diagnostics.Debug.WriteLine($"选中城市: {value.Name}");
        }
    }

    #endregion 城市数据（示例2和示例4使用）

    #region 部门数据（示例3使用）

    [ObservableProperty]
    private ObservableCollection<Department> departments;

    [ObservableProperty]
    private Department selectedDepartment;

    [ObservableProperty]
    private bool showFullPath = true;

    private void InitializeDepartments()
    {
        Departments = new ObservableCollection<Department>
        {
            new Department
            {
                Name = "总经办",
                Code = "CEO",
                EmployeeCount = 5,
                SubDepartments = new List<Department>
                {
                    new Department { Name = "秘书处", Code = "CEO-SEC", EmployeeCount = 3 },
                    new Department { Name = "战略发展部", Code = "CEO-STR", EmployeeCount = 8 }
                }
            },
            new Department
            {
                Name = "技术中心",
                Code = "TECH",
                EmployeeCount = 120,
                SubDepartments = new List<Department>
                {
                    new Department
                    {
                        Name = "研发部",
                        Code = "TECH-DEV",
                        EmployeeCount = 80,
                        SubDepartments = new List<Department>
                        {
                            new Department { Name = "前端组", Code = "TECH-DEV-FE", EmployeeCount = 25 },
                            new Department { Name = "后端组", Code = "TECH-DEV-BE", EmployeeCount = 35 },
                            new Department { Name = "移动端组", Code = "TECH-DEV-MOB", EmployeeCount = 20 }
                        }
                    },
                    new Department { Name = "测试部", Code = "TECH-QA", EmployeeCount = 25 },
                    new Department { Name = "运维部", Code = "TECH-OPS", EmployeeCount = 15 }
                }
            },
            new Department
            {
                Name = "市场营销中心",
                Code = "MARKET",
                EmployeeCount = 45,
                SubDepartments = new List<Department>
                {
                    new Department { Name = "品牌部", Code = "MARKET-BRAND", EmployeeCount = 12 },
                    new Department { Name = "推广部", Code = "MARKET-PR", EmployeeCount = 18 },
                    new Department { Name = "客户服务部", Code = "MARKET-CS", EmployeeCount = 15 }
                }
            }
        };
    }

    #endregion 部门数据（示例3使用）
}

#region 数据模型

/// <summary>
/// 城市数据模型
/// </summary>
public class City
{
    public string Name { get; set; } = string.Empty;
    public List<City> Children { get; set; } = new List<City>();

    public override string ToString() => Name;
}

/// <summary>
/// 部门数据模型
/// </summary>
public class Department
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public List<Department> SubDepartments { get; set; } = new List<Department>();

    public override string ToString() => Name;
}

#endregion 数据模型