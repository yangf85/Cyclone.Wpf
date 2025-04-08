using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Converters;

public class BooleanConverter
{
    public static FuncValueConverter<bool, Visibility> ToVisibility { get; } =
        new FuncValueConverter<bool, Visibility>(i => i ? Visibility.Visible : Visibility.Collapsed);

    public static FuncValueConverter<bool, bool> Inverse { get; } =
           new((value) => !value);

    public static FuncValueConverter<string, string, bool> StringEquality { get; } =
       new((value, parameter) => value == parameter);

    public static FuncValueConverter<string, string, bool> StringNotEquality { get; } =
    new((value, parameter) => !(value == parameter));

    public static FuncValueConverter<object, bool> NullToBoolean { get; } =
        new((value) => value == null);

    public static FuncValueConverter<object, bool> NotNullToBoolean { get; } =
        new((value) => value != null);
}