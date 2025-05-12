using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 圆形菜单控件，提供多层级的圆形菜单布局和交互
    /// </summary>
    [TemplatePart(Name = "PART_CenterContent", Type = typeof(ContentPresenter))]
    public class RadialMenu : Menu
    {
        #region 依赖属性

        /// <summary>
        /// 定义外圆半径
        /// </summary>
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(double), typeof(RadialMenu),
                new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                      FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 定义内圆半径
        /// </summary>
        public static readonly DependencyProperty InnerRadiusProperty =
            DependencyProperty.Register("InnerRadius", typeof(double), typeof(RadialMenu),
                new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                    FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// 定义中心内容
        /// </summary>
        public static readonly DependencyProperty CenterContentProperty =
            DependencyProperty.Register("CenterContent", typeof(object), typeof(RadialMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// 定义中心内容模板
        /// </summary>
        public static readonly DependencyProperty CenterContentTemplateProperty =
            DependencyProperty.Register("CenterContentTemplate", typeof(DataTemplate), typeof(RadialMenu),
                new PropertyMetadata(null));

        /// <summary>
        /// 定义子菜单半径
        /// </summary>
        public static readonly DependencyProperty SubMenuRadiusProperty =
            DependencyProperty.Register("SubMenuRadius", typeof(double), typeof(RadialMenu),
                new PropertyMetadata(150.0));

        /// <summary>
        /// 定义子菜单弧度角度
        /// </summary>
        public static readonly DependencyProperty SubMenuArcAngleProperty =
            DependencyProperty.Register("SubMenuArcAngle", typeof(double), typeof(RadialMenu),
                new PropertyMetadata(120.0));

        /// <summary>
        /// 定义是否在菜单边缘显示标签
        /// </summary>
        public static readonly DependencyProperty ShowLabelsProperty =
            DependencyProperty.Register("ShowLabels", typeof(bool), typeof(RadialMenu),
                new PropertyMetadata(true));

        /// <summary>
        /// 定义菜单项间的间隔角度
        /// </summary>
        public static readonly DependencyProperty ItemSpacingAngleProperty =
            DependencyProperty.Register("ItemSpacingAngle", typeof(double), typeof(RadialMenu),
                new PropertyMetadata(2.0));

        /// <summary>
        /// 定义菜单打开状态
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(RadialMenu),
                new PropertyMetadata(false, OnIsOpenChanged));

        #endregion

        #region 属性访问器

        /// <summary>
        /// 获取或设置外圆半径
        /// </summary>
        public double Radius
        {
            get { return (double)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        /// <summary>
        /// 获取或设置内圆半径
        /// </summary>
        public double InnerRadius
        {
            get { return (double)GetValue(InnerRadiusProperty); }
            set { SetValue(InnerRadiusProperty, value); }
        }

        /// <summary>
        /// 获取内圆直径（内半径的两倍）
        /// </summary>
        public double InnerDiameter
        {
            get { return InnerRadius * 2; }
        }

        /// <summary>
        /// 获取或设置中心内容
        /// </summary>
        public object CenterContent
        {
            get { return GetValue(CenterContentProperty); }
            set { SetValue(CenterContentProperty, value); }
        }

        /// <summary>
        /// 获取或设置中心内容模板
        /// </summary>
        public DataTemplate CenterContentTemplate
        {
            get { return (DataTemplate)GetValue(CenterContentTemplateProperty); }
            set { SetValue(CenterContentTemplateProperty, value); }
        }

        /// <summary>
        /// 获取或设置子菜单半径
        /// </summary>
        public double SubMenuRadius
        {
            get { return (double)GetValue(SubMenuRadiusProperty); }
            set { SetValue(SubMenuRadiusProperty, value); }
        }

        /// <summary>
        /// 获取或设置子菜单弧度角度
        /// </summary>
        public double SubMenuArcAngle
        {
            get { return (double)GetValue(SubMenuArcAngleProperty); }
            set { SetValue(SubMenuArcAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置是否在菜单边缘显示标签
        /// </summary>
        public bool ShowLabels
        {
            get { return (bool)GetValue(ShowLabelsProperty); }
            set { SetValue(ShowLabelsProperty, value); }
        }

        /// <summary>
        /// 获取或设置菜单项间的间隔角度
        /// </summary>
        public double ItemSpacingAngle
        {
            get { return (double)GetValue(ItemSpacingAngleProperty); }
            set { SetValue(ItemSpacingAngleProperty, value); }
        }

        /// <summary>
        /// 获取或设置菜单是否打开
        /// </summary>
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        #endregion

        #region 构造函数

        static RadialMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RadialMenu),
                new FrameworkPropertyMetadata(typeof(RadialMenu)));
        }

        public RadialMenu()
        {
            // 设置渲染性能优化
            RenderOptions.SetCachingHint(this, CachingHint.Cache);
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        #endregion

        #region 重写方法

        protected override DependencyObject GetContainerForItemOverride()
        {
            RadialMenuItem item = new RadialMenuItem();
            item.ParentRadialMenu = this;
            item.Level = 0; // 根级菜单
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
                menuItem.ParentRadialMenu = this;
                menuItem.Level = 0;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 如果需要引用模板部分可以在这里获取
        }

        #endregion

        #region 事件处理程序

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RadialMenu menu = (RadialMenu)d;
            bool isOpen = (bool)e.NewValue;

            if (isOpen)
            {
                menu.OpenMenu();
            }
            else
            {
                menu.CloseMenu();
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 打开菜单
        /// </summary>
        public void OpenMenu()
        {
            // 实际的菜单打开逻辑
            Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 关闭菜单
        /// </summary>
        public void CloseMenu()
        {
            // 关闭所有子菜单
            CloseAllSubmenus();

            // 实际的菜单关闭逻辑
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 关闭所有子菜单
        /// </summary>
        public void CloseAllSubmenus()
        {
            foreach (object item in Items)
            {
                if (item is RadialMenuItem menuItem)
                {
                    menuItem.CloseSubmenu();
                }
            }
        }

        #endregion
    }
}