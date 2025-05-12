using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 圆形菜单项，支持自绘制的扇形外观
    /// </summary>
    [TemplatePart(Name = "PART_Popup", Type = typeof(System.Windows.Controls.Primitives.Popup))]
    public class RadialMenuItem : MenuItem
    {
        #region 依赖属性

        /// <summary>
        /// 定义扇形起始角度
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(RadialMenuItem),
                new PropertyMetadata(0.0, OnGeometryPropertyChanged));

        /// <summary>
        /// 定义扇形结束角度
        /// </summary>
        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(RadialMenuItem),
                new PropertyMetadata(45.0, OnGeometryPropertyChanged));

        /// <summary>
        /// 定义扇形内圆半径
        /// </summary>
        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(RadialMenuItem),
                new PropertyMetadata(50.0, OnGeometryPropertyChanged));

        /// <summary>
        /// 定义扇形外圆半径
        /// </summary>
        public static readonly DependencyProperty OuterRadiusProperty =
            DependencyProperty.Register("OuterRadius", typeof(double), typeof(RadialMenuItem),
                new PropertyMetadata(100.0, OnGeometryPropertyChanged));

        /// <summary>
        /// 定义菜单项在圆周上的角度位置
        /// </summary>
        public static readonly DependencyProperty AnglePositionProperty =
            DependencyProperty.Register("AnglePosition", typeof(double), typeof(RadialMenuItem),
                new PropertyMetadata(0.0));

        /// <summary>
        /// 定义子菜单弧形的起始角度
        /// </summary>
        public static readonly DependencyProperty ArcStartAngleProperty =
            DependencyProperty.Register("ArcStartAngle", typeof(double), typeof(RadialMenuItem),
                new PropertyMetadata(0.0));

        /// <summary>
        /// 定义子菜单弧形的结束角度
        /// </summary>
        public static readonly DependencyProperty ArcEndAngleProperty =
            DependencyProperty.Register("ArcEndAngle", typeof(double), typeof(RadialMenuItem),
                new PropertyMetadata(90.0));

        /// <summary>
        /// 定义菜单项的层级
        /// </summary>
        public static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register("Level", typeof(int), typeof(RadialMenuItem),
                new PropertyMetadata(0));

        /// <summary>
        /// 定义扇形几何形状
        /// </summary>
        public static readonly DependencyProperty SectorGeometryProperty =
            DependencyProperty.Register("SectorGeometry", typeof(Geometry), typeof(RadialMenuItem),
                new PropertyMetadata(null));

        #endregion

        #region 私有字段

        private bool _isHovered = false;
        private bool _isPressed = false;
        private bool _geometryInvalidated = true;

        #endregion

        #region 属性访问器

        /// <summary>
        /// 获取或设置扇形起始角度
        /// </summary>
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置扇形结束角度
        /// </summary>
        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置扇形内圆半径
        /// </summary>
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        /// <summary>
        /// 获取或设置扇形外圆半径
        /// </summary>
        public double OuterRadius
        {
            get { return (double)GetValue(OuterRadiusProperty); }
            set { SetValue(OuterRadiusProperty, value); }
        }

        /// <summary>
        /// 获取或设置菜单项在圆周上的角度位置
        /// </summary>
        public double AnglePosition
        {
            get { return (double)GetValue(AnglePositionProperty); }
            set { SetValue(AnglePositionProperty, value); }
        }

        /// <summary>
        /// 获取或设置子菜单弧形的起始角度
        /// </summary>
        public double ArcStartAngle
        {
            get { return (double)GetValue(ArcStartAngleProperty); }
            set { SetValue(ArcStartAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置子菜单弧形的结束角度
        /// </summary>
        public double ArcEndAngle
        {
            get { return (double)GetValue(ArcEndAngleProperty); }
            set { SetValue(ArcEndAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置菜单项的层级
        /// </summary>
        public int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        /// <summary>
        /// 获取或设置扇形几何形状
        /// </summary>
        public Geometry SectorGeometry
        {
            get
            {
                if (_geometryInvalidated)
                {
                    UpdateSectorGeometry();
                    _geometryInvalidated = false;
                }
                return (Geometry)GetValue(SectorGeometryProperty);
            }
            private set { SetValue(SectorGeometryProperty, value); }
        }

        /// <summary>
        /// 获取中心角度
        /// </summary>
        public double MiddleAngle
        {
            get { return (StartAngle + EndAngle) / 2; }
        }

        /// <summary>
        /// 获取文本放置的X偏移
        /// </summary>
        public double ContentOffsetX
        {
            get
            {
                double middleRadius = (InnerRadius + OuterRadius) / 2;
                double angle = MiddleAngle * Math.PI / 180;
                return middleRadius * Math.Cos(angle);
            }
        }

        /// <summary>
        /// 获取文本放置的Y偏移
        /// </summary>
        public double ContentOffsetY
        {
            get
            {
                double middleRadius = (InnerRadius + OuterRadius) / 2;
                double angle = MiddleAngle * Math.PI / 180;
                return middleRadius * Math.Sin(angle);
            }
        }

        /// <summary>
        /// 获取或设置父菜单
        /// </summary>
        public RadialMenu ParentRadialMenu { get; set; }

        #endregion

        #region 构造函数

        static RadialMenuItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadialMenuItem),
                new FrameworkPropertyMetadata(typeof(RadialMenuItem)));
        }

        #endregion

        #region 重写方法

        protected override DependencyObject GetContainerForItemOverride()
        {
            RadialMenuItem item = new RadialMenuItem();
            item.ParentRadialMenu = ParentRadialMenu;
            item.Level = Level + 1;
            return item;
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is RadialMenuItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is RadialMenuItem menuItem)
            {
                menuItem.ParentRadialMenu = ParentRadialMenu;
                menuItem.Level = Level + 1;
            }
        }

        protected override void OnSubmenuOpened(RoutedEventArgs e)
        {
            base.OnSubmenuOpened(e);

            if (ParentRadialMenu != null)
            {
                // 计算子菜单的弧形位置
                double arcAngle = ParentRadialMenu.SubMenuArcAngle;
                ArcStartAngle = AnglePosition - (arcAngle / 2);
                ArcEndAngle = AnglePosition + (arcAngle / 2);
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            // 不调用基类的OnRender，完全自定义绘制

            // 确保几何形状是最新的
            if (_geometryInvalidated)
            {
                UpdateSectorGeometry();
                _geometryInvalidated = false;
            }

            // 选择填充颜色
            Brush fillBrush;
            Pen strokePen;

            if (_isPressed)
            {
                fillBrush = TryFindResource("Background.Pressed") as SolidColorBrush ??
                            new SolidColorBrush(Color.FromRgb(50, 50, 50));
            }
            else if (_isHovered)
            {
                fillBrush = TryFindResource("Background.Hover") as SolidColorBrush ??
                            new SolidColorBrush(Color.FromRgb(100, 100, 100));
            }
            else if (IsChecked)
            {
                fillBrush = TryFindResource("Background.Checked") as SolidColorBrush ??
                            new SolidColorBrush(Colors.Black);
            }
            else if (IsSubmenuOpen)
            {
                fillBrush = TryFindResource("Background.Selected") as SolidColorBrush ??
                            new SolidColorBrush(Color.FromRgb(40, 40, 40));
            }
            else
            {
                fillBrush = TryFindResource("Background.Default") as SolidColorBrush ??
                            new SolidColorBrush(Color.FromRgb(70, 70, 70));
            }

            strokePen = new Pen(TryFindResource("Border.Default") as Brush ?? Brushes.DarkGray, 1);

            // 绘制扇形
            drawingContext.DrawGeometry(fillBrush, strokePen, SectorGeometry);

            // 绘制文本内容
            if (Header != null)
            {
                // 计算文本位置
                double middleAngle = MiddleAngle * Math.PI / 180;
                double middleRadius = (InnerRadius + OuterRadius) / 2;

                Point textPosition = new Point(
                    ActualWidth / 2 + middleRadius * Math.Cos(middleAngle),
                    ActualHeight / 2 + middleRadius * Math.Sin(middleAngle));

                // 准备文本格式
                Brush textBrush;
                if (IsChecked || IsSubmenuOpen)
                {
                    textBrush = TryFindResource("Foreground.Selected") as SolidColorBrush ?? Brushes.White;
                }
                else
                {
                    textBrush = TryFindResource("Foreground.Default") as SolidColorBrush ?? Brushes.White;
                }

                FormattedText formattedText = new FormattedText(
                    Header.ToString(),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    14,
                    textBrush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                // 文本居中处理
                textPosition.X -= formattedText.Width / 2;
                textPosition.Y -= formattedText.Height / 2;

                // 绘制文本
                drawingContext.DrawText(formattedText, textPosition);
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            InvalidateVisual();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
            InvalidateVisual();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (IsPointInSector(e.GetPosition(this)))
            {
                _isPressed = true;
                CaptureMouse();
                InvalidateVisual();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isPressed && IsPointInSector(e.GetPosition(this)))
            {
                if (HasItems)
                {
                    IsSubmenuOpen = !IsSubmenuOpen;
                }
                else
                {
                    Command?.Execute(CommandParameter);
                    IsChecked = true;
                }
            }

            _isPressed = false;
            ReleaseMouseCapture();
            InvalidateVisual();
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            Point point = hitTestParameters.HitPoint;

            if (IsPointInSector(point))
            {
                return new PointHitTestResult(this, point);
            }

            return null;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 更新扇形几何形状
        /// </summary>
        private void UpdateSectorGeometry()
        {
            Point center = new Point(OuterRadius, OuterRadius);

            // 创建扇形路径
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext ctx = geometry.Open())
            {
                // 内圆弧起点
                double startRad = StartAngle * Math.PI / 180;
                Point startPoint = new Point(
                    center.X + InnerRadius * Math.Cos(startRad),
                    center.Y + InnerRadius * Math.Sin(startRad));

                ctx.BeginFigure(startPoint, true, true);

                // 外圆弧起点
                Point outerStart = new Point(
                    center.X + OuterRadius * Math.Cos(startRad),
                    center.Y + OuterRadius * Math.Sin(startRad));

                ctx.LineTo(outerStart, true, false);

                // 外圆弧
                double endRad = EndAngle * Math.PI / 180;
                Point outerEnd = new Point(
                    center.X + OuterRadius * Math.Cos(endRad),
                    center.Y + OuterRadius * Math.Sin(endRad));

                bool isLargeArc = (EndAngle - StartAngle) > 180;

                ctx.ArcTo(
                    outerEnd,
                    new Size(OuterRadius, OuterRadius),
                    0,
                    isLargeArc,
                    SweepDirection.Clockwise,
                    true, false);

                // 内圆弧终点
                Point innerEnd = new Point(
                    center.X + InnerRadius * Math.Cos(endRad),
                    center.Y + InnerRadius * Math.Sin(endRad));

                ctx.LineTo(innerEnd, true, false);

                // 内圆弧
                ctx.ArcTo(
                    startPoint,
                    new Size(InnerRadius, InnerRadius),
                    0,
                    isLargeArc,
                    SweepDirection.Counterclockwise,
                    true, false);
            }

            // 冻结几何形状以提高性能
            geometry.Freeze();
            SectorGeometry = geometry;
        }

        /// <summary>
        /// 判断点是否在扇形区域内
        /// </summary>
        private bool IsPointInSector(Point point)
        {
            Point center = new Point(ActualWidth / 2, ActualHeight / 2);
            double dx = point.X - center.X;
            double dy = point.Y - center.Y;

            // 计算距离
            double distance = Math.Sqrt(dx * dx + dy * dy);
            if (distance < InnerRadius || distance > OuterRadius)
                return false;

            // 计算角度
            double angle = Math.Atan2(dy, dx) * 180 / Math.PI;
            if (angle < 0)
                angle += 360;

            // 检查角度是否在范围内
            if (StartAngle <= EndAngle)
            {
                return angle >= StartAngle && angle <= EndAngle;
            }
            else
            {
                // 处理跨越0度线的情况
                return angle >= StartAngle || angle <= EndAngle;
            }
        }

        #endregion

        #region 事件处理程序

        private static void OnGeometryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialMenuItem item = (RadialMenuItem)d;
            item._geometryInvalidated = true;
            item.InvalidateVisual();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 关闭子菜单并级联关闭所有下级子菜单
        /// </summary>
        public void CloseSubmenu()
        {
            // 关闭子项的子菜单
            foreach (object item in Items)
            {
                if (item is RadialMenuItem menuItem)
                {
                    menuItem.CloseSubmenu();
                }
            }

            // 关闭当前子菜单
            IsSubmenuOpen = false;
        }

        #endregion
    }
}