using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 滚动方向
/// </summary>
public enum RunningDirection
{
    /// <summary>
    /// 从右往左
    /// </summary>
    RightToLeft,

    /// <summary>
    /// 从左往右
    /// </summary>
    LeftToRight,

    /// <summary>
    /// 从下往上
    /// </summary>
    BottomToTop,

    /// <summary>
    /// 从上往下
    /// </summary>
    TopToBottom
}

/// <summary>
/// 滚动文字
/// </summary>
[ContentProperty("Text")]
[TemplatePart(Name = "PART_Canvas", Type = typeof(Canvas))]
[TemplatePart(Name = "PART_Txt1", Type = typeof(TextBlock))]
[TemplatePart(Name = "PART_Txt2", Type = typeof(TextBlock))]
public class RunningText : Control
{
    #region DependencyProperties

    /// <summary>
    /// 滚动的文字
    /// </summary>
    public static readonly DependencyProperty TextProperty = TextBlock.TextProperty.AddOwner(typeof(RunningText));

    /// <summary>
    /// 间距
    /// </summary>
    public static readonly DependencyProperty SpaceProperty = DependencyProperty.Register("Space", typeof(double), typeof(RunningText), new PropertyMetadata(double.NaN, OnSpacePropertyChanged));

    /// <summary>
    /// 滚动速度（每秒wpf单位数）
    /// </summary>
    public static readonly DependencyProperty SpeedProperty = DependencyProperty.Register("Speed", typeof(double), typeof(RunningText), new PropertyMetadata(120d, OnSpeedPropertyChanged));

    /// <summary>
    /// 滚动方向
    /// </summary>
    public static readonly DependencyProperty DirectionProperty = DependencyProperty.Register("Direction", typeof(RunningDirection), typeof(RunningText), new PropertyMetadata(RunningDirection.RightToLeft, OnDirectionPropertyChanged));

    /// <summary>
    /// 滚动的文字
    /// </summary>
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }

    /// <summary>
    /// 间距
    /// </summary>
    public double Space { get => (double)GetValue(SpaceProperty); set => SetValue(SpaceProperty, value); }

    /// <summary>
    /// 滚动速度（每秒wpf单位数）
    /// </summary>
    public double Speed { get => (double)GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }

    /// <summary>
    /// 滚动方向
    /// </summary>
    public RunningDirection Direction { get => (RunningDirection)GetValue(DirectionProperty); set => SetValue(DirectionProperty, value); }

    private static void OnSpacePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RunningText)d).BeginUpdate();
    }

    private static void OnSpeedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RunningText)d).BeginUpdate();
    }

    private static void OnDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((RunningText)d).BeginUpdate();
    }

    #endregion DependencyProperties

    static RunningText()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RunningText), new FrameworkPropertyMetadata(typeof(RunningText)));        // 重写默认样式
    }

    /// <summary>
    /// 滚动文字
    /// </summary>
    public RunningText()
    {
        IsVisibleChanged += delegate { BeginUpdate(); };
    }

    #region Methods

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        BeginUpdate();
    }

    /// <summary>
    /// 多个事件同时触发时，仅执行一次
    /// </summary>
    private void BeginUpdate()
    {
        _delayUpdate = Update;

        Dispatcher.InvokeAsync(() =>
        {
            if (_delayUpdate != null)
            {
                _delayUpdate.Invoke();
                _delayUpdate = null;
            }
        }, System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void Update()
    {
        // 停用动画
        if (_storyboard != null)
        {
            _storyboard.Stop();
            _storyboard.Remove();
            _storyboard = null;
        }

        if (_canvas == null || _txt1 == null || _txt2 == null || string.IsNullOrEmpty(Text) || !IsVisible)
            return;

        switch (Direction)
        {
            case RunningDirection.RightToLeft:
                UpdateRightToLeft();
                break;

            case RunningDirection.LeftToRight:
                UpdateLeftToRight();
                break;

            case RunningDirection.BottomToTop:
                UpdateBottomToTop();
                break;

            case RunningDirection.TopToBottom:
                UpdateTopToBottom();
                break;
        }
    }

    private void UpdateRightToLeft()
    {
        GetHorizontal(out double to, out double from, out double len);
        UpdateHorizontal(from, to, len);
    }

    private void UpdateLeftToRight()
    {
        GetHorizontal(out double from, out double to, out double len);
        UpdateHorizontal(from, to, len);
    }

    private void UpdateBottomToTop()
    {
        GetVertical(out double to, out double from, out double len);
        UpdateVertical(from, to, len);
    }

    private void UpdateTopToBottom()
    {
        GetVertical(out double from, out double to, out double len);
        UpdateVertical(from, to, len);
    }

    private void GetHorizontal(out double from, out double to, out double len)
    {
        // 计算起始位置
        var width_canvas = _canvas.ActualWidth;
        var width_txt = _txt1.ActualWidth;

        from = -width_txt;
        to = width_canvas;

        if (double.IsNaN(Space) || Space < 0)
            len = width_txt < width_canvas ? width_canvas : width_txt + width_canvas;
        else
            len = width_txt < width_canvas - Space ? width_canvas : width_txt + Space;
    }

    private void UpdateHorizontal(double from, double to, double len)
    {
        // 复位
        Canvas.SetLeft(_txt1, from);
        Canvas.SetLeft(_txt2, from);
        _txt1.SetCurrentValue(Canvas.LeftProperty, from);
        _txt2.SetCurrentValue(Canvas.LeftProperty, from);

        var begin = TimeSpan.FromSeconds(len / Speed);      // 第二个动画延迟时间
        var duration = TimeSpan.FromSeconds(Math.Abs(from - to) / Speed);     // 动画从开始到结束的时间
        var total = begin + begin;      // 加上延迟，一次动画的时间

        _storyboard = new Storyboard();

        var ani1 = new DoubleAnimationUsingKeyFrames
        {
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani1.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, TimeSpan.FromSeconds(0)));
        ani1.KeyFrames.Add(new LinearDoubleKeyFrame(to, duration));

        Storyboard.SetTarget(ani1, _txt1);
        Storyboard.SetTargetProperty(ani1, new PropertyPath(Canvas.LeftProperty));

        _storyboard.Children.Add(ani1);

        var ani2 = new DoubleAnimationUsingKeyFrames()
        {
            BeginTime = begin,
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani2.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, TimeSpan.FromSeconds(0)));
        ani2.KeyFrames.Add(new LinearDoubleKeyFrame(to, duration));

        Storyboard.SetTarget(ani2, _txt2);
        Storyboard.SetTargetProperty(ani2, new PropertyPath(Canvas.LeftProperty));

        _storyboard.Children.Add(ani2);

        #region 不显示时隐藏，不造成性能浪费

        _txt1.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
        _txt2.SetCurrentValue(VisibilityProperty, Visibility.Hidden);

        var ani11 = new ObjectAnimationUsingKeyFrames
        {
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani11.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Visible, KeyTime = TimeSpan.FromSeconds(0) });
        ani11.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Hidden, KeyTime = duration });

        Storyboard.SetTarget(ani11, _txt1);
        Storyboard.SetTargetProperty(ani11, new PropertyPath(VisibilityProperty));

        _storyboard.Children.Add(ani11);

        var ani22 = new ObjectAnimationUsingKeyFrames
        {
            BeginTime = begin,
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani22.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Visible, KeyTime = TimeSpan.FromSeconds(0) });
        ani22.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Hidden, KeyTime = duration });

        Storyboard.SetTarget(ani22, _txt2);
        Storyboard.SetTargetProperty(ani22, new PropertyPath(VisibilityProperty));

        _storyboard.Children.Add(ani22);

        #endregion 不显示时隐藏，不造成性能浪费

        _storyboard.Begin();
    }

    private void GetVertical(out double from, out double to, out double len)
    {
        // 计算起始位置
        var height_canvas = _canvas.ActualHeight;
        var height_txt = _txt1.ActualHeight;

        from = -height_txt;
        to = height_canvas;

        if (double.IsNaN(Space) || Space < 0)
            len = height_txt < height_canvas ? height_canvas : height_txt + height_canvas;
        else
            len = height_txt < height_canvas - Space ? height_canvas : height_txt + Space;
    }

    private void UpdateVertical(double from, double to, double len)
    {
        // 复位
        Canvas.SetTop(_txt1, from);
        Canvas.SetTop(_txt2, from);
        _txt1.SetCurrentValue(Canvas.TopProperty, from);
        _txt2.SetCurrentValue(Canvas.TopProperty, from);

        var begin = TimeSpan.FromSeconds(len / Speed);      // 第二个动画延迟时间
        var duration = TimeSpan.FromSeconds(Math.Abs(from - to) / Speed);     // 动画从开始到结束的时间
        var total = begin + begin;      // 加上延迟，一次动画的时间

        _storyboard = new Storyboard();

        var ani1 = new DoubleAnimationUsingKeyFrames
        {
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani1.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, TimeSpan.FromSeconds(0)));
        ani1.KeyFrames.Add(new LinearDoubleKeyFrame(to, duration));

        Storyboard.SetTarget(ani1, _txt1);
        Storyboard.SetTargetProperty(ani1, new PropertyPath(Canvas.TopProperty));

        _storyboard.Children.Add(ani1);

        var ani2 = new DoubleAnimationUsingKeyFrames()
        {
            BeginTime = begin,
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani2.KeyFrames.Add(new DiscreteDoubleKeyFrame(from, TimeSpan.FromSeconds(0)));
        ani2.KeyFrames.Add(new LinearDoubleKeyFrame(to, duration));

        Storyboard.SetTarget(ani2, _txt2);
        Storyboard.SetTargetProperty(ani2, new PropertyPath(Canvas.TopProperty));

        _storyboard.Children.Add(ani2);

        #region 不显示时隐藏，不造成性能浪费

        _txt1.SetCurrentValue(VisibilityProperty, Visibility.Hidden);
        _txt2.SetCurrentValue(VisibilityProperty, Visibility.Hidden);

        var ani11 = new ObjectAnimationUsingKeyFrames
        {
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani11.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Visible, KeyTime = TimeSpan.FromSeconds(0) });
        ani11.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Hidden, KeyTime = duration });

        Storyboard.SetTarget(ani11, _txt1);
        Storyboard.SetTargetProperty(ani11, new PropertyPath(VisibilityProperty));

        _storyboard.Children.Add(ani11);

        var ani22 = new ObjectAnimationUsingKeyFrames
        {
            BeginTime = begin,
            Duration = total,
            RepeatBehavior = RepeatBehavior.Forever
        };

        ani22.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Visible, KeyTime = TimeSpan.FromSeconds(0) });
        ani22.KeyFrames.Add(new DiscreteObjectKeyFrame { Value = Visibility.Hidden, KeyTime = duration });

        Storyboard.SetTarget(ani22, _txt2);
        Storyboard.SetTargetProperty(ani22, new PropertyPath(VisibilityProperty));

        _storyboard.Children.Add(ani22);

        #endregion 不显示时隐藏，不造成性能浪费

        _storyboard.Begin();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_canvas != null)
            _canvas.SizeChanged -= OnSizeChanged;

        if (_txt1 != null)
            _txt1.SizeChanged -= OnSizeChanged;

        _canvas = Template.FindName("PART_Canvas", this) as Canvas;
        _txt1 = Template.FindName("PART_Txt1", this) as TextBlock;
        _txt2 = Template.FindName("PART_Txt2", this) as TextBlock;

        if (_canvas == null || _txt1 == null || _txt2 == null)
            return;

        _txt1.SizeChanged += OnSizeChanged;
        _canvas.SizeChanged += OnSizeChanged;
    }

    #endregion Methods

    #region Fields

    private Canvas _canvas;

    private TextBlock _txt1;

    private TextBlock _txt2;

    private Action _delayUpdate;

    private Storyboard _storyboard;

    #endregion Fields
}