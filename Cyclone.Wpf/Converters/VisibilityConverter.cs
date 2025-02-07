using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Converters;

internal class VisibilityConverter
{
    public static FuncValueConverter<bool?, Visibility> BooleanToVisibilityConverter { get; } =
       new(b =>
       {
           return b switch
           {
               true => Visibility.Visible,
               false => Visibility.Collapsed,
               _ => Visibility.Hidden
           };
       });
}
