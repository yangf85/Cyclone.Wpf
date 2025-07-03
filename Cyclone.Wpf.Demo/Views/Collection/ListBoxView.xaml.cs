using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// ListBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class ListBoxView : UserControl
    {
        public ListBoxView()
        {
            InitializeComponent();
            DataContext = new ListBoxViewModel();
        }
    }

    public partial class ListBoxViewModel : ObservableObject
    {
        private readonly ObservableCollection<FakerData> _originalData;
        private ICollectionView _filteredData;

        // 基础ListBox数据
        [ObservableProperty]
        public partial ObservableCollection<FakerData> BasicData { get; set; }

        [ObservableProperty]
        public partial FakerData? SelectedBasicPerson { get; set; }

        // 高级ListBox的过滤和排序属性
        [ObservableProperty]
        public partial string SearchText { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string SelectedAgeRange { get; set; } = "全部";

        [ObservableProperty]
        public partial string SelectedSortOption { get; set; } = "姓名";

        [ObservableProperty]
        public partial bool IsDescending { get; set; } = false;

        [ObservableProperty]
        public partial string SelectedGroupOption { get; set; } = "无";

        [ObservableProperty]
        public partial string SelectedStatusFilter { get; set; } = "全部";

        [ObservableProperty]
        public partial FakerData? SelectedAdvancedPerson { get; set; }

        [ObservableProperty]
        public partial string StatusText { get; set; } = string.Empty;

        // 选项集合
        public ObservableCollection<string> AgeRanges { get; }

        public ObservableCollection<string> SortOptions { get; }
        public ObservableCollection<string> GroupOptions { get; }
        public ObservableCollection<string> StatusFilters { get; }

        public ICollectionView FilteredData
        {
            get => _filteredData;
            private set => SetProperty(ref _filteredData, value);
        }

        public ListBoxViewModel()
        {
            // 生成测试数据
            var testData = FakerDataHelper.GenerateFakerDataCollection(50);

            // 基础ListBox数据
            BasicData = new ObservableCollection<FakerData>(testData);

            // 高级ListBox数据
            _originalData = new ObservableCollection<FakerData>(testData);

            // 初始化选项
            AgeRanges = new ObservableCollection<string> { "全部", "0-18", "19-30", "31-45", "46-60", "60+" };
            SortOptions = new ObservableCollection<string> { "姓名", "年龄", "城市", "邮箱", "状态" };
            GroupOptions = new ObservableCollection<string> { "无", "城市", "国家", "状态" };
            StatusFilters = new ObservableCollection<string> { "全部", "激活", "未激活", "待激活" };

            // 设置高级ListBox的数据视图
            FilteredData = CollectionViewSource.GetDefaultView(_originalData);
            FilteredData.Filter = FilterItems;

            UpdateStatusText();
        }

        // 属性变化时重新应用过滤和排序
        partial void OnSearchTextChanged(string value) => ApplyFiltersAndSort();

        partial void OnSelectedAgeRangeChanged(string value) => ApplyFiltersAndSort();

        partial void OnSelectedSortOptionChanged(string value) => ApplyFiltersAndSort();

        partial void OnIsDescendingChanged(bool value) => ApplyFiltersAndSort();

        partial void OnSelectedGroupOptionChanged(string value) => ApplyFiltersAndSort();

        partial void OnSelectedStatusFilterChanged(string value) => ApplyFiltersAndSort();

        [RelayCommand]
        private void ClearFilters()
        {
            SearchText = string.Empty;
            SelectedAgeRange = "全部";
            SelectedSortOption = "姓名";
            IsDescending = false;
            SelectedGroupOption = "无";
            SelectedStatusFilter = "全部";
        }

        private void ApplyFiltersAndSort()
        {
            // 刷新过滤
            FilteredData.Refresh();

            // 清除现有排序和分组
            FilteredData.SortDescriptions.Clear();
            FilteredData.GroupDescriptions.Clear();

            // 应用排序
            var sortProperty = SelectedSortOption switch
            {
                "姓名" => nameof(FakerData.FirstName),
                "年龄" => nameof(FakerData.Age),
                "城市" => nameof(FakerData.City),
                "邮箱" => nameof(FakerData.Email),
                "状态" => nameof(FakerData.Status),
                _ => nameof(FakerData.FirstName)
            };

            var sortDirection = IsDescending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            FilteredData.SortDescriptions.Add(new SortDescription(sortProperty, sortDirection));

            // 应用分组
            if (SelectedGroupOption != "无")
            {
                var groupProperty = SelectedGroupOption switch
                {
                    "城市" => nameof(FakerData.City),
                    "国家" => nameof(FakerData.Country),
                    "状态" => nameof(FakerData.Status),
                    _ => null
                };

                if (groupProperty != null)
                {
                    FilteredData.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty));
                }
            }

            UpdateStatusText();
        }

        private bool FilterItems(object item)
        {
            if (item is not FakerData person)
                return false;

            // 搜索文本过滤
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchLower = SearchText.ToLower();
                if (!person.FirstName.ToLower().Contains(searchLower) &&
                    !person.LastName.ToLower().Contains(searchLower) &&
                    !person.Email.ToLower().Contains(searchLower) &&
                    !person.City.ToLower().Contains(searchLower) &&
                    !person.Country.ToLower().Contains(searchLower))
                {
                    return false;
                }
            }

            // 年龄范围过滤
            if (SelectedAgeRange != "全部")
            {
                var ageInRange = SelectedAgeRange switch
                {
                    "0-18" => person.Age >= 0 && person.Age <= 18,
                    "19-30" => person.Age >= 19 && person.Age <= 30,
                    "31-45" => person.Age >= 31 && person.Age <= 45,
                    "46-60" => person.Age >= 46 && person.Age <= 60,
                    "60+" => person.Age > 60,
                    _ => true
                };

                if (!ageInRange)
                    return false;
            }

            // 状态过滤
            if (SelectedStatusFilter != "全部")
            {
                var statusMatch = SelectedStatusFilter switch
                {
                    "激活" => person.Status == UserStatus.Active,
                    "未激活" => person.Status == UserStatus.Inactive,
                    "待激活" => person.Status == UserStatus.Pending,
                    _ => true
                };

                if (!statusMatch)
                    return false;
            }

            return true;
        }

        private void UpdateStatusText()
        {
            if (FilteredData != null)
            {
                var filteredCount = FilteredData.Cast<object>().Count();
                var totalCount = _originalData.Count;

                StatusText = filteredCount == totalCount
                    ? $"显示 {totalCount} 项"
                    : $"显示 {filteredCount} / {totalCount} 项";
            }
        }
    }

    // 简单的分组转换器
    public class AgeGroupConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is FakerData person)
            {
                return person.Age switch
                {
                    >= 0 and <= 18 => "未成年 (0-18)",
                    >= 19 and <= 30 => "青年 (19-30)",
                    >= 31 and <= 45 => "中年 (31-45)",
                    >= 46 and <= 60 => "中老年 (46-60)",
                    > 60 => "老年 (60+)",
                    _ => "未知"
                };
            }
            return "未知";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}