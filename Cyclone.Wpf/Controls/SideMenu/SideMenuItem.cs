using Cyclone.Wpf.Helpers;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class SideMenuItem : HeaderedItemsControl,ICommandSource
{
    static SideMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenuItem), new FrameworkPropertyMetadata(typeof(SideMenuItem)));
    }

    SideMenu _root;
    public SideMenuItem()
    {
       
    }

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
    public bool IsActived
    {
        get => (bool)GetValue(IsActivedProperty);
        set => SetValue(IsActivedProperty, value);
    }

    public static readonly DependencyProperty IsActivedProperty =
        DependencyProperty.Register(nameof(IsActived), typeof(bool), typeof(SideMenuItem), new PropertyMetadata(default(bool),OnIsActivedChanged));

    private static void OnIsActivedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        
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


    private int GetDepth()
    {
        int depth = 0;
        DependencyObject current = this;

        // 逐级向上查找父级 SideMenuItem
        while (current is SideMenuItem)
        {
            // 获取父级容器（可能是 SideMenu 或另一个 SideMenuItem）
            DependencyObject parent = VisualTreeHelper.GetParent(current);

            // 如果父级是 SideMenu，说明已到达根节点，终止循环
            if (parent is SideMenu)
            {
                break;
            }

            // 如果父级是 SideMenuItem，深度加一
            if (parent is SideMenuItem)
            {
                depth++;
                current = parent;
            }
            else
            {
                // 其他情况（例如非容器父级），直接终止循环
                break;
            }
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

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (Command != null && Command.CanExecute(CommandParameter))
        {
            Command.Execute(CommandParameter);
        }

        if (e.Source ==this)
        {
            SetCurrentValue(IsExpandedProperty, !IsExpanded);
            SetCurrentValue(IsActivedProperty, !IsActived);
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
        _root = VisualHelper.TryFindParent<SideMenu>(this);

        Loaded += SideMenuItem_Loaded;
        Unloaded += SideMenuItem_Unloaded;

        
    }

    #endregion



}
