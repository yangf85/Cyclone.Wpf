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

    #region SharedScopeLevel

    /// <summary>
    /// 共享尺寸作用域层级属性
    /// 指定向上查找多少层级来设置 IsSharedSizeScope
    /// 0 = 直接父容器（默认）
    /// 1 = 父容器的父容器
    /// 2 = 父容器的父容器的父容器
    /// -1 = 查找到根容器
    /// </summary>
    public static readonly DependencyProperty SharedScopeLevelProperty =
        DependencyProperty.Register(nameof(SharedScopeLevel), typeof(int), typeof(FormItem),
            new FrameworkPropertyMetadata(0, OnSharedScopeLevelChanged));

    /// <summary>
    /// 获取或设置共享尺寸作用域层级
    /// </summary>
    public int SharedScopeLevel
    {
        get => (int)GetValue(SharedScopeLevelProperty);
        set => SetValue(SharedScopeLevelProperty, value);
    }

    /// <summary>
    /// 共享尺寸作用域层级改变处理
    /// </summary>
    private static void OnSharedScopeLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FormItem formItem)
        {
            // 重新处理 SharedName 逻辑
            if (!string.IsNullOrEmpty(formItem.SharedName))
            {
                formItem.Loaded -= FormItem_Loaded;
                formItem.Loaded += FormItem_Loaded;
            }
        }
    }

    #endregion SharedScopeLevel

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
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// 获取或设置标签水平对齐方式
    /// </summary>
    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelHorizontalAlignmentProperty);
        set => SetValue(LabelHorizontalAlignmentProperty, value);
    }

    #endregion LabelHorizontalAlignment

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

    #region Description

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(FormItem), new PropertyMetadata(default(string)));

    #endregion Description

    /// <summary>
    /// 增强的控件加载时处理共享尺寸组
    /// 根据 SharedScopeLevel 属性查找指定层级的容器
    /// </summary>
    private static void FormItem_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is FormItem formItem)
        {
            var targetPanel = FindTargetPanel(formItem, formItem.SharedScopeLevel);

            if (targetPanel != null)
            {
                var isAlreadySet = Grid.GetIsSharedSizeScope(targetPanel);

                if (!isAlreadySet)
                {
                    targetPanel.SetValue(Grid.IsSharedSizeScopeProperty, true);
                }
            }
        }
    }

    /// <summary>
    /// 根据指定层级查找目标容器
    /// </summary>
    /// <param name="formItem">FormItem 实例</param>
    /// <param name="level">查找层级：0=直接父容器, 1=祖父容器, -1=根容器</param>
    /// <returns>找到的目标 Panel，如果没找到则返回 null</returns>
    private static Panel FindTargetPanel(FormItem formItem, int level)
    {
        DependencyObject current = formItem.Parent;

        if (current == null) return null;

        // 如果 level = -1，查找到根容器
        if (level == -1)
        {
            Panel lastPanel = null;

            while (current != null)
            {
                if (current is Panel panel)
                {
                    lastPanel = panel;
                }

                // 继续向上查找
                if (current is FrameworkElement element)
                {
                    var parent = element.Parent ?? element.TemplatedParent;
                    if (parent == null) break;
                    current = parent;
                }
                else
                {
                    break;
                }
            }

            return lastPanel;
        }

        // 查找指定层级的容器
        int currentLevel = 0;

        while (current != null)
        {
            if (current is Panel panel)
            {
                if (currentLevel == level)
                {
                    return panel;
                }
                currentLevel++;
            }

            // 继续向上查找
            if (current is FrameworkElement element)
            {
                current = element.Parent ?? element.TemplatedParent;
            }
            else
            {
                break;
            }
        }

        return null;
    }

    /// <summary>
    /// 初始化共享尺寸组名称
    /// </summary>
    public FormItem()
    {
        // 如果父控件是Form且Form.ShareLabelColumn为true，则自动设置共享尺寸组
        Loaded += (s, e) =>
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
            }
        };
    }
}