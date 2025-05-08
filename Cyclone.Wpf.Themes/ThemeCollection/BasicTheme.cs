using Cyclone.Wpf.Themes.ThemeManagement;
using System.ComponentModel;

namespace Cyclone.Wpf.Themes.ThemeCollection
{
    /// <summary>
    /// 基础主题
    /// </summary>
    public class BasicTheme : Theme
    {
        public BasicTheme()
        {
            Source = new Uri("pack://application:,,,/Cyclone.Wpf.Themes;component/Resources/BasicTheme.xaml", UriKind.Absolute);
        }

        public override string Name => nameof(BasicTheme);
    }
}