using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Themes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Demo.ViewModels;

public partial class ShellWindowViewModel : ObservableObject
{
    [RelayCommand]
    private void SwitchTheme(string themeName)
    {
        ThemeManager.CurrentTheme = ThemeManager.AvailableThemes.FirstOrDefault(x => x.Name.StartsWith(themeName));
    }
}