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
    /// TabControlView.xaml 的交互逻辑
    /// </summary>
    public partial class TabControlView : UserControl
    {
        public ObservableCollection<User> Users { get; set; }

        public TabControlView()
        {
            InitializeComponent();

            InitializeUsers();
            DataContext = this;
        }

        private void InitializeUsers()
        {
            Users = new ObservableCollection<User>
            {
                new User { Name = "张三", Age = 28, Email = "zhangsan@example.com" },
                new User { Name = "李四", Age = 32, Email = "lisi@example.com" },
                new User { Name = "王五", Age = 25, Email = "wangwu@example.com" }
            };

            userDataGrid.ItemsSource = Users;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("按钮被点击！", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class User
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public string Email { get; set; }
    }
}