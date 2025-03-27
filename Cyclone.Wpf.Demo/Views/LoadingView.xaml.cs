using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Controls;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using static System.Net.WebRequestMethods;

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

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string script = "document.body.style.overflow ='hidden';document.body.style.margin='0px';";
            WebBrowser wb = (WebBrowser)sender;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            CountdownControl.Start();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            CountdownControl.Reset();
        }

        private void AnimationTypeCombo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (CountdownControl != null)
            {
                CountdownControl.AnimationType = (AnimationType)AnimationTypeCombo.SelectedIndex;
            }
        }

        private void DurationSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CountdownControl != null)
            {
                CountdownControl.AnimationDuration = DurationSlider.Value;
            }
        }
    }

    public partial class ImageViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial BitmapImage Image { get; set; }
    }

    public partial class LoadingViewModel : ObservableObject
    {
        private List<string> _urls =
        [
            "http://img11.360buyimg.com//n3/g2/M00/06/1D/rBEGEVAkffUIAAAAAAB54F55qh8AABWrQLxLr0AAHn4106.jpg",
            "http://img12.360buyimg.com//n3/g1/M00/06/1D/rBEGDVAkffQIAAAAAAB0mDavAccAABWrQMCUdwAAHSw197.jpg",
            "http://img13.360buyimg.com//n3/g2/M00/06/1D/rBEGElAkffIIAAAAAADVR1yd_X0AABWrQKlu2MAANVf537.jpg",
            "http://img10.360buyimg.com//n3/g5/M02/1C/00/rBEIC1Akfe8IAAAAAABDtsBt3bQAAFeCQAh13kAAEPO445.jpg",
            "http://img11.360buyimg.com//n3/g3/M00/06/1D/rBEGE1AkfgIIAAAAAACfm_MhwRYAABWrQMmK8kAAJ-z240.jpg",
            "http://img12.360buyimg.com//n3/g3/M00/06/1D/rBEGFFAkfhQIAAAAAABHekJE6jQAABWrQOGiEUAAEeS965.jpg",
            "http://img13.360buyimg.com//n3/g2/M00/06/1D/rBEGElAkfegIAAAAAAClvhjSNQoAABWrQJ0KTIAAKXW818.jpg",
            "http://img14.360buyimg.com//n3/g1/M00/06/1D/rBEGDlAkfe4IAAAAAABQsM9eGEoAABWrQJ4WIwAAFDI883.jpg",
            "http://img10.360buyimg.com//n3/g3/M00/06/1D/rBEGE1AkfgQIAAAAAACBZc_HeVAAABWrQM293sAAIF9407.jpg",
            "http://img11.360buyimg.com//n3/g3/M00/06/1D/rBEGE1AkfgkIAAAAAAC_6A3AnhwAABWrQOfht8AAMAA406.jpg",
            "http://img12.360buyimg.com//n3/g5/M02/1C/00/rBEDilAkfeAIAAAAAACdJBYljH0AAFeCQAuIsMAAJ08326.jpg",
            "http://img13.360buyimg.com//n3/g1/M00/06/1D/rBEGDVAkfe4IAAAAAACXzwGDqfoAABWrQKpCmEAAJfn685.jpg",
            "http://img12.360buyimg.com//n3/g3/M00/06/1D/rBEGE1AkfgcIAAAAAAC5nK25hEQAABWrQOCa3sAALm0258.jpg",
            "http://img14.360buyimg.com//n3/g2/M00/06/1D/rBEGEFAkfdUIAAAAAACZblNaX_kAABWrQJ0zwgAAJmG566.jpg",
            "http://img14.360buyimg.com//n3/g2/M00/06/1D/rBEGEFAkfewIAAAAAACfqQVJlNoAABWrQOirGwAAJ_B820.jpg",
            "http://img11.360buyimg.com//n3/g2/M01/06/1D/rBEGEFAkffMIAAAAAACgY4EpzwYAABWrgAfHyIAAKB7880.jpg",
        ];

        [ObservableProperty]
        public partial ObservableCollection<ImageViewModel> Images { get; set; } = [];

        public LoadingViewModel()
        {
        }

        [RelayCommand]
        private async Task LoadImageAsync(bool isLoad)
        {
            if (!isLoad)
            {
                Images.Clear();
                return;
            }

            for (int i = 0; i < _urls.Count; i++)
            {
                try
                {
                    var image = new ImageViewModel();
                    var imageBytes = await new HttpClient().GetByteArrayAsync(_urls[i]);
                    image.Image = new BitmapImage();
                    image.Image.BeginInit();
                    image.Image.StreamSource = new MemoryStream(imageBytes);
                    image.Image.EndInit();
                    Images.Add(image);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}