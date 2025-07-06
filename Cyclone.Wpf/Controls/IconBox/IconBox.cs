// IconBox.cs
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 一个简单的图标控件，可以同时支持路径、图片和字体图标
/// </summary>
public class IconBox : ContentControl
{
    static IconBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(IconBox),
            new FrameworkPropertyMetadata(typeof(IconBox)));
    }
}