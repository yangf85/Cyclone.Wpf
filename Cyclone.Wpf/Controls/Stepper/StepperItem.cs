using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

public enum StepStatus
{
    Completed,
    Current,
    Pending
}

public class StepperItem : ContentControl
{
    static StepperItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepperItem),
            new FrameworkPropertyMetadata(typeof(StepperItem)));
    }

    #region Header

    public object Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion Header

    #region Description

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(StepperItem),
            new PropertyMetadata(string.Empty));

    #endregion Description

    #region Status

    public StepStatus Status
    {
        get => (StepStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(StepStatus), typeof(StepperItem),
            new PropertyMetadata(StepStatus.Pending));

    #endregion Status

    #region StepNumber

    public int StepNumber
    {
        get => (int)GetValue(StepNumberProperty);
        set => SetValue(StepNumberProperty, value);
    }

    public static readonly DependencyProperty StepNumberProperty =
        DependencyProperty.Register(nameof(StepNumber), typeof(int), typeof(StepperItem),
            new PropertyMetadata(0));

    #endregion StepNumber

    #region IsFirstStep

    public bool IsFirstStep
    {
        get => (bool)GetValue(IsFirstStepProperty);
        set => SetValue(IsFirstStepProperty, value);
    }

    public static readonly DependencyProperty IsFirstStepProperty =
        DependencyProperty.Register(nameof(IsFirstStep), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(false));

    #endregion IsFirstStep

    #region IsLastStep

    public bool IsLastStep
    {
        get => (bool)GetValue(IsLastStepProperty);
        set => SetValue(IsLastStepProperty, value);
    }

    public static readonly DependencyProperty IsLastStepProperty =
        DependencyProperty.Register(nameof(IsLastStep), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(false));

    #endregion IsLastStep

    #region StepIcon

    public object StepIcon
    {
        get => GetValue(StepIconProperty);
        set => SetValue(StepIconProperty, value);
    }

    public static readonly DependencyProperty StepIconProperty =
        DependencyProperty.Register(nameof(StepIcon), typeof(object), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion StepIcon

    #region CompletedIcon

    public object CompletedIcon
    {
        get => GetValue(CompletedIconProperty);
        set => SetValue(CompletedIconProperty, value);
    }

    public static readonly DependencyProperty CompletedIconProperty =
        DependencyProperty.Register(nameof(CompletedIcon), typeof(object), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion CompletedIcon

    #region ErrorIcon

    public object ErrorIcon
    {
        get => GetValue(ErrorIconProperty);
        set => SetValue(ErrorIconProperty, value);
    }

    public static readonly DependencyProperty ErrorIconProperty =
        DependencyProperty.Register(nameof(ErrorIcon), typeof(object), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion ErrorIcon

    #region IconBackground

    public Brush IconBackground
    {
        get => (Brush)GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    public static readonly DependencyProperty IconBackgroundProperty =
        DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion IconBackground

    #region CompletedIconBackground

    public Brush CompletedIconBackground
    {
        get => (Brush)GetValue(CompletedIconBackgroundProperty);
        set => SetValue(CompletedIconBackgroundProperty, value);
    }

    public static readonly DependencyProperty CompletedIconBackgroundProperty =
        DependencyProperty.Register(nameof(CompletedIconBackground), typeof(Brush), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion CompletedIconBackground

    #region CurrentIconBackground

    public Brush CurrentIconBackground
    {
        get => (Brush)GetValue(CurrentIconBackgroundProperty);
        set => SetValue(CurrentIconBackgroundProperty, value);
    }

    public static readonly DependencyProperty CurrentIconBackgroundProperty =
        DependencyProperty.Register(nameof(CurrentIconBackground), typeof(Brush), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion CurrentIconBackground

    #region IconForeground

    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    public static readonly DependencyProperty IconForegroundProperty =
        DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion IconForeground

    #region CompletedIconForeground

    public Brush CompletedIconForeground
    {
        get => (Brush)GetValue(CompletedIconForegroundProperty);
        set => SetValue(CompletedIconForegroundProperty, value);
    }

    public static readonly DependencyProperty CompletedIconForegroundProperty =
        DependencyProperty.Register(nameof(CompletedIconForeground), typeof(Brush), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion CompletedIconForeground

    #region CurrentIconForeground

    public Brush CurrentIconForeground
    {
        get => (Brush)GetValue(CurrentIconForegroundProperty);
        set => SetValue(CurrentIconForegroundProperty, value);
    }

    public static readonly DependencyProperty CurrentIconForegroundProperty =
        DependencyProperty.Register(nameof(CurrentIconForeground), typeof(Brush), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion CurrentIconForeground

    #region HasError

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public static readonly DependencyProperty HasErrorProperty =
        DependencyProperty.Register(nameof(HasError), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(false));

    #endregion HasError

    #region ParentStepper

    internal Stepper ParentStepper
    {
        get => (Stepper)GetValue(ParentStepperProperty);
        set => SetValue(ParentStepperProperty, value);
    }

    internal static readonly DependencyProperty ParentStepperProperty =
        DependencyProperty.Register(nameof(ParentStepper), typeof(Stepper), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion ParentStepper

    #region HasConnector

    public bool HasConnector
    {
        get => (bool)GetValue(HasConnectorProperty);
        set => SetValue(HasConnectorProperty, value);
    }

    public static readonly DependencyProperty HasConnectorProperty =
        DependencyProperty.Register(nameof(HasConnector), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(false));

    #endregion HasConnector

    #region IsConnectorActive

    public bool IsConnectorActive
    {
        get => (bool)GetValue(IsConnectorActiveProperty);
        set => SetValue(IsConnectorActiveProperty, value);
    }

    public static readonly DependencyProperty IsConnectorActiveProperty =
        DependencyProperty.Register(nameof(IsConnectorActive), typeof(bool), typeof(StepperItem),
            new PropertyMetadata(false));

    #endregion IsConnectorActive

    #region ConnectorContent

    public object ConnectorContent
    {
        get => GetValue(ConnectorContentProperty);
        set => SetValue(ConnectorContentProperty, value);
    }

    public static readonly DependencyProperty ConnectorContentProperty =
        DependencyProperty.Register(nameof(ConnectorContent), typeof(object), typeof(StepperItem),
            new PropertyMetadata(null));

    #endregion ConnectorContent
}