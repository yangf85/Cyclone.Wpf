﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Converters;

public class VisibilityConverter
{
    public static FuncValueConverter<bool?, Visibility> VisibleWhenTrue { get; } =
        new(b =>
        {
            return b switch
            {
                true => Visibility.Visible,
                false => Visibility.Collapsed,
                _ => Visibility.Hidden
            };
        });

    public static FuncValueConverter<bool?, Visibility> VisibleWhenFalse { get; } =
        new(b =>
        {
            return b switch
            {
                true => Visibility.Collapsed,
                false => Visibility.Visible,
                _ => Visibility.Hidden
            };
        });

    public static FuncValueConverter<string, Visibility> VisibleWhenNullOrEmpty { get; } =
        new(s => string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed);

    public static FuncValueConverter<string, Visibility> VisibleWhenNotNullOrEmpty { get; } =
        new(s => string.IsNullOrEmpty(s) ? Visibility.Collapsed : Visibility.Visible);
}