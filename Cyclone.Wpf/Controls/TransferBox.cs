using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_SourceListBox, Type = typeof(ListBox))]
[TemplatePart(Name = PART_TargetListBox, Type = typeof(ListBox))]
[TemplatePart(Name = PART_ToSourceRepeatButton, Type = typeof(System.Windows.Controls.Primitives.RepeatButton))]
[TemplatePart(Name = PART_ToTargetRepeatButton, Type = typeof(System.Windows.Controls.Primitives.RepeatButton))]
public class TransferBox : Control
{
    private const string PART_SourceListBox = nameof(PART_SourceListBox);

    private const string PART_TargetListBox = nameof(PART_TargetListBox);

    private const string PART_ToSourceRepeatButton = nameof(PART_ToSourceRepeatButton);

    private const string PART_ToTargetRepeatButton = nameof(PART_ToTargetRepeatButton);

    private ListBox _sourceListBox;

    private ListBox _targetListBox;

    private System.Windows.Controls.Primitives.RepeatButton _toSourceRepeatButton;

    private System.Windows.Controls.Primitives.RepeatButton _toTargetRepeatButton;

    static TransferBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TransferBox), new FrameworkPropertyMetadata(typeof(TransferBox)));

        CommandManager.RegisterClassCommandBinding(typeof(TransferBox),
            new CommandBinding(ToSourceCommand, OnToSourceCommand, OnCanToSourceCommand));
        CommandManager.RegisterClassCommandBinding(typeof(TransferBox),
            new CommandBinding(ToTargetCommand, OnToTargetCommand, OnCanToTargetCommand));

    }

    #region Command

    #region ToSourceCommand

    public static RoutedCommand ToSourceCommand { get; private set; } = new RoutedCommand("ToSource", typeof(TransferBox));

    private static void OnToSourceCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TransferBox shuttleBox)
        {
            MoveItem(shuttleBox, false);
        }
    }

    private static void OnCanToSourceCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var shuttleBox = sender as TransferBox;
        if (shuttleBox != null)
        {
            e.CanExecute = shuttleBox.ItemsTarget != null && shuttleBox.ItemsTarget.Count > 0;
        }
    }

    #endregion ToSourceCommand

    #region ToTargetCommand

    public static RoutedCommand ToTargetCommand { get; private set; } = new RoutedCommand("ToTarget", typeof(TransferBox));

    private static void MoveItem(TransferBox transferBox, bool moveFlag)
    {
        if (transferBox.ItemsSource == null || transferBox.ItemsTarget == null) { return; }
        var ItemSourceListBox = transferBox.GetTemplateChild(PART_SourceListBox) as ListBox;
        var targetListBox = transferBox.GetTemplateChild(PART_TargetListBox) as ListBox;
        if (ItemSourceListBox == null || targetListBox == null) { return; }

        //对所有选择项进行移动，如果选择项为空，从第1项开始移动
        if (moveFlag)
        {
            if (ItemSourceListBox.SelectedItems != null && ItemSourceListBox.SelectedItems.Count != 0)
            {
                for (int i = ItemSourceListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    transferBox.ItemsTarget.Add(ItemSourceListBox.SelectedItems[0]);
                    transferBox.ItemsSource.Remove(ItemSourceListBox.SelectedItems[0]);
                }
            }
            else
            {
                if (transferBox.ItemsSource.Count > 0)
                {
                    var last = transferBox.ItemsSource[0];
                    transferBox.ItemsTarget.Add(last);
                    transferBox.ItemsSource.Remove(last);
                }
            }
        }
        else
        {
            if (targetListBox.SelectedItems != null && targetListBox.SelectedItems.Count != 0)
            {
                for (int i = targetListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    transferBox.ItemsSource.Add(targetListBox.SelectedItems[0]);
                    transferBox.ItemsTarget.Remove(targetListBox.SelectedItems[0]);
                }
            }
            else
            {
                if (transferBox.ItemsTarget.Count > 0)
                {
                    var last = transferBox.ItemsTarget[0];
                    transferBox.ItemsSource.Add(last);
                    transferBox.ItemsTarget.Remove(last);
                }
            }
        }

        transferBox.RaiseEvent(new RoutedEventArgs(ItemsSourceChangedEvent, transferBox));
        transferBox.RaiseEvent(new RoutedEventArgs(ItemsTargetChangedEvent, transferBox));
    }

    private static void OnCanToTargetCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var shuttleBox = sender as TransferBox;
        if (shuttleBox != null)
        {
            e.CanExecute = shuttleBox.ItemsSource != null && shuttleBox.ItemsSource.Count > 0;
        }
    }

    private static void OnToTargetCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TransferBox shuttleBox)
        {
            MoveItem(shuttleBox, true);
        }
    }

    #endregion ToTargetCommand

    #endregion Command

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

    #region SourceHeader

    public static readonly DependencyProperty SourceHeaderProperty =
        DependencyProperty.Register(nameof(SourceHeader), typeof(object), typeof(TransferBox), new PropertyMetadata(default(object)));

    public object SourceHeader
    {
        get => (object)GetValue(SourceHeaderProperty);
        set => SetValue(SourceHeaderProperty, value);
    }

    #endregion SourceHeader

    #region TargetHeader

    public static readonly DependencyProperty TargetHeaderProperty =
        DependencyProperty.Register(nameof(TargetHeader), typeof(object), typeof(TransferBox), new PropertyMetadata(default(object)));

    public object TargetHeader
    {
        get => (object)GetValue(TargetHeaderProperty);
        set => SetValue(TargetHeaderProperty, value);
    }

    #endregion TargetHeader

    #region SourceDismemberPath

    public static readonly DependencyProperty SourceDismemberPathProperty =
        DependencyProperty.Register(nameof(SourceDismemberPath), typeof(string), typeof(TransferBox), new PropertyMetadata(default(string)));

    public string SourceDismemberPath
    {
        get => (string)GetValue(SourceDismemberPathProperty);
        set => SetValue(SourceDismemberPathProperty, value);
    }

    #endregion SourceDismemberPath

    #region TargetDismemberPath

    public static readonly DependencyProperty TargetDismemberPathProperty =
        DependencyProperty.Register(nameof(TargetDismemberPath), typeof(string), typeof(TransferBox), new PropertyMetadata(default(string)));

    public string TargetDismemberPath
    {
        get => (string)GetValue(TargetDismemberPathProperty);
        set => SetValue(TargetDismemberPathProperty, value);
    }

    #endregion TargetDismemberPath

    #region ItemsSource

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(TransferBox), new PropertyMetadata(default(IList)));

    public IList ItemsSource
    {
        get => (IList)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    #endregion ItemsSource

    #region ItemsTarget

    public static readonly DependencyProperty ItemsTargetProperty =
        DependencyProperty.Register(nameof(ItemsTarget), typeof(IList), typeof(TransferBox), new PropertyMetadata(default(IList)));

    public IList ItemsTarget
    {
        get => (IList)GetValue(ItemsTargetProperty);
        set => SetValue(ItemsTargetProperty, value);
    }

    #endregion ItemsTarget

    #region SourceChanged

    public static readonly RoutedEvent ItemsSourceChangedEvent = EventManager.RegisterRoutedEvent(nameof(ItemsSourceChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TransferBox));

    public event RoutedEventHandler ItemsSourceChanged
    {
        add { AddHandler(ItemsSourceChangedEvent, value); }
        remove { RemoveHandler(ItemsSourceChangedEvent, value); }
    }

    #endregion SourceChanged

    #region TargetChanged

    public static readonly RoutedEvent ItemsTargetChangedEvent = EventManager.RegisterRoutedEvent(nameof(ItemsTargetChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(TransferBox));

    public event RoutedEventHandler ItemsTargetChanged
    {
        add { AddHandler(ItemsTargetChangedEvent, value); }
        remove { RemoveHandler(ItemsTargetChangedEvent, value); }
    }

    #endregion TargetChanged

    #region ItemTemplate

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(TransferBox), new PropertyMetadata(default(DataTemplate)));

    public DataTemplate ItemTemplate
    {
        get => (DataTemplate)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    #endregion ItemTemplate

    #region ItemPanel

    public static readonly DependencyProperty ItemPanelProperty =
        DependencyProperty.Register(nameof(ItemPanel), typeof(ItemsPanelTemplate), typeof(TransferBox), new PropertyMetadata(default(ItemsPanelTemplate)));

    public ItemsPanelTemplate ItemPanel
    {
        get => (ItemsPanelTemplate)GetValue(ItemPanelProperty);
        set => SetValue(ItemPanelProperty, value);
    }

    #endregion ItemPanel
}