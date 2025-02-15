using Cyclone.Wpf.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

[StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(SideMenuItem))]
public class SideMenuItem : HeaderedItemsControl,ICommandSource
{
    static SideMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenuItem), new FrameworkPropertyMetadata(typeof(SideMenuItem)));
    }

    SideMenu _root;
   



    #region RowHeight
    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(SideMenuItem), new PropertyMetadata(32d));

    #endregion

    #region IsExpanded
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(SideMenuItem), new PropertyMetadata(default(bool)));

    #endregion



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
    

    #endregion







    #region Indent

    private void SideMenuItem_Unloaded(object sender, RoutedEventArgs e)
    {
        
        if (_root != null)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(SideMenu.IndentProperty, typeof(SideMenu));
            descriptor.RemoveValueChanged(_root, OnIndentChanged);
        }

    }

    private void SideMenuItem_Loaded(object sender, RoutedEventArgs e)
    {

        if (_root != null)
        {
            var descriptor = DependencyPropertyDescriptor.FromProperty(SideMenu.IndentProperty, typeof(SideMenu));
            descriptor.AddValueChanged(_root, OnIndentChanged);
            SetValue(IndentPropertyKey, GetDepth()* _root.Indent);
        }
    }

    private void OnIndentChanged(object? sender, EventArgs e)
    {
        if (_root != null)
        {
            var indent=GetDepth()*_root.Indent;
            SetValue(IndentPropertyKey, indent);
        }
    }


    /// <summary>
    /// 在视觉树中查找当前元素的上一级（最近的） SideMenuItem 父元素，跳过其他非 SideMenuItem 的容器
    /// </summary>
    private static SideMenuItem FindVisualParentSideMenuItem(DependencyObject child)
    {
        DependencyObject parent = VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not SideMenuItem)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }
        return parent as SideMenuItem;
    }

    private int GetDepth()
    {
        if (_root.IsCompact)
        {
            return 0;
        }

        int depth = 0;
        DependencyObject current = this;
        // 每次获取视觉树中最近的 SideMenuItem 父级
        while ((current = FindVisualParentSideMenuItem(current)) != null)
        {
            depth++;
        }

        return depth;
    }



    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        
    }

    private static readonly DependencyPropertyKey IndentPropertyKey=
        DependencyProperty.RegisterReadOnly(nameof(Indent), typeof(double), typeof(SideMenuItem), new PropertyMetadata(default(double)));
    public static readonly DependencyProperty IndentProperty =IndentPropertyKey.DependencyProperty;
       

    #endregion


    #region Icon
    public object Icon
    {
        get => (object)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }


    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(SideMenuItem), new PropertyMetadata(default(object)));

    #endregion


    #region IconTemplate
    public DataTemplate IconTemplate
    {
        get => (DataTemplate)GetValue(IconTemplateProperty);
        set => SetValue(IconTemplateProperty, value);
    }

    public static readonly DependencyProperty IconTemplateProperty =
        DependencyProperty.Register(nameof(IconTemplate), typeof(DataTemplate), typeof(SideMenuItem), new PropertyMetadata(default(DataTemplate)));

    #endregion



    #region Impl CommandSource


    #region Command
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SideMenuItem), new PropertyMetadata(default(ICommand)));

    #endregion


    #region CommandParameter
    public object CommandParameter
    {
        get => (object)GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SideMenuItem), new PropertyMetadata(default(object)));

    #endregion



    #region CommandTarget
    public IInputElement CommandTarget
    {
        get => (IInputElement)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(SideMenuItem), new PropertyMetadata(default(IInputElement)));

    #endregion


    #endregion


    #region Override

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        
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
        var clickedItem = ElementHelper.TryFindVisualParent<SideMenuItem>(source);

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
        return new SideMenuItem();
    }

    public override void OnApplyTemplate()
    {
    
        base.OnApplyTemplate();
        _root = ElementHelper.TryFindVisualParent<SideMenu>(this);

        Loaded += SideMenuItem_Loaded;
        Unloaded += SideMenuItem_Unloaded;

        
    }

    #endregion

    internal void SetInactive()
    {
        SetValue(IsActivedPropertyKey, false);
    }


}
