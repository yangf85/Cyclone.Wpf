using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Windows.Controls;

namespace Cyclone.Wpf.Themes;

/// <summary>
/// 主题管理 在xaml中使用，要在资源的最后加载，否则无效
/// </summary>
public class ThemeManager : ResourceDictionary
{
    public ThemeManager()
    {
        Initial();
    }

    private static ThemeManager _instance;

    private Theme _theme;

    public static ThemeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                throw new InvalidOperationException("The Resource is not loaded!");
            }
            return _instance;
        }
    }

    public Theme Theme
    {
        get => _theme;
        set => Switch(value);
    }

    private void Initial()
    {
        _instance = this;
        _theme = new BasicsTheme();

        MergedDictionaries.Add(_theme);
    }

    private void Switch(Theme theme)
    {
        _theme = theme;
        MergedDictionaries[0] = _theme;
    }
}