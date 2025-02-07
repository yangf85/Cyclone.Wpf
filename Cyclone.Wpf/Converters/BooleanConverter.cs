using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Converters;

public class BooleanConverter
{
    public static FuncValueConverter<bool, Visibility> ToVisibility =
        new FuncValueConverter<bool, Visibility>(i => i ? Visibility.Visible : Visibility.Collapsed);

    public static FuncValueConverter<bool, bool> InverseConverter { get; } =
           new((value) => !value);

    public static FuncValueConverter<string, string, bool> StringEqualityConverter { get; } =
       new((value, parameter) => value == parameter);

    public static FuncValueConverter<string, string, bool> StringNotEqualityConverter { get; } =
    new((value, parameter) => !(value == parameter));
}