using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

public class AlertOption
{
    public double Width { get; set; } = 400;

    public double Height { get; set; } = 240;

    public AlertButton ButtonType { get; set; } = AlertButton.Yes;

    public string Title { get; set; } = "Alert";

    public object Icon { get; set; } = new Path()
    {
        Stretch = Stretch.Uniform,
        Width = 18,
        Height = 18,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
        Fill = new SolidColorBrush(Color.FromArgb(0XFF, 0XFF, 0XFF, 0XFF)),
        Data = PathGeometry.Parse(@"M853.333333 384V213.333333H170.666667v170.666667h682.666666z m0 85.333333H170.666667v341.333334h682.666666v-341.333334zM128 128h768a42.666667 42.666667 0 0 1 42.666667 42.666667v682.666666a42.666667 42.666667 0 0 1-42.666667 42.666667H128a42.666667 42.666667 0 0 1-42.666667-42.666667V170.666667a42.666667 42.666667 0 0 1 42.666667-42.666667z m85.333333 384h128v213.333333H213.333333v-213.333333z m0-256h85.333334v85.333333H213.333333V256z m170.666667 0h85.333333v85.333333H384V256z")
    };

    public double CaptionHeight { get; set; } = 32d;

    public Brush CaptionBackground { get; set; } = new SolidColorBrush(Color.FromArgb(0XFF, 0X0D, 0X47, 0XA1));

    public Brush TitleForeground { get; set; } = new SolidColorBrush(Color.FromArgb(0XFF, 0XFF, 0XFF, 0XFF));

    public Brush AlertButtonGroupBackground { get; set; } = new SolidColorBrush(Color.FromArgb(0x10, 0x80, 0x80, 0x80));

    public Brush ContentForeground { get; set; } = Brushes.DarkGray;

    public Brush AlertIconForeground { get; set; } = Brushes.DarkGray;

    public double AlertButtonGroupHeight { get; set; } = 56d;

    public string OkButtonText { get; set; } = "Ok";

    public string CancelButtonText { get; set; } = "Cancel";

    public bool IsShowMask { get; set; } = true;

    public Brush MaskBrush { get; set; } = new SolidColorBrush(Color.FromArgb(0x80, 0x00, 0x00, 0x00));

    public HorizontalAlignment AlertButtonHorizontalAlignment { get; set; } = HorizontalAlignment.Center;

    public AlertIcon AlertIcon { get; set; }
}