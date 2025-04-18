using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Cyclone.Wpf.Helpers;

public class TextBoxHelper
{
    static TextBoxHelper()
    {
        CommandManager.RegisterClassCommandBinding(typeof(TextBoxHelper),
            new CommandBinding(TextBoxHelper.ClearCommand, OnClear, OnCanClear));
    }

    #region Watermark

    public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);

    public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

    public static readonly DependencyProperty WatermarkProperty =
                DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(TextBoxHelper), new PropertyMetadata(default(string)));

    #endregion Watermark

    #region HasClearButton

    public static readonly DependencyProperty HasClearButtonProperty =
                DependencyProperty.RegisterAttached("HasClearButton", typeof(bool), typeof(TextBoxHelper), new PropertyMetadata(default(bool), OnHasClearButtonChanged));

    private static void OnHasClearButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox textBox)
        {
            if ((bool)e.NewValue)
            {
                textBox.CommandBindings.Add(new CommandBinding(ClearCommand, OnClear, OnCanClear));
            }
            else
            {
                textBox.CommandBindings.Remove(new CommandBinding(ClearCommand, OnClear, OnCanClear));
            }
        }
    }

    public static bool GetHasClearButton(DependencyObject obj) => (bool)obj.GetValue(HasClearButtonProperty);

    public static void SetHasClearButton(DependencyObject obj, bool value) => obj.SetValue(HasClearButtonProperty, value);

    #endregion HasClearButton

    #region ClearCommand

    public static RoutedCommand ClearCommand { get; private set; } =
        new RoutedCommand("Clear", typeof(TextBoxHelper));

    private static void OnCanClear(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private static void OnClear(object sender, ExecutedRoutedEventArgs e)
    {
        var textBox = sender as TextBox;
        textBox.Clear();
        var ps = new PasswordBox();
    }

    #endregion ClearCommand
}