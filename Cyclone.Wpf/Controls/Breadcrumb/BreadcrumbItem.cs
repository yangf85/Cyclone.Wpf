using System.Windows;
using System.Windows.Controls.Primitives;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 面包屑项控件
    /// </summary>
    public class BreadcrumbItem : ButtonBase
    {
        #region 依赖属性

        public static readonly DependencyProperty ShowSeparatorProperty =
            DependencyProperty.Register("ShowSeparator", typeof(bool), typeof(BreadcrumbItem),
                new PropertyMetadata(true));

        public static readonly DependencyProperty IsLastItemProperty =
            DependencyProperty.Register("IsLastItem", typeof(bool), typeof(BreadcrumbItem),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(BreadcrumbItem),
                new PropertyMetadata(false));

        #endregion

        #region 属性

        /// <summary>
        /// 获取或设置是否显示分隔符
        /// </summary>
        public bool ShowSeparator
        {
            get { return (bool)GetValue(ShowSeparatorProperty); }
            set { SetValue(ShowSeparatorProperty, value); }
        }

        /// <summary>
        /// 获取或设置是否是最后一项
        /// </summary>
        public bool IsLastItem
        {
            get { return (bool)GetValue(IsLastItemProperty); }
            set { SetValue(IsLastItemProperty, value); }
        }

        /// <summary>
        /// 获取或设置是否被选中
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        #endregion

        static BreadcrumbItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbItem),
                new FrameworkPropertyMetadata(typeof(BreadcrumbItem)));
        }
    }
}