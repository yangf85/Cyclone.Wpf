using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Converters;

public class MathConverter
{
    public static FuncValueConverter<double,double, double> ScaleConverter { get; } =
       new((number,scale) => number * scale);
     
}
