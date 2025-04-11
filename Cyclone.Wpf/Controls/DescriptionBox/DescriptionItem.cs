// DescriptionItem.cs
using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// DescriptionItem - 显示标签、值和描述的项目控件
/// 用于在 DescriptionBox 中显示信息项
/// </summary>
public class DescriptionItem : ContentControl
{
    static DescriptionItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DescriptionItem),
            new FrameworkPropertyMetadata(typeof(DescriptionItem)));
    }

    #region LabelSharedSizeGroup

    public string LabelSharedSizeGroup
    {
        get => (string)GetValue(LabelSharedSizeGroupProperty);
        set => SetValue(LabelSharedSizeGroupProperty, value);
    }

    public static readonly DependencyProperty LabelSharedSizeGroupProperty =
        DependencyProperty.Register(nameof(LabelSharedSizeGroup), typeof(string), typeof(DescriptionItem), new PropertyMetadata(default(string)));

    #endregion LabelSharedSizeGroup

    #region DescriptionSharedSizeGroup

    public string DescriptionSharedSizeGroup
    {
        get => (string)GetValue(DescriptionSharedSizeGroupProperty);
        set => SetValue(DescriptionSharedSizeGroupProperty, value);
    }

    public static readonly DependencyProperty DescriptionSharedSizeGroupProperty =
        DependencyProperty.Register(nameof(DescriptionSharedSizeGroup), typeof(string), typeof(DescriptionItem), new PropertyMetadata(default(string)));

    #endregion DescriptionSharedSizeGroup

    #region Label

    public string Label
    {
        get { return (string)GetValue(LabelProperty); }
        set { SetValue(LabelProperty, value); }
    }

    public static readonly DependencyProperty LabelProperty =
       DependencyProperty.Register("Label", typeof(string), typeof(DescriptionItem),
           new PropertyMetadata(string.Empty));

    #endregion Label

    #region Value

    public static readonly DependencyProperty ValueProperty =
     DependencyProperty.Register("Value", typeof(object), typeof(DescriptionItem),
         new PropertyMetadata(null));

    public object Value
    {
        get { return GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    #endregion Value

    #region Description

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(DescriptionItem), new PropertyMetadata(default(string)));

    #endregion Description

    #region Column

    public int Column
    {
        get => (int)GetValue(ColumnProperty);
        set => SetValue(ColumnProperty, value);
    }

    public static readonly DependencyProperty ColumnProperty =
        DependencyProperty.Register(nameof(Column), typeof(int), typeof(DescriptionItem), new PropertyMetadata(default(int)));

    #endregion Column

    #region Row

    public int Row
    {
        get => (int)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    public static readonly DependencyProperty RowProperty =
        DependencyProperty.Register(nameof(Row), typeof(int), typeof(DescriptionItem), new PropertyMetadata(default(int)));

    #endregion Row

    #region ColumnSpan

    public int ColumnSpan
    {
        get => (int)GetValue(ColumnSpanProperty);
        set => SetValue(ColumnSpanProperty, value);
    }

    public static readonly DependencyProperty ColumnSpanProperty =
        DependencyProperty.Register(nameof(ColumnSpan), typeof(int), typeof(DescriptionItem), new PropertyMetadata(default(int)));

    #endregion ColumnSpan

    #region RowSpan

    public int RowSpan
    {
        get => (int)GetValue(RowSpanProperty);
        set => SetValue(RowSpanProperty, value);
    }

    public static readonly DependencyProperty RowSpanProperty =
        DependencyProperty.Register(nameof(RowSpan), typeof(int), typeof(DescriptionItem), new PropertyMetadata(default(int)));

    #endregion RowSpan
}