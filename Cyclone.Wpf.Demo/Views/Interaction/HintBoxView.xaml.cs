using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Helper;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// HintBoxView.xaml 的交互逻辑 - 纯 MVVM 模式
    /// </summary>
    public partial class HintBoxView : UserControl
    {
        public HintBoxView()
        {
            InitializeComponent();
            DataContext = new HintBoxViewModel();
        }
    }

    public partial class HintBoxViewModel : ObservableObject
    {
        #region 原始数据集合

        [ObservableProperty]
        public partial ObservableCollection<FakerData> AllPeople { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<SkillData> AllSkills { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<GameClassData> GameClasses { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<CompanyData> AllCompanies { get; set; } = [];

        #endregion 原始数据集合

        #region 过滤后的集合

        [ObservableProperty]
        public partial ObservableCollection<FakerData> FilteredPeople { get; set; } = [];

        [ObservableProperty]
        public partial ObservableCollection<SkillData> FilteredSkills { get; set; } = [];

        [ObservableProperty]
        public partial CollectionViewSource CompaniesViewSource { get; set; } = new();

        #endregion 过滤后的集合

        #region 搜索文本

        [ObservableProperty]
        public partial string PeopleSearchText { get; set; } = "";

        [ObservableProperty]
        public partial string SkillSearchText { get; set; } = "";

        [ObservableProperty]
        public partial string CompanySearchText { get; set; } = "";

        #endregion 搜索文本

        #region 选中项属性

        [ObservableProperty]
        public partial GameClassData? SelectedGameClass { get; set; }

        partial void OnSelectedGameClassChanged(GameClassData? value)
        {
            if (value != null)
            {
                GameClassResult = $"Selected: {value.ClassName}";
                _gameClassSelectionCount++;
                LogEvent($"Game class selected: {value.ClassName}");
                UpdateStatistics();
            }
        }

        [ObservableProperty]
        public partial FakerData? SelectedPerson { get; set; }

        partial void OnSelectedPersonChanged(FakerData? value)
        {
            if (value != null)
            {
                PeopleResult = $"Selected: {value.FirstName} {value.LastName} ({value.Email})";
                _peopleSelectionCount++;
                LogEvent($"Person selected: {value.FirstName} {value.LastName}");
                UpdateStatistics();
            }
        }

        [ObservableProperty]
        public partial SkillData? SelectedSkill { get; set; }

        partial void OnSelectedSkillChanged(SkillData? value)
        {
            if (value != null && !SelectedSkills.Contains(value.SkillName))
            {
                SelectedSkills.Add(value.SkillName);
                _skillSelectionCount++;
                LogEvent($"Skill added: {value.SkillName}");
                UpdateStatistics();

                // 清空搜索文本和选中项
                SkillSearchText = "";
                SelectedSkill = null;
            }
        }

        [ObservableProperty]
        public partial CompanyData? SelectedCompany { get; set; }

        partial void OnSelectedCompanyChanged(CompanyData? value)
        {
            if (value != null)
            {
                CompanyResult = $"Selected: {value.CompanyName} - {value.Industry} ({value.Country})";
                _companySelectionCount++;
                LogEvent($"Company selected: {value.CompanyName} from {value.Industry}");
                UpdateStatistics();
            }
        }

        #endregion 选中项属性

        #region 其他属性

        [ObservableProperty]
        public partial ObservableCollection<string> SelectedSkills { get; set; } = [];

        [ObservableProperty]
        public partial string GameClassResult { get; set; } = "Selected: None";

        [ObservableProperty]
        public partial string PeopleResult { get; set; } = "Selected: None";

        [ObservableProperty]
        public partial string CompanyResult { get; set; } = "Selected: None";

        [ObservableProperty]
        public partial string SelectionStatistics { get; set; } = "";

        [ObservableProperty]
        public partial string EventLog { get; set; } = "";

        #endregion 其他属性

        #region 搜索文本变化的分部方法

        partial void OnPeopleSearchTextChanged(string value)
        {
            FilterPeople();
        }

        partial void OnSkillSearchTextChanged(string value)
        {
            FilterSkills();
        }

        partial void OnCompanySearchTextChanged(string value)
        {
            FilterCompanies();
        }

        #endregion 搜索文本变化的分部方法

        #region 私有字段

        private int _gameClassSelectionCount = 0;
        private int _peopleSelectionCount = 0;
        private int _skillSelectionCount = 0;
        private int _companySelectionCount = 0;

        #endregion 私有字段

        #region 命令

        [RelayCommand]
        private void RemoveSkill(string skillName)
        {
            if (SelectedSkills.Contains(skillName))
            {
                SelectedSkills.Remove(skillName);
                LogEvent($"Skill removed: {skillName}");
                UpdateStatistics();
            }
        }

        [RelayCommand]
        private void ClearLog()
        {
            EventLog = "Log cleared...";
        }

        #endregion 命令

        #region 构造函数

        public HintBoxViewModel()
        {
            InitializeData();
            LogEvent("HintBox MVVM demo initialized");
            UpdateStatistics();
        }

        #endregion 构造函数

        #region 数据初始化

        private void InitializeData()
        {
            // 初始化游戏职业数据
            GameClasses = new ObservableCollection<GameClassData>
            {
                new GameClassData { ClassName = "Demon Hunter" },
                new GameClassData { ClassName = "Death Knight" },
                new GameClassData { ClassName = "Blade Master" },
                new GameClassData { ClassName = "Paladin" },
                new GameClassData { ClassName = "Warrior" },
                new GameClassData { ClassName = "Mage" },
                new GameClassData { ClassName = "Warlock" },
                new GameClassData { ClassName = "Hunter" },
                new GameClassData { ClassName = "Rogue" },
                new GameClassData { ClassName = "Priest" }
            };

            // 生成假数据
            AllPeople = new ObservableCollection<FakerData>(FakerDataHelper.GenerateFakerDataCollection(100));
            FilteredPeople = new ObservableCollection<FakerData>(AllPeople);

            // 初始化技能数据
            AllSkills = new ObservableCollection<SkillData>
            {
                new SkillData { SkillName = "Project Management" },
                new SkillData { SkillName = "Software Development" },
                new SkillData { SkillName = "Data Analysis" },
                new SkillData { SkillName = "Machine Learning" },
                new SkillData { SkillName = "UI/UX Design" },
                new SkillData { SkillName = "Database Design" },
                new SkillData { SkillName = "Cloud Computing" },
                new SkillData { SkillName = "DevOps" },
                new SkillData { SkillName = "Cybersecurity" },
                new SkillData { SkillName = "Mobile Development" },
                new SkillData { SkillName = "Web Development" },
                new SkillData { SkillName = "API Development" },
                new SkillData { SkillName = "Quality Assurance" },
                new SkillData { SkillName = "Agile Methodology" },
                new SkillData { SkillName = "Technical Writing" },
                new SkillData { SkillName = "System Architecture" },
                new SkillData { SkillName = "Performance Optimization" },
                new SkillData { SkillName = "Team Leadership" }
            };
            FilteredSkills = new ObservableCollection<SkillData>(AllSkills);

            // 初始化公司数据
            AllCompanies = new ObservableCollection<CompanyData>
            {
                // 科技行业
                new CompanyData { CompanyName = "Apple Inc.", Industry = "Technology", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Microsoft Corporation", Industry = "Technology", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Google LLC", Industry = "Technology", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Amazon.com Inc.", Industry = "Technology", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Meta Platforms Inc.", Industry = "Technology", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Tesla Inc.", Industry = "Technology", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Netflix Inc.", Industry = "Technology", Country = "United States", Size = "Medium" },
                new CompanyData { CompanyName = "Spotify Technology S.A.", Industry = "Technology", Country = "Sweden", Size = "Medium" },
                new CompanyData { CompanyName = "Samsung Electronics", Industry = "Technology", Country = "South Korea", Size = "Large" },
                new CompanyData { CompanyName = "Sony Corporation", Industry = "Technology", Country = "Japan", Size = "Large" },

                // 金融行业
                new CompanyData { CompanyName = "JPMorgan Chase & Co.", Industry = "Finance", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Bank of America Corp.", Industry = "Finance", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Wells Fargo & Company", Industry = "Finance", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Goldman Sachs Group Inc.", Industry = "Finance", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Morgan Stanley", Industry = "Finance", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Credit Suisse Group AG", Industry = "Finance", Country = "Switzerland", Size = "Large" },
                new CompanyData { CompanyName = "Deutsche Bank AG", Industry = "Finance", Country = "Germany", Size = "Large" },
                new CompanyData { CompanyName = "HSBC Holdings plc", Industry = "Finance", Country = "United Kingdom", Size = "Large" },
                new CompanyData { CompanyName = "Barclays PLC", Industry = "Finance", Country = "United Kingdom", Size = "Large" },
                new CompanyData { CompanyName = "PayPal Holdings Inc.", Industry = "Finance", Country = "United States", Size = "Medium" },

                // 制造业
                new CompanyData { CompanyName = "Toyota Motor Corporation", Industry = "Manufacturing", Country = "Japan", Size = "Large" },
                new CompanyData { CompanyName = "Volkswagen AG", Industry = "Manufacturing", Country = "Germany", Size = "Large" },
                new CompanyData { CompanyName = "General Motors Company", Industry = "Manufacturing", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Ford Motor Company", Industry = "Manufacturing", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "BMW AG", Industry = "Manufacturing", Country = "Germany", Size = "Large" },
                new CompanyData { CompanyName = "Mercedes-Benz Group AG", Industry = "Manufacturing", Country = "Germany", Size = "Large" },
                new CompanyData { CompanyName = "Honda Motor Co. Ltd.", Industry = "Manufacturing", Country = "Japan", Size = "Large" },
                new CompanyData { CompanyName = "Nissan Motor Co. Ltd.", Industry = "Manufacturing", Country = "Japan", Size = "Large" },
                new CompanyData { CompanyName = "Caterpillar Inc.", Industry = "Manufacturing", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "3M Company", Industry = "Manufacturing", Country = "United States", Size = "Large" },

                // 零售业
                new CompanyData { CompanyName = "Walmart Inc.", Industry = "Retail", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "The Home Depot Inc.", Industry = "Retail", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Costco Wholesale Corporation", Industry = "Retail", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Target Corporation", Industry = "Retail", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "IKEA", Industry = "Retail", Country = "Sweden", Size = "Large" },
                new CompanyData { CompanyName = "H&M Hennes & Mauritz AB", Industry = "Retail", Country = "Sweden", Size = "Medium" },
                new CompanyData { CompanyName = "Zara (Inditex)", Industry = "Retail", Country = "Spain", Size = "Medium" },
                new CompanyData { CompanyName = "Best Buy Co. Inc.", Industry = "Retail", Country = "United States", Size = "Medium" },
                new CompanyData { CompanyName = "Lowe's Companies Inc.", Industry = "Retail", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Starbucks Corporation", Industry = "Retail", Country = "United States", Size = "Medium" },

                // 医疗保健
                new CompanyData { CompanyName = "Johnson & Johnson", Industry = "Healthcare", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Pfizer Inc.", Industry = "Healthcare", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Roche Holding AG", Industry = "Healthcare", Country = "Switzerland", Size = "Large" },
                new CompanyData { CompanyName = "Novartis AG", Industry = "Healthcare", Country = "Switzerland", Size = "Large" },
                new CompanyData { CompanyName = "Merck & Co. Inc.", Industry = "Healthcare", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "AbbVie Inc.", Industry = "Healthcare", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Bristol Myers Squibb Company", Industry = "Healthcare", Country = "United States", Size = "Large" },
                new CompanyData { CompanyName = "Moderna Inc.", Industry = "Healthcare", Country = "United States", Size = "Medium" },
                new CompanyData { CompanyName = "Gilead Sciences Inc.", Industry = "Healthcare", Country = "United States", Size = "Medium" },
                new CompanyData { CompanyName = "Bayer AG", Industry = "Healthcare", Country = "Germany", Size = "Large" }
            };

            // 设置分组的CollectionViewSource
            CompaniesViewSource.Source = AllCompanies;
            CompaniesViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Industry"));
            CompaniesViewSource.SortDescriptions.Add(new SortDescription("Industry", ListSortDirection.Ascending));
            CompaniesViewSource.SortDescriptions.Add(new SortDescription("CompanyName", ListSortDirection.Ascending));
        }

        #endregion 数据初始化

        #region 过滤方法

        private void FilterPeople()
        {
            if (string.IsNullOrWhiteSpace(PeopleSearchText))
            {
                FilteredPeople = new ObservableCollection<FakerData>(AllPeople);
            }
            else
            {
                var filtered = AllPeople.Where(p =>
                    (p.FirstName + " " + p.LastName).Contains(PeopleSearchText, StringComparison.OrdinalIgnoreCase)).ToList();
                FilteredPeople = new ObservableCollection<FakerData>(filtered);
            }
        }

        private void FilterSkills()
        {
            if (string.IsNullOrWhiteSpace(SkillSearchText))
            {
                FilteredSkills = new ObservableCollection<SkillData>(AllSkills);
            }
            else
            {
                var filtered = AllSkills.Where(s =>
                    s.SkillName?.Contains(SkillSearchText, StringComparison.OrdinalIgnoreCase) == true).ToList();
                FilteredSkills = new ObservableCollection<SkillData>(filtered);
            }
        }

        private void FilterCompanies()
        {
            if (string.IsNullOrWhiteSpace(CompanySearchText))
            {
                CompaniesViewSource.Source = AllCompanies;
            }
            else
            {
                var filtered = AllCompanies.Where(c =>
                    c.CompanyName?.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase) == true ||
                    c.Industry?.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase) == true).ToList();
                CompaniesViewSource.Source = filtered;
            }

            // 刷新视图
            CompaniesViewSource.View?.Refresh();
        }

        #endregion 过滤方法

        #region 辅助方法

        private void LogEvent(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var newEvent = $"[{timestamp}] {message}";

            if (string.IsNullOrEmpty(EventLog))
            {
                EventLog = newEvent;
            }
            else
            {
                EventLog = $"{newEvent}\n{EventLog}";

                // 限制日志长度，保持最新的20条记录
                var lines = EventLog.Split('\n');
                if (lines.Length > 20)
                {
                    EventLog = string.Join('\n', lines.Take(20));
                }
            }
        }

        private void UpdateStatistics()
        {
            var stats = new StringBuilder();
            stats.AppendLine($"Game Classes Selected: {_gameClassSelectionCount}");
            stats.AppendLine($"People Selected: {_peopleSelectionCount}");
            stats.AppendLine($"Skills Added: {_skillSelectionCount}");
            stats.AppendLine($"Companies Selected: {_companySelectionCount}");
            stats.AppendLine($"Active Skills: {SelectedSkills.Count}");
            stats.AppendLine();
            stats.AppendLine($"Total Items in Collections:");
            stats.AppendLine($"- People: {AllPeople.Count} (Filtered: {FilteredPeople.Count})");
            stats.AppendLine($"- Skills: {AllSkills.Count} (Filtered: {FilteredSkills.Count})");
            stats.AppendLine($"- Companies: {AllCompanies.Count}");

            SelectionStatistics = stats.ToString();
        }

        #endregion 辅助方法
    }

    #region 数据模型类

    public class GameClassData
    {
        public string ClassName { get; set; }
    }

    public class SkillData
    {
        public string SkillName { get; set; }
    }

    public class CompanyData
    {
        public string CompanyName { get; set; }
        public string Industry { get; set; }
        public string Country { get; set; }
        public string Size { get; set; }

        public Brush IndustryColor => Industry switch
        {
            "Technology" => new SolidColorBrush(Colors.Blue),
            "Finance" => new SolidColorBrush(Colors.Green),
            "Manufacturing" => new SolidColorBrush(Colors.Orange),
            "Retail" => new SolidColorBrush(Colors.Purple),
            "Healthcare" => new SolidColorBrush(Colors.Red),
            _ => new SolidColorBrush(Colors.Gray)
        };

        public Brush Background { get; set; }
    }

    #endregion 数据模型类
}