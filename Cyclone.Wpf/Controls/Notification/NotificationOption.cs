﻿namespace Cyclone.Wpf.Controls;

public class NotificationOption
{
    /// <summary>
    /// 显示持续时间
    /// </summary>
    public TimeSpan DisplayDuration { get; set; } = TimeSpan.FromMilliseconds(2400);

    /// <summary>
    /// 位置
    /// </summary>
    public NotificationPosition Position { get; set; } = NotificationPosition.BottomRight;

    /// <summary>
    /// X轴偏移量
    /// </summary>
    public double OffsetX { get; set; } = 5;

    /// <summary>
    /// Y轴偏移量
    /// </summary>
    public double OffsetY { get; set; } = 5;

    /// <summary>
    /// 通知之间的间隙
    /// </summary>
    public double Spacing { get; set; } = 5;

    /// <summary>
    /// 最多显示多少个通知
    /// </summary>
    public int MaxCount { get; set; } = 5;

    /// <summary>
    /// 通知宽度
    /// </summary>
    public double Width { get; set; } = 240;

    /// <summary>
    /// 最大高度
    /// </summary>
    public double Height { get; set; } = 75;

    /// <summary>
    /// 是否显示关闭按钮
    /// </summary>
    public bool IsShowCloseButton { get; set; } = true;
}