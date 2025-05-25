using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 加载遮罩控件 - 在内容上显示加载动画
/// </summary>
[TemplatePart(Name = "PART_Mask", Type = typeof(Rectangle))]
[TemplatePart(Name = "PART_LoadingPresenter", Type = typeof(ContentPresenter))]
public class LoadingBox : ContentControl
{
    static LoadingBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingBox),
            new FrameworkPropertyMetadata(typeof(LoadingBox)));
    }

    #region IsLoading

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingBox),
            new PropertyMetadata(false));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    #endregion IsLoading

    #region LoadingContent

    public static readonly DependencyProperty LoadingContentProperty =
        DependencyProperty.Register(nameof(LoadingContent), typeof(ILoadingIndicator), typeof(LoadingBox),
            new PropertyMetadata(null, OnLoadingContentChanged));

    public ILoadingIndicator LoadingContent
    {
        get => (ILoadingIndicator)GetValue(LoadingContentProperty);
        set => SetValue(LoadingContentProperty, value);
    }

    private static void OnLoadingContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var loadingBox = (LoadingBox)d;

        // 移除旧的绑定
        if (e.OldValue is LoadingIndicator oldIndicator)
        {
            BindingOperations.ClearBinding(oldIndicator, LoadingIndicator.IsActiveProperty);
        }

        // 建立新的绑定
        if (e.NewValue is LoadingIndicator newIndicator)
        {
            var binding = new Binding(nameof(IsLoading))
            {
                Source = loadingBox,
                Mode = BindingMode.OneWay
            };

            BindingOperations.SetBinding(newIndicator, LoadingIndicator.IsActiveProperty, binding);
        }
    }

    #endregion LoadingContent

    #region MaskBackground

    public static readonly DependencyProperty MaskBackgroundProperty =
        DependencyProperty.Register(nameof(MaskBackground), typeof(Brush), typeof(LoadingBox),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 0, 0, 0))));

    public Brush MaskBackground
    {
        get => (Brush)GetValue(MaskBackgroundProperty);
        set => SetValue(MaskBackgroundProperty, value);
    }

    #endregion MaskBackground

    private Rectangle _maskRectangle;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _maskRectangle = GetTemplateChild("PART_Mask") as Rectangle;
    }
}