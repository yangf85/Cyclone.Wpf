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
        }

        private void WebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            string script = "document.body.style.overflow ='hidden';document.body.style.margin='0px';";
            WebBrowser wb = (WebBrowser)sender;
            wb.InvokeScript("execScript", new Object[] { script, "JavaScript" });
        }
    }
}