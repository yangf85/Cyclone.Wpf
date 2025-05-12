using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 圆形布局面板，用于将子元素均匀排布在圆周上
    /// </summary>
    public class CircularPanel : Panel
    {
        #region 重写方法

        protected override Size MeasureOverride(Size availableSize)
        {
            // 获取所属菜单以获取半径信息
            RadialMenu parentMenu = FindParentMenu();
            double outerRadius = parentMenu?.Radius ?? 100;
            Size desiredSize = new Size(outerRadius * 2, outerRadius * 2);

            // 测量所有子元素，让它们告知自己需要的空间大小
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

            // 获取所属菜单
            RadialMenu parentMenu = FindParentMenu();
            if (parentMenu == null)
                return finalSize;

            double outerRadius = parentMenu.Radius;
            double innerRadius = parentMenu.InnerRadius;
            double itemSpacing = parentMenu.ItemSpacingAngle;

            // 计算总的可用角度和每个项的角度
            double totalAngle = 360.0 - (Children.Count * itemSpacing);
            double angleStep = totalAngle / Children.Count;
            double currentAngle = 0;

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

                    // 保存中心角度用于子菜单
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