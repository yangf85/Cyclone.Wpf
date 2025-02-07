using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Cyclone.Wpf.Converters;

public class BrushConverter
{
    public static FuncValueConverter<bool, Brush> BooleanToBrushConverter { get; } =
        new(b => b ? Brushes.Green : Brushes.Red);

    public static FuncValueConverter<int, Brush> IntToBrushConverter { get; } =
        new(i => i switch
        {
            -1 => Brushes.Yellow,
            < 1 => Brushes.Red,
            _ => Brushes.Green
        });
      
}
