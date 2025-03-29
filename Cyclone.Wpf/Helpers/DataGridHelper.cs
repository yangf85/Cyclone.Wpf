using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Helpers;

public class DataGridHelper
{
    #region SelectedItems

    public static readonly DependencyProperty SelectedItemsProperty =
        DependencyProperty.RegisterAttached("SelectedItems", typeof(IList), typeof(DataGridHelper),
                    new FrameworkPropertyMetadata(default(IList), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGrid dataGrid)
        {
            dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
            dataGrid.SelectionChanged += DataGrid_SelectionChanged;
        }
    }

    private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid dataGrid)
        {
            var selectedItems = GetSelectedItems(dataGrid);
            if (selectedItems == null || dataGrid.SelectedItems == null) { return; }
            selectedItems.Clear();
            foreach (var item in dataGrid.SelectedItems)
            {
                selectedItems.Add(item);
            }
        }
    }

    public static IList GetSelectedItems(DataGrid obj) => (IList)obj.GetValue(SelectedItemsProperty);

    public static void SetSelectedItems(DataGrid obj, IList value) => obj.SetValue(SelectedItemsProperty, value);

    #endregion SelectedItems
}