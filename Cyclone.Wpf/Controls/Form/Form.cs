using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

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

    #region Header

    public object Header
    {
        get => (object)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(Form), new PropertyMetadata(default(object)));

    #endregion Header

    #region Footer

    public object Footer
    {
        get => (object)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(Form), new PropertyMetadata(default(object)));

    #endregion Footer

    #region LabelHorizontalAlignment

    public HorizontalAlignment LabelHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(LabelHorizontalAlignmentProperty);
        set => SetValue(LabelHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty LabelHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(LabelHorizontalAlignment), typeof(HorizontalAlignment), typeof(Form),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left));

    #endregion LabelHorizontalAlignment

    #region IsSyncLabelHorizontalAlignment

    public bool IsSyncLabelHorizontalAlignment
    {
        get => (bool)GetValue(IsSyncLabelHorizontalAlignmentProperty);
        set => SetValue(IsSyncLabelHorizontalAlignmentProperty, value);
    }

    public static readonly DependencyProperty IsSyncLabelHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(IsSyncLabelHorizontalAlignment), typeof(bool), typeof(Form), new FrameworkPropertyMetadata(true));

    #endregion IsSyncLabelHorizontalAlignment

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

    #region SharedName

    public static string GetSharedName(DependencyObject obj) => (string)obj.GetValue(SharedNameProperty);

    public static void SetSharedName(DependencyObject obj, string value) => obj.SetValue(SharedNameProperty, value);

    public static readonly DependencyProperty SharedNameProperty =
                DependencyProperty.RegisterAttached("SharedName", typeof(string), typeof(Form), new PropertyMetadata(default(string)));

    #endregion SharedName

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

    #region Description

    public static string GetDescription(DependencyObject obj) => (string)obj.GetValue(DescriptionProperty);

    public static void SetDescription(DependencyObject obj, string value) => obj.SetValue(DescriptionProperty, value);

    public static readonly DependencyProperty DescriptionProperty =
                DependencyProperty.RegisterAttached("Description", typeof(string), typeof(Form), new PropertyMetadata(default(string)));

    #endregion Description

    #region AttachedObject

    public static object GetAttachedObject(DependencyObject obj) => (object)obj.GetValue(AttachedObjectProperty);

    public static void SetAttachedObject(DependencyObject obj, object value) => obj.SetValue(AttachedObjectProperty, value);

    public static readonly DependencyProperty AttachedObjectProperty =
                DependencyProperty.RegisterAttached("AttachedObject", typeof(object), typeof(Form), new PropertyMetadata(default(object)));

    #endregion AttachedObject

    #endregion 附加属性

    #region Override

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new FormItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is FormItem || item is FormSeperater;
    }

    protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
    {
        if (item is FormSeperater) { return false; }
        return container is FormItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is FormItem formItem)
        {
            // 如果item是DependencyObject，从它获取附加属性并设置到formItem
            if (item is DependencyObject depObj)
            {
                var label = GetLabel(depObj);
                var isRequired = GetIsRequired(depObj);
                var sharedName = GetSharedName(depObj);
                var attachedObject = GetAttachedObject(depObj);
                var description = GetDescription(depObj);

                if (label != null)
                {
                    formItem.Label = label;
                    formItem.IsRequired = isRequired;
                }

                if (!string.IsNullOrEmpty(sharedName))
                {
                    formItem.SharedName = sharedName;
                }
                if (attachedObject != null)
                {
                    formItem.AttachedObject = attachedObject;
                }
                if (!string.IsNullOrEmpty(description))
                {
                    formItem.Description = description;
                }
            }

            // 如果item不是FormItem，则设置为Content
            if (item is not FormItem)
            {
                formItem.Content = item;
            }
        }
    }

    #endregion Override
}