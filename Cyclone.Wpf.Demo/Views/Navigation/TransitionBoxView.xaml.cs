using System;
using System.Collections.Generic;
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

namespace Cyclone.Wpf.Demo.Views.Navigation
{
    /// <summary>
    /// TransitionBoxView.xaml 的交互逻辑
    /// </summary>
    public partial class TransitionBoxView : UserControl
    {
        public TransitionBoxView()
        {
            InitializeComponent();
        }

        private void OnChangeContent(object sender, RoutedEventArgs e)
        {
            // 切换内容
            transitionBox.Content = new TextBlock
            {
                Text = "New Content",
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 20
            };
        }
    }
}