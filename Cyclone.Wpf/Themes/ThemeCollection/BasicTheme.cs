using Cyclone.Wpf.Themes;
using System.ComponentModel;

namespace Cyclone.Wpf.Themes
{
    /// <summary>
    /// 基础主题
    /// </summary>
    public class BasicTheme : Theme
    {
        public BasicTheme()
        {
            Source = new Uri("pack://application:,,,/Cyclone.Wpf;component/Themes/BasicTheme.xaml", UriKind.Absolute);
        }

        public override string Name => nameof(BasicTheme);
    }
}