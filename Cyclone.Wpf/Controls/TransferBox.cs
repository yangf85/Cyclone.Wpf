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
        CommandManager.RegisterClassCommandBinding(typeof(TransferBox),
            new CommandBinding(ToSourceCommand, OnToSourceCommand, OnCanToSourceCommand));
        CommandManager.RegisterClassCommandBinding(typeof(TransferBox),
            new CommandBinding(ToTargetCommand, OnToTargetCommand, OnCanToTargetCommand));
    }

    #region Command

    public static RoutedCommand ToSourceCommand { get; private set; } = new RoutedCommand("ToSource", typeof(TransferBox));

    public static RoutedCommand ToTargetCommand { get; private set; } = new RoutedCommand("ToTarget", typeof(TransferBox));

    private static void MoveItem(TransferBox shuttleBox, bool moveFlag)
    {
        if (shuttleBox.ItemsSource == null || shuttleBox.ItemsTarget == null) { return; }
        var ItemSourceListBox = shuttleBox.GetTemplateChild(PART_SourceListBox) as ListBox;
        var targetListBox = shuttleBox.GetTemplateChild(PART_TargetListBox) as ListBox;
        if (ItemSourceListBox == null || targetListBox == null) { return; }

        //对所有选择项进行移动，如果选择项为空，从第1项开始移动
        if (moveFlag)
        {
            if (ItemSourceListBox.SelectedItems != null && ItemSourceListBox.SelectedItems.Count != 0)
            {
                for (int i = ItemSourceListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    shuttleBox.ItemsTarget.Add(ItemSourceListBox.SelectedItems[0]);
                    shuttleBox.ItemsSource.Remove(ItemSourceListBox.SelectedItems[0]);
                }
            }
            else
            {
                if (shuttleBox.ItemsSource.Count > 0)
                {
                    var last = shuttleBox.ItemsSource[0];
                    shuttleBox.ItemsTarget.Add(last);
                    shuttleBox.ItemsSource.Remove(last);
                }
            }
        }
        else
        {
            if (targetListBox.SelectedItems != null && targetListBox.SelectedItems.Count != 0)
            {
                for (int i = targetListBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    shuttleBox.ItemsSource.Add(targetListBox.SelectedItems[0]);
                    shuttleBox.ItemsTarget.Remove(targetListBox.SelectedItems[0]);
                }
            }
            else
            {
                if (shuttleBox.ItemsTarget.Count > 0)
                {
                    var last = shuttleBox.ItemsTarget[0];
                    shuttleBox.ItemsSource.Add(last);
                    shuttleBox.ItemsTarget.Remove(last);
                }
            }
        }

        shuttleBox.RaiseEvent(new RoutedEventArgs(ItemsSourceChangedEvent, shuttleBox));
        shuttleBox.RaiseEvent(new RoutedEventArgs(ItemsTargetChangedEvent, shuttleBox));
    }

    private static void OnCanToSourceCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var shuttleBox = sender as TransferBox;
        if (shuttleBox != null)
        {
            e.CanExecute = shuttleBox.ItemsTarget != null && shuttleBox.ItemsTarget.Count > 0;
        }
    }

    private static void OnCanToTargetCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var shuttleBox = sender as TransferBox;
        if (shuttleBox != null)
        {
            e.CanExecute = shuttleBox.ItemsSource != null && shuttleBox.ItemsSource.Count > 0;
        }
    }

    private static void OnToSourceCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TransferBox shuttleBox)
        {
            MoveItem(shuttleBox, false);
        }
    }

    private static void OnToTargetCommand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is TransferBox shuttleBox)
        {
            MoveItem(shuttleBox, true);
        }
    }

    #endregion Command

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
    }

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

    #region SelectionMode

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode), typeof(TransferBox), new PropertyMetadata(default(SelectionMode)));

    public SelectionMode SelectionMode
    {
        get => (SelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    #endregion SelectionMode

    #region Orientation

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(TransferBox), new PropertyMetadata(default(Orientation)));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    #endregion Orientation

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