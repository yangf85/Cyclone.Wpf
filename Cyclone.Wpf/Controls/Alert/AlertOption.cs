using System.Windows;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public class AlertOption
{
    public double Width
    {
        get => field;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Width must be > 0");
            }
            field = value;
        }
    } = 400;

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
}