using Cyclone.Wpf.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(SideMenuItem))]
public class SideMenuItem : HeaderedItemsControl, ICommandSource
{
    static SideMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenuItem), new FrameworkPropertyMetadata(typeof(SideMenuItem)));
    }

    private SideMenu _root;
    private int _level = 0;

    #region RowHeight

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(SideMenuItem), new PropertyMetadata(32d));

    #endregion RowHeight

    #region Level

    // Level属性，表示菜单项的层级
    public int Level
    {
        get => _level;
        internal set
        {
            if (_level != value)
            {
                _level = value;
                UpdateIndent();
            }
        }
    }

    #endregion Level

    #region Indent

    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        private set => SetValue(IndentProperty, value);
    }

    public static readonly DependencyProperty IndentProperty =
        DependencyProperty.Register(nameof(Indent), typeof(double), typeof(SideMenuItem),
        new PropertyMetadata(0d));

    #endregion Indent

    #region IsExpanded

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(SideMenuItem), new PropertyMetadata(default(bool)));

    #endregion IsExpanded

    #region IsActived

    private static readonly DependencyPropertyKey IsActivedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsActived),
            typeof(bool),
            typeof(SideMenuItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsActivedProperty = IsActivedPropertyKey.DependencyProperty;

    public bool IsActived
    {
        get => (bool)GetValue(IsActivedProperty);
    }

    #endregion IsActived

    #region Icon

    public object Icon
    {
        get => (object)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(SideMenuItem), new PropertyMetadata(default(object)));

    #endregion Icon

    #region IconTemplate

    public DataTemplate IconTemplate
    {
        get => (DataTemplate)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(SideMenuItem), new PropertyMetadata(default(DataTemplate)));

    #endregion IconTemplate

    #region Impl CommandSource

    #region Command

    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SideMenuItem), new PropertyMetadata(default(ICommand)));

    #endregion Command

    #region CommandParameter

    public object CommandParameter
    {
        get => (object)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SideMenuItem), new PropertyMetadata(default(object)));

    #endregion CommandParameter

    #region CommandTarget

    public IInputElement CommandTarget
    {
        get => (IInputElement)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(SideMenuItem), new PropertyMetadata(default(IInputElement)));

    #endregion CommandTarget

    #endregion Impl CommandSource

    #region Override

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        // 设置子项的缩进级别
        if (element is SideMenuItem childItem)
        {
            // 子菜单项的Level是父菜单项的Level+1
            childItem.Level = this.Level + 1;

            // 获取SideMenu的缩进大小
            if (_root != null)
            {
                childItem.UpdateIndent(_root.Indent);
            }
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }

        // 获取当前点击的 SideMenuItem
        var source = e.OriginalSource as DependencyObject;
        var clickedItem = VisualTreeHelperExtension.TryFindVisualParent<SideMenuItem>(source);

        // 如果点击的是当前的 SideMenuItem
        if (clickedItem == this)
        {
            SetValue(IsExpandedProperty, !IsExpanded);
            var flag = IsActived;
            if (!flag)
            {
                _root?.DeactivateItems();
                SetValue(IsActivedPropertyKey, !flag);
            }
        }
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is SideMenuItem;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        var item = new SideMenuItem();
        // 新创建的子菜单项，Level是当前菜单项的Level+1
        item.Level = this.Level + 1;

        // 获取SideMenu的缩进大小
        if (_root != null)
        {
            item.UpdateIndent(_root.Indent);
        }

        return item;
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = VisualTreeHelperExtension.TryFindVisualParent<SideMenu>(this);

        if (_root != null)
        {
            UpdateIndent(_root.Indent);
        }
    }

    #endregion Override

    internal void SetInactive()
    {
        SetValue(IsActivedPropertyKey, false);
    }

    internal void UpdateIndent(double indentSize)
    {
        // 根据层级计算缩进，Level为0时没有缩进
        Indent = (Level > 0) ? Level * indentSize : 0;
    }

    // 无参数版本，用于内部调用
    private void UpdateIndent()
    {
        if (_root != null)
        {
            UpdateIndent(_root.Indent);
        }
    }
}