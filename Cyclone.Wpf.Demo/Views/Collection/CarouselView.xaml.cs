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
    /// CarouselView.xaml 的交互逻辑
    /// </summary>
    public partial class CarouselView : UserControl
    {
        public CarouselView()
        {
            InitializeComponent();
            DataContext = new CarouselViewModel();
        }
    }

    public partial class CarouselViewModel : ObservableValidator
    {
        public CarouselViewModel()
        {
            Images = new ObservableCollection<ImageViewModel>
            {
                new ImageViewModel
                {
                    MainTitle = "Golden Horizon",
                    SubTitle = "A Serene Evening Painted in Shades of Gold",
                    ImagePath = "/Assets/carousel1.jpeg"
                },
                new ImageViewModel
                {
                    MainTitle = "Reflections of Tranquility",
                    SubTitle = "Nature’s Mirror Reflecting the Beauty of the Surroundings",
                    ImagePath = "/Assets/carousel2.jpeg"
                },
                new ImageViewModel
                {
                    MainTitle = "Majestic Peaks",
                    SubTitle = "Touching the Sky with Their Towering Presence and Rugged Beauty",
                    ImagePath = "/Assets/carousel3.jpeg"
                },
                new ImageViewModel
                {
                    MainTitle = "Winter Wonderland",
                    SubTitle = "A Blanket of Serenity Transforming the Landscape into a Magical Realm",
                    ImagePath = "/Assets/carousel4.jpeg"
                }
            };
        }

        public ObservableCollection<ImageViewModel> Images { get; private set; }
    }

    public partial class ImageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial string MainTitle { get; set; }

        [ObservableProperty]
        public partial string SubTitle { get; set; }

        [ObservableProperty]
        public partial string ImagePath { get; set; }
    }
}