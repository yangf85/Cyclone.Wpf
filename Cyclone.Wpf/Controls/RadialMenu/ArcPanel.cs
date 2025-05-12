using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 弧形布局面板，用于将子元素均匀排布在弧形上
    /// </summary>
    public class ArcPanel : Panel
    {
        #region 依赖属性

        /// <summary>
        /// 定义弧形的起始角度
        /// </summary>
        public static readonly DependencyProperty StartAngleProperty =
            DependencyProperty.Register("StartAngle", typeof(double), typeof(ArcPanel),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 定义弧形的结束角度
        /// </summary>
        public static readonly DependencyProperty EndAngleProperty =
            DependencyProperty.Register("EndAngle", typeof(double), typeof(ArcPanel),
                new FrameworkPropertyMetadata(120.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 定义弧形的半径
        /// </summary>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(ArcPanel),
                new FrameworkPropertyMetadata(150.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        #endregion

        #region 属性访问器

        /// <summary>
        /// 获取或设置弧形的起始角度
        /// </summary>
        public double StartAngle
        {
            get { return (double)GetValue(StartAngleProperty); }
            set { SetValue(StartAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置弧形的结束角度
        /// </summary>
        public double EndAngle
        {
            get { return (double)GetValue(EndAngleProperty); }
            set { SetValue(EndAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置弧形的半径
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        #endregion

        #region 重写方法

        protected override Size MeasureOverride(Size availableSize)
        {
            // 确定面板大小
            double radius = Radius;
            Size desiredSize = new Size(radius * 2, radius * 2);

            // 测量所有子元素
            foreach (UIElement child in Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            Point center = new Point(finalSize.Width / 2, finalSize.Height / 2);

            // 获取父级菜单
            RadialMenu parentMenu = FindParentMenu();
            if (parentMenu == null)
                return finalSize;

            double outerRadius = Radius;
            double innerRadius = parentMenu.Radius; // 使用父菜单的外半径作为当前内半径
            double itemSpacing = parentMenu.ItemSpacingAngle;

            // 计算弧形参数
            double arcLength = EndAngle - StartAngle;
            if (arcLength < 0) arcLength += 360;

            // 计算可用角度和每个项的角度
            double totalAngle = arcLength - (Children.Count * itemSpacing);
            double angleStep = totalAngle / Children.Count;
            double currentAngle = StartAngle;

            // 按角度均匀排布子元素
            for (int i = 0; i < Children.Count; i++)
            {
                if (Children[i] is RadialMenuItem menuItem)
                {
                    // 设置扇形参数
                    menuItem.StartAngle = currentAngle;
                    menuItem.EndAngle = currentAngle + angleStep;
                    menuItem.InnerRadius = innerRadius;
                    menuItem.OuterRadius = outerRadius;

                    // 保存中心角度用于下一级子菜单
                    menuItem.AnglePosition = currentAngle + (angleStep / 2);

                    // 排布项
                    double size = outerRadius * 2;
                    double x = center.X - outerRadius;
                    double y = center.Y - outerRadius;

                    menuItem.Arrange(new Rect(x, y, size, size));

                    currentAngle += angleStep + itemSpacing;
                }
            }

            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            // 可以添加背景弧形绘制代码
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 查找父菜单
        /// </summary>
        private RadialMenu FindParentMenu()
        {
            DependencyObject current = this;
            while (current != null && !(current is RadialMenu))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as RadialMenu;
        }

        #endregion
    }
}