using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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
    }

    public partial class LoadingViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsLoadingImage { get; set; }

        [ObservableProperty]
        public partial BitmapImage ImageSource { get; set; }
        public LoadingViewModel()
        {
            IsLoadingImage = true;
        }

        [RelayCommand]
        private async Task LoadImageAsync()
        {
            try
            {
                IsLoadingImage = true;

                string imageUrl = "https://images.unsplash.com/photo-1541963463532-d68292c34b19";

                // 在后台线程中下载图片数据

                // 在 UI 线程上更新 ImageSource
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    using HttpClient httpClient = new HttpClient();
                    var imageData = await httpClient.GetByteArrayAsync(imageUrl);

                    using var stream = new MemoryStream(imageData);
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad; // 确保图片加载完成后释放流
                    image.StreamSource = stream;
                    image.EndInit();

                    ImageSource = image;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load image: {ex.Message}");
            }
            finally
            {
                IsLoadingImage = false;
            }
        }

    }
}