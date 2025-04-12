using System;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// Form控件：垂直排列的表单容器
    /// 可以包含FormItem或任意其他控件
    /// </summary>
    public class Form : ItemsControl
    {
        static Form()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Form), new FrameworkPropertyMetadata(typeof(Form)));
        }

        #region 控件属性

        /// <summary>
        /// 默认标签位置
        /// </summary>
        public static readonly DependencyProperty DefaultLabelPositionProperty =
            DependencyProperty.Register(nameof(DefaultLabelPosition), typeof(LabelPosition), typeof(Form),
                new FrameworkPropertyMetadata(LabelPosition.Left, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 获取或设置默认标签位置
        /// </summary>
        public LabelPosition DefaultLabelPosition
        {
            get => (LabelPosition)GetValue(DefaultLabelPositionProperty);
            set => SetValue(DefaultLabelPositionProperty, value);
        }

        /// <summary>
        /// 默认标签宽度
        /// </summary>
        public static readonly DependencyProperty DefaultLabelWidthProperty =
            DependencyProperty.Register(nameof(DefaultLabelWidth), typeof(double), typeof(Form),
                new FrameworkPropertyMetadata(120.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 获取或设置默认标签宽度
        /// </summary>
        public double DefaultLabelWidth
        {
            get => (double)GetValue(DefaultLabelWidthProperty);
            set => SetValue(DefaultLabelWidthProperty, value);
        }

        /// <summary>
        /// 表单项间距
        /// </summary>
        public static readonly DependencyProperty ItemSpacingProperty =
            DependencyProperty.Register(nameof(ItemSpacing), typeof(double), typeof(Form),
                new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 获取或设置表单项间距
        /// </summary>
        public double ItemSpacing
        {
            get => (double)GetValue(ItemSpacingProperty);
            set => SetValue(ItemSpacingProperty, value);
        }

        /// <summary>
        /// 是否启用分组功能
        /// </summary>
        public static readonly DependencyProperty EnableGroupingProperty =
            DependencyProperty.Register(nameof(EnableGrouping), typeof(bool), typeof(Form),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 获取或设置是否启用分组功能
        /// </summary>
        public bool EnableGrouping
        {
            get => (bool)GetValue(EnableGroupingProperty);
            set => SetValue(EnableGroupingProperty, value);
        }

        /// <summary>
        /// 是否共享标签列宽度
        /// </summary>
        public static readonly DependencyProperty ShareLabelColumnProperty =
            DependencyProperty.Register(nameof(ShareLabelColumn), typeof(bool), typeof(Form),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// 获取或设置是否共享标签列宽度
        /// </summary>
        public bool ShareLabelColumn
        {
            get => (bool)GetValue(ShareLabelColumnProperty);
            set => SetValue(ShareLabelColumnProperty, value);
        }

        #endregion 控件属性

        #region 附加属性

        /// <summary>
        /// 标签附加属性 - 可应用于任何控件
        /// </summary>
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.RegisterAttached("Label", typeof(object), typeof(Form),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 获取标签
        /// </summary>
        public static object GetLabel(DependencyObject obj)
        {
            return obj.GetValue(LabelProperty);
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        public static void SetLabel(DependencyObject obj, object value)
        {
            obj.SetValue(LabelProperty, value);
        }

        /// <summary>
        /// 标签位置附加属性 - 控制标签相对于控件的位置
        /// </summary>
        public static readonly DependencyProperty LabelPositionProperty =
            DependencyProperty.RegisterAttached("LabelPosition", typeof(LabelPosition), typeof(Form),
                new FrameworkPropertyMetadata(LabelPosition.Inherit, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 获取标签位置
        /// </summary>
        public static LabelPosition GetLabelPosition(DependencyObject obj)
        {
            return (LabelPosition)obj.GetValue(LabelPositionProperty);
        }

        /// <summary>
        /// 设置标签位置
        /// </summary>
        public static void SetLabelPosition(DependencyObject obj, LabelPosition value)
        {
            obj.SetValue(LabelPositionProperty, value);
        }

        /// <summary>
        /// 必填字段附加属性
        /// </summary>
        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.RegisterAttached("IsRequired", typeof(bool), typeof(Form),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 获取是否必填
        /// </summary>
        public static bool GetIsRequired(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRequiredProperty);
        }

        /// <summary>
        /// 设置是否必填
        /// </summary>
        public static void SetIsRequired(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRequiredProperty, value);
        }

        /// <summary>
        /// 分组名称附加属性 - 用于将表单项分组
        /// </summary>
        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.RegisterAttached("Group", typeof(string), typeof(Form),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 获取分组名称
        /// </summary>
        public static string GetGroup(DependencyObject obj)
        {
            return (string)obj.GetValue(GroupProperty);
        }

        /// <summary>
        /// 设置分组名称
        /// </summary>
        public static void SetGroup(DependencyObject obj, string value)
        {
            obj.SetValue(GroupProperty, value);
        }

        /// <summary>
        /// 标签宽度附加属性
        /// </summary>
        public static readonly DependencyProperty LabelWidthProperty =
            DependencyProperty.RegisterAttached("LabelWidth", typeof(double), typeof(Form),
                new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 获取标签宽度
        /// </summary>
        public static double GetLabelWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(LabelWidthProperty);
        }

        /// <summary>
        /// 设置标签宽度
        /// </summary>
        public static void SetLabelWidth(DependencyObject obj, double value)
        {
            obj.SetValue(LabelWidthProperty, value);
        }

        /// <summary>
        /// 附加对象属性
        /// </summary>
        public static readonly DependencyProperty AttachedObjectProperty =
            DependencyProperty.RegisterAttached("AttachedObject", typeof(object), typeof(Form),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 获取附加对象
        /// </summary>
        public static object GetAttachedObject(DependencyObject obj)
        {
            return obj.GetValue(AttachedObjectProperty);
        }

        /// <summary>
        /// 设置附加对象
        /// </summary>
        public static void SetAttachedObject(DependencyObject obj, object value)
        {
            obj.SetValue(AttachedObjectProperty, value);
        }

        #endregion 附加属性

        /// <summary>
        /// 重写准备容器方法，处理附加属性
        /// </summary>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            // 如果元素不是FormItem，检查是否需要应用Form附加属性
            if (element is ContentPresenter presenter && !(item is FormItem))
            {
                // 如果项目有Label附加属性，创建一个FormItem
                var label = GetLabel(item as DependencyObject);
                if (label != null)
                {
                    var formItem = new FormItem
                    {
                        Label = label,
                        Content = item
                    };

                    // 应用其他附加属性
                    var labelPos = GetLabelPosition(item as DependencyObject);
                    if (labelPos != LabelPosition.Inherit)
                    {
                        formItem.LabelPosition = labelPos;
                    }
                    else
                    {
                        formItem.LabelPosition = DefaultLabelPosition;
                    }

                    formItem.IsRequired = GetIsRequired(item as DependencyObject);
                    formItem.AttachedObject = GetAttachedObject(item as DependencyObject);

                    // 替换内容
                    presenter.Content = formItem;
                }
            }
        }
    }

    /// <summary>
    /// 标签位置枚举
    /// </summary>
    public enum LabelPosition
    {
        /// <summary>继承自Form</summary>
        Inherit = 0,

        /// <summary>标签在左侧</summary>
        Left,

        /// <summary>标签在顶部</summary>
        Top
    }
}