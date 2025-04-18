using System;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// FormItem控件：表单项控件
/// 可单独使用，也可作为Form控件的子项
/// </summary>
public class FormItem : ContentControl
{
    static FormItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FormItem), new FrameworkPropertyMetadata(typeof(FormItem)));
    }

    #region Label

    /// <summary>
    /// 标签内容属性
    /// </summary>
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(FormItem),
            new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 获取或设置标签内容
    /// </summary>
    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion Label

    #region LabelPosition

    /// <summary>
    /// 标签位置属性
    /// </summary>
    public static readonly DependencyProperty LabelPositionProperty =
        DependencyProperty.Register(nameof(LabelPosition), typeof(LabelPosition), typeof(FormItem),
            new FrameworkPropertyMetadata(LabelPosition.Left, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// 获取或设置标签位置
    /// </summary>
    public LabelPosition LabelPosition
    {
        get => (LabelPosition)GetValue(LabelPositionProperty);
        set => SetValue(LabelPositionProperty, value);
    }

    #endregion LabelPosition

    #region SharedName

    /// <summary>
    /// 共享尺寸组名称属性
    /// </summary>
    public static readonly DependencyProperty SharedNameProperty =
        DependencyProperty.Register(nameof(SharedName), typeof(string), typeof(FormItem),
            new FrameworkPropertyMetadata(default(string), OnSharedNameChanged));

    /// <summary>
    /// 获取或设置共享尺寸组名称
    /// </summary>
    public string SharedName
    {
        get => (string)GetValue(SharedNameProperty);
        set => SetValue(SharedNameProperty, value);
    }

    /// <summary>
    /// 控件加载时处理共享尺寸组
    /// </summary>
    private static void FormItem_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FormItem formItem && formItem.Parent is Panel panel)
        {
            panel.SetValue(Grid.IsSharedSizeScopeProperty, true);
        }
    }

    /// <summary>
    /// 共享尺寸组名称改变处理
    /// </summary>
    private static void OnSharedNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FormItem formItem)
        {
            if (!string.IsNullOrEmpty(e.NewValue?.ToString()))
            {
                formItem.Loaded += FormItem_Loaded;
            }
            else
            {
                formItem.Loaded -= FormItem_Loaded;
            }
        }
    }

    #endregion SharedName

    #region AttachedObject

    /// <summary>
    /// 附加对象属性
    /// </summary>
    public static readonly DependencyProperty AttachedObjectProperty =
        DependencyProperty.Register(nameof(AttachedObject), typeof(object), typeof(FormItem),
            new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 获取或设置附加对象
    /// </summary>
    public object AttachedObject
    {
        get => (object)GetValue(AttachedObjectProperty);
        set => SetValue(AttachedObjectProperty, value);
    }

    #endregion AttachedObject

    #region LabelHorizontalAlignment

    /// <summary>
    /// 标签水平对齐方式属性
    /// </summary>
    public static readonly DependencyProperty LabelHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(LabelHorizontalAlignment), typeof(HorizontalAlignment), typeof(FormItem),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 获取或设置标签水平对齐方式
    /// </summary>
    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelHorizontalAlignmentProperty);
        set => SetValue(LabelHorizontalAlignmentProperty, value);
    }

    #endregion LabelHorizontalAlignment

    #region LabelWidth

    /// <summary>
    /// 标签宽度属性
    /// </summary>
    public static readonly DependencyProperty LabelWidthProperty =
        DependencyProperty.Register(nameof(LabelWidth), typeof(double), typeof(FormItem),
            new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// 获取或设置标签宽度
    /// </summary>
    public double LabelWidth
    {
        get => (double)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    #endregion LabelWidth

    #region IsRequired

    /// <summary>
    /// 是否必填属性
    /// </summary>
    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(nameof(IsRequired), typeof(bool), typeof(FormItem),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// 获取或设置是否必填
    /// </summary>
    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    #endregion IsRequired

    /// <summary>
    /// 初始化共享尺寸组名称
    /// </summary>
    public FormItem()
    {
        // 如果父控件是Form且Form.ShareLabelColumn为true，则自动设置共享尺寸组
        this.Loaded += (s, e) =>
        {
            if (string.IsNullOrEmpty(SharedName) && Parent is DependencyObject parent)
            {
                // 查找父Form控件
                Form parentForm = null;
                DependencyObject current = parent;

                while (current != null && parentForm == null)
                {
                    if (current is Form form)
                    {
                        parentForm = form;
                    }
                    else if (current is FrameworkElement element)
                    {
                        current = element.Parent ?? element.TemplatedParent;
                    }
                    else
                    {
                        break;
                    }
                }

                if (parentForm != null && parentForm.ShareLabelColumn)
                {
                    SharedName = "FormLabels";
                }
            }
        };
    }
}