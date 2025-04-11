using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 抽屉控件：从容器边缘滑入滑出的面板控件
    /// </summary>
    [TemplatePart(Name = ElementDrawerPanel, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementOverlay, Type = typeof(FrameworkElement))]
    public class Drawer : ContentControl
    {
        #region 常量

        /// <summary>
        /// 抽屉面板元素名称
        /// </summary>
        private const string ElementDrawerPanel = "PART_DrawerPanel";

        /// <summary>
        /// 遮罩层元素名称
        /// </summary>
        private const string ElementOverlay = "PART_Overlay";

        #endregion 常量

        #region 私有字段

        private FrameworkElement _drawerPanel;
        private FrameworkElement _overlay;
        private TranslateTransform _translateTransform;

        #endregion 私有字段

        #region 依赖属性

        /// <summary>
        /// 定义抽屉内容的依赖属性
        /// </summary>
        public static readonly DependencyProperty DrawerContentProperty =
            DependencyProperty.Register(nameof(DrawerContent), typeof(object), typeof(Drawer),
                new PropertyMetadata(null));

        /// <summary>
        /// 定义抽屉是否打开的依赖属性
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Drawer),
                new PropertyMetadata(false, OnIsOpenChanged));

        /// <summary>
        /// 定义抽屉宽度的依赖属性
        /// </summary>
        public static readonly DependencyProperty DrawerWidthProperty =
            DependencyProperty.Register(nameof(DrawerWidth), typeof(double), typeof(Drawer),
                new PropertyMetadata(300.0, OnDrawerSizeChanged));

        /// <summary>
        /// 定义抽屉高度的依赖属性
        /// </summary>
        public static readonly DependencyProperty DrawerHeightProperty =
            DependencyProperty.Register(nameof(DrawerHeight), typeof(double), typeof(Drawer),
                new PropertyMetadata(double.NaN, OnDrawerSizeChanged));

        /// <summary>
        /// 定义抽屉位置的依赖属性
        /// </summary>
        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(nameof(Placement), typeof(DrawerPlacement), typeof(Drawer),
                new PropertyMetadata(DrawerPlacement.Left, OnPlacementChanged));

        /// <summary>
        /// 定义动画持续时间的依赖属性
        /// </summary>
        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(nameof(AnimationDuration), typeof(Duration), typeof(Drawer),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(300))));

        /// <summary>
        /// 定义是否可以通过点击遮罩层关闭抽屉的依赖属性
        /// </summary>
        public static readonly DependencyProperty CloseOnOverlayClickProperty =
            DependencyProperty.Register(nameof(CloseOnOverlayClick), typeof(bool), typeof(Drawer),
                new PropertyMetadata(true));

        #endregion 依赖属性

        #region 路由事件

        /// <summary>
        /// 抽屉开始打开时触发的路由事件
        /// </summary>
        public static readonly RoutedEvent OpeningEvent =
            EventManager.RegisterRoutedEvent(nameof(Opening), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(Drawer));

        /// <summary>
        /// 抽屉打开完成后触发的路由事件
        /// </summary>
        public static readonly RoutedEvent OpenedEvent =
            EventManager.RegisterRoutedEvent(nameof(Opened), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(Drawer));

        /// <summary>
        /// 抽屉开始关闭时触发的路由事件
        /// </summary>
        public static readonly RoutedEvent ClosingEvent =
            EventManager.RegisterRoutedEvent(nameof(Closing), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(Drawer));

        /// <summary>
        /// 抽屉关闭完成后触发的路由事件
        /// </summary>
        public static readonly RoutedEvent ClosedEvent =
            EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(Drawer));

        #endregion 路由事件

        #region 事件

        /// <summary>
        /// 抽屉开始打开时触发的事件
        /// </summary>
        public event RoutedEventHandler Opening
        {
            add { AddHandler(OpeningEvent, value); }
            remove { RemoveHandler(OpeningEvent, value); }
        }

        /// <summary>
        /// 抽屉打开完成后触发的事件
        /// </summary>
        public event RoutedEventHandler Opened
        {
            add { AddHandler(OpenedEvent, value); }
            remove { RemoveHandler(OpenedEvent, value); }
        }

        /// <summary>
        /// 抽屉开始关闭时触发的事件
        /// </summary>
        public event RoutedEventHandler Closing
        {
            add { AddHandler(ClosingEvent, value); }
            remove { RemoveHandler(ClosingEvent, value); }
        }

        /// <summary>
        /// 抽屉关闭完成后触发的事件
        /// </summary>
        public event RoutedEventHandler Closed
        {
            add { AddHandler(ClosedEvent, value); }
            remove { RemoveHandler(ClosedEvent, value); }
        }

        #endregion 事件

        #region 属性

        /// <summary>
        /// 获取或设置抽屉的内容
        /// </summary>
        public object DrawerContent
        {
            get { return GetValue(DrawerContentProperty); }
            set { SetValue(DrawerContentProperty, value); }
        }

        /// <summary>
        /// 获取或设置抽屉是否打开
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// 获取或设置抽屉的宽度
        /// </summary>
        public double DrawerWidth
        {
            get { return (double)GetValue(DrawerWidthProperty); }
            set { SetValue(DrawerWidthProperty, value); }
        }

        /// <summary>
        /// 获取或设置抽屉的高度
        /// </summary>
        public double DrawerHeight
        {
            get { return (double)GetValue(DrawerHeightProperty); }
            set { SetValue(DrawerHeightProperty, value); }
        }

        /// <summary>
        /// 获取或设置抽屉的放置位置
        /// </summary>
        public DrawerPlacement Placement
        {
            get { return (DrawerPlacement)GetValue(PlacementProperty); }
            set { SetValue(PlacementProperty, value); }
        }

        /// <summary>
        /// 获取或设置动画的持续时间
        /// </summary>
        public Duration AnimationDuration
        {
            get { return (Duration)GetValue(AnimationDurationProperty); }
            set { SetValue(AnimationDurationProperty, value); }
        }

        /// <summary>
        /// 获取或设置是否可以通过点击遮罩层关闭抽屉
        /// </summary>
        public bool CloseOnOverlayClick
        {
            get { return (bool)GetValue(CloseOnOverlayClickProperty); }
            set { SetValue(CloseOnOverlayClickProperty, value); }
        }

        #endregion 属性

        #region 构造函数

        /// <summary>
        /// 静态构造函数，注册默认样式
        /// </summary>
        static Drawer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Drawer),
                new FrameworkPropertyMetadata(typeof(Drawer)));
        }

        /// <summary>
        /// 初始化Drawer的新实例
        /// </summary>
        public Drawer()
        {
            // 设置默认值
            Panel.SetZIndex(this, 1000);
        }

        #endregion 构造函数

        #region 公共方法

        /// <summary>
        /// 当应用模板时调用
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 解除旧模板元素的事件
            if (_overlay != null)
            {
                _overlay.MouseLeftButtonDown -= OnOverlayClick;
            }

            // 获取模板元素
            _drawerPanel = GetTemplateChild(ElementDrawerPanel) as FrameworkElement;
            _overlay = GetTemplateChild(ElementOverlay) as FrameworkElement;

            if (_drawerPanel != null)
            {
                // 创建并应用平移变换
                _translateTransform = new TranslateTransform();
                _drawerPanel.RenderTransform = _translateTransform;

                // 更新抽屉尺寸
                UpdateDrawerSize();

                // 设置初始位置和可见性
                if (IsOpen)
                {
                    _drawerPanel.Visibility = Visibility.Visible;
                    _translateTransform.X = 0;
                    _translateTransform.Y = 0;
                }
                else
                {
                    _drawerPanel.Visibility = Visibility.Collapsed;
                    UpdateClosedPosition();
                }
            }

            // 添加遮罩层点击事件
            if (_overlay != null && CloseOnOverlayClick)
            {
                _overlay.MouseLeftButtonDown += OnOverlayClick;
            }

            // 设置遮罩层的初始状态
            UpdateOverlayVisibility();
        }

        /// <summary>
        /// 打开抽屉
        /// </summary>
        public void Open()
        {
            IsOpen = true;
        }

        /// <summary>
        /// 关闭抽屉
        /// </summary>
        public void Close()
        {
            IsOpen = false;
        }

        /// <summary>
        /// 切换抽屉状态
        /// </summary>
        public void Toggle()
        {
            IsOpen = !IsOpen;
        }

        #endregion 公共方法

        #region 私有方法

        /// <summary>
        /// 更新遮罩层的可见性
        /// </summary>
        private void UpdateOverlayVisibility()
        {
            if (_overlay == null) return;

            if (IsOpen)
            {
                _overlay.Visibility = Visibility.Visible;
                // 添加渐入动画
                var animation = new DoubleAnimation(0, 1, AnimationDuration);
                _overlay.BeginAnimation(OpacityProperty, animation);
            }
            else
            {
                // 添加渐出动画
                var animation = new DoubleAnimation(0, AnimationDuration);
                animation.Completed += (s, e) => _overlay.Visibility = Visibility.Collapsed;
                _overlay.BeginAnimation(OpacityProperty, animation);
            }
        }

        /// <summary>
        /// IsOpen属性改变时的回调
        /// </summary>
        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Drawer drawer = (Drawer)d;
            bool newValue = (bool)e.NewValue;
            bool oldValue = (bool)e.OldValue;

            if (newValue != oldValue)
            {
                if (newValue)
                {
                    // 触发Opening事件
                    drawer.RaiseEvent(new RoutedEventArgs(OpeningEvent, drawer));

                    // 确保抽屉面板在打开时可见
                    if (drawer._drawerPanel != null)
                    {
                        drawer._drawerPanel.Visibility = Visibility.Visible;
                    }

                    drawer.UpdateOverlayVisibility();
                    drawer.AnimateToOpenPosition();
                }
                else
                {
                    // 触发Closing事件
                    drawer.RaiseEvent(new RoutedEventArgs(ClosingEvent, drawer));
                    drawer.UpdateOverlayVisibility();

                    // 确保抽屉面板在动画期间保持可见
                    if (drawer._drawerPanel != null)
                    {
                        drawer._drawerPanel.Visibility = Visibility.Visible;
                    }

                    drawer.AnimateToClosedPosition();
                }
            }
        }

        /// <summary>
        /// 抽屉尺寸改变时的回调
        /// </summary>
        private static void OnDrawerSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Drawer drawer = (Drawer)d;
            drawer.UpdateDrawerSize();

            // 如果抽屉是关闭状态，更新关闭位置
            if (!drawer.IsOpen)
            {
                drawer.UpdateClosedPosition();
            }
        }

        /// <summary>
        /// 抽屉位置改变时的回调
        /// </summary>
        private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Drawer drawer = (Drawer)d;
            drawer.UpdateDrawerSize();

            // 如果抽屉是关闭状态，更新关闭位置
            if (!drawer.IsOpen)
            {
                drawer.UpdateClosedPosition();
            }
        }

        /// <summary>
        /// 更新抽屉的尺寸
        /// </summary>
        private void UpdateDrawerSize()
        {
            if (_drawerPanel == null) return;

            switch (Placement)
            {
                case DrawerPlacement.Left:
                case DrawerPlacement.Right:
                    _drawerPanel.Width = DrawerWidth;
                    _drawerPanel.Height = double.NaN; // 自动
                    break;

                case DrawerPlacement.Top:
                case DrawerPlacement.Bottom:
                    _drawerPanel.Width = double.NaN; // 自动
                    _drawerPanel.Height = double.IsNaN(DrawerHeight) ? 300 : DrawerHeight;
                    break;
            }
        }

        /// <summary>
        /// 更新关闭状态下的位置
        /// </summary>
        private void UpdateClosedPosition()
        {
            if (_translateTransform == null) return;

            switch (Placement)
            {
                case DrawerPlacement.Left:
                    _translateTransform.X = -DrawerWidth;
                    _translateTransform.Y = 0;
                    break;

                case DrawerPlacement.Right:
                    _translateTransform.X = DrawerWidth;
                    _translateTransform.Y = 0;
                    break;

                case DrawerPlacement.Top:
                    _translateTransform.X = 0;
                    _translateTransform.Y = -(_drawerPanel?.ActualHeight ?? 300);
                    break;

                case DrawerPlacement.Bottom:
                    _translateTransform.X = 0;
                    _translateTransform.Y = _drawerPanel?.ActualHeight ?? 300;
                    break;
            }
        }

        /// <summary>
        /// 执行打开动画
        /// </summary>
        private void AnimateToOpenPosition()
        {
            if (_translateTransform == null) return;

            DoubleAnimation animation = null;

            switch (Placement)
            {
                case DrawerPlacement.Left:
                case DrawerPlacement.Right:
                    animation = new DoubleAnimation(0, AnimationDuration);
                    animation.Completed += (s, e) => OnOpenCompleted();
                    _translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                    break;

                case DrawerPlacement.Top:
                case DrawerPlacement.Bottom:
                    animation = new DoubleAnimation(0, AnimationDuration);
                    animation.Completed += (s, e) => OnOpenCompleted();
                    _translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
                    break;
            }
        }

        /// <summary>
        /// 执行关闭动画
        /// </summary>
        private void AnimateToClosedPosition()
        {
            if (_translateTransform == null) return;

            DoubleAnimation animation = null;
            double targetPosition = 0;

            switch (Placement)
            {
                case DrawerPlacement.Left:
                    targetPosition = -DrawerWidth;
                    animation = new DoubleAnimation(targetPosition, AnimationDuration);
                    animation.Completed += (s, e) => OnCloseCompleted();
                    _translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                    break;

                case DrawerPlacement.Right:
                    targetPosition = DrawerWidth;
                    animation = new DoubleAnimation(targetPosition, AnimationDuration);
                    animation.Completed += (s, e) => OnCloseCompleted();
                    _translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
                    break;

                case DrawerPlacement.Top:
                    targetPosition = -(_drawerPanel?.ActualHeight ?? 300);
                    animation = new DoubleAnimation(targetPosition, AnimationDuration);
                    animation.Completed += (s, e) => OnCloseCompleted();
                    _translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
                    break;

                case DrawerPlacement.Bottom:
                    targetPosition = _drawerPanel?.ActualHeight ?? 300;
                    animation = new DoubleAnimation(targetPosition, AnimationDuration);
                    animation.Completed += (s, e) => OnCloseCompleted();
                    _translateTransform.BeginAnimation(TranslateTransform.YProperty, animation);
                    break;
            }
        }

        /// <summary>
        /// 点击遮罩层时的处理方法
        /// </summary>
        private void OnOverlayClick(object sender, MouseButtonEventArgs e)
        {
            if (CloseOnOverlayClick)
            {
                Close();
                e.Handled = true;
            }
        }

        /// <summary>
        /// 打开动画完成时的处理方法
        /// </summary>
        private void OnOpenCompleted()
        {
            // 触发Opened事件
            RaiseEvent(new RoutedEventArgs(OpenedEvent, this));
        }

        /// <summary>
        /// 关闭动画完成时的处理方法
        /// </summary>
        private void OnCloseCompleted()
        {
            // 动画完成后手动隐藏抽屉面板
            if (_drawerPanel != null)
            {
                _drawerPanel.Visibility = Visibility.Collapsed;
            }

            // 触发Closed事件
            RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        }

        #endregion 私有方法
    }

    /// <summary>
    /// 指定抽屉的放置位置
    /// </summary>
    public enum DrawerPlacement
    {
        /// <summary>
        /// 从左侧滑入
        /// </summary>
        Left,

        /// <summary>
        /// 从右侧滑入
        /// </summary>
        Right,

        /// <summary>
        /// 从顶部滑入
        /// </summary>
        Top,

        /// <summary>
        /// 从底部滑入
        /// </summary>
        Bottom
    }
}