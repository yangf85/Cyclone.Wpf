using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// TreeControlView.xaml 的交互逻辑
    /// </summary>
    public partial class TreeControlView : UserControl
    {
        public TreeControlView()
        {
            InitializeComponent();
            DataContext = new TreeControlViewModel();
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            if (DataContext is TreeControlViewModel vm && sender is TreeViewItem item && item.DataContext is City city)
            {
                vm.LastOperation = $"选中: {city.NodePath}";
                vm.TreeViewModel.City = city;
            }
        }
    }

    public partial class TreeControlViewModel : ObservableObject
    {
        public TreeControlViewModel()
        {
            TreeViewModel = new TreeViewModel();
            InitializeData();

            // 监听搜索属性变化
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SearchKeyword) ||
                    e.PropertyName == nameof(ShowCities) ||
                    e.PropertyName == nameof(ShowCountries) ||
                    e.PropertyName == nameof(CaseSensitive))
                {
                    UpdateFilteredItems();
                }
            };

            UpdateFilteredItems();
        }

        #region 基本属性

        [ObservableProperty]
        public partial TreeViewModel TreeViewModel { get; set; }

        [ObservableProperty]
        public partial string SelectedCityPath { get; set; } = "";

        [ObservableProperty]
        public partial string LastOperation { get; set; } = "等待操作...";

        #endregion 基本属性

        #region 文件系统数据

        [ObservableProperty]
        public partial ObservableCollection<City> FileSystemItems { get; set; } = new();

        #endregion 文件系统数据

        #region 组织架构数据

        [ObservableProperty]
        public partial ObservableCollection<City> OrganizationItems { get; set; } = new();

        #endregion 组织架构数据

        #region 搜索过滤

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SearchResultCount))]
        public partial string SearchKeyword { get; set; } = "";

        [ObservableProperty]
        public partial bool ShowCities { get; set; } = true;

        [ObservableProperty]
        public partial bool ShowCountries { get; set; } = true;

        [ObservableProperty]
        public partial bool CaseSensitive { get; set; } = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SearchResultCount))]
        public partial ObservableCollection<City> FilteredItems { get; set; } = new();

        public int SearchResultCount => CountAllItems(FilteredItems);

        #endregion 搜索过滤

        #region 初始化数据

        private void InitializeData()
        {
            // 初始化城市数据
            TreeViewModel.Cities = new ObservableCollection<City>
            {
                new City { NodePath = "世界", Level = 0, Population = "", Cities = new ObservableCollection<City>
                {
                    new City { NodePath = "亚洲", Level = 1, Population = "46亿人", Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "中国", Level = 1, Population = "14亿人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "北京", Level = 2, Population = "2171万人" },
                            new City { NodePath = "上海", Level = 2, Population = "2487万人" },
                            new City { NodePath = "广州", Level = 2, Population = "1867万人" },
                            new City { NodePath = "深圳", Level = 2, Population = "1756万人" }
                        }},
                        new City { NodePath = "日本", Level = 1, Population = "1.25亿人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "东京", Level = 2, Population = "1396万人" },
                            new City { NodePath = "大阪", Level = 2, Population = "882万人" },
                            new City { NodePath = "横滨", Level = 2, Population = "377万人" }
                        }},
                        new City { NodePath = "韩国", Level = 1, Population = "5180万人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "首尔", Level = 2, Population = "967万人" },
                            new City { NodePath = "釜山", Level = 2, Population = "342万人" }
                        }}
                    }},
                    new City { NodePath = "欧洲", Level = 1, Population = "7.4亿人", Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "英国", Level = 1, Population = "6708万人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "伦敦", Level = 2, Population = "898万人" },
                            new City { NodePath = "曼彻斯特", Level = 2, Population = "280万人" }
                        }},
                        new City { NodePath = "法国", Level = 1, Population = "6739万人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "巴黎", Level = 2, Population = "1084万人" },
                            new City { NodePath = "里昂", Level = 2, Population = "233万人" }
                        }},
                        new City { NodePath = "德国", Level = 1, Population = "8319万人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "柏林", Level = 2, Population = "365万人" },
                            new City { NodePath = "慕尼黑", Level = 2, Population = "147万人" }
                        }}
                    }},
                    new City { NodePath = "北美洲", Level = 1, Population = "5.8亿人", Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "美国", Level = 1, Population = "3.3亿人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "纽约", Level = 2, Population = "851万人" },
                            new City { NodePath = "洛杉矶", Level = 2, Population = "399万人" },
                            new City { NodePath = "芝加哥", Level = 2, Population = "271万人" }
                        }},
                        new City { NodePath = "加拿大", Level = 1, Population = "3800万人", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "多伦多", Level = 2, Population = "294万人" },
                            new City { NodePath = "温哥华", Level = 2, Population = "67万人" }
                        }}
                    }}
                }}
            };

            // 初始化文件系统数据
            FileSystemItems = new ObservableCollection<City>
            {
                new City { NodePath = "我的电脑", IsExpanded = true, Cities = new ObservableCollection<City>
                {
                    new City { NodePath = "C:", IsExpanded = true, Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "Windows", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "System32" },
                            new City { NodePath = "notepad.exe", IsFile = true, FileType = "exe", FileSize = "1.2 MB" }
                        }},
                        new City { NodePath = "Users", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "Documents", Cities = new ObservableCollection<City>
                            {
                                new City { NodePath = "报告.txt", IsFile = true, FileType = "txt", FileSize = "15 KB" },
                                new City { NodePath = "图片.jpg", IsFile = true, FileType = "jpg", FileSize = "2.5 MB" },
                                new City { NodePath = "音乐.mp3", IsFile = true, FileType = "mp3", FileSize = "4.1 MB" }
                            }}
                        }}
                    }},
                    new City { NodePath = "D:", Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "Projects" },
                        new City { NodePath = "Media" }
                    }}
                }}
            };

            // 初始化组织架构数据
            OrganizationItems = new ObservableCollection<City>
            {
                new City { NodePath = "科技公司", Cities = new ObservableCollection<City>
                {
                    new City { NodePath = "技术部", IsDepartment = true, EmployeeCount = "(25人)", Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "张三", IsEmployee = true, Position = "Director" },
                        new City { NodePath = "李四", IsEmployee = true, Position = "Manager" },
                        new City { NodePath = "前端组", IsDepartment = true, EmployeeCount = "(8人)", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "小明", IsEmployee = true, Position = "Leader" },
                            new City { NodePath = "小红", IsEmployee = true, Position = "Developer" }
                        }},
                        new City { NodePath = "后端组", IsDepartment = true, EmployeeCount = "(12人)", Cities = new ObservableCollection<City>
                        {
                            new City { NodePath = "小华", IsEmployee = true, Position = "Leader" },
                            new City { NodePath = "小林", IsEmployee = true, Position = "Developer" }
                        }}
                    }},
                    new City { NodePath = "市场部", IsDepartment = true, EmployeeCount = "(15人)", Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "陈总", IsEmployee = true, Position = "Director" },
                        new City { NodePath = "刘经理", IsEmployee = true, Position = "Manager" }
                    }},
                    new City { NodePath = "人事部", IsDepartment = true, EmployeeCount = "(8人)", Cities = new ObservableCollection<City>
                    {
                        new City { NodePath = "HR经理", IsEmployee = true, Position = "Manager" },
                        new City { NodePath = "招聘专员", IsEmployee = true, Position = "Specialist" }
                    }}
                }}
            };
        }

        #endregion 初始化数据

        #region 搜索过滤方法

        private void UpdateFilteredItems()
        {
            if (TreeViewModel.Cities == null)
            {
                FilteredItems = new ObservableCollection<City>();
                return;
            }

            var filtered = FilterItems(TreeViewModel.Cities.ToList());
            FilteredItems = new ObservableCollection<City>(filtered);
        }

        private List<City> FilterItems(List<City> items)
        {
            var result = new List<City>();

            foreach (var item in items)
            {
                var matches = string.IsNullOrEmpty(SearchKeyword) ||
                             (CaseSensitive ?
                              item.NodePath.Contains(SearchKeyword) :
                              item.NodePath.ToLower().Contains(SearchKeyword.ToLower()));

                // 根据类型过滤
                var typeMatches = true;
                if (item.Level == 2) // 城市
                {
                    typeMatches = ShowCities;
                }
                else if (item.Level == 1) // 国家
                {
                    typeMatches = ShowCountries;
                }

                if (matches && typeMatches)
                {
                    var newItem = new City
                    {
                        NodePath = item.NodePath,
                        Level = item.Level,
                        Population = item.Population,
                        Cities = new ObservableCollection<City>()
                    };

                    if (item.Cities != null && item.Cities.Any())
                    {
                        var filteredChildren = FilterItems(item.Cities.ToList());
                        foreach (var child in filteredChildren)
                        {
                            newItem.Cities.Add(child);
                        }
                    }

                    result.Add(newItem);
                }
                else if (item.Cities != null && item.Cities.Any())
                {
                    // 即使当前项不匹配，也检查子项
                    var filteredChildren = FilterItems(item.Cities.ToList());
                    if (filteredChildren.Any())
                    {
                        var newItem = new City
                        {
                            NodePath = item.NodePath,
                            Level = item.Level,
                            Population = item.Population,
                            Cities = new ObservableCollection<City>(filteredChildren)
                        };
                        result.Add(newItem);
                    }
                }
            }

            return result;
        }

        private int CountAllItems(ObservableCollection<City> items)
        {
            if (items == null) return 0;

            int count = items.Count;
            foreach (var item in items)
            {
                if (item.Cities != null)
                {
                    count += CountAllItems(item.Cities);
                }
            }
            return count;
        }

        #endregion 搜索过滤方法

        #region 城市相关命令

        [RelayCommand]
        private void ExpandAll()
        {
            SetExpandedState(TreeViewModel.Cities, true);
            LastOperation = "展开所有节点";
            NotificationService.Instance.Information("已展开所有节点");
        }

        [RelayCommand]
        private void CollapseAll()
        {
            SetExpandedState(TreeViewModel.Cities, false);
            LastOperation = "折叠所有节点";
            NotificationService.Instance.Information("已折叠所有节点");
        }

        [RelayCommand]
        private void AddCity()
        {
            var random = new Random();
            var newCity = new City
            {
                NodePath = $"新城市{random.Next(100, 999)}",
                Level = 2,
                Population = $"{random.Next(100, 2000)}万人"
            };

            if (TreeViewModel.Cities?.FirstOrDefault()?.Cities?.FirstOrDefault()?.Cities?.FirstOrDefault()?.Cities != null)
            {
                TreeViewModel.Cities.First().Cities.First().Cities.First().Cities.Add(newCity);
                LastOperation = $"添加城市: {newCity.NodePath}";
                NotificationService.Instance.Success($"已添加城市: {newCity.NodePath}");
            }
        }

        [RelayCommand]
        private void AddCountry()
        {
            var random = new Random();
            var newCountry = new City
            {
                NodePath = $"新国家{random.Next(100, 999)}",
                Level = 1,
                Population = $"{random.Next(1000, 5000)}万人",
                Cities = new ObservableCollection<City>
                {
                    new City { NodePath = "首都", Level = 2, Population = "500万人" }
                }
            };

            if (TreeViewModel.Cities?.FirstOrDefault()?.Cities?.FirstOrDefault()?.Cities != null)
            {
                TreeViewModel.Cities.First().Cities.First().Cities.Add(newCountry);
                LastOperation = $"添加国家: {newCountry.NodePath}";
                NotificationService.Instance.Success($"已添加国家: {newCountry.NodePath}");
            }
        }

        [RelayCommand]
        private void ClearSelection()
        {
            SelectedCityPath = "";
            TreeViewModel.City = null;
            LastOperation = "清空选择";
            NotificationService.Instance.Information("已清空选择");
        }

        #endregion 城市相关命令

        #region 文件系统命令

        [RelayCommand]
        private void NewFolder()
        {
            var random = new Random();
            var newFolder = new City
            {
                NodePath = $"新文件夹{random.Next(1, 100)}",
                Cities = new ObservableCollection<City>()
            };

            if (FileSystemItems?.FirstOrDefault()?.Cities?.FirstOrDefault()?.Cities != null)
            {
                FileSystemItems.First().Cities.First().Cities.Add(newFolder);
                LastOperation = $"新建文件夹: {newFolder.NodePath}";
                NotificationService.Instance.Success($"已创建文件夹: {newFolder.NodePath}");
            }
        }

        [RelayCommand]
        private void NewFile()
        {
            var random = new Random();
            var extensions = new[] { "txt", "jpg", "mp3", "exe" };
            var ext = extensions[random.Next(extensions.Length)];
            var newFile = new City
            {
                NodePath = $"新文件{random.Next(1, 100)}.{ext}",
                IsFile = true,
                FileType = ext,
                FileSize = $"{random.Next(1, 100)} KB"
            };

            if (FileSystemItems?.FirstOrDefault()?.Cities?.FirstOrDefault()?.Cities?.FirstOrDefault()?.Cities != null)
            {
                FileSystemItems.First().Cities.First().Cities.First().Cities.Add(newFile);
                LastOperation = $"新建文件: {newFile.NodePath}";
                NotificationService.Instance.Success($"已创建文件: {newFile.NodePath}");
            }
        }

        [RelayCommand]
        private void DeleteItem()
        {
            LastOperation = "删除选中项";
            NotificationService.Instance.Warning("删除功能需要选中具体项目");
        }

        #endregion 文件系统命令

        #region 组织架构命令

        [RelayCommand]
        private void AddDepartment()
        {
            var random = new Random();
            var newDept = new City
            {
                NodePath = $"新部门{random.Next(1, 100)}",
                IsDepartment = true,
                EmployeeCount = "(0人)",
                Cities = new ObservableCollection<City>()
            };

            if (OrganizationItems?.FirstOrDefault()?.Cities != null)
            {
                OrganizationItems.First().Cities.Add(newDept);
                LastOperation = $"添加部门: {newDept.NodePath}";
                NotificationService.Instance.Success($"已添加部门: {newDept.NodePath}");
            }
        }

        [RelayCommand]
        private void AddEmployee()
        {
            var random = new Random();
            var positions = new[] { "Developer", "Manager", "Leader", "Specialist" };
            var names = new[] { "张三", "李四", "王五", "赵六", "孙七", "周八", "吴九", "郑十" };

            var newEmployee = new City
            {
                NodePath = names[random.Next(names.Length)] + random.Next(1, 100),
                IsEmployee = true,
                Position = positions[random.Next(positions.Length)]
            };

            if (OrganizationItems?.FirstOrDefault()?.Cities?.FirstOrDefault()?.Cities != null)
            {
                OrganizationItems.First().Cities.First().Cities.Add(newEmployee);
                LastOperation = $"添加员工: {newEmployee.NodePath}";
                NotificationService.Instance.Success($"已添加员工: {newEmployee.NodePath}");
            }
        }

        [RelayCommand]
        private void ShowStats()
        {
            var totalDepts = CountDepartments(OrganizationItems);
            var totalEmps = CountEmployees(OrganizationItems);

            LastOperation = $"统计: {totalDepts}个部门, {totalEmps}名员工";
            NotificationService.Instance.Information($"组织统计: {totalDepts}个部门, {totalEmps}名员工");
        }

        #endregion 组织架构命令

        #region 辅助方法

        private void SetExpandedState(ObservableCollection<City> items, bool isExpanded)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                item.IsExpanded = isExpanded;
                if (item.Cities != null)
                {
                    SetExpandedState(item.Cities, isExpanded);
                }
            }
        }

        private int CountDepartments(ObservableCollection<City> items)
        {
            if (items == null) return 0;

            int count = items.Count(i => i.IsDepartment);
            foreach (var item in items)
            {
                if (item.Cities != null)
                {
                    count += CountDepartments(item.Cities);
                }
            }
            return count;
        }

        private int CountEmployees(ObservableCollection<City> items)
        {
            if (items == null) return 0;

            int count = items.Count(i => i.IsEmployee);
            foreach (var item in items)
            {
                if (item.Cities != null)
                {
                    count += CountEmployees(item.Cities);
                }
            }
            return count;
        }

        #endregion 辅助方法
    }

    public partial class TreeViewModel : ObservableObject
    {
        public TreeViewModel()
        {
        }

        [ObservableProperty]
        public partial City City { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<City> Cities { get; set; }

        [ObservableProperty]
        public partial string Text { get; set; } = "NewYork";

        [RelayCommand]
        private void GetCity()
        {
            if (City != null)
            {
                NotificationService.Instance.Information($"选择的城市为：{City.NodePath}");
            }
            else
            {
                NotificationService.Instance.Warning("请先选择一个城市");
            }
        }
    }

    // 扩展 City 类以支持更多属性
    public partial class City : ObservableObject
    {
        [ObservableProperty]
        public partial string NodePath { get; set; } = "";

        [ObservableProperty]
        public partial ObservableCollection<City> Cities { get; set; }

        [ObservableProperty]
        public partial int Level { get; set; } = 0;

        [ObservableProperty]
        public partial string Population { get; set; } = "";

        [ObservableProperty]
        public partial bool IsExpanded { get; set; } = false;

        [ObservableProperty]
        public partial bool IsSelected { get; set; } = false;

        // 文件系统属性
        [ObservableProperty]
        public partial bool IsFile { get; set; } = false;

        [ObservableProperty]
        public partial string FileType { get; set; } = "";

        [ObservableProperty]
        public partial string FileSize { get; set; } = "";

        // 组织架构属性
        [ObservableProperty]
        public partial bool IsDepartment { get; set; } = false;

        [ObservableProperty]
        public partial bool IsEmployee { get; set; } = false;

        [ObservableProperty]
        public partial string Position { get; set; } = "";

        [ObservableProperty]
        public partial string EmployeeCount { get; set; } = "";
    }
}