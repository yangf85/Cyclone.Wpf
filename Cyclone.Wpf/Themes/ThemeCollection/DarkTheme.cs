using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.UI.Themes
{
    public class DarkTheme : Theme
    {
        public DarkTheme() : base(nameof(DarkTheme), @"pack://application:,,,/Cyclone.UI;component/Styles/Resources/Themes/DarkTheme.xaml")
        {
        }

        public override string Name { get; set; } = nameof(DarkTheme);
    }
}