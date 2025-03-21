using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    /// TextView.xaml 的交互逻辑
    /// </summary>
    public partial class InputView : UserControl
    {
        public InputView()
        {
            InitializeComponent();

            DataContext = new InputViewModel();
        }
    }

    public partial class InputViewModel : ObservableValidator
    {
        [Required]
        [NotifyDataErrorInfo]
        [ObservableProperty]
        public partial string Text { get; set; }

        [ObservableProperty]
        public partial double Number { get; set; } = 1800d;

        [ObservableProperty]
        public partial string SourceText { get; set; } = "ABCDEFGabcdefg一二三四五";

        public InputViewModel()
        {
            this.ErrorsChanged += TextViewModel_ErrorsChanged;
        }

        private void TextViewModel_ErrorsChanged(object? sender, System.ComponentModel.DataErrorsChangedEventArgs e)
        {
            var tt = HasErrors;
            var t = 1;
        }
    }
}