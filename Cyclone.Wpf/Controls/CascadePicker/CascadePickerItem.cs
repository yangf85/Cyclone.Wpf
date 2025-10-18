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

    private Binding _nodePathBinding;

    static CascadePickerItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CascadePickerItem), new FrameworkPropertyMetadata(typeof(CascadePickerItem)));
    }

    #region IsHighlighted

    public static readonly DependencyProperty IsHighlightedProperty =
        DependencyProperty.Register(nameof(IsHighlighted), typeof(bool), typeof(CascadePickerItem),
            new PropertyMetadata(false));

    public bool IsHighlighted
    {
        get => (bool)GetValue(IsHighlightedProperty);
        set => SetValue(IsHighlightedProperty, value);
    }

    #endregion IsHighlighted

    #region IsExpanded

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(CascadePickerItem),
            new PropertyMetadata(false, OnIsExpandedChanged));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePickerItem item && item._popup != null)
        {
            item._popup.IsOpen = (bool)e.NewValue;
        }
    }

    #endregion IsExpanded

    #region NodeMemberPath

    public static readonly DependencyProperty NodeMemberPathProperty =
        DependencyProperty.Register(nameof(NodeMemberPath), typeof(string), typeof(CascadePickerItem),
            new PropertyMetadata(null, OnNodeMemberPathChanged));

    public string NodeMemberPath
    {
        get => (string)GetValue(NodeMemberPathProperty);
        set => SetValue(NodeMemberPathProperty, value);
    }

    private static void OnNodeMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CascadePickerItem item)
        {
            item._nodePathBinding = null;
            item.UpdateNodePath();
        }
    }

    #endregion NodeMemberPath

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _popup = GetTemplateChild(PART_ItemsPopup) as Popup;
        _root = VisualTreeHelperExtension.TryFindLogicalParent<CascadePicker>(this);

        if (string.IsNullOrEmpty(NodeMemberPath) && _root != null)
        {
            NodeMemberPath = _root.NodeMemberPath;
        }

        // 同步弹出状态
        if (_popup != null)
        {
            _popup.IsOpen = IsExpanded;
        }
    }

    protected override void OnHeaderChanged(object oldHeader, object newHeader)
    {
        base.OnHeaderChanged(oldHeader, newHeader);
        UpdateNodePath();
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        // 鼠标悬停时展开子菜单
        if (HasItems && !IsExpanded)
        {
            IsExpanded = true;
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        // 检查鼠标是否真的离开了项目区域（包括弹出菜单）
        if (!IsMouseOverWithPopup())
        {
            IsExpanded = false;
        }
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
            container.NodeMemberPath = this.NodeMemberPath;
        }
    }

    private bool IsMouseOverWithPopup()
    {
        // 检查鼠标是否在项目上
        if (IsMouseOver)
            return true;

        // 检查鼠标是否在弹出菜单上
        if (_popup != null && _popup.IsOpen && _popup.Child != null)
        {
            var popupPosition = Mouse.GetPosition(_popup.Child);
            var hitTest = _popup.Child.InputHitTest(popupPosition);
            return hitTest != null;
        }

        return false;
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

    public void UpdateNodePath()
    {
        var dataContext = DataContext;
        if (dataContext == null)
        {
            SetCurrentValue(NodePathProperty, Header?.ToString() ?? string.Empty);
            return;
        }

        if (!string.IsNullOrEmpty(NodeMemberPath))
        {
            var value = GetValueFromMemberPath(dataContext, NodeMemberPath);
            if (value != null)
            {
                SetCurrentValue(NodePathProperty, value.ToString());
                return;
            }
        }

        SetCurrentValue(NodePathProperty, Header?.ToString() ?? dataContext.ToString() ?? string.Empty);
    }

    private object GetValueFromMemberPath(object source, string memberPath)
    {
        if (source == null || string.IsNullOrEmpty(memberPath))
            return null;

        try
        {
            if (_nodePathBinding == null || _nodePathBinding.Path.Path != memberPath)
            {
                _nodePathBinding = new Binding(memberPath) { Source = source };
            }
            else
            {
                _nodePathBinding.Source = source;
            }

            var evaluator = new BindingEvaluator();
            BindingOperations.SetBinding(evaluator, BindingEvaluator.ValueProperty, _nodePathBinding);
            var value = evaluator.Value;

            BindingOperations.ClearBinding(evaluator, BindingEvaluator.ValueProperty);

            return value;
        }
        catch
        {
            return null;
        }
    }

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

    protected virtual void OnItemClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ItemClickEvent, this));
    }

    public event RoutedEventHandler ItemClick
    {
        add => AddHandler(ItemClickEvent, value);
        remove => RemoveHandler(ItemClickEvent, value);
    }

    #endregion ItemClickEvent

    #region IsPressed

    #region 必须放在IsPressedProperty前面 否则报空引用异常

    private static readonly DependencyPropertyKey IsPressedPropertyKey =
                   DependencyProperty.RegisterReadOnly(
           nameof(IsPressed),
           typeof(bool),
           typeof(CascadePickerItem),
           new PropertyMetadata(false));

    #endregion 必须放在IsPressedProperty前面 否则报空引用异常

    public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

    public bool IsPressed
    {
        get => (bool)GetValue(IsPressedProperty);
    }

    #endregion IsPressed
}