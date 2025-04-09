using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class AlertOption
{
    public double Width { get; set; } = 400;

    public double Height { get; set; } = 200;

    public AlertButton ButtonType { get; set; } = AlertButton.Ok;

    public string Title { get; set; } = "Alert";

    public object Icon { get; set; }

    public double CaptionHeight { get; set; } = SystemParameters.CaptionHeight;

    public Brush CaptionBackground { get; set; } = SystemColors.ActiveCaptionBrush;

    public Brush TitleForeground { get; set; }

    public Brush AlertButtonGroupBackground { get; set; }

    public double AlertButtonGroupHeight { get; set; }

    public string OkButtonText { get; set; } = "Ok";

    public string CancelButtonText { get; set; } = "Cancel";

    public bool IsShowMask { get; set; } = true;

    public Brush MaskBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));
}