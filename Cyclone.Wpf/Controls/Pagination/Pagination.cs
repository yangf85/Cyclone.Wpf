using Cyclone.Wpf.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_InfoTextBlock, Type = typeof(TextBlock))]
[TemplatePart(Name = PART_PerpageCountComboBox, Type = typeof(ComboBox))]
[TemplatePart(Name = PART_SelectListBox, Type = typeof(ListBox))]
[TemplatePart(Name = PART_GotoPageNumberBox, Type = typeof(NumberBox))]
[TemplatePart(Name = PART_GotoPageButton, Type = typeof(Button))]
[TemplatePart(Name = PART_PrevRepeatButton, Type = typeof(RepeatButton))]
[TemplatePart(Name = PART_NextRepeatButton, Type = typeof(RepeatButton))]
public class Pagination : Control
{
    private const string Ellipsis = "···";

    private const string PART_GotoPageButton = nameof(PART_GotoPageButton);

    private const string PART_GotoPageNumberBox = nameof(PART_GotoPageNumberBox);

    private const string PART_InfoTextBlock = nameof(PART_InfoTextBlock);

    private const string PART_NextRepeatButton = nameof(PART_NextRepeatButton);

    private const string PART_PerpageCountComboBox = nameof(PART_PerpageCountComboBox);

    private const string PART_PrevRepeatButton = nameof(PART_PrevRepeatButton);

    private const string PART_SelectListBox = nameof(PART_SelectListBox);

    private Button _gotoPageButton;

    private NumberBox _gotoPageNumberBox;

    private RepeatButton _nextPageButton;

    private ComboBox _perPageCountComboBox;

    private TextBox _perPageCountTextBox;

    private RepeatButton _prevPageButton;

    private ListBox _selectListBox;

    static Pagination()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Pagination), new FrameworkPropertyMetadata(typeof(Pagination)));
        InitializeCommands();
    }

    #region PageCount

    private static readonly DependencyPropertyKey PageCountPropertyKey =
              DependencyProperty.RegisterReadOnly("PageCount", typeof(int), typeof(Pagination), new PropertyMetadata(1, OnPageCountPropertyChanged));

    public static readonly DependencyProperty PageCountProperty = PageCountPropertyKey.DependencyProperty;

    /// <summary>
    /// 总页数
    /// </summary>
    public int PageCount => (int)GetValue(PageCountProperty);

    private static void OnPageCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
    }

    #endregion PageCount

    #region ItemCount

    public static readonly DependencyProperty ItemCountProperty = DependencyProperty.Register(nameof(ItemCount), typeof(int),
        typeof(Pagination), new PropertyMetadata(0, OnItemCountPropertyChanged, CoerceItemCount));

    /// <summary>
    /// 总数
    /// </summary>
    public int ItemCount
    {
        get => (int)GetValue(ItemCountProperty);
        set => SetValue(ItemCountProperty, value);
    }

    private static object CoerceItemCount(DependencyObject d, object value)
    {
        var count = (int)value;
        return Math.Max(count, 0);
    }

    private static void OnItemCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as Pagination;
        var count = (int)e.NewValue;

        ctrl.SetValue(PageCountPropertyKey, (int)Math.Ceiling(count * 1.0 / ctrl.PerpageCount));
        ctrl.UpdatePages();
    }

    #endregion ItemCount

    #region PerPageCount

    public static readonly DependencyProperty PerpageCountProperty = DependencyProperty.Register(nameof(PerpageCount),
        typeof(int), typeof(Pagination), new PropertyMetadata(50, OnPerpageCountPropertyChanged, CoercePerpageCount));

    /// <summary>
    /// 每页数量
    /// </summary>
    public int PerpageCount
    {
        get => (int)GetValue(PerpageCountProperty);
        set => SetValue(PerpageCountProperty, value);
    }

    private static object CoercePerpageCount(DependencyObject d, object value)
    {
        var countPerpage = (int)value;
        return Math.Max(countPerpage, 1);
    }

    private static void OnPerpageCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var ctrl = d as Pagination;
        var countPerpage = (int)e.NewValue;

        if (ctrl._perPageCountTextBox != null)
            ctrl._perPageCountTextBox.Text = countPerpage.ToString();

        ctrl.SetValue(PageCountPropertyKey, (int)Math.Ceiling(ctrl.ItemCount * 1.0 / countPerpage));

        if (ctrl.PageIndex != 1)
            ctrl.PageIndex = 1;
        else
            ctrl.UpdatePages();
    }

    #endregion PerPageCount

    #region PageIndex

    public static readonly DependencyProperty PageIndexProperty = DependencyProperty.Register(nameof(PageIndex), typeof(int),
        typeof(Pagination), new PropertyMetadata(1, OnPageIndexPropertyChanged, CoercePageIndex));

    /// <summary>
    /// 当前页面位置
    /// </summary>
    public int PageIndex
    {
        get => (int)GetValue(PageIndexProperty);
        set => SetValue(PageIndexProperty, value);
    }

    private static object CoercePageIndex(DependencyObject d, object value)
    {
        return Math.Max((int)value, 1);
    }

    private static void OnPageIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var pagination = d as Pagination;
        var current = (int)e.NewValue;

        if (pagination._selectListBox != null)
            pagination._selectListBox.SelectedItem = current.ToString();

        if (pagination._gotoPageNumberBox != null)
            pagination._gotoPageNumberBox.Value = current;

        var arg = new RoutedEventArgs(PageIndexChangedEvent, pagination);
        pagination.RaiseEvent(arg);
        pagination.UpdatePages();
    }

    #endregion PageIndex

    #region Pages

    private static readonly DependencyPropertyKey PagesPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(Pages), typeof(IEnumerable<string>), typeof(Pagination),
            new PropertyMetadata(null));

    public static readonly DependencyProperty PagesProperty = PagesPropertyKey.DependencyProperty;

    /// <summary>
    /// 总页数
    /// </summary>
    public IEnumerable<string> Pages => (IEnumerable<string>)GetValue(PagesProperty);

    #endregion Pages

    #region Command

    public static RoutedCommand NextCommand { get; private set; }

    public static RoutedCommand PrevCommand { get; private set; }

    private static void InitializeCommands()
    {
        PrevCommand = new RoutedCommand("Prev", typeof(Pagination));
        NextCommand = new RoutedCommand("Next", typeof(Pagination));

        CommandManager.RegisterClassCommandBinding(typeof(Pagination),
            new CommandBinding(PrevCommand, OnPrevCommand, OnCanPrevCommand));
        CommandManager.RegisterClassCommandBinding(typeof(Pagination),
            new CommandBinding(NextCommand, OnNextCommand, OnCanNextCommand));
    }

    private static void OnCanNextCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var ctrl = sender as Pagination;
        e.CanExecute = ctrl.PageIndex < ctrl.PageCount;
    }

    private static void OnCanPrevCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        var ctrl = sender as Pagination;
        e.CanExecute = ctrl.PageIndex > 1;
    }

    private static void OnNextCommand(object sender, RoutedEventArgs e)
    {
        var ctrl = sender as Pagination;
        ctrl.PageIndex++;
    }

    private static void OnPrevCommand(object sender, RoutedEventArgs e)
    {
        var ctrl = sender as Pagination;
        ctrl.PageIndex--;
    }

    #endregion Command

    #region Override

    private void GotoPageButton_Click(object sender, RoutedEventArgs e)
    {
        PageIndex = (int)_gotoPageNumberBox.Value;
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_selectListBox.SelectedItem == null)
            return;

        PageIndex = int.Parse(_selectListBox.SelectedItem.ToString());
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _perPageCountComboBox = GetTemplateChild(PART_PerpageCountComboBox) as ComboBox;
        if (_perPageCountComboBox != null)
        {
            _perPageCountComboBox.ItemsSource = GeneratePerpageCount();
            _perPageCountComboBox.SelectedIndex = 0;
        }

        _selectListBox = GetTemplateChild(PART_SelectListBox) as ListBox;
        if (_selectListBox != null)
        {
            _selectListBox.SelectedItem = PageIndex.ToString();
            _selectListBox.SelectionChanged -= OnSelectionChanged;
            _selectListBox.SelectionChanged += OnSelectionChanged;
        }

        _gotoPageNumberBox = GetTemplateChild(PART_GotoPageNumberBox) as NumberBox;

        _gotoPageButton = GetTemplateChild(PART_GotoPageButton) as Button;
        if (_gotoPageButton != null)
        {
            _gotoPageButton.Click -= GotoPageButton_Click;
            _gotoPageButton.Click += GotoPageButton_Click;
        }

        SetValue(PageCountPropertyKey, (int)Math.Ceiling(ItemCount * 1.0 / PerpageCount));
    }

    #endregion Override

    #region Private

    private IEnumerable<string> GeneratePageNumber(int count, int current)
    {
        if (count == 0)
            return null;

        if (PageCount <= 7)
            return Enumerable.Range(1, PageCount).Select(p => p.ToString()).ToArray();

        if (current <= 4)
            return new[] { "1", "2", "3", "4", "5", Ellipsis, PageCount.ToString() };

        if (current >= PageCount - 3)
            return new[]
            {
                "1", Ellipsis, (PageCount - 4).ToString(), (PageCount - 3).ToString(), (PageCount - 2).ToString(),
                (PageCount - 1).ToString(), PageCount.ToString()
            };

        return new[]
        {
            "1", Ellipsis, (current - 1).ToString(), current.ToString(), (current + 1).ToString(), Ellipsis,
            PageCount.ToString()
        };
    }

    private IEnumerable<int> GeneratePerpageCount()
    {
        return new int[] { 10, 20, 30, 40, 50 };
    }

    private void UpdatePages()
    {
        SetValue(PagesPropertyKey, GeneratePageNumber(ItemCount, PageIndex));

        if (_selectListBox != null && _selectListBox.SelectedItem == null)
            _selectListBox.SelectedItem = PageIndex.ToString();
    }

    #endregion Private

    #region PageIndexChanged

    public static readonly RoutedEvent PageIndexChangedEvent = EventManager.RegisterRoutedEvent("PageIndexChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Pagination));

    public event RoutedEventHandler PageIndexChanged
    {
        add { AddHandler(PageIndexChangedEvent, value); }
        remove { RemoveHandler(PageIndexChangedEvent, value); }
    }

    #endregion PageIndexChanged
}