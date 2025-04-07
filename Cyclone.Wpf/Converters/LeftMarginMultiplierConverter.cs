using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using Cyclone.Wpf.Helpers;

namespace Cyclone.Wpf.Converters;

public class LeftMarginMultiplierConverter : BasicConverter
{
    public double Length { get; set; }

    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not TreeViewItem item) return new Thickness(0);
        return new Thickness(Length * item.GetDepth(), 0, 0, 0);
    }

    public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}