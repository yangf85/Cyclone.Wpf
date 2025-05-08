using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 倒计时控件，提供从起始秒数到结束秒数的倒计时功能，支持动画效果
    /// </summary>
    [TemplatePart(Name = "PART_AnimationContainer", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_DisplayText", Type = typeof(TextBlock))]
    public class Countdown : Control
    {
        #region 私有字段

        private DispatcherTimer _timer;
        private FrameworkElement _animationContainer;
        private TextBlock _displayText;
        private bool _isTemplateApplied;

        #endregion 私有字段

        #region 构造函数

        /// <summary>
        /// 初始化Countdown类的静态成员
        /// </summary>
        static Countdown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Countdown),
                new FrameworkPropertyMetadata(typeof(Countdown)));
        }

        /// <summary>
        /// 初始化Countdown类的新实例
        /// </summary>
        public Countdown()
        {
            InitializeTimer();
        }

        #endregion 构造函数

        #region StartSeconds

        public int StartSeconds
        {
            get => (int)GetValue(StartSecondsProperty);
            set => SetValue(StartSecondsProperty, value);
        }

        public static readonly DependencyProperty StartSecondsProperty =
            DependencyProperty.Register(nameof(StartSeconds), typeof(int), typeof(Countdown),
                new PropertyMetadata(60, OnStartSecondsChanged));

        private static void OnStartSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Countdown countdown && !countdown.IsRunning)
            {
                countdown.CurrentSeconds = (int)e.NewValue;
            }
        }

        #endregion StartSeconds

        #region EndSeconds

        public int EndSeconds
        {
            get => (int)GetValue(EndSecondsProperty);
            set => SetValue(EndSecondsProperty, value);
        }

        public static readonly DependencyProperty EndSecondsProperty =
            DependencyProperty.Register(nameof(EndSeconds), typeof(int), typeof(Countdown),
                new PropertyMetadata(0));

        #endregion EndSeconds

        #region CurrentSeconds

        public int CurrentSeconds
        {
            get => (int)GetValue(CurrentSecondsProperty);
            set => SetValue(CurrentSecondsProperty, value);
        }

        public static readonly DependencyProperty CurrentSecondsProperty =
            DependencyProperty.Register(nameof(CurrentSeconds), typeof(int), typeof(Countdown),
                new PropertyMetadata(60, OnCurrentSecondsChanged));

        private static void OnCurrentSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Countdown countdown)
            {
                countdown.UpdateDisplayText();
                countdown.CheckWarningState();

                if (countdown.IsRunning && countdown.CurrentSeconds <= countdown.EndSeconds)
                {
                    countdown.Complete();
                }
            }
        }

        #endregion CurrentSeconds

        #region IsRunning

        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        public static readonly DependencyProperty IsRunningProperty =
            DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(Countdown),
                new PropertyMetadata(false, OnIsRunningChanged));

        private static void OnIsRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Countdown countdown)
            {
                bool newValue = (bool)e.NewValue;
                bool oldValue = (bool)e.OldValue;

                if (newValue && !oldValue)
                {
                    countdown.UpdateVisualState(true);
                    countdown.RaiseCountdownStarted();
                }
                else if (!newValue && oldValue)
                {
                    countdown.UpdateVisualState(true);
                }
            }
        }

        #endregion IsRunning

        #region IsCompleted

        public bool IsCompleted
        {
            get => (bool)GetValue(IsCompletedProperty);
            set => SetValue(IsCompletedProperty, value);
        }

        public static readonly DependencyProperty IsCompletedProperty =
            DependencyProperty.Register(nameof(IsCompleted), typeof(bool), typeof(Countdown),
                new PropertyMetadata(false, OnIsCompletedChanged));

        private static void OnIsCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Countdown countdown && (bool)e.NewValue)
            {
                countdown.UpdateVisualState(true);
            }
        }

        #endregion IsCompleted

        #region AnimationEnabled

        public bool AnimationEnabled
        {
            get => (bool)GetValue(AnimationEnabledProperty);
            set => SetValue(AnimationEnabledProperty, value);
        }

        public static readonly DependencyProperty AnimationEnabledProperty =
            DependencyProperty.Register(nameof(AnimationEnabled), typeof(bool), typeof(Countdown),
                new PropertyMetadata(true));

        #endregion AnimationEnabled

        #region WarningThreshold

        public int WarningThreshold
        {
            get => (int)GetValue(WarningThresholdProperty);
            set => SetValue(WarningThresholdProperty, value);
        }

        public static readonly DependencyProperty WarningThresholdProperty =
            DependencyProperty.Register(nameof(WarningThreshold), typeof(int), typeof(Countdown),
                new PropertyMetadata(10));

        #endregion WarningThreshold

        #region TimeFormat

        public string TimeFormat
        {
            get => (string)GetValue(TimeFormatProperty);
            set => SetValue(TimeFormatProperty, value);
        }

        public static readonly DependencyProperty TimeFormatProperty =
            DependencyProperty.Register(nameof(TimeFormat), typeof(string), typeof(Countdown),
                new PropertyMetadata("mm\\:ss", OnTimeFormatChanged));

        private static void OnTimeFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Countdown countdown)
            {
                countdown.UpdateDisplayText();
            }
        }

        #endregion TimeFormat

        #region DisplayText

        public string DisplayText
        {
            get => (string)GetValue(DisplayTextProperty);
            private set => SetValue(DisplayTextProperty, value);
        }

        public static readonly DependencyProperty DisplayTextProperty =
            DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(Countdown),
                new PropertyMetadata(string.Empty));

        #endregion DisplayText

        #region 事件

        /// <summary>
        /// 在倒计时开始时发生
        /// </summary>
        public event EventHandler CountdownStarted;

        /// <summary>
        /// 在每秒计时时发生
        /// </summary>
        public event EventHandler<CountdownTickEventArgs> CountdownTick;

        /// <summary>
        /// 在倒计时暂停时发生
        /// </summary>
        public event EventHandler CountdownPaused;

        /// <summary>
        /// 在倒计时恢复时发生
        /// </summary>
        public event EventHandler CountdownResumed;

        /// <summary>
        /// 在倒计时完成时发生
        /// </summary>
        public event EventHandler CountdownCompleted;

        /// <summary>
        /// 在进入警告状态时发生
        /// </summary>
        public event EventHandler WarningStateEntered;

        #endregion 事件

        #region 公共方法

        /// <summary>
        /// 开始倒计时
        /// </summary>
        public void Start()
        {
            if (IsRunning || IsCompleted)
                return;

            if (!IsRunning && CurrentSeconds <= EndSeconds)
            {
                Reset();
            }

            IsRunning = true;
            IsCompleted = false;
            _timer.Start();

            UpdateVisualState(true);
        }

        /// <summary>
        /// 暂停倒计时
        /// </summary>
        public void Pause()
        {
            if (!IsRunning)
                return;

            _timer.Stop();
            IsRunning = false;
            RaiseCountdownPaused();
            UpdateVisualState(true);
        }

        /// <summary>
        /// 恢复倒计时
        /// </summary>
        public void Resume()
        {
            if (IsRunning || IsCompleted)
                return;

            _timer.Start();
            IsRunning = true;
            RaiseCountdownResumed();
            UpdateVisualState(true);
        }

        /// <summary>
        /// 重置倒计时到初始状态
        /// </summary>
        public void Reset()
        {
            _timer.Stop();
            IsRunning = false;
            IsCompleted = false;
            CurrentSeconds = StartSeconds;
            UpdateVisualState(true);
        }

        /// <summary>
        /// 设置当前时间
        /// </summary>
        /// <param name="seconds">秒数</param>
        public void SetTime(int seconds)
        {
            if (IsRunning)
                return;

            CurrentSeconds = seconds;
        }

        #endregion 公共方法

        #region 重写方法

        /// <summary>
        /// 当应用模板时调用
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _animationContainer = GetTemplateChild("PART_AnimationContainer") as FrameworkElement;
            _displayText = GetTemplateChild("PART_DisplayText") as TextBlock;

            _isTemplateApplied = true;
            UpdateDisplayText();
            UpdateVisualState(false);
        }

        #endregion 重写方法

        #region 私有方法

        /// <summary>
        /// 初始化计时器
        /// </summary>
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += OnTimerTick;
        }

        /// <summary>
        /// 计时器Tick事件处理
        /// </summary>
        private void OnTimerTick(object sender, EventArgs e)
        {
            if (CurrentSeconds > EndSeconds)
            {
                CurrentSeconds--;
                RaiseCountdownTick();
            }
            else
            {
                Complete();
            }
        }

        /// <summary>
        /// 完成倒计时
        /// </summary>
        private void Complete()
        {
            _timer.Stop();
            IsRunning = false;
            IsCompleted = true;
            RaiseCountdownCompleted();
        }

        /// <summary>
        /// 更新显示文本
        /// </summary>
        private void UpdateDisplayText()
        {
            TimeSpan time = TimeSpan.FromSeconds(CurrentSeconds);
            DisplayText = time.ToString(TimeFormat);
        }

        /// <summary>
        /// 检查警告状态
        /// </summary>
        private void CheckWarningState()
        {
            if (IsRunning && CurrentSeconds <= WarningThreshold && CurrentSeconds > EndSeconds)
            {
                RaiseWarningStateEntered();
                UpdateVisualState(true);
            }
        }

        /// <summary>
        /// 更新视觉状态
        /// </summary>
        private void UpdateVisualState(bool useTransitions)
        {
            if (!_isTemplateApplied)
                return;

            // 如果动画未启用，则不应用任何视觉状态变化
            if (!AnimationEnabled)
                return;

            if (IsCompleted)
            {
                VisualStateManager.GoToState(this, "Completed", useTransitions);
            }
            else if (IsRunning)
            {
                if (CurrentSeconds <= WarningThreshold)
                {
                    VisualStateManager.GoToState(this, "Warning", useTransitions);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Running", useTransitions);
                }
            }
            else
            {
                if (CurrentSeconds == StartSeconds)
                {
                    VisualStateManager.GoToState(this, "Idle", useTransitions);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Paused", useTransitions);
                }
            }
        }

        /// <summary>
        /// 启用动画效果
        /// </summary>
        private void ApplySlideAnimation()
        {
            // 此方法不需要做任何事情，因为动画效果已经在XAML模板中通过VisualStateManager定义
            // 我们只需要在状态改变时调用UpdateVisualState方法，VisualStateManager会处理动画效果
        }

        /// <summary>
        /// 停止动画效果
        /// </summary>
        private void StopSlideAnimation()
        {
            // 此方法不需要做任何事情，因为动画效果已经在XAML模板中通过VisualStateManager定义
            // 我们只需要在状态改变时调用UpdateVisualState方法，VisualStateManager会处理动画效果
        }

        /// <summary>
        /// 引发CountdownStarted事件
        /// </summary>
        private void RaiseCountdownStarted()
        {
            CountdownStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 引发CountdownTick事件
        /// </summary>
        private void RaiseCountdownTick()
        {
            CountdownTick?.Invoke(this, new CountdownTickEventArgs(CurrentSeconds));
        }

        /// <summary>
        /// 引发CountdownPaused事件
        /// </summary>
        private void RaiseCountdownPaused()
        {
            CountdownPaused?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 引发CountdownResumed事件
        /// </summary>
        private void RaiseCountdownResumed()
        {
            CountdownResumed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 引发CountdownCompleted事件
        /// </summary>
        private void RaiseCountdownCompleted()
        {
            CountdownCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 引发WarningStateEntered事件
        /// </summary>
        private void RaiseWarningStateEntered()
        {
            WarningStateEntered?.Invoke(this, EventArgs.Empty);
        }

        #endregion 私有方法
    }

    /// <summary>
    /// 倒计时Tick事件参数
    /// </summary>
    public class CountdownTickEventArgs : EventArgs
    {
        /// <summary>
        /// 获取当前秒数
        /// </summary>
        public int CurrentSeconds { get; private set; }

        /// <summary>
        /// 初始化CountdownTickEventArgs类的新实例
        /// </summary>
        /// <param name="currentSeconds">当前秒数</param>
        public CountdownTickEventArgs(int currentSeconds)
        {
            CurrentSeconds = currentSeconds;
        }
    }
}