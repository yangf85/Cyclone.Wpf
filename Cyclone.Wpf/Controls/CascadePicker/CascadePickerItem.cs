using Cyclone.Wpf.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_ItemsPopup, Type = typeof(Popup))]
public class CascadePickerItem : HeaderedItemsControl
{
    private const string PART_ItemsPopup = "PART_ItemsPopup";

    private Popup _popup;

    private CascadePicker _root;

    // 用于缓存绑定对象，避免重复创建
    private Binding _nodePathBinding;

    static CascadePickerItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePickerItem), new FrameworkPropertyMetadata(typeof(CascadePickerItem)));
    }

    #region NodePathMemberPath

    public static readonly DependencyProperty NodePathMemberPathProperty =
        DependencyProperty.Register(nameof(NodePathMemberPath), typeof(string), typeof(CascadePickerItem),
            new PropertyMetadata(null, OnNodePathMemberPathChanged));

    public string NodePathMemberPath
    {
        get => (string)GetValue(NodePathMemberPathProperty);
        set => SetValue(NodePathMemberPathProperty, value);
    }

    private static void OnNodePathMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePickerItem item)
        {
            item._nodePathBinding = null; // 清除缓存的绑定
            item.UpdateNodePath();
        }
    }

    #endregion NodePathMemberPath

    #region Override

    protected override void OnHeaderChanged(object oldHeader, object newHeader)
    {
        base.OnHeaderChanged(oldHeader, newHeader);
        UpdateNodePath();
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);
        SetValue(IsPressedPropertyKey, true);
        CaptureMouse();
        RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        SetValue(IsPressedPropertyKey, false);
        ReleaseMouseCapture();
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);
        SetValue(IsPressedPropertyKey, false);
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is CascadePickerItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new CascadePickerItem();
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is CascadePickerItem container)
        {
            // 传递 NodePathMemberPath 到子项
            container.NodePathMemberPath = this.NodePathMemberPath;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = VisualTreeHelperExtension.TryFindLogicalParent<CascadePicker>(this);

        // 如果还没有设置 NodePathMemberPath，尝试从根 CascadePicker 获取
        if (string.IsNullOrEmpty(NodePathMemberPath) && _root != null)
        {
            NodePathMemberPath = _root.NodePathMemberPath;
        }
    }

    #endregion Override

    #region NodePath

    public static readonly DependencyProperty NodePathProperty =
        DependencyProperty.Register(nameof(NodePath), typeof(string), typeof(CascadePickerItem),
            new PropertyMetadata(default(string)));

    public string NodePath
    {
        get => (string)GetValue(NodePathProperty);
        set => SetValue(NodePathProperty, value);
    }

    private object GetValueFromMemberPath(object source, string memberPath)
    {
        if (source == null || string.IsNullOrEmpty(memberPath))
            return null;

        try
        {
            // 使用缓存的绑定或创建新的绑定
            if (_nodePathBinding == null || _nodePathBinding.Path.Path != memberPath)
            {
                _nodePathBinding = new Binding(memberPath) { Source = source };
            }
            else
            {
                _nodePathBinding.Source = source;
            }

            // 创建临时对象来评估绑定
            var evaluator = new BindingEvaluator();
            BindingOperations.SetBinding(evaluator, BindingEvaluator.ValueProperty, _nodePathBinding);
            var value = evaluator.Value;

            // 清理绑定
            BindingOperations.ClearBinding(evaluator, BindingEvaluator.ValueProperty);

            return value;
        }
        catch
        {
            // 如果绑定失败，返回 null
            return null;
        }
    }

    public void UpdateNodePath()
    {
        var dataContext = DataContext;
        if (dataContext == null)
        {
            SetCurrentValue(NodePathProperty, Header?.ToString() ?? string.Empty);
            return;
        }

        // 优先使用 NodePathMemberPath
        if (!string.IsNullOrEmpty(NodePathMemberPath))
        {
            var value = GetValueFromMemberPath(dataContext, NodePathMemberPath);
            if (value != null)
            {
                SetCurrentValue(NodePathProperty, value.ToString());
                return;
            }
        }

        // 如果没有设置 NodePathMemberPath，使用 Header 或 ToString()
        SetCurrentValue(NodePathProperty, Header?.ToString() ?? dataContext.ToString() ?? string.Empty);
    }

    // 辅助类用于评估绑定
    private class BindingEvaluator : DependencyObject
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(BindingEvaluator));

        public object Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
    }

    #endregion NodePath

    #region ItemClickEvent

    public static readonly RoutedEvent ItemClickEvent = EventManager.RegisterRoutedEvent("ItemClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CascadePickerItem));

    public event RoutedEventHandler ItemClick
    {
        add => AddHandler(ItemClickEvent, value);
        remove => RemoveHandler(ItemClickEvent, value);
    }

    protected virtual void OnItemClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
    }

    #endregion ItemClickEvent

    #region IsPressed

    private static readonly DependencyPropertyKey IsPressedPropertyKey =
               DependencyProperty.RegisterReadOnly(
           nameof(IsPressed),
           typeof(bool),
           typeof(CascadePickerItem),
           new PropertyMetadata(false));

    public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

    public bool IsPressed
    {
        get => (bool)GetValue(IsPressedProperty);
    }

    #endregion IsPressed
}