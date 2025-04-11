using CommunityToolkit.Mvvm.ComponentModel;
using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Demo.Views
{
    /// <summary>
    /// StepperView.xaml 的交互逻辑
    /// </summary>
    public partial class StepperView : UserControl
    {
        public StepperView()
        {
            InitializeComponent();
            DataContext = new StepperViewModel();
        }

        /// <summary>
        /// 前往上一步
        /// </summary>
        private void PreviousStep_Click(object sender, RoutedEventArgs e)
        {
            if (BasicStepper.CurrentStep > 0)
            {
                BasicStepper.CurrentStep--;
            }
        }

        /// <summary>
        /// 前往下一步
        /// </summary>
        private void NextStep_Click(object sender, RoutedEventArgs e)
        {
            if (BasicStepper.CurrentStep < BasicStepper.Items.Count - 1)
            {
                BasicStepper.CurrentStep++;
            }
        }
    }

    /// <summary>
    /// Stepper 控件的视图模型
    /// </summary>
    public partial class StepperViewModel : ObservableObject
    {
        /// <summary>
        /// 当前步骤索引
        /// </summary>
        [ObservableProperty]
        private int _currentStepIndex = 1;

        /// <summary>
        /// 步骤项集合
        /// </summary>
        [ObservableProperty]
        private ObservableCollection<StepperItemViewModel> _stepperItems;

        /// <summary>
        /// 构造函数
        /// </summary>
        public StepperViewModel()
        {
            // 初始化步骤项集合
            StepperItems = new ObservableCollection<StepperItemViewModel>
            {
                new StepperItemViewModel
                {
                    Header = "需求分析",
                    Description = "收集和分析用户需求",
                    HasError = false
                },
                new StepperItemViewModel
                {
                    Header = "设计方案",
                    Description = "制定系统设计方案",
                    HasError = false
                },
                new StepperItemViewModel
                {
                    Header = "编码实现",
                    Description = "实现系统功能",
                    HasError = false
                },
                new StepperItemViewModel
                {
                    Header = "测试验证",
                    Description = "进行系统测试",
                    HasError = false
                },
                new StepperItemViewModel
                {
                    Header = "部署上线",
                    Description = "系统部署和上线",
                    HasError = false
                }
            };
        }
    }

    /// <summary>
    /// 步骤项视图模型
    /// </summary>
    public partial class StepperItemViewModel : ObservableObject
    {
        /// <summary>
        /// 步骤标题
        /// </summary>
        [ObservableProperty]
        private string _header;

        /// <summary>
        /// 步骤描述
        /// </summary>
        [ObservableProperty]
        private string _description;

        /// <summary>
        /// 是否有错误
        /// </summary>
        [ObservableProperty]
        private bool _hasError;
    }
}