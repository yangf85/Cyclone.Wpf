using Cyclone.Wpf.Themes;
using System.ComponentModel;

namespace Cyclone.Wpf.Themes
{
    /// <summary>
    /// 基础主题
    /// </summary>
    public class BasicsTheme : Theme
    {
        public BasicsTheme() : base(nameof(BasicsTheme), @"pack://application:,,,/Cyclone.UI;component/Styles/Resources/Themes/BasicTheme.xaml")
        {
        }

        public override string Name { get; set; } = nameof(BasicsTheme);
    }
}