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

        #endregion 控件属性

        #region 附加属性

        #region Label

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.RegisterAttached("Label", typeof(object), typeof(Form),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public static object GetLabel(DependencyObject obj)
        {
            return obj.GetValue(LabelProperty);
        }

        public static void SetLabel(DependencyObject obj, object value)
        {
            obj.SetValue(LabelProperty, value);
        }

        #endregion Label

        #region LabelPosition

        public static readonly DependencyProperty LabelPositionProperty =
            DependencyProperty.RegisterAttached("LabelPosition", typeof(LabelPosition), typeof(Form),
                new FrameworkPropertyMetadata(LabelPosition.Inherit, FrameworkPropertyMetadataOptions.AffectsRender));

        public static LabelPosition GetLabelPosition(DependencyObject obj)
        {
            return (LabelPosition)obj.GetValue(LabelPositionProperty);
        }

        public static void SetLabelPosition(DependencyObject obj, LabelPosition value)
        {
            obj.SetValue(LabelPositionProperty, value);
        }

        #endregion LabelPosition

        #region IsRequired

        public static readonly DependencyProperty IsRequiredProperty =
            DependencyProperty.RegisterAttached("IsRequired", typeof(bool), typeof(Form),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

        public static bool GetIsRequired(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRequiredProperty);
        }

        public static void SetIsRequired(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRequiredProperty, value);
        }

        #endregion IsRequired

        #endregion 附加属性

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
        Inherit,

        Left,

        Top
    }
}