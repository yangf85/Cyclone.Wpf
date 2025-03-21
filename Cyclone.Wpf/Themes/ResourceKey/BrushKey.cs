using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Cyclone.Wpf.Themes;

using System.Windows;

public class BrushResourceKey
{
    // 基础
    public static ComponentResourceKey DefaultBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DefaultBorderBrush));

    public static ComponentResourceKey DefaultBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DefaultBackgroundBrush));

    public static ComponentResourceKey DefaultForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DefaultForegroundBrush));

    // 悬停
    public static ComponentResourceKey HoverBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HoverBackgroundBrush));

    public static ComponentResourceKey HoverBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HoverBorderBrush));

    public static ComponentResourceKey HoverForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HoverForegroundBrush));

    // 按压
    public static ComponentResourceKey PressedBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(PressedBackgroundBrush));

    public static ComponentResourceKey PressedBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(PressedBorderBrush));

    public static ComponentResourceKey PressedForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(PressedForegroundBrush));

    // 布尔
    public static ComponentResourceKey CheckedBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(CheckedBackgroundBrush));

    public static ComponentResourceKey CheckedBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(CheckedBorderBrush));

    public static ComponentResourceKey CheckedForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(CheckedForegroundBrush));

    // 编辑
    public static ComponentResourceKey EditingBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(EditingBackgroundBrush));

    public static ComponentResourceKey EditingBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(EditingBorderBrush));

    public static ComponentResourceKey EditingForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(EditingForegroundBrush));

    // 聚焦
    public static ComponentResourceKey FocusedBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(FocusedBackgroundBrush));

    public static ComponentResourceKey FocusedBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(FocusedBorderBrush));

    public static ComponentResourceKey FocusedForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(FocusedForegroundBrush));

    // 高亮
    public static ComponentResourceKey HighlightedSelectedBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HighlightedSelectedBackgroundBrush));

    public static ComponentResourceKey HighlightedSelectedForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HighlightedSelectedForegroundBrush));

    public static ComponentResourceKey HighlightedSelectedBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HighlightedSelectedBorderBrush));

    // 加载
    public static ComponentResourceKey LoadingBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(LoadingBackgroundBrush));

    public static ComponentResourceKey LoadingForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(LoadingForegroundBrush));

    public static ComponentResourceKey LoadingBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(LoadingBorderBrush));

    // 拖拽
    public static ComponentResourceKey DraggingBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DraggingBackgroundBrush));

    public static ComponentResourceKey DraggingForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DraggingForegroundBrush));

    public static ComponentResourceKey DraggingBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DraggingBorderBrush));

    // 输入错误
    public static ComponentResourceKey InputErrorBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(InputErrorBackgroundBrush));

    public static ComponentResourceKey InputErrorForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(InputErrorForegroundBrush));

    public static ComponentResourceKey InputErrorBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(InputErrorBorderBrush));

    // 选中元素
    public static ComponentResourceKey SelectedBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(SelectedBackgroundBrush));

    public static ComponentResourceKey SelectedBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(SelectedBorderBrush));

    public static ComponentResourceKey SelectedForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(SelectedForegroundBrush));

    // 警告
    public static ComponentResourceKey WarningBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(WarningBackgroundBrush));

    public static ComponentResourceKey WarningBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(WarningBorderBrush));

    public static ComponentResourceKey WarningForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(WarningForegroundBrush));

    // 错误
    public static ComponentResourceKey ErrorBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(ErrorBackgroundBrush));

    public static ComponentResourceKey ErrorBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(ErrorBorderBrush));

    public static ComponentResourceKey ErrorForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(ErrorForegroundBrush));

    // 成功
    public static ComponentResourceKey SuccessBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(SuccessBackgroundBrush));

    public static ComponentResourceKey SuccessBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(SuccessBorderBrush));

    public static ComponentResourceKey SuccessForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(SuccessForegroundBrush));

    // 信息
    public static ComponentResourceKey InfoBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(InfoBackgroundBrush));

    public static ComponentResourceKey InfoBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(InfoBorderBrush));

    public static ComponentResourceKey InfoForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(InfoForegroundBrush));

    // 禁用
    public static ComponentResourceKey DisabledBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DisabledBackgroundBrush));

    public static ComponentResourceKey DisabledBorderBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DisabledBorderBrush));

    public static ComponentResourceKey DisabledForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(DisabledForegroundBrush));

    // 图标
    public static ComponentResourceKey IconForegroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(IconForegroundBrush));

    public static ComponentResourceKey IconBackgroundBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(IconBackgroundBrush));

    // 阴影和高光
    public static ComponentResourceKey ShadowBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(ShadowBrush));

    public static ComponentResourceKey HighlightBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HighlightBrush));

    public static ComponentResourceKey HighlightInvertBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(HighlightInvertBrush));

    // 强调颜色
    public static ComponentResourceKey AccentBrush { get; } =
        new ComponentResourceKey(typeof(BrushResourceKey), nameof(AccentBrush));
}