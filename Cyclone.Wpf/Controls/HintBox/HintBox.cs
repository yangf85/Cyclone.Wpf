using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 继承自 ComboBox 的智能提示框控件
/// 当IsEditable为true时，HintBox将变成一个可编辑的ComboBox，并且支持输入文本进行搜索。
/// 但是SelectionBoxItem将不显示，只显示HintBox的文本。
/// </summary>
[TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class HintBox : ComboBox
{
    private TextBox _editableTextBox;
    private string _currentFilter = string.Empty;

    static HintBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(HintBox),
            new FrameworkPropertyMetadata(typeof(HintBox)));

        // 重写这些属性以确保正确的默认行为
        IsEditableProperty.OverrideMetadata(typeof(HintBox),
            new FrameworkPropertyMetadata(true));

        IsTextSearchEnabledProperty.OverrideMetadata(typeof(HintBox),
            new FrameworkPropertyMetadata(false));
    }

    public HintBox()
    {
        IsEditable = true;
        IsTextSearchEnabled = false;
        CommandBindings.Add(new CommandBinding(ClearTextCommand, OnClearTextCommand, OnCanClearTextCommand));
    }

    #region SearchMemberPath

    public static readonly DependencyProperty SearchMemberPathProperty =
        DependencyProperty.Register(nameof(SearchMemberPath), typeof(string), typeof(HintBox),
            new PropertyMetadata(null, OnSearchMemberPathChanged));

    /// <summary>
    /// 获取或设置用于搜索的属性路径。如果不设置，将使用 DisplayMemberPath
    /// </summary>
    public string SearchMemberPath
    {
        get => (string)GetValue(SearchMemberPathProperty);
        set => SetValue(SearchMemberPathProperty, value);
    }

    private static void OnSearchMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var hintBox = (HintBox)d;
        hintBox.RefreshFilter();
    }

    #endregion SearchMemberPath

    #region StringComparison

    public static readonly DependencyProperty StringComparisonProperty =
        DependencyProperty.Register(nameof(StringComparison), typeof(StringComparison), typeof(HintBox),
            new PropertyMetadata(StringComparison.OrdinalIgnoreCase, OnStringComparisonChanged));

    public StringComparison StringComparison
    {
        get => (StringComparison)GetValue(StringComparisonProperty);
        set => SetValue(StringComparisonProperty, value);
    }

    private static void OnStringComparisonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var hintBox = (HintBox)d;
        hintBox.RefreshFilter();
    }

    #endregion StringComparison

    #region ClearTextCommand

    public static readonly RoutedCommand ClearTextCommand = new RoutedCommand("ClearTextCommand", typeof(HintBox));

    private void OnClearTextCommand(object sender, ExecutedRoutedEventArgs e)
    {
        _editableTextBox.Clear();
        _editableTextBox.Focus();
        SelectedItem = null;
        _currentFilter = string.Empty;
        RefreshFilter();
    }

    private void OnCanClearTextCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        if (_editableTextBox == null)
        {
            e.CanExecute = false;
            return;
        }
        e.CanExecute = _editableTextBox.Text.Length > 0;
    }

    #endregion ClearTextCommand

    #region Override Methods

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _editableTextBox = GetTemplateChild("PART_EditableTextBox") as TextBox;
        var clearButton = GetTemplateChild("PART_ClearButton") as Button;
    }

    protected override void OnPreviewTextInput(TextCompositionEventArgs e)
    {
        base.OnPreviewTextInput(e);
        IsDropDownOpen = true;
        _currentFilter = _editableTextBox?.Text ?? string.Empty;
        RefreshFilter();
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        Text = GetItemText(SelectedItem);
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new HintBoxItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is HintBoxItem;
    }

    #endregion Override Methods

    #region Private Methods

    private void RefreshFilter()
    {
        if (Items.CanFilter)
        {
            Items.Filter = FilterPredicate;
        }
    }

    private bool FilterPredicate(object item)
    {
        if (string.IsNullOrEmpty(_currentFilter) || item == null)
        {
            return true;
        }

        var searchText = GetSearchText(item);
        return searchText.IndexOf(_currentFilter, StringComparison) >= 0;
    }

    private string GetItemText(object item)
    {
        if (item == null) { return string.Empty; }

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            return GetPropertyValue(item, DisplayMemberPath)?.ToString() ?? string.Empty;
        }
        if (!string.IsNullOrEmpty(SearchMemberPath))
        {
            return GetPropertyValue(item, SearchMemberPath)?.ToString() ?? string.Empty;
        }

        return item.ToString();
    }

    private string GetSearchText(object item)
    {
        if (item == null) { return string.Empty; }

        if (!string.IsNullOrEmpty(SearchMemberPath))
        {
            return GetPropertyValue(item, SearchMemberPath)?.ToString() ?? string.Empty;
        }

        return GetItemText(item);
    }

    private object GetPropertyValue(object obj, string propertyPath)
    {
        if (obj == null || string.IsNullOrEmpty(propertyPath)) { return null; }

        try
        {
            // 支持嵌套属性
            var properties = propertyPath.Split('.');
            object currentValue = obj;

            foreach (var propertyName in properties)
            {
                if (currentValue == null)
                    return null;

                var property = currentValue.GetType().GetProperty(propertyName);
                if (property == null)
                    return null;

                currentValue = property.GetValue(currentValue);
            }

            return currentValue;
        }
        catch
        {
            return null;
        }
    }

    #endregion Private Methods
}