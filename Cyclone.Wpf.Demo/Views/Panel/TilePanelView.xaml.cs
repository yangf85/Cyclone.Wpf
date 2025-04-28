using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// TilePanel示例视图的交互逻辑
    /// </summary>
    public partial class TilePanelView : UserControl
    {
        public TilePanelView()
        {
            InitializeComponent();
            DataContext = new TilePanelViewModel();
        }
    }

    /// <summary>
    /// TilePanel示例的ViewModel
    /// </summary>
    public partial class TilePanelViewModel : ObservableObject
    {
        /// <summary>
        /// 磁贴数据集合
        /// </summary>
        [ObservableProperty]
        public partial ObservableCollection<TileItem> Items { get; set; }

        public TilePanelViewModel()
        {
            // 生成50个随机磁贴数据
            Items = TileItemHelper.GenerateRandomTileItems(27);
        }
    }

    /// <summary>
    /// 磁贴项数据模型
    /// </summary>
    public class TileItem
    {
        /// <summary>
        /// 背景颜色
        /// </summary>
        public Brush Background { get; set; }

        /// <summary>
        /// 显示内容
        /// </summary>
        public string Content { get; set; }
    }

    /// <summary>
    /// 磁贴数据生成辅助类
    /// </summary>
    public static class TileItemHelper
    {
        /// <summary>
        /// 生成随机磁贴数据集合
        /// </summary>
        public static ObservableCollection<TileItem> GenerateRandomTileItems(int count)
        {
            var result = new ObservableCollection<TileItem>();
            var random = new Random();

            // 可用的背景颜色
            var backgrounds = new List<Brush>
            {
                new SolidColorBrush(Color.FromRgb(41, 128, 185)),  // 蓝色
                new SolidColorBrush(Color.FromRgb(231, 76, 60)),   // 红色
                new SolidColorBrush(Color.FromRgb(46, 204, 113)),  // 绿色
                new SolidColorBrush(Color.FromRgb(155, 89, 182)),  // 紫色
                new SolidColorBrush(Color.FromRgb(52, 152, 219)),  // 亮蓝色
                new SolidColorBrush(Color.FromRgb(241, 196, 15)),  // 黄色
                new SolidColorBrush(Color.FromRgb(230, 126, 34))   // 橙色
            };

            for (int i = 0; i < count; i++)
            {
                var item = new TileItem
                {
                    Background = backgrounds[random.Next(backgrounds.Count)],
                    Content = $"磁贴 {i + 1}"
                };

                result.Add(item);
            }

            return result;
        }
    }
}