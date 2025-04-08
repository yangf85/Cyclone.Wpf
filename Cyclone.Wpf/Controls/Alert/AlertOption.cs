namespace Cyclone.Wpf.Controls;

public class AlertOption
{
    /// <summary>
    /// 警告窗口宽度
    /// </summary>
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

    /// <summary>
    /// 警告窗口高度
    /// </summary>
    public double Height { get; set; } = 200;

    /// <summary>
    /// 要显示的按钮类型
    /// </summary>
    public AlertButton ButtonType { get; set; } = AlertButton.Ok;

    /// <summary>
    /// 窗口标题
    /// </summary>
    public string Title { get; set; } = "Alert";

    /// <summary>
    /// 自定义确定按钮文本
    /// </summary>
    public string OkButtonText { get; set; } = "Ok";

    /// <summary>
    /// 自定义取消按钮文本
    /// </summary>
    public string CancelButtonText { get; set; } = "Cancel";
}