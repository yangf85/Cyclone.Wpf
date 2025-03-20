using System;
using System.ComponentModel;
using System.Windows;

namespace Cyclone.Wpf.Themes
{
    /// <summary>
    /// 主题 所有的主题资源字典的文件名称以 ThemeBrush.xaml 结尾
    /// </summary>
    public abstract class Theme : ResourceDictionary
    {
        protected Theme(string name, string uriString)
        {
            Name = name;
            var resource = new ResourceDictionary()
            {
                Source = new Uri(uriString),
            };

            MergedDictionaries.Add(resource);
        }

        public abstract string Name { get; set; }
    }
}