using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

[ContentProperty("Items")]
public class Stepper : ItemsControl
{
    static Stepper()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Stepper),
            new FrameworkPropertyMetadata(typeof(Stepper)));
    }

    #region CurrentStep

    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public static readonly DependencyProperty CurrentStepProperty =
        DependencyProperty.Register(nameof(CurrentStep), typeof(int), typeof(Stepper),
            new PropertyMetadata(0, OnCurrentStepChanged));

    private static void OnCurrentStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Stepper stepper)
        {
            stepper.UpdateStepperItems();
        }
    }

    #endregion CurrentStep

    #region Orientation

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Stepper),
            new PropertyMetadata(Orientation.Horizontal));

    #endregion Orientation

    #region StepSpacing

    public double StepSpacing
    {
        get => (double)GetValue(StepSpacingProperty);
        set => SetValue(StepSpacingProperty, value);
    }

    public static readonly DependencyProperty StepSpacingProperty =
        DependencyProperty.Register(nameof(StepSpacing), typeof(double), typeof(Stepper),
            new PropertyMetadata(40.0));

    #endregion StepSpacing

    #region ConnectorTemplate

    public DataTemplate ConnectorTemplate
    {
        get => (DataTemplate)GetValue(ConnectorTemplateProperty);
        set => SetValue(ConnectorTemplateProperty, value);
    }

    public static readonly DependencyProperty ConnectorTemplateProperty =
        DependencyProperty.Register(nameof(ConnectorTemplate), typeof(DataTemplate), typeof(Stepper),
            new PropertyMetadata(null));

    #endregion ConnectorTemplate

    #region ActiveConnectorTemplate

    public DataTemplate ActiveConnectorTemplate
    {
        get => (DataTemplate)GetValue(ActiveConnectorTemplateProperty);
        set => SetValue(ActiveConnectorTemplateProperty, value);
    }

    public static readonly DependencyProperty ActiveConnectorTemplateProperty =
        DependencyProperty.Register(nameof(ActiveConnectorTemplate), typeof(DataTemplate), typeof(Stepper),
            new PropertyMetadata(null));

    #endregion ActiveConnectorTemplate

    #region DefaultConnectorStyle

    public Style DefaultConnectorStyle
    {
        get => (Style)GetValue(DefaultConnectorStyleProperty);
        set => SetValue(DefaultConnectorStyleProperty, value);
    }

    public static readonly DependencyProperty DefaultConnectorStyleProperty =
        DependencyProperty.Register(nameof(DefaultConnectorStyle), typeof(Style), typeof(Stepper),
            new PropertyMetadata(null));

    #endregion DefaultConnectorStyle

    #region DefaultActiveConnectorStyle

    public Style DefaultActiveConnectorStyle
    {
        get => (Style)GetValue(DefaultActiveConnectorStyleProperty);
        set => SetValue(DefaultActiveConnectorStyleProperty, value);
    }

    public static readonly DependencyProperty DefaultActiveConnectorStyleProperty =
        DependencyProperty.Register(nameof(DefaultActiveConnectorStyle), typeof(Style), typeof(Stepper),
            new PropertyMetadata(null));

    #endregion DefaultActiveConnectorStyle

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateStepperItems();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateStepperItems();
    }

    // 构造函数
    public Stepper()
    {
        Loaded += (s, e) =>
        {
            UpdateStepperItems();
        };
    }

    // 更新所有步骤项的状态和连接器
    private void UpdateStepperItems()
    {
        int itemCount = Items.Count;
        if (itemCount == 0) return;

        // 更新步骤项状态
        for (int i = 0; i < itemCount; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is StepperItem stepperItem)
            {
                stepperItem.StepNumber = i + 1;
                stepperItem.IsFirstStep = (i == 0);
                stepperItem.IsLastStep = (i == itemCount - 1);

                // 设置状态
                if (i < CurrentStep)
                {
                    stepperItem.Status = StepStatus.Completed;
                }
                else if (i == CurrentStep)
                {
                    stepperItem.Status = StepStatus.Current;
                }
                else
                {
                    stepperItem.Status = StepStatus.Pending;
                }

                // 设置连接器可见性和状态
                if (i < itemCount - 1)
                {
                    stepperItem.HasConnector = true;
                    stepperItem.IsConnectorActive = (i < CurrentStep);
                }
                else
                {
                    stepperItem.HasConnector = false;
                }
            }
        }
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new StepperItem();
    }

    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is StepperItem;
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is StepperItem stepperItem)
        {
            stepperItem.ParentStepper = this;

            // 如果项目不是StepperItem，则设置Header
            if (!(item is StepperItem) && item != null)
            {
                stepperItem.Header = item.ToString();
            }
        }
    }

    protected override void ClearContainerForItemOverride(DependencyObject element, object item)
    {
        base.ClearContainerForItemOverride(element, item);

        if (element is StepperItem stepperItem)
        {
            stepperItem.ParentStepper = null;
        }
    }
}