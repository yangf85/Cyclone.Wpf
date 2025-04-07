using Cyclone.Wpf.Helpers;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 改进的轮播控件，提供平滑的动画和更好的性能
    /// </summary>
    [TemplatePart(Name = PART_PrevButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_NextButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_ScrollViewer, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = PART_IndicatorsListBox, Type = typeof(ListBox))]
    public class Carousel : ListBox
    {
        #region 常量和字段

        private const string PART_PrevButton = "PART_PrevButton";
        private const string PART_NextButton = "PART_NextButton";
        private const string PART_ScrollViewer = "PART_ScrollViewer";
        private const string PART_IndicatorsListBox = "PART_IndicatorsListBox";

        private Button _prevButton;
        private Button _nextButton;
        private ScrollViewer _scrollViewer;
        private ListBox _indicatorsListBox;
        private DispatcherTimer _autoPlayTimer;
        private Storyboard _storyboard;
        private DoubleAnimation _animation;
        private bool _isAnimating;

        #endregion 常量和字段

        #region 构造函数

        static Carousel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Carousel),
                new FrameworkPropertyMetadata(typeof(Carousel)));
        }

        public Carousel()
        {
            // 初始化命令和自动播放计时器
            InitializeCommand();

            _autoPlayTimer = new DispatcherTimer();
            _autoPlayTimer.Tick += AutoPlayTimer_Tick;

            // 初始化动画
            _animation = new DoubleAnimation
            {
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            _storyboard = new Storyboard();
            _storyboard.Children.Add(_animation);
            _storyboard.Completed += OnAnimationCompleted;

            // 注册事件处理器
            Loaded += Carousel_Loaded;
            Unloaded += Carousel_Unloaded;
        }

        #endregion 构造函数

        #region 依赖属性

        #region IsEnableAnimation

        public static readonly DependencyProperty IsEnableAnimationProperty =
            DependencyProperty.Register(nameof(IsEnableAnimation), typeof(bool), typeof(Carousel),
                new PropertyMetadata(true));

        public bool IsEnableAnimation
        {
            get => (bool)GetValue(IsEnableAnimationProperty);
            set => SetValue(IsEnableAnimationProperty, value);
        }

        #endregion IsEnableAnimation

        #region IsRepeatPlayback

        public static readonly DependencyProperty IsRepeatPlaybackProperty =
            DependencyProperty.Register(nameof(IsRepeatPlayback), typeof(bool), typeof(Carousel),
                new PropertyMetadata(true));

        public bool IsRepeatPlayback
        {
            get => (bool)GetValue(IsRepeatPlaybackProperty);
            set => SetValue(IsRepeatPlaybackProperty, value);
        }

        #endregion IsRepeatPlayback

        #region AnimationDuration

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(Duration), typeof(Carousel),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(200)), OnAnimationDurationChanged));

        public Duration AnimationDuration
        {
            get => (Duration)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        private static void OnAnimationDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (Carousel)d;
            carousel._animation.Duration = (Duration)e.NewValue;
        }

        #endregion AnimationDuration

        #region FunctionBar

        public static readonly DependencyProperty FunctionBarProperty =
            DependencyProperty.Register(nameof(FunctionBar), typeof(object), typeof(Carousel),
                new PropertyMetadata(default));

        public object FunctionBar
        {
            get => (object)GetValue(FunctionBarProperty);
            set => SetValue(FunctionBarProperty, value);
        }

        #endregion FunctionBar

        #region NavigationBar

        public static readonly DependencyProperty NavigationBarProperty =
            DependencyProperty.Register(nameof(NavigationBar), typeof(object), typeof(Carousel),
                new PropertyMetadata(default(object), OnNavigationBarChanged));

        [Bindable(true)]
        public object NavigationBar
        {
            get => (object)GetValue(NavigationBarProperty);
            set => SetValue(NavigationBarProperty, value);
        }

        private static void OnNavigationBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (Carousel)d;
            if (e.NewValue != null)
            {
                carousel.RemoveLogicalChild(e.OldValue);
                carousel.AddLogicalChild(e.NewValue);
            }
        }

        #endregion NavigationBar

        #region AutoPlay

        public static readonly DependencyProperty AutoPlayProperty =
            DependencyProperty.Register(nameof(AutoPlay), typeof(bool), typeof(Carousel),
                new PropertyMetadata(false, OnAutoPlayChanged));

        public bool AutoPlay
        {
            get => (bool)GetValue(AutoPlayProperty);
            set => SetValue(AutoPlayProperty, value);
        }

        private static void OnAutoPlayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (Carousel)d;
            carousel.UpdateAutoPlayState();
        }

        #endregion AutoPlay

        #region AutoPlayInterval

        public static readonly DependencyProperty AutoPlayIntervalProperty =
            DependencyProperty.Register(nameof(AutoPlayInterval), typeof(TimeSpan), typeof(Carousel),
                new PropertyMetadata(TimeSpan.FromSeconds(3), OnAutoPlayIntervalChanged));

        public TimeSpan AutoPlayInterval
        {
            get => (TimeSpan)GetValue(AutoPlayIntervalProperty);
            set => SetValue(AutoPlayIntervalProperty, value);
        }

        private static void OnAutoPlayIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var carousel = (Carousel)d;
            carousel.UpdateAutoPlayInterval();
        }

        #endregion AutoPlayInterval

        #endregion 依赖属性

        #region 命令

        public static readonly RoutedCommand PrevCommand = new RoutedCommand("Prev", typeof(Carousel));
        public static readonly RoutedCommand NextCommand = new RoutedCommand("Next", typeof(Carousel));

        private void InitializeCommand()
        {
            CommandBindings.Add(new CommandBinding(PrevCommand, OnExecutedPrevCommand, OnCanExecutePrevCommand));
            CommandBindings.Add(new CommandBinding(NextCommand, OnExecutedNextCommand, OnCanExecuteNextCommand));
        }

        private void OnCanExecutePrevCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsRepeatPlayback || SelectedIndex > 0;
        }

        private void OnExecutedPrevCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (_isAnimating) return;

            if (SelectedIndex > 0)
            {
                SelectedIndex--;
            }
            else if (IsRepeatPlayback && Items.Count > 0)
            {
                SelectedIndex = Items.Count - 1;
            }
        }

        private void OnCanExecuteNextCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsRepeatPlayback || SelectedIndex < Items.Count - 1;
        }

        private void OnExecutedNextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (_isAnimating) return;

            if (SelectedIndex < Items.Count - 1)
            {
                SelectedIndex++;
            }
            else if (IsRepeatPlayback && Items.Count > 0)
            {
                SelectedIndex = 0;
            }
        }

        #endregion 命令

        #region 事件处理器

        private void Carousel_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAutoPlayState();

            // 确保指示器显示正确的选中状态
            if (_indicatorsListBox != null && Items.Count > 0)
            {
                _indicatorsListBox.SelectedIndex = SelectedIndex;
            }

            CommandManager.InvalidateRequerySuggested();
        }

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                // 当所有项容器生成后，确保选中状态正确
                if (_indicatorsListBox != null && Items.Count > 0)
                {
                    _indicatorsListBox.SelectedIndex = SelectedIndex;
                }

                // 如果是首次加载，滚动到选中项
                if (_scrollViewer != null && SelectedIndex >= 0 && SelectedIndex < Items.Count)
                {
                    ScrollIntoView(Items[SelectedIndex]);
                }
            }
        }

        private void Carousel_Unloaded(object sender, RoutedEventArgs e)
        {
            StopAutoPlay();

            if (_autoPlayTimer != null)
            {
                _autoPlayTimer.Tick -= AutoPlayTimer_Tick;
            }

            if (_storyboard != null)
            {
                _storyboard.Completed -= OnAnimationCompleted;
            }

            ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
        }

        private void AutoPlayTimer_Tick(object sender, EventArgs e)
        {
            if (_isAnimating) return;

            SelectedIndex = (SelectedIndex + 1) % Items.Count;
        }

        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            // 立即重置动画状态，不要延迟
            _isAnimating = false;
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion 事件处理器

        #region 重写方法

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _prevButton = GetTemplateChild(PART_PrevButton) as Button;
            _nextButton = GetTemplateChild(PART_NextButton) as Button;
            _scrollViewer = GetTemplateChild(PART_ScrollViewer) as ScrollViewer;
            _indicatorsListBox = GetTemplateChild(PART_IndicatorsListBox) as ListBox;

            if (_scrollViewer != null)
            {
                // 设置动画目标属性
                Storyboard.SetTarget(_animation, _scrollViewer);
                Storyboard.SetTargetProperty(_animation, new PropertyPath(ScrollViewerHelper.HorizontalOffsetProperty));

                // 设置动画持续时间
                _animation.Duration = AnimationDuration;
            }

            // 确保选中状态在模板应用后同步
            if (_indicatorsListBox != null && Items.Count > 0)
            {
                _indicatorsListBox.SelectedIndex = SelectedIndex;
            }

            SizeChanged += Carousel_SizeChanged;
        }

        private void Carousel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_scrollViewer == null || Items.Count == 0 || SelectedIndex < 0)
                return;

            // 强制重新计算布局
            UpdateLayout();

            // 滚动到当前选中项
            ScrollIntoView(Items[SelectedIndex]);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new CarouselItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is CarouselItem;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            if (_scrollViewer == null || Items.Count == 0) return;

            // 获取旧选择和新选择
            int oldIndex = -1;
            int newIndex = SelectedIndex;

            if (e.RemovedItems.Count > 0)
            {
                oldIndex = Items.IndexOf(e.RemovedItems[0]);
            }

            // 如果是首次加载，不播放动画
            if (oldIndex == -1)
            {
                ScrollIntoView(Items[newIndex]);
                return;
            }

            // 确定是否应该播放动画
            if (IsEnableAnimation && Math.Abs(newIndex - oldIndex) == 1)
            {
                PlayAnimation(oldIndex, newIndex);
            }
            else
            {
                ScrollIntoView(Items[newIndex]);
            }
        }

        #endregion 重写方法

        #region 私有方法

        private void PlayAnimation(int oldIndex, int newIndex)
        {
            if (_scrollViewer == null || _isAnimating) return;

            _isAnimating = true;

            // 获取当前项的容器
            CarouselItem item = ItemContainerGenerator.ContainerFromIndex(oldIndex) as CarouselItem;
            if (item == null)
            {
                _isAnimating = false;
                ScrollIntoView(Items[newIndex]);
                return;
            }

            double from = _scrollViewer.HorizontalOffset;
            double to;

            // 计算目标位置
            if (newIndex > oldIndex)
            {
                // 向后滚动
                to = from + item.ActualWidth;
            }
            else
            {
                // 向前滚动
                to = from - item.ActualWidth;
            }

            // 播放动画
            _storyboard.Stop();
            _animation.From = from;
            _animation.To = to;

            try
            {
                _storyboard.Begin();
            }
            catch (Exception)
            {
                _isAnimating = false;
                ScrollIntoView(Items[newIndex]);
            }
        }

        private void UpdateAutoPlayState()
        {
            if (AutoPlay && Items.Count > 1)
            {
                StartAutoPlay();
            }
            else
            {
                StopAutoPlay();
            }
        }

        private void UpdateAutoPlayInterval()
        {
            _autoPlayTimer.Interval = AutoPlayInterval;
            if (AutoPlay)
            {
                StopAutoPlay();
                StartAutoPlay();
            }
        }

        private void StartAutoPlay()
        {
            _autoPlayTimer.Interval = AutoPlayInterval;
            _autoPlayTimer.Start();
        }

        private void StopAutoPlay()
        {
            _autoPlayTimer.Stop();
        }

        #endregion 私有方法
    }
}