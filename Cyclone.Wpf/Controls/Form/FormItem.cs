using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Cyclone.Wpf.Helpers;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// FormItem控件：表单项控件
/// 可单独使用，也可作为Form控件的子项
/// SharedName 可以设置多个控件共享同一个名称，使用的时候在某个层级的Panel控件上设置Grid.IsSharedSizeScope="True" 就可以实现Label宽度共享
/// </summary>
public class FormItem : ContentControl
{
    static FormItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FormItem), new FrameworkPropertyMetadata(typeof(FormItem)));
    }

    public FormItem()
    {
        Loaded += OnLoaded;
    }

    #region Label

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(object), typeof(FormItem),
            new FrameworkPropertyMetadata(default(object), FrameworkPropertyMetadataOptions.AffectsRender));

    public object Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion Label

    #region SharedName

    public static readonly DependencyProperty SharedNameProperty =
        DependencyProperty.Register(nameof(SharedName), typeof(string), typeof(FormItem),
            new FrameworkPropertyMetadata(default(string)));

    public string SharedName
    {
        get => (string)GetValue(SharedNameProperty);
        set => SetValue(SharedNameProperty, value);
    }

    #endregion SharedName

    #region AttachedObject

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

    public static readonly DependencyProperty LabelHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(LabelHorizontalAlignment), typeof(HorizontalAlignment), typeof(FormItem),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.AffectsArrange));

    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelHorizontalAlignmentProperty);
        set => SetValue(LabelHorizontalAlignmentProperty, value);
    }

    #endregion LabelHorizontalAlignment

    #region LabelVerticalAlignment

    public static readonly DependencyProperty LabelVerticalAlignmentProperty =
        DependencyProperty.Register(nameof(LabelVerticalAlignment), typeof(VerticalAlignment), typeof(FormItem), new PropertyMetadata(default(VerticalAlignment)));

    public VerticalAlignment LabelVerticalAlignment
    {
        get => (VerticalAlignment)GetValue(LabelVerticalAlignmentProperty);
        set => SetValue(LabelVerticalAlignmentProperty, value);
    }

    #endregion LabelVerticalAlignment

    #region IsRequired

    public static readonly DependencyProperty IsRequiredProperty =
        DependencyProperty.Register(nameof(IsRequired), typeof(bool), typeof(FormItem),
            new FrameworkPropertyMetadata(default(bool), FrameworkPropertyMetadataOptions.AffectsRender));

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    #endregion IsRequired

    #region Description

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(FormItem), new PropertyMetadata(default(string)));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion Description

    #region Form 属性同步逻辑

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var parentForm = this.TryFindVisualParent<Form>();
        if (parentForm != null && parentForm.IsSyncLabelHorizontalAlignment)
        {
            // 直接从 Form 同步对齐方式
            LabelHorizontalAlignment = parentForm.LabelHorizontalAlignment;
        }
    }

    #endregion Form 属性同步逻辑
}