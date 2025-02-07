using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Cyclone.Wpf.Controls
{
    [TemplatePart(Name = PART_InputTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PART_ValueSlider, Type = typeof(Slider))]
    [TemplatePart(Name = PART_ValueSliderPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PART_IncreaseRepeatButton, Type = typeof(RepeatButton))]
    [TemplatePart(Name = PART_DecreaseRepeatButton, Type = typeof(RepeatButton))]
    public class NumberBox : Control
    {
        private const string PART_IncreaseRepeatButton = nameof(PART_IncreaseRepeatButton);

        private const string PART_InputTextBox = nameof(PART_InputTextBox);

        private const string PART_DecreaseRepeatButton = nameof(PART_DecreaseRepeatButton);

        private const string PART_ValueSlider = nameof(PART_ValueSlider);

        private const string PART_ValueSliderPopup = nameof(PART_ValueSliderPopup);

        private RepeatButton _additionRepeatButton;

        private TextBox _inputTextBox;

        private RepeatButton _subtractionRepeatButton;

        private Slider _valueSlider;

        private Popup _valueSliderPopup;

        static NumberBox()
        {
            InitializeCommand();
        }

        public NumberBox()
        {
        }

        #region Value

        public static readonly DependencyProperty ValueProperty =
           RangeBase.ValueProperty.AddOwner(typeof(NumberBox), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnCoerceValue));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static object OnCoerceValue(DependencyObject d, object baseValue)
        {
            var num = (double)baseValue;
            var box = (NumberBox)d;
            if (num < box.Minimum)
            {
                num = box.Minimum;
            }
            else if (num > box.Maximum)
            {
                num = box.Maximum;
            }
            return num;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (NumberBox)d;
            var textBox = box.GetTemplateChild(PART_InputTextBox) as TextBox;
            if (textBox != null)
            {
                textBox.Text = box.Value.ToString();
            }
        }

        #endregion Value

        #region Step

        public static readonly DependencyProperty StepProperty =
                            DependencyProperty.Register(nameof(Step), typeof(double), typeof(NumberBox), new PropertyMetadata(1d, OnStepChanged, OnCoerceStep));

        public double Step
        {
            get => (double)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        private static object OnCoerceStep(DependencyObject d, object baseValue)
        {
            var num = (double)baseValue;
            var box = (NumberBox)d;

            if (box.NumberStyle.HasFlag(NumberStyles.Integer))
            {
                num = Math.Ceiling(num);
            }

            if (num > box.Maximum)
            {
                num = box.Maximum;
            }
            return num;
        }

        private static void OnStepChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion Step

        #region DecimalPlaces

        public static readonly DependencyProperty DecimalPlacesProperty =
                            DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(NumberBox), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDecimalPlacesChanged, CoerceDecimalPlaces));

        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        private static object CoerceDecimalPlaces(DependencyObject d, object baseValue)
        {
            if (baseValue is int digit)
            {
                return Math.Max(digit, 0);
            }
            return baseValue;
        }

        private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = d as NumberBox;
            box.Value = Math.Round(box.Value, (int)e.NewValue);
        }

        #endregion DecimalPlaces

        #region Minimum

        public static readonly DependencyProperty MinimumProperty =
            RangeBase.MinimumProperty.AddOwner(typeof(NumberBox), new PropertyMetadata(double.MinValue, null, CoerceMinimum));

        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        private static object CoerceMinimum(DependencyObject d, object baseValue)
        {
            NumberBox box = (NumberBox)d;
            double max = box.Maximum;
            double min = (double)baseValue;
            if (min > max)
            {
                return max;
            }
            return min;
        }

        #endregion Minimum

        #region Maximum

        public static readonly DependencyProperty MaximumProperty =
            RangeBase.MaximumProperty.AddOwner(typeof(NumberBox), new PropertyMetadata(double.MaxValue, null, CoerceMaximum));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        private static object CoerceMaximum(DependencyObject d, object baseValue)
        {
            NumberBox box = (NumberBox)d;
            double min = box.Minimum;
            double max = (double)baseValue;
            if (max < min)
            {
                return min;
            }
            return max;
        }

        #endregion Maximum

        #region ValueChanged

        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumberBox));

        public event RoutedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        #endregion ValueChanged

        #region IsVisibleSlider

        public static readonly DependencyProperty IsVisibleSliderProperty =
            DependencyProperty.Register(nameof(IsVisibleSlider), typeof(bool), typeof(NumberBox), new PropertyMetadata(false));

        public bool IsVisibleSlider
        {
            get => (bool)GetValue(IsVisibleSliderProperty);
            set => SetValue(IsVisibleSliderProperty, value);
        }

        #endregion IsVisibleSlider

        #region IsVisibleButton

        public static readonly DependencyProperty IsVisibleButtonProperty =
            DependencyProperty.Register(nameof(IsVisibleButton), typeof(bool), typeof(NumberBox), new PropertyMetadata(true));

        public bool IsVisibleButton
        {
            get => (bool)GetValue(IsVisibleButtonProperty);
            set => SetValue(IsVisibleButtonProperty, value);
        }

        #endregion IsVisibleButton

        #region ButtonOrientation

        public static readonly DependencyProperty ButtonOrientationProperty =
            DependencyProperty.Register(nameof(ButtonOrientation), typeof(Orientation), typeof(NumberBox), new PropertyMetadata(Orientation.Vertical));

        public Orientation ButtonOrientation
        {
            get => (Orientation)GetValue(ButtonOrientationProperty);
            set => SetValue(ButtonOrientationProperty, value);
        }

        #endregion ButtonOrientation

        #region NumberStyle

        public static readonly DependencyProperty NumberStyleProperty =
            DependencyProperty.Register(nameof(NumberStyle), typeof(NumberStyles), typeof(NumberBox), new PropertyMetadata(NumberStyles.Float));

        public NumberStyles NumberStyle
        {
            get => (NumberStyles)GetValue(NumberStyleProperty);
            set => SetValue(NumberStyleProperty, value);
        }

        #endregion NumberStyle

        #region Command

        #region Increase
        public static RoutedCommand IncreaseCommand { get; private set; }
        private static void OnIncreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            numberBox.Value += numberBox.Step;
        }
        private static void OnCanIncreaseCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            e.CanExecute = numberBox.Value <= numberBox.Maximum;
        }
        #endregion
        #region Decrease
        public static RoutedCommand DecreaseCommand { get; private set; }
        private static void OnCanDecreaseCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            e.CanExecute = numberBox.Value >= numberBox.Minimum;
        }

        private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            numberBox.Value -= numberBox.Step;
        }
        #endregion




        private static void InitializeCommand()
        {
            IncreaseCommand = new RoutedCommand("Increase", typeof(NumberBox));
            DecreaseCommand = new RoutedCommand("Decrease", typeof(NumberBox));
            CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
                new CommandBinding(IncreaseCommand, OnIncreaseCommand, OnCanIncreaseCommand));
            CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
                new CommandBinding(DecreaseCommand, OnDecreaseCommand, OnCanDecreaseCommand));
        }

      

      

       

        #endregion Command

        #region Override

        private string _inputText = "";

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _inputTextBox = GetTemplateChild(PART_InputTextBox) as TextBox;
            if (_inputTextBox != null)
            {
                _inputTextBox.Text = Value.ToString();
                _inputTextBox.PreviewTextInput -= InputTextBox_PreviewTextInput;
                _inputTextBox.PreviewTextInput += InputTextBox_PreviewTextInput;
                _inputTextBox.MouseWheel -= InputTextBox_MouseWheel;
                _inputTextBox.MouseWheel += InputTextBox_MouseWheel;
                _inputTextBox.TextChanged -= InputTextBox_TextChanged;
                _inputTextBox.TextChanged += InputTextBox_TextChanged;
            }
        }

        private void InputTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 判断滚动方向，向上则增加值，向下则减少值
            if (e.Delta > 0)
            {
                Value += Step;
            }
            else
            {
                Value -= Step;
            }
            // 将光标放置在输入框的末尾
            _inputTextBox.CaretIndex = _inputTextBox.Text.Length + 1;
        }

        // 此方法预览输入文本事件
        private void InputTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 获取选定文本
            var selectedText = _inputTextBox.SelectedText;
            // 获取全部文本
            var fullText = _inputTextBox.Text;

            // 移除选定的文本
            var textWithoutSelection = fullText.Remove(_inputTextBox.SelectionStart, selectedText.Length);

            // 在当前光标位置插入新输入的文本
            var newText = textWithoutSelection.Insert(_inputTextBox.CaretIndex, e.Text);

            // 检查新文本是否为有效的数字输入
            var flag = IsValidNumericInput(newText);

            // 如果不是有效的数字输入，则阻止输入
            e.Handled = !flag;
        }

        // 当输入框文本发生改变时，此方法会被调用
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 获取输入框的文本
            var text = _inputTextBox.Text;

            // 尝试将文本转换为双精度浮点数
            if (double.TryParse(text, NumberStyle, CultureInfo.InvariantCulture, out var value))
            {
                // 对值进行四舍五入，保留10位小数
                value = Math.Round(value, 10);
                // 如果值小于最小值，则将输入框的文本设置为最小值
                if (value < Minimum)
                {
                    _inputTextBox.Text = Minimum.ToString(CultureInfo.InvariantCulture);
                }
                // 如果值大于最大值，则将输入框的文本设置为最大值
                else if (value > Maximum)
                {
                    _inputTextBox.Text = Maximum.ToString(CultureInfo.InvariantCulture);
                }
                // 更新当前的值
                Value = value;
            }
            else
            {
                // 如果无法转换为双精度浮点数，则将输入框的文本设置为"0"
                _inputTextBox.Text = "0";
                Value = 0d;
            }
        }

        // 检查给定的文本是否是有效的数字输入
        private bool IsValidNumericInput(string text)
        {
            // 如果文本包含"."
            if (text.Contains('.'))
            {
                // 获取"."的位置
                var index = text.IndexOf('.');
                // 判断小数点后的位数是否小于等于允许的最大小数位数
                var flag = DecimalPlaces >= text.Length - index - 1;

                // 返回标志和尝试将文本转换为双精度浮点数的结果
                return flag && double.TryParse(text, NumberStyle, CultureInfo.InvariantCulture, out _);
            }

            // 尝试将文本转换为双精度浮点数
            return double.TryParse(text, NumberStyle, CultureInfo.InvariantCulture, out _);
        }

        #endregion Override
    }
}