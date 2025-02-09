using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Converters;

public class MathConverter
{
    public static FuncValueConverter<double,double, double> Scale { get; } =
       new((number,scale) => number * scale);
    public static FuncValueConverter<double,double> Half { get; } =
      new((number) => number * 0.5);

    public static FuncValueConverter<double,double,double> Subtraction { get; }=
        new((number,sub) => number - sub);
    public static FuncValueConverter<double,double,double> Addition { get; }=
        new((number,add) => number + add);
    public static FuncValueConverter<double,double,double> Multiplication { get; }=
        new((number,mult) => number * mult);
    public static FuncValueConverter<double,double,double> Division { get; }=
        new((number,div) => number / div);

}
