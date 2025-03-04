using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Helpers;

public class ListBoxHelper
{
    #region IsSelectedAll

    public static readonly DependencyProperty IsSelectedAllProperty =
        DependencyProperty.RegisterAttached(
            "IsSelectedAll",
            typeof(bool),
            typeof(ListBoxHelper),
            new PropertyMetadata(false, OnIsSelectedAllChanged));

    private static void OnIsSelectedAllChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListBox listBox)
        {
            if (!GetIsSelectAllEnabled(listBox))
            {
                return; // If selecting all is not enabled, do nothing
            }

            if (listBox.SelectionMode == SelectionMode.Single)
            {
                listBox.SelectionMode = SelectionMode.Extended;
            }

            listBox.SelectionChanged -= ListBox_SelectionChanged;

            if ((bool)e.NewValue)
            {
                listBox.SelectAll();
            }
            else
            {
                listBox.UnselectAll();
            }

            listBox.SelectionChanged += ListBox_SelectionChanged;
        }
    }

    public static bool GetIsSelectedAll(ListBox obj) => (bool)obj.GetValue(IsSelectedAllProperty);

    public static void SetIsSelectedAll(ListBox obj, bool value) => obj.SetValue(IsSelectedAllProperty, value);

    #endregion IsSelectedAll

    #region IsSelectAllEnabled

    public static readonly DependencyProperty IsSelectAllEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsSelectAllEnabled",
            typeof(bool),
            typeof(ListBoxHelper),
            new PropertyMetadata(false));

    public static bool GetIsSelectAllEnabled(ListBox obj) => (bool)obj.GetValue(IsSelectAllEnabledProperty);

    public static void SetIsSelectAllEnabled(ListBox obj, bool value) => obj.SetValue(IsSelectAllEnabledProperty, value);

    #endregion IsSelectAllEnabled

    #region SelectedItems

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached(
            "SelectedItems",
            typeof(IList),
            typeof(ListBoxHelper),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged));

    private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListBox listBox)
        {
            if (e.NewValue is IList newSelectedItems)
            {
                listBox.SelectionChanged -= ListBox_SelectionChanged;
                SyncListBoxWithSelectedItems(listBox, newSelectedItems);
                listBox.SelectionChanged += ListBox_SelectionChanged;
            }
        }
    }

    private static void SyncListBoxWithSelectedItems(ListBox listBox, IList selectedItems)
    {
        if (listBox.SelectionMode == SelectionMode.Single)
        {
            return;
        }
        listBox.SelectedItems.Clear();
        foreach (var item in selectedItems)
        {
            listBox.SelectedItems.Add(item);
        }
    }

    private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox)
        {
            var selectedItems = GetSelectedItems(listBox);
            if (selectedItems == null) return;

            foreach (var item in e.RemovedItems)
            {
                selectedItems.Remove(item);
            }

            foreach (var item in e.AddedItems)
            {
                if (!selectedItems.Contains(item))
                {
                    selectedItems.Add(item);
                }
            }

            // Update IsSelectedAll property based on current selection
            SetIsSelectedAll(listBox, GetIsSelectAllEnabled(listBox) && listBox.SelectedItems.Count == listBox.Items.Count);
        }
    }

    public static IList GetSelectedItems(ListBox obj) => (IList)obj.GetValue(SelectedItemsProperty);

    public static void SetSelectedItems(ListBox obj, IList value) => obj.SetValue(SelectedItemsProperty, value);

    #endregion SelectedItems
}