using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    [ContentProperty("Items")]
    [TemplatePart(Name = "PART_EditableTextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_ItemsPresenter", Type = typeof(ItemsPresenter))]
    [TemplatePart(Name = "PART_SelectAllButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_UnselectAllButton", Type = typeof(Button))]
    [TemplatePart(Name = "PART_InvertSelectionButton", Type = typeof(Button))]
    public class MultiComboBox : Control
    {
        #region Fields

        private TextBox _textBox;
        private Popup _popup;
        private Button _selectAllButton;
        private Button _unselectAllButton;
        private Button _invertSelectionButton;
        private bool _isInternalChange;
        private ObservableCollection<object> _selectedItems;

        #endregion Fields

        #region Constructors

        static MultiComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiComboBox), new FrameworkPropertyMetadata(typeof(MultiComboBox)));
        }

        public MultiComboBox()
        {
            _selectedItems = new ObservableCollection<object>();
            _selectedItems.CollectionChanged += SelectedItems_CollectionChanged;

            Items = new ObservableCollection<object>();
            CommandBindings.Add(new CommandBinding(SelectAllCommand, OnSelectAllCommand));
            CommandBindings.Add(new CommandBinding(UnselectAllCommand, OnUnselectAllCommand));
            CommandBindings.Add(new CommandBinding(InvertSelectionCommand, OnInvertSelectionCommand));
        }

        #endregion Constructors

        #region Commands

        public static readonly RoutedCommand SelectAllCommand = new RoutedCommand("SelectAll", typeof(MultiComboBox));
        public static readonly RoutedCommand UnselectAllCommand = new RoutedCommand("UnselectAll", typeof(MultiComboBox));
        public static readonly RoutedCommand InvertSelectionCommand = new RoutedCommand("InvertSelection", typeof(MultiComboBox));

        private void OnSelectAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            SelectAll();
        }

        private void OnUnselectAllCommand(object sender, ExecutedRoutedEventArgs e)
        {
            UnselectAll();
        }

        private void OnInvertSelectionCommand(object sender, ExecutedRoutedEventArgs e)
        {
            InvertSelection();
        }

        #endregion Commands

        #region Properties

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            "IsDropDownOpen", typeof(bool), typeof(MultiComboBox), new PropertyMetadata(false, OnIsDropDownOpenChanged));

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            "ItemsSource", typeof(IEnumerable), typeof(MultiComboBox), new PropertyMetadata(null, OnItemsSourceChanged));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items", typeof(ObservableCollection<object>), typeof(MultiComboBox), new PropertyMetadata(null));

        public ObservableCollection<object> Items
        {
            get { return (ObservableCollection<object>)GetValue(ItemsProperty); }
            private set { SetValue(ItemsProperty, value); }
        }

        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
            "DisplayMemberPath", typeof(string), typeof(MultiComboBox), new PropertyMetadata(string.Empty, OnDisplayMemberPathChanged));

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set { SetValue(DisplayMemberPathProperty, value); }
        }

        public static readonly DependencyProperty ValueMemberPathProperty = DependencyProperty.Register(
            "ValueMemberPath", typeof(string), typeof(MultiComboBox), new PropertyMetadata(string.Empty));

        public string ValueMemberPath
        {
            get { return (string)GetValue(ValueMemberPathProperty); }
            set { SetValue(ValueMemberPathProperty, value); }
        }

        public static readonly DependencyProperty SeparatorProperty = DependencyProperty.Register(
            "Separator", typeof(string), typeof(MultiComboBox), new PropertyMetadata(", ", OnSeparatorChanged));

        public string Separator
        {
            get { return (string)GetValue(SeparatorProperty); }
            set { SetValue(SeparatorProperty, value); }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(MultiComboBox), new PropertyMetadata(string.Empty));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            "SelectedItems", typeof(IList), typeof(MultiComboBox), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged));

        public IList SelectedItems
        {
            get { return (IList)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedValuesProperty = DependencyProperty.Register(
            "SelectedValues", typeof(IList), typeof(MultiComboBox), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValuesChanged));

        public IList SelectedValues
        {
            get { return (IList)GetValue(SelectedValuesProperty); }
            set { SetValue(SelectedValuesProperty, value); }
        }

        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(MultiComboBox), new PropertyMetadata(null));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
            "MaxDropDownHeight", typeof(double), typeof(MultiComboBox), new PropertyMetadata(300.0));

        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        #endregion Properties

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Get template parts
            _textBox = GetTemplateChild("PART_EditableTextBox") as TextBox;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _selectAllButton = GetTemplateChild("PART_SelectAllButton") as Button;
            _unselectAllButton = GetTemplateChild("PART_UnselectAllButton") as Button;
            _invertSelectionButton = GetTemplateChild("PART_InvertSelectionButton") as Button;

            if (_textBox != null)
            {
                _textBox.GotFocus += (s, e) => IsDropDownOpen = true;
                _textBox.IsReadOnly = true;
            }

            if (_popup != null)
            {
                _popup.StaysOpen = false;
                _popup.Opened += (s, e) => { Focus(); };
            }

            if (_selectAllButton != null)
            {
                _selectAllButton.Click += (s, e) => SelectAll();
            }

            if (_unselectAllButton != null)
            {
                _unselectAllButton.Click += (s, e) => UnselectAll();
            }

            if (_invertSelectionButton != null)
            {
                _invertSelectionButton.Click += (s, e) => InvertSelection();
            }

            // Initial update of the text
            UpdateText();
        }

        public void SelectAll()
        {
            _isInternalChange = true;
            foreach (var item in Items)
            {
                if (!_selectedItems.Contains(item))
                {
                    _selectedItems.Add(item);
                }
            }
            _isInternalChange = false;

            UpdateText();
            UpdateSelectedItemsSource();
            UpdateSelectedValuesSource();
        }

        public void UnselectAll()
        {
            _isInternalChange = true;
            _selectedItems.Clear();
            _isInternalChange = false;

            UpdateText();
            UpdateSelectedItemsSource();
            UpdateSelectedValuesSource();
        }

        public void InvertSelection()
        {
            _isInternalChange = true;
            var newSelection = new List<object>();

            foreach (var item in Items)
            {
                if (!_selectedItems.Contains(item))
                {
                    newSelection.Add(item);
                }
            }

            _selectedItems.Clear();
            foreach (var item in newSelection)
            {
                _selectedItems.Add(item);
            }
            _isInternalChange = false;

            UpdateText();
            UpdateSelectedItemsSource();
            UpdateSelectedValuesSource();
        }

        private void UpdateText()
        {
            if (_selectedItems.Count == 0)
            {
                Text = string.Empty;
                return;
            }

            var displayValues = new List<string>();
            foreach (var item in _selectedItems)
            {
                string displayValue = GetDisplayValue(item);
                displayValues.Add(displayValue);
            }

            Text = string.Join(Separator, displayValues);
        }

        private string GetDisplayValue(object item)
        {
            if (string.IsNullOrEmpty(DisplayMemberPath))
            {
                return item.ToString();
            }

            try
            {
                var property = TypeDescriptor.GetProperties(item).Find(DisplayMemberPath, true);
                if (property != null)
                {
                    object value = property.GetValue(item);
                    return value?.ToString() ?? string.Empty;
                }
            }
            catch
            {
                // Fallback to ToString if any error occurs
            }

            return item.ToString();
        }

        private object GetItemValue(object item)
        {
            if (string.IsNullOrEmpty(ValueMemberPath))
            {
                return item;
            }

            try
            {
                var property = TypeDescriptor.GetProperties(item).Find(ValueMemberPath, true);
                if (property != null)
                {
                    return property.GetValue(item);
                }
            }
            catch
            {
                // Fallback to the item itself if any error occurs
            }

            return item;
        }

        private object FindItemByValue(object value)
        {
            if (string.IsNullOrEmpty(ValueMemberPath))
            {
                return Items.Cast<object>().FirstOrDefault(item =>
                    item?.Equals(value) == true);
            }

            return Items.Cast<object>().FirstOrDefault(item =>
            {
                try
                {
                    var property = TypeDescriptor.GetProperties(item).Find(ValueMemberPath, true);
                    if (property != null)
                    {
                        object itemValue = property.GetValue(item);
                        return itemValue?.Equals(value) == true;
                    }
                }
                catch
                {
                    // Ignore exceptions
                }
                return false;
            });
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isInternalChange)
                return;

            UpdateText();
            UpdateSelectedItemsSource();
            UpdateSelectedValuesSource();
        }

        private void UpdateSelectedItemsSource()
        {
            if (SelectedItems == null || _isInternalChange)
                return;

            _isInternalChange = true;

            // Create a new list or clear the existing one
            if (SelectedItems is IList selectedItemsList)
            {
                selectedItemsList.Clear();
                foreach (var item in _selectedItems)
                {
                    selectedItemsList.Add(item);
                }
            }

            _isInternalChange = false;
        }

        private void UpdateSelectedValuesSource()
        {
            if (SelectedValues == null || _isInternalChange)
                return;

            _isInternalChange = true;

            // Create a new list or clear the existing one
            if (SelectedValues is IList selectedValuesList)
            {
                selectedValuesList.Clear();
                foreach (var item in _selectedItems)
                {
                    selectedValuesList.Add(GetItemValue(item));
                }
            }

            _isInternalChange = false;
        }

        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiComboBox;
            if (control._popup != null)
            {
                control._popup.IsOpen = (bool)e.NewValue;
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiComboBox;
            control.Items.Clear();

            if (e.NewValue != null)
            {
                foreach (var item in (IEnumerable)e.NewValue)
                {
                    control.Items.Add(item);
                }
            }

            // Clear selection when ItemsSource changes
            control._isInternalChange = true;
            control._selectedItems.Clear();
            control._isInternalChange = false;
            control.UpdateText();
            control.UpdateSelectedItemsSource();
            control.UpdateSelectedValuesSource();
        }

        private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiComboBox;
            control.UpdateText();
        }

        private static void OnSeparatorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiComboBox;
            control.UpdateText();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiComboBox;
            if (control._isInternalChange)
                return;

            control._isInternalChange = true;
            control._selectedItems.Clear();

            if (e.NewValue is IEnumerable items)
            {
                foreach (var item in items)
                {
                    control._selectedItems.Add(item);
                }
            }

            control._isInternalChange = false;
            control.UpdateText();
            control.UpdateSelectedValuesSource();
        }

        private static void OnSelectedValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as MultiComboBox;
            if (control._isInternalChange)
                return;

            control._isInternalChange = true;
            control._selectedItems.Clear();

            if (e.NewValue is IEnumerable values)
            {
                foreach (var value in values)
                {
                    var item = control.FindItemByValue(value);
                    if (item != null)
                    {
                        control._selectedItems.Add(item);
                    }
                }
            }

            control._isInternalChange = false;
            control.UpdateText();
            control.UpdateSelectedItemsSource();
        }

        #endregion Methods
    }
}