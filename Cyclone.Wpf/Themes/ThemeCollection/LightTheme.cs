using Cyclone.Wpf.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Themes
{
    public class LightTheme : Theme
    {
        public LightTheme() : base(nameof(LightTheme), @"pack://application:,,,/Cyclone.UI;component/Styles/Resources/Themes/LightTheme.xaml")
        {
        }

        public override string Name { get; set; } = nameof(LightTheme);
    }
}