using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Cyclone.Wpf.Themes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cyclone.Wpf.Demo.ViewModels;

public partial class ShellWindowViewModel : ObservableObject
{
    [ObservableProperty]
    public partial bool IsClosing { get; set; }

    [RelayCommand]
    private void SwitchTheme(string themeName)
    {
        ThemeManager.CurrentTheme = ThemeManager.AvailableThemes.FirstOrDefault(x => x.Name.StartsWith(themeName));
    }

    partial void OnIsClosingChanged(bool value)
    {
        if (value)
        {
            Debug.WriteLine("Closing application");
        }
    }
}