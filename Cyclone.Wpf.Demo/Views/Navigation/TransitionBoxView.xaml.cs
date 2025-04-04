using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _fadeTransition = (FadeTransition)Resources["FadeTransition"];

            _slideLeftTransition = new SlideTransition { Direction = SlideTransition.SlideDirection.RightToLeft };
            _slideRightTransition = new SlideTransition { Direction = SlideTransition.SlideDirection.LeftToRight };

            // 添加调试跟踪
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;

            // 初始化Debug输出
            UpdateDebugInfo("控件已初始化");
        }

        private readonly FadeTransition _fadeTransition;
        private readonly SlideTransition _slideLeftTransition;
        private readonly SlideTransition _slideRightTransition;

        private void UpdateDebugInfo(string message)
        {
            debugText.Text += message + "\n";
            Debug.WriteLine(message);

            // 限制显示的行数
            if (debugText.Text.Length > 500)
            {
                debugText.Text = debugText.Text.Substring(debugText.Text.Length - 500);
            }
        }

        private void ShowRedContent_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("切换到红色内容");
            ContentControl content = new ContentControl
            {
                ContentTemplate = (DataTemplate)Resources["RedContent"]
            };
            transitionBox.Content = content;
        }

        private void ShowBlueContent_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("切换到蓝色内容");
            ContentControl content = new ContentControl
            {
                ContentTemplate = (DataTemplate)Resources["BlueContent"]
            };
            transitionBox.Content = content;
        }

        private void ShowGreenContent_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("切换到绿色内容");
            ContentControl content = new ContentControl
            {
                ContentTemplate = (DataTemplate)Resources["GreenContent"]
            };
            transitionBox.Content = content;
        }

        private void ShowTextContent_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("切换到文本内容");

            TextBlock textBlock = new TextBlock
            {
                Text = "TransitionBox 示例\n" + DateTime.Now.ToString(),
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.DarkBlue)
            };

            Border border = new Border
            {
                Child = textBlock,
                Background = new SolidColorBrush(Colors.LightYellow),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                MinWidth = 200,
                MinHeight = 200
            };

            transitionBox.Content = border;
        }

        private void UseFadeTransition_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("设置淡入淡出过渡");
            transitionBox.Transition = _fadeTransition;
        }

        private void UseSlideLeftTransition_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("设置向左滑动过渡");
            transitionBox.Transition = _slideLeftTransition;
        }

        private void UseSlideRightTransition_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("设置向右滑动过渡");
            transitionBox.Transition = _slideRightTransition;
        }

        private void RefreshControl_Click(object sender, RoutedEventArgs e)
        {
            UpdateDebugInfo("刷新控件");

            // 强制重建控件内容
            object currentContent = transitionBox.Content;
            transitionBox.Content = null;

            // 短暂延迟后恢复内容
            Dispatcher.BeginInvoke(new Action(() =>
            {
                transitionBox.Content = currentContent;
                UpdateDebugInfo("控件已刷新");
            }), System.Windows.Threading.DispatcherPriority.Render);
        }
    }
}