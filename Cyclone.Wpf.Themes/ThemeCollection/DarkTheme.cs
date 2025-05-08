using Cyclone.Wpf.Themes.ThemeManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Themes.ThemeCollection;

public class DarkTheme : Theme
{
    public DarkTheme()
    {
        Source = new Uri(@"pack://application:,,,/Cyclone.Wpf.Themes;component/Resources/DarkTheme.xaml", UriKind.Absolute);
    }

    public override string Name => nameof(DarkTheme);
}