using Cyclone.Wpf.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Themes;

public class NormalTheme : Theme
{
    public NormalTheme() : base(nameof(NormalTheme), @"pack://application:,,,/Cyclone.UI;component/Styles/Resources/Themes/NormalTheme.xaml")
    {
    }

    public override string Name { get; set; } = nameof(NormalTheme);
}