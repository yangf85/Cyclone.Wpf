using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Shell;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

[TemplatePart(Name = PART_MinimizeButton, Type = typeof(Button))]
[TemplatePart(Name = PART_MaximizeButton, Type = typeof(Button))]
[TemplatePart(Name = PART_CloseButton, Type = typeof(Button))]
[TemplatePart(Name = PART_RestoreButton, Type = typeof(Button))]
[TemplatePart(Name = PART_TopmostButton, Type = typeof(ToggleButton))]
public class AdvancedWindow : System.Windows.Window
{
    static AdvancedWindow()
    {
        
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AdvancedWindow), new FrameworkPropertyMetadata(typeof(AdvancedWindow)));
    }

    const string PART_CloseButton = nameof(PART_CloseButton);

    const string PART_RestoreButton = nameof(PART_RestoreButton);

    const string PART_MaximizeButton = nameof(PART_MaximizeButton);

    const string PART_MinimizeButton = nameof(PART_MinimizeButton);

    const string PART_TopmostButton = nameof(PART_TopmostButton);

    Button _close;

    Button _restore;

    Button _maximize;

    Button _minimize;

    ToggleButton _topmost;

    

    #region Icon

    public new static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(AdvancedWindow), new PropertyMetadata(default(object)));

    public new object Icon
    {
        get => (object)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion Icon

  




    #region CaptionBackground

    public static readonly DependencyProperty CaptionBackgroundProperty =
        DependencyProperty.Register(nameof(CaptionBackground), typeof(Brush), typeof(AdvancedWindow), new PropertyMetadata(default(Brush)));

    public Brush CaptionBackground
    {
        get => (Brush)GetValue(CaptionBackgroundProperty);
        set => SetValue(CaptionBackgroundProperty, value);
    }

    #endregion CaptionBackground


    #region CaptionButtonType
    public CaptionButtonType CaptionButtonType
    {
        get => (CaptionButtonType)GetValue(CaptionButtonTypeProperty);
        set => SetValue(CaptionButtonTypeProperty, value);
    }

    public static readonly DependencyProperty CaptionButtonTypeProperty =
        DependencyProperty.Register(nameof(CaptionButtonType), typeof(CaptionButtonType), typeof(AdvancedWindow), new PropertyMetadata(default(CaptionButtonType)));

    #endregion
    #region TitleBrush
    public Brush TitleBrush
    {
        get => (Brush)GetValue(TitleBrushProperty);
        set => SetValue(TitleBrushProperty, value);
    }

    public static readonly DependencyProperty TitleBrushProperty =
        DependencyProperty.Register(nameof(TitleBrush), typeof(Brush), typeof(AdvancedWindow), new PropertyMetadata(default(Brush)));

    #endregion


    #region CaptionHeight
    public double CaptionHeight
    {
        get => (double)GetValue(CaptionHeightProperty);
        set => SetValue(CaptionHeightProperty, value);
    }

    public static readonly DependencyProperty CaptionHeightProperty =
        DependencyProperty.Register(nameof(CaptionHeight), typeof(double), typeof(AdvancedWindow), new PropertyMetadata(default(double)));

    #endregion



    #region FunctionalZone

    public static readonly DependencyProperty FunctionalZoneProperty =
        DependencyProperty.Register(nameof(FunctionalZone), typeof(object), typeof(AdvancedWindow), new PropertyMetadata(default(object)));

    public object FunctionalZone
    {
        get => (object)GetValue(FunctionalZoneProperty);
        set => SetValue(FunctionalZoneProperty, value);
    }

    #endregion FunctionalZone



    #region Override


    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        CommandBindings.Add(new CommandBinding(CaptionButtonCommand.CloseCommand, CaptionButtonCommand.OnClose, CaptionButtonCommand.OnCanClose));
        CommandBindings.Add(new CommandBinding(CaptionButtonCommand.MaximizeCommand, CaptionButtonCommand.OnMaximize, CaptionButtonCommand.OnCanMaximize));
        CommandBindings.Add(new CommandBinding(CaptionButtonCommand.RestoreCommand, CaptionButtonCommand.OnRestore, CaptionButtonCommand.OnCanRestore));
        CommandBindings.Add(new CommandBinding(CaptionButtonCommand.MinimizeCommand, CaptionButtonCommand.OnMinimize, CaptionButtonCommand.OnCanMinimize));
        CommandBindings.Add(new CommandBinding(CaptionButtonCommand.TopmostCommand, CaptionButtonCommand.OnTopmost, CaptionButtonCommand.OnCanTopmost));

    }
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        
     
    }
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        //解决WindowChrome样式下，设置SizeToContent="WidthAndHeight"以后出现的黑色区域
        if (SizeToContent == SizeToContent.WidthAndHeight && WindowChrome.GetWindowChrome(this) != null)
        {
            InvalidateMeasure();
        }

    }

    #endregion Override
}