using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = nameof(PART_NavigationButton), Type = typeof(Button))]
[TemplatePart(Name = nameof(PART_DisplayTextBlock), Type = typeof(TextBlock))]
public class HyperlinkButton : Control
{
    private const string PART_DisplayTextBlock = nameof(PART_DisplayTextBlock);
    private const string PART_NavigationButton = nameof(PART_NavigationButton);

    static HyperlinkButton()
    {
        HorizontalAlignmentProperty.OverrideMetadata(typeof(HyperlinkButton), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
        CommandManager.RegisterClassCommandBinding(typeof(HyperlinkButton),
            new CommandBinding(OpenUrlCommand, OnExecuteOpenUrlCommand, OnCanEexcuteOpenUrlCommand));
    }

    #region NavigateUri

    public static readonly DependencyProperty NavigateUriProperty =
        DependencyProperty.Register(nameof(NavigateUri), typeof(Uri), typeof(HyperlinkButton), new PropertyMetadata(default(Uri)));

    public Uri NavigateUri
    {
        get => (Uri)GetValue(NavigateUriProperty);
        set => SetValue(NavigateUriProperty, value);
    }

    #endregion NavigateUri

    #region DisplayText

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(HyperlinkButton), new PropertyMetadata(default(string)));

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    #endregion DisplayText

    #region OpenUrlCommand

    public static RoutedCommand OpenUrlCommand { get; private set; } = new RoutedCommand("OpenUrl", typeof(Hyperlink));

    private static void OnCanEexcuteOpenUrlCommand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = true;
    }

    private static void OnExecuteOpenUrlCommand(object sender, ExecutedRoutedEventArgs e)
    {
        var hyperLink = (HyperlinkButton)sender;

        try
        {
            string uri = hyperLink.NavigateUri.AbsoluteUri;
            Process.Start(new ProcessStartInfo
            {
                FileName = uri,
                UseShellExecute = true
            });

        }
        catch (Win32Exception)
        {
        }
    }

    #endregion OpenUrlCommand
}