using System;
using System.ComponentModel;
using System.Windows;

namespace Cyclone.Wpf.Themes.ThemeManagement;

/// <summary>
/// 主题 所有的主题资源字典的文件名称以 ThemeBrush.xaml 结尾
/// </summary>
public abstract class Theme : ResourceDictionary
{
    public abstract string Name { get; }
}