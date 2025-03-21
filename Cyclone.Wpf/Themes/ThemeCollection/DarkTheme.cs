using Cyclone.Wpf.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Themes;

public class DarkTheme : Theme
{
    public DarkTheme()
    {
        base.Source = new Uri(@"pack://application:,,,/Cyclone.Wpf;component/Themes/DarkTheme.xaml", UriKind.Absolute);
    }

    public override string Name => nameof(DarkTheme);
}