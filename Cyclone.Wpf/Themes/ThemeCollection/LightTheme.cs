using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Themes;

public class LightTheme : Theme
{
    public override string Name => nameof(LightTheme);

    public LightTheme()
    {
        Source = new Uri(@"pack://application:,,,/Cyclone.Wpf.Themes;component/Themes/LightTheme.xaml", UriKind.Absolute);
    }
}