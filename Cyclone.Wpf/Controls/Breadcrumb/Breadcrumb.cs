using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 面包屑导航控件
    /// </summary>
    public class Breadcrumb : ItemsControl
    {
        #region 依赖属性

        public static readonly DependencyProperty SeparatorProperty =
            DependencyProperty.Register("Separator", typeof(object), typeof(Breadcrumb),
                new PropertyMetadata("›"));

        public static readonly DependencyProperty SeparatorTemplateProperty =
            DependencyProperty.Register("SeparatorTemplate", typeof(DataTemplate), typeof(Breadcrumb),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Breadcrumb),
                new PropertyMetadata(-1, OnSelectedIndexChanged));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(Breadcrumb),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register("ItemClickCommand", typeof(ICommand), typeof(Breadcrumb),
                new PropertyMetadata(null));

        #endregion 依赖属性

        #region 属性

        /// <summary>
        /// 获取或设置分隔符
        /// </summary>
        public object Separator
        {
            get { return GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
        }

        /// <summary>
        /// 获取或设置分隔符模板
        /// </summary>
        public DataTemplate SeparatorTemplate
        {
            get { return (DataTemplate)GetValue(SeparatorTemplateProperty); }
            set { SetValue(SeparatorTemplateProperty, value); }
        }

        /// <summary>
        /// 获取或设置选中项的索引
        /// </summary>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// 获取或设置选中的项
        /// </summary>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// 获取或设置项点击命令
        /// </summary>
        public ICommand ItemClickCommand
        {
            get { return (ICommand)GetValue(ItemClickCommandProperty); }
            set { SetValue(ItemClickCommandProperty, value); }
        }

        #endregion 属性

        /// <summary>
        /// 项点击事件
        /// </summary>
        public event RoutedEventHandler ItemClick;

        static Breadcrumb()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Breadcrumb),
                new FrameworkPropertyMetadata(typeof(Breadcrumb)));
        }

        public Breadcrumb()
        {
            this.Loaded += Breadcrumb_Loaded;
        }

        private void Breadcrumb_Loaded(object sender, RoutedEventArgs e)
        {
            // 如果未设置选中项，默认选中最后一项
            if (Items.Count > 0 && SelectedIndex == -1)
            {
                SelectedIndex = Items.Count - 1;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadcrumbItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is BreadcrumbItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var breadcrumbItem = element as BreadcrumbItem;
            if (breadcrumbItem != null)
            {
                // 获取项的索引
                int index = ItemContainerGenerator.IndexFromContainer(breadcrumbItem);

                // 设置是否显示分隔符（最后一项不显示分隔符）
                breadcrumbItem.ShowSeparator = (index < Items.Count - 1);

                // 设置是否是最后一项
                breadcrumbItem.IsLastItem = (index == Items.Count - 1);

                // 设置点击事件处理
                breadcrumbItem.Click += BreadcrumbItem_Click;

                // 设置是否选中
                breadcrumbItem.IsSelected = (index == SelectedIndex);
            }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);

            var breadcrumbItem = element as BreadcrumbItem;
            if (breadcrumbItem != null)
            {
                breadcrumbItem.Click -= BreadcrumbItem_Click;
            }
        }

        private void BreadcrumbItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as BreadcrumbItem;
            if (item != null)
            {
                int index = ItemContainerGenerator.IndexFromContainer(item);
                SelectedIndex = index;
                SelectedItem = Items[index];

                // 引发ItemClick事件
                ItemClick?.Invoke(this, new RoutedEventArgs());

                // 执行命令
                if (ItemClickCommand != null && ItemClickCommand.CanExecute(index))
                {
                    ItemClickCommand.Execute(index);
                }
            }
        }

        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var breadcrumb = d as Breadcrumb;
            if (breadcrumb != null)
            {
                int newIndex = (int)e.NewValue;

                // 更新SelectedItem
                if (newIndex >= 0 && newIndex < breadcrumb.Items.Count)
                {
                    breadcrumb.SelectedItem = breadcrumb.Items[newIndex];
                }
                else
                {
                    breadcrumb.SelectedItem = null;
                }

                // 更新各项的选中状态
                for (int i = 0; i < breadcrumb.Items.Count; i++)
                {
                    var container = breadcrumb.ItemContainerGenerator.ContainerFromIndex(i) as BreadcrumbItem;
                    if (container != null)
                    {
                        container.IsSelected = (i == newIndex);
                    }
                }
            }
        }
    }
}