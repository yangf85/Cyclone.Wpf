using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views;

/// <summary>
/// FilterBoxView.xaml 的交互逻辑
/// </summary>
public partial class FilterBoxView : UserControl
{
    public FilterBoxView()
    {
        InitializeComponent();
        DataContext = new FilterBoxViewModel();
    }
}

public partial class FilterBoxViewModel : ObservableValidator
{
    #region 数字过滤器属性

    [ObservableProperty]
    public partial bool IsPriceFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial NumberOperator PriceOperator { get; set; } = NumberOperator.GreaterThanOrEqual;

    [ObservableProperty]
    public partial double PriceValue { get; set; } = 100.0;

    [ObservableProperty]
    public partial bool IsAgeFilterActive { get; set; } = false;

    [ObservableProperty]
    public partial NumberOperator AgeOperator { get; set; } = NumberOperator.GreaterThan;

    [ObservableProperty]
    public partial double AgeValue { get; set; } = 18;

    [ObservableProperty]
    public partial bool IsRatingFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial NumberOperator RatingOperator { get; set; } = NumberOperator.GreaterThanOrEqual;

    [ObservableProperty]
    public partial double RatingValue { get; set; } = 4.0;

    // 数字过滤器对齐示例
    [ObservableProperty]
    public partial bool IsAlignedPriceFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial NumberOperator AlignedPriceOperator { get; set; } = NumberOperator.LessThanOrEqual;

    [ObservableProperty]
    public partial double AlignedPriceValue { get; set; } = 999.99;

    [ObservableProperty]
    public partial bool IsAlignedStockFilterActive { get; set; } = false;

    [ObservableProperty]
    public partial NumberOperator AlignedStockOperator { get; set; } = NumberOperator.GreaterThan;

    [ObservableProperty]
    public partial double AlignedStockValue { get; set; } = 10;

    [ObservableProperty]
    public partial bool IsAlignedWeightFilterActive { get; set; } = false;

    [ObservableProperty]
    public partial NumberOperator AlignedWeightOperator { get; set; } = NumberOperator.LessThan;

    [ObservableProperty]
    public partial double AlignedWeightValue { get; set; } = 5.0;

    [ObservableProperty]
    public partial string NumberFilterResults { get; set; } = "点击按钮查看数字过滤结果...";

    #endregion 数字过滤器属性

    #region 文本过滤器属性

    [ObservableProperty]
    public partial bool IsProductNameFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial TextOperator ProductNameOperator { get; set; } = TextOperator.Contains;

    [ObservableProperty]
    public partial string ProductNameText { get; set; } = "手机";

    [ObservableProperty]
    public partial bool IsBrandFilterActive { get; set; } = false;

    [ObservableProperty]
    public partial TextOperator BrandOperator { get; set; } = TextOperator.Equal;

    [ObservableProperty]
    public partial string BrandText { get; set; } = "苹果";

    [ObservableProperty]
    public partial bool IsDescriptionFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial TextOperator DescriptionOperator { get; set; } = TextOperator.Contains;

    [ObservableProperty]
    public partial string DescriptionText { get; set; } = "高清";

    // 文本过滤器对齐示例
    [ObservableProperty]
    public partial bool IsAlignedEmailFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial TextOperator AlignedEmailOperator { get; set; } = TextOperator.EndsWith;

    [ObservableProperty]
    public partial string AlignedEmailText { get; set; } = "@gmail.com";

    [ObservableProperty]
    public partial bool IsAlignedNameFilterActive { get; set; } = false;

    [ObservableProperty]
    public partial TextOperator AlignedNameOperator { get; set; } = TextOperator.StartsWith;

    [ObservableProperty]
    public partial string AlignedNameText { get; set; } = "张";

    [ObservableProperty]
    public partial bool IsAlignedCompanyFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial TextOperator AlignedCompanyOperator { get; set; } = TextOperator.Contains;

    [ObservableProperty]
    public partial string AlignedCompanyText { get; set; } = "科技";

    // 扩展功能
    [ObservableProperty]
    public partial bool IsAdvancedFilterActive { get; set; } = true;

    [ObservableProperty]
    public partial TextOperator AdvancedOperator { get; set; } = TextOperator.Contains;

    [ObservableProperty]
    public partial string AdvancedText { get; set; } = "智能";

    [ObservableProperty]
    public partial bool IsRegexFilterActive { get; set; } = false;

    [ObservableProperty]
    public partial string RegexText { get; set; } = @"\d{4}";

    [ObservableProperty]
    public partial string TextFilterResults { get; set; } = "点击按钮查看文本过滤结果...";

    #endregion 文本过滤器属性

    #region 数字过滤器命令

    [RelayCommand]
    private void ApplyNumberFilters()
    {
        var activeFilters = new List<string>();

        if (IsPriceFilterActive)
            activeFilters.Add($"价格 {GetNumberOperatorSymbol(PriceOperator)} {PriceValue:F2}");

        if (IsAgeFilterActive)
            activeFilters.Add($"年龄 {GetNumberOperatorSymbol(AgeOperator)} {AgeValue:F0}");

        if (IsRatingFilterActive)
            activeFilters.Add($"评分 {GetNumberOperatorSymbol(RatingOperator)} {RatingValue:F1}");

        if (IsAlignedPriceFilterActive)
            activeFilters.Add($"对齐价格 {GetNumberOperatorSymbol(AlignedPriceOperator)} {AlignedPriceValue:F2}");

        if (IsAlignedStockFilterActive)
            activeFilters.Add($"对齐库存 {GetNumberOperatorSymbol(AlignedStockOperator)} {AlignedStockValue:F0}");

        if (IsAlignedWeightFilterActive)
            activeFilters.Add($"对齐重量 {GetNumberOperatorSymbol(AlignedWeightOperator)} {AlignedWeightValue:F2}kg");

        var message = activeFilters.Any()
            ? $"数字过滤条件:\n\n{string.Join("\n", activeFilters.Select(f => $"• {f}"))}\n\n共 {activeFilters.Count} 个条件"
            : "没有激活的数字过滤条件";

        NumberFilterResults = message;
        MessageBox.Show(message, "数字过滤", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ClearNumberFilters()
    {
        IsPriceFilterActive = false;
        IsAgeFilterActive = false;
        IsRatingFilterActive = false;
        IsAlignedPriceFilterActive = false;
        IsAlignedStockFilterActive = false;
        IsAlignedWeightFilterActive = false;

        NumberFilterResults = "数字过滤条件已清除";
        MessageBox.Show("数字过滤条件已清除", "清除", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void TestNumberFilters()
    {
        var testData = new[]
        {
            new { Name = "产品A", Price = 150.0, Age = 25, Rating = 4.2, Stock = 45, Weight = 1.8 },
            new { Name = "产品B", Price = 75.0, Age = 17, Rating = 3.8, Stock = 120, Weight = 3.2 },
            new { Name = "产品C", Price = 350.0, Age = 30, Rating = 4.8, Stock = 8, Weight = 0.9 },
            new { Name = "产品D", Price = 99.99, Age = 22, Rating = 3.5, Stock = 50, Weight = 2.1 },
            new { Name = "产品E", Price = 650.0, Age = 45, Rating = 4.9, Stock = 75, Weight = 6.7 }
        };

        var results = new StringBuilder();
        results.AppendLine("数字过滤测试:");
        results.AppendLine(new string('=', 40));

        foreach (var item in testData)
        {
            var passed = true;
            var reasons = new List<string>();

            if (IsPriceFilterActive && !CheckNumberFilter(item.Price, PriceOperator, PriceValue))
            {
                passed = false;
                reasons.Add($"价格 {item.Price:F2} 不满足 {GetNumberOperatorSymbol(PriceOperator)} {PriceValue:F2}");
            }

            if (IsAgeFilterActive && !CheckNumberFilter(item.Age, AgeOperator, AgeValue))
            {
                passed = false;
                reasons.Add($"年龄 {item.Age} 不满足 {GetNumberOperatorSymbol(AgeOperator)} {AgeValue:F0}");
            }

            if (IsRatingFilterActive && !CheckNumberFilter(item.Rating, RatingOperator, RatingValue))
            {
                passed = false;
                reasons.Add($"评分 {item.Rating:F1} 不满足 {GetNumberOperatorSymbol(RatingOperator)} {RatingValue:F1}");
            }

            if (IsAlignedPriceFilterActive && !CheckNumberFilter(item.Price, AlignedPriceOperator, AlignedPriceValue))
            {
                passed = false;
                reasons.Add($"对齐价格不满足条件");
            }

            if (IsAlignedStockFilterActive && !CheckNumberFilter(item.Stock, AlignedStockOperator, AlignedStockValue))
            {
                passed = false;
                reasons.Add($"对齐库存不满足条件");
            }

            if (IsAlignedWeightFilterActive && !CheckNumberFilter(item.Weight, AlignedWeightOperator, AlignedWeightValue))
            {
                passed = false;
                reasons.Add($"对齐重量不满足条件");
            }

            results.AppendLine($"{item.Name}: {(passed ? "✓" : "✗")}");
            if (!passed)
            {
                foreach (var reason in reasons)
                {
                    results.AppendLine($"  - {reason}");
                }
            }
        }

        var passedCount = testData.Count(item =>
        {
            bool passes = true;
            if (IsPriceFilterActive) passes &= CheckNumberFilter(item.Price, PriceOperator, PriceValue);
            if (IsAgeFilterActive) passes &= CheckNumberFilter(item.Age, AgeOperator, AgeValue);
            if (IsRatingFilterActive) passes &= CheckNumberFilter(item.Rating, RatingOperator, RatingValue);
            if (IsAlignedPriceFilterActive) passes &= CheckNumberFilter(item.Price, AlignedPriceOperator, AlignedPriceValue);
            if (IsAlignedStockFilterActive) passes &= CheckNumberFilter(item.Stock, AlignedStockOperator, AlignedStockValue);
            if (IsAlignedWeightFilterActive) passes &= CheckNumberFilter(item.Weight, AlignedWeightOperator, AlignedWeightValue);
            return passes;
        });

        results.AppendLine($"\n通过: {passedCount}/{testData.Length}");
        NumberFilterResults = results.ToString();
    }

    #endregion 数字过滤器命令

    #region 文本过滤器命令

    [RelayCommand]
    private void ApplyTextFilters()
    {
        var activeFilters = new List<string>();

        if (IsProductNameFilterActive)
            activeFilters.Add($"产品名称 {GetTextOperatorSymbol(ProductNameOperator)} \"{ProductNameText}\"");

        if (IsBrandFilterActive)
            activeFilters.Add($"品牌 {GetTextOperatorSymbol(BrandOperator)} \"{BrandText}\"");

        if (IsDescriptionFilterActive)
            activeFilters.Add($"描述 {GetTextOperatorSymbol(DescriptionOperator)} \"{DescriptionText}\"");

        if (IsAlignedEmailFilterActive)
            activeFilters.Add($"邮箱 {GetTextOperatorSymbol(AlignedEmailOperator)} \"{AlignedEmailText}\"");

        if (IsAlignedNameFilterActive)
            activeFilters.Add($"姓名 {GetTextOperatorSymbol(AlignedNameOperator)} \"{AlignedNameText}\"");

        if (IsAlignedCompanyFilterActive)
            activeFilters.Add($"公司 {GetTextOperatorSymbol(AlignedCompanyOperator)} \"{AlignedCompanyText}\"");

        if (IsAdvancedFilterActive)
            activeFilters.Add($"高级搜索 {GetTextOperatorSymbol(AdvancedOperator)} \"{AdvancedText}\"");

        if (IsRegexFilterActive)
            activeFilters.Add($"正则搜索 R \"{RegexText}\"");

        var message = activeFilters.Any()
            ? $"文本过滤条件:\n\n{string.Join("\n", activeFilters.Select(f => $"• {f}"))}\n\n共 {activeFilters.Count} 个条件"
            : "没有激活的文本过滤条件";

        TextFilterResults = message;
        MessageBox.Show(message, "文本过滤", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ClearTextFilters()
    {
        IsProductNameFilterActive = false;
        IsBrandFilterActive = false;
        IsDescriptionFilterActive = false;
        IsAlignedEmailFilterActive = false;
        IsAlignedNameFilterActive = false;
        IsAlignedCompanyFilterActive = false;
        IsAdvancedFilterActive = false;
        IsRegexFilterActive = false;

        TextFilterResults = "文本过滤条件已清除";
        MessageBox.Show("文本过滤条件已清除", "清除", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void TestTextFilters()
    {
        var testData = new[]
        {
            new { Name = "iPhone 15 Pro 手机", Brand = "苹果", Description = "高清摄像头，智能芯片", Email = "user1@gmail.com", FullName = "张三", Company = "北京苹果科技" },
            new { Name = "华为 Mate 60 手机", Brand = "华为", Description = "麒麟芯片，智能拍照", Email = "user2@qq.com", FullName = "李四", Company = "深圳华为科技" },
            new { Name = "小米14 智能手机", Brand = "小米", Description = "骁龙处理器，高清屏幕", Email = "admin@gmail.com", FullName = "王五", Company = "小米科技公司" },
            new { Name = "三星 Galaxy S24", Brand = "三星", Description = "AMOLED屏幕，AI相机", Email = "test@163.com", FullName = "赵六", Company = "三星电子" },
            new { Name = "OPPO Find X7", Brand = "OPPO", Description = "影像旗舰，停产型号", Email = "support@gmail.com", FullName = "钱七", Company = "OPPO科技" }
        };

        var results = new StringBuilder();
        results.AppendLine("文本过滤测试:");
        results.AppendLine(new string('=', 40));

        foreach (var item in testData)
        {
            var passed = true;
            var reasons = new List<string>();

            if (IsProductNameFilterActive && !CheckTextFilter(item.Name, ProductNameOperator, ProductNameText))
            {
                passed = false;
                reasons.Add($"产品名称不满足 {GetTextOperatorSymbol(ProductNameOperator)} \"{ProductNameText}\"");
            }

            if (IsBrandFilterActive && !CheckTextFilter(item.Brand, BrandOperator, BrandText))
            {
                passed = false;
                reasons.Add($"品牌不满足 {GetTextOperatorSymbol(BrandOperator)} \"{BrandText}\"");
            }

            if (IsDescriptionFilterActive && !CheckTextFilter(item.Description, DescriptionOperator, DescriptionText))
            {
                passed = false;
                reasons.Add($"描述不满足 {GetTextOperatorSymbol(DescriptionOperator)} \"{DescriptionText}\"");
            }

            if (IsAlignedEmailFilterActive && !CheckTextFilter(item.Email, AlignedEmailOperator, AlignedEmailText))
            {
                passed = false;
                reasons.Add($"邮箱不满足条件");
            }

            if (IsAlignedNameFilterActive && !CheckTextFilter(item.FullName, AlignedNameOperator, AlignedNameText))
            {
                passed = false;
                reasons.Add($"姓名不满足条件");
            }

            if (IsAlignedCompanyFilterActive && !CheckTextFilter(item.Company, AlignedCompanyOperator, AlignedCompanyText))
            {
                passed = false;
                reasons.Add($"公司不满足条件");
            }

            if (IsAdvancedFilterActive && !CheckTextFilter(item.Description, AdvancedOperator, AdvancedText))
            {
                passed = false;
                reasons.Add($"高级搜索不满足条件");
            }

            if (IsRegexFilterActive && !CheckTextFilter(item.Name, TextOperator.Regex, RegexText))
            {
                passed = false;
                reasons.Add($"正则搜索不匹配");
            }

            results.AppendLine($"{item.Name}: {(passed ? "✓" : "✗")}");
            if (!passed)
            {
                foreach (var reason in reasons)
                {
                    results.AppendLine($"  - {reason}");
                }
            }
        }

        var passedCount = testData.Count(item =>
        {
            bool passes = true;
            if (IsProductNameFilterActive) passes &= CheckTextFilter(item.Name, ProductNameOperator, ProductNameText);
            if (IsBrandFilterActive) passes &= CheckTextFilter(item.Brand, BrandOperator, BrandText);
            if (IsDescriptionFilterActive) passes &= CheckTextFilter(item.Description, DescriptionOperator, DescriptionText);
            if (IsAlignedEmailFilterActive) passes &= CheckTextFilter(item.Email, AlignedEmailOperator, AlignedEmailText);
            if (IsAlignedNameFilterActive) passes &= CheckTextFilter(item.FullName, AlignedNameOperator, AlignedNameText);
            if (IsAlignedCompanyFilterActive) passes &= CheckTextFilter(item.Company, AlignedCompanyOperator, AlignedCompanyText);
            if (IsAdvancedFilterActive) passes &= CheckTextFilter(item.Description, AdvancedOperator, AdvancedText);
            if (IsRegexFilterActive) passes &= CheckTextFilter(item.Name, TextOperator.Regex, RegexText);
            return passes;
        });

        results.AppendLine($"\n通过: {passedCount}/{testData.Length}");
        TextFilterResults = results.ToString();
    }

    [RelayCommand]
    private void ClearAdvancedFilter()
    {
        AdvancedText = string.Empty;
        MessageBox.Show("高级搜索已清除", "清除", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ClearRegexFilter()
    {
        RegexText = string.Empty;
        MessageBox.Show("正则搜索已清除", "清除", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    [RelayCommand]
    private void ShowRegexHelp()
    {
        var helpText = @"正则表达式常用语法:

• ^     - 匹配行首
• $     - 匹配行尾
• .     - 匹配任意单个字符
• *     - 匹配前面的字符0次或多次
• +     - 匹配前面的字符1次或多次
• \d    - 匹配数字
• \w    - 匹配字母、数字或下划线
• [a-z] - 匹配a到z的任意字符

示例:
• \d{4}         - 匹配4位数字
• ^[A-Z][a-z]+  - 以大写字母开头的单词
• \w+@\w+\.\w+  - 简单邮箱格式";

        MessageBox.Show(helpText, "正则表达式帮助", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    #endregion 文本过滤器命令

    #region 辅助方法

    private string GetNumberOperatorSymbol(NumberOperator op) => op switch
    {
        NumberOperator.Equal => "=",
        NumberOperator.NotEqual => "≠",
        NumberOperator.LessThan => "<",
        NumberOperator.LessThanOrEqual => "≤",
        NumberOperator.GreaterThan => ">",
        NumberOperator.GreaterThanOrEqual => "≥",
        _ => "?"
    };

    private string GetTextOperatorSymbol(TextOperator op) => op switch
    {
        TextOperator.Equal => "=",
        TextOperator.NotEqual => "≠",
        TextOperator.Contains => "~",
        TextOperator.NotContains => "!~",
        TextOperator.StartsWith => "^",
        TextOperator.EndsWith => "$",
        TextOperator.Regex => "R",
        _ => "?"
    };

    /// <summary>
    /// 检查数值是否满足过滤条件
    /// </summary>
    public static bool CheckNumberFilter(double value, NumberOperator filterOperator, double filterValue, double tolerance = double.Epsilon)
    {
        return filterOperator switch
        {
            NumberOperator.Equal => Math.Abs(value - filterValue) <= tolerance,
            NumberOperator.NotEqual => Math.Abs(value - filterValue) > tolerance,
            NumberOperator.LessThan => value < filterValue,
            NumberOperator.LessThanOrEqual => value <= filterValue,
            NumberOperator.GreaterThan => value > filterValue,
            NumberOperator.GreaterThanOrEqual => value >= filterValue,
            _ => false
        };
    }

    /// <summary>
    /// 检查文本是否满足过滤条件
    /// </summary>
    public static bool CheckTextFilter(string text, TextOperator textOperator, string filterText)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(filterText))
            return textOperator == TextOperator.NotEqual || textOperator == TextOperator.NotContains;

        try
        {
            return textOperator switch
            {
                TextOperator.Equal => text.Equals(filterText, StringComparison.OrdinalIgnoreCase),
                TextOperator.NotEqual => !text.Equals(filterText, StringComparison.OrdinalIgnoreCase),
                TextOperator.Contains => text.Contains(filterText, StringComparison.OrdinalIgnoreCase),
                TextOperator.NotContains => !text.Contains(filterText, StringComparison.OrdinalIgnoreCase),
                TextOperator.StartsWith => text.StartsWith(filterText, StringComparison.OrdinalIgnoreCase),
                TextOperator.EndsWith => text.EndsWith(filterText, StringComparison.OrdinalIgnoreCase),
                TextOperator.Regex => Regex.IsMatch(text, filterText, RegexOptions.IgnoreCase),
                _ => false
            };
        }
        catch (ArgumentException)
        {
            // 正则表达式语法错误时返回false
            return false;
        }
    }

    #endregion 辅助方法
}