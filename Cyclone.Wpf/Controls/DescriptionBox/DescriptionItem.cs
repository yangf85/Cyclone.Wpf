// DescriptionBox.cs
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// DescriptionItem - 显示标签、值和描述的项目
/// </summary>
public class DescriptionItem : ContentControl
{
    static DescriptionItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DescriptionItem),
            new FrameworkPropertyMetadata(typeof(DescriptionItem)));
    }

    public DescriptionItem()
    {
        // 设置默认的行列设置
        Grid.SetRow(this, 0);
        Grid.SetColumn(this, 0);
        Grid.SetRowSpan(this, 1);
        Grid.SetColumnSpan(this, 1);
    }

    #region 依赖属性

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register("Label", typeof(string), typeof(DescriptionItem),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(object), typeof(DescriptionItem),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register("Description", typeof(string), typeof(DescriptionItem),
            new PropertyMetadata(string.Empty));

    #endregion 依赖属性

    #region 属性

    /// <summary>
    /// 标签文本
    /// </summary>
    public string Label
    {
        get { return (string)GetValue(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }

    /// <summary>
    /// 值对象
    /// </summary>
    public object Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    /// <summary>
    /// 描述文本
    /// </summary>
    public string Description
    {
        get { return (string)GetValue(DescriptionProperty); }
        set { SetValue(DescriptionProperty, value); }
    }

    #endregion 属性
}