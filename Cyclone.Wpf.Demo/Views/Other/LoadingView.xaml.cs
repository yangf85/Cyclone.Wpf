using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Demo.Helper;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// LoadingView.xaml 的交互逻辑
    /// </summary>
    public partial class LoadingView : UserControl
    {
        public LoadingView()
        {
            InitializeComponent();
            DataContext = new LoadingViewModel();
        }

        #region Ring颜色事件处理

        private void RingColorBlue_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.RingColor = Colors.Blue;
            }
        }

        private void RingColorRed_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.RingColor = Colors.Red;
            }
        }

        private void RingColorGreen_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.RingColor = Colors.Green;
            }
        }

        #endregion Ring颜色事件处理

        #region Pulse颜色事件处理

        private void PulseColorGreen_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.PulseColor = Colors.Green;
            }
        }

        private void PulseColorOrange_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.PulseColor = Colors.Orange;
            }
        }

        private void PulseColorPurple_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.PulseColor = Colors.Purple;
            }
        }

        #endregion Pulse颜色事件处理

        #region Particle颜色事件处理

        private void ParticleColorOrange_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.ParticleColor = Colors.Orange;
            }
        }

        private void ParticleColorWhite_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.ParticleColor = Colors.White;
            }
        }

        private void ParticleColorBlue_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is LoadingBasicViewModel vm)
            {
                vm.ParticleColor = Colors.Blue;
            }
        }

        #endregion Particle颜色事件处理

        #region Cube颜色事件处理

        private void CubeColorBlue_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.FlipCubeColor = Colors.DodgerBlue;
            }
        }

        private void CubeColorRed_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.FlipCubeColor = Colors.Red;
            }
        }

        private void CubeColorGold_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.FlipCubeColor = Colors.Gold;
            }
        }

        #endregion Cube颜色事件处理

        #region Tesseract颜色事件处理

        private void TesseractColorBlack_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.TesseractLineColor = Colors.Black;
            }
        }

        private void TesseractColorPurple_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.TesseractLineColor = Colors.Purple;
            }
        }

        private void TesseractColorCyan_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.TesseractLineColor = Colors.Cyan;
            }
        }

        #endregion Tesseract颜色事件处理

        #region Chase颜色事件处理

        private void ChaseColorWhite_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.ChaseDotColor = Colors.White;
            }
        }

        private void ChaseColorRed_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.ChaseDotColor = Colors.Red;
            }
        }

        private void ChaseColorYellow_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is Loading3DViewModel vm)
            {
                vm.ChaseDotColor = Colors.Yellow;
            }
        }

        #endregion Chase颜色事件处理
    }

    #region ViewModels

    // 主ViewModel
    public partial class LoadingViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial LoadingBasicViewModel BasicTab { get; set; }

        [ObservableProperty]
        public partial Loading3DViewModel ThreeDTab { get; set; }

        [ObservableProperty]
        public partial LoadingScenarioViewModel ScenarioTab { get; set; }

        public LoadingViewModel()
        {
            BasicTab = new LoadingBasicViewModel();
            ThreeDTab = new Loading3DViewModel();
            ScenarioTab = new LoadingScenarioViewModel();
        }
    }

    // Tab1: 基础Loading动画ViewModel
    public partial class LoadingBasicViewModel : ObservableObject
    {
        // Tab级开关
        [ObservableProperty]
        public partial bool IsEnabled { get; set; } = true;

        // LoadingRing参数
        [ObservableProperty]
        public partial double RingSize { get; set; } = 50;

        [ObservableProperty]
        public partial Color RingColor { get; set; } = Colors.Blue;

        [ObservableProperty]
        public partial double RingSpeed { get; set; } = 1.5;

        // LoadingPulse参数
        [ObservableProperty]
        public partial double PulseSize { get; set; } = 12;

        [ObservableProperty]
        public partial Color PulseColor { get; set; } = Colors.Green;

        [ObservableProperty]
        public partial double PulseDuration { get; set; } = 0.6;

        // LoadingParticle参数
        [ObservableProperty]
        public partial double ParticleSize { get; set; } = 5;

        [ObservableProperty]
        public partial Color ParticleColor { get; set; } = Colors.Orange;

        [ObservableProperty]
        public partial double OrbitRadius { get; set; } = 75;
    }

    // Tab2: 3D Loading特效ViewModel
    public partial class Loading3DViewModel : ObservableObject
    {
        // Tab级开关
        [ObservableProperty]
        public partial bool IsEnabled { get; set; } = true;

        // LoadingFlipCube参数
        [ObservableProperty]
        public partial Color FlipCubeColor { get; set; } = Colors.DodgerBlue;

        [ObservableProperty]
        public partial double FlipCubeSize { get; set; } = 1.5;

        [ObservableProperty]
        public partial double FlipCubeSpeed { get; set; } = 0.5;

        // LoadingTesseract参数
        [ObservableProperty]
        public partial Color TesseractLineColor { get; set; } = Colors.Black;

        [ObservableProperty]
        public partial double TesseractScale { get; set; } = 1.0;

        [ObservableProperty]
        public partial double TesseractSpeed { get; set; } = 0.3;

        // LoadingChase参数
        [ObservableProperty]
        public partial Color ChaseDotColor { get; set; } = Colors.White;

        [ObservableProperty]
        public partial double ChaseCircleSize { get; set; } = 60;

        [ObservableProperty]
        public partial int ChaseDotCount { get; set; } = 8;

        [ObservableProperty]
        public partial double ChaseSpeed { get; set; } = 1.2;
    }

    // Tab3: Loading业务场景ViewModel
    public partial class LoadingScenarioViewModel : ObservableObject
    {
        // Tab级开关
        [ObservableProperty]
        public partial bool IsEnabled { get; set; } = true;

        // 数据加载场景
        [ObservableProperty]
        public partial ObservableCollection<FakerData> LoadingDataList { get; set; } = new();

        [ObservableProperty]
        public partial bool IsDataLoading { get; set; } = false;

        // 表单提交场景
        [ObservableProperty]
        public partial FakerData LoadingFormData { get; set; } = new();

        [ObservableProperty]
        public partial bool IsFormSubmitting { get; set; } = false;

        // 搜索场景
        [ObservableProperty]
        public partial string LoadingSearchText { get; set; } = "";

        [ObservableProperty]
        public partial ObservableCollection<FakerData> LoadingSearchResults { get; set; } = new();

        [ObservableProperty]
        public partial bool IsSearching { get; set; } = false;

        [RelayCommand]
        private async Task ExecuteLoadingDataAsync()
        {
            IsDataLoading = true;
            await Task.Delay(2000);

            LoadingDataList.Clear();
            var data = FakerDataHelper.GenerateFakerDataCollection(20);
            foreach (var item in data)
            {
                LoadingDataList.Add(item);
            }

            IsDataLoading = false;
        }

        [RelayCommand]
        private async Task ExecuteLoadingSubmitAsync()
        {
            IsFormSubmitting = true;
            await Task.Delay(1500);
            MessageBox.Show("表单提交成功！");
            IsFormSubmitting = false;
        }

        [RelayCommand]
        private async Task ExecuteLoadingSearchAsync()
        {
            if (string.IsNullOrWhiteSpace(LoadingSearchText)) return;

            IsSearching = true;
            await Task.Delay(1000);

            LoadingSearchResults.Clear();
            var results = FakerDataHelper.GenerateFakerDataCollection(5);
            foreach (var item in results)
            {
                LoadingSearchResults.Add(item);
            }

            IsSearching = false;
        }
    }

    #endregion ViewModels
}