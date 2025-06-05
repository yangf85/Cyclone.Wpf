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
    [TemplatePart(Name = PART_IncreaseRepeatButton, Type = typeof(RepeatButton))]
    [TemplatePart(Name = PART_DecreaseRepeatButton, Type = typeof(RepeatButton))]
    [TemplatePart(Name = PART_ClearButton, Type = typeof(Button))]
    public class NumberBox : Control
    {
        private const string PART_IncreaseRepeatButton = nameof(PART_IncreaseRepeatButton);
        private const string PART_InputTextBox = nameof(PART_InputTextBox);
        private const string PART_DecreaseRepeatButton = nameof(PART_DecreaseRepeatButton);
        private const string PART_ClearButton = nameof(PART_ClearButton);

        private RepeatButton _increaseRepeatButton;
        private TextBox _inputTextBox;
        private RepeatButton _decreaseRepeatButton;
        private Button _clearButton;

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
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
            textBox?.Text = box.FormatValue(box.Value);

            // 触发 ValueChanged 事件
            box.RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
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

            // 只有当不允许小数点时才取整
            if (!box.NumberStyle.HasFlag(NumberStyles.AllowDecimalPoint))
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
            System.Diagnostics.Debug.WriteLine($"Step changed from {e.OldValue} to {e.NewValue}");
        }

        #endregion Step

        #region DecimalPlaces

        public static readonly DependencyProperty DecimalPlacesProperty =
                            DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(NumberBox), new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDecimalPlacesChanged, CoerceDecimalPlaces));

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

            // 更新文本框显示
            box._inputTextBox?.Text = box.FormatValue(box.Value);
        }

        #endregion DecimalPlaces

        #region Prefix

        public object Prefix
        {
            get => (object)GetValue(PrefixProperty);
            set => SetValue(PrefixProperty, value);
        }

        public static readonly DependencyProperty PrefixProperty =
            DependencyProperty.Register(nameof(Prefix), typeof(object), typeof(NumberBox), new PropertyMetadata(default(object)));

        #endregion Prefix

        #region IsReadOnly

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(NumberBox), new PropertyMetadata(default(bool), OnIsReadOnlyChanged));

        private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var box = (NumberBox)d;
            if (box._inputTextBox != null)
            {
                box._inputTextBox.IsReadOnly = (bool)e.NewValue;
            }
        }

        #endregion IsReadOnly

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

        #region IsVisibleSpinButton

        public static readonly DependencyProperty IsVisibleSpinButtonProperty =
            DependencyProperty.Register(nameof(IsVisibleSpinButton), typeof(bool), typeof(NumberBox), new PropertyMetadata(true));

        public bool IsVisibleSpinButton
        {
            get => (bool)GetValue(IsVisibleSpinButtonProperty);
            set => SetValue(IsVisibleSpinButtonProperty, value);
        }

        #endregion IsVisibleSpinButton

        #region IsVisibleClearButton

        public static readonly DependencyProperty IsVisibleClearButtonProperty =
            DependencyProperty.Register(nameof(IsVisibleClearButton), typeof(bool), typeof(NumberBox), new PropertyMetadata(false));

        public bool IsVisibleClearButton
        {
            get => (bool)GetValue(IsVisibleClearButtonProperty);
            set => SetValue(IsVisibleClearButtonProperty, value);
        }

        #endregion IsVisibleClearButton

        #region SpinButtonOrientation

        public static readonly DependencyProperty SpinButtonOrientationProperty =
            DependencyProperty.Register(nameof(SpinButtonOrientation), typeof(Orientation), typeof(NumberBox), new PropertyMetadata(Orientation.Vertical));

        public Orientation SpinButtonOrientation
        {
            get => (Orientation)GetValue(SpinButtonOrientationProperty);
            set => SetValue(SpinButtonOrientationProperty, value);
        }

        #endregion SpinButtonOrientation

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
            if (!numberBox.IsReadOnly)
            {
                numberBox.Value += numberBox.Step;
            }
        }

        private static void OnCanIncreaseCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            e.CanExecute = !numberBox.IsReadOnly && numberBox.Value + numberBox.Step <= numberBox.Maximum;
        }

        #endregion Increase

        #region Decrease

        public static RoutedCommand DecreaseCommand { get; private set; }

        private static void OnCanDecreaseCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            e.CanExecute = !numberBox.IsReadOnly && numberBox.Value - numberBox.Step >= numberBox.Minimum;
        }

        private static void OnDecreaseCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            if (!numberBox.IsReadOnly)
            {
                numberBox.Value -= numberBox.Step;
            }
        }

        #endregion Decrease

        #region Clear

        public static RoutedCommand ClearCommand { get; private set; }

        private static void OnClearCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            if (!numberBox.IsReadOnly)
            {
                // 将值设置为0，但要确保0在允许的范围内
                var clearValue = Math.Max(0, numberBox.Minimum);
                numberBox.Value = clearValue;
            }
        }

        private static void OnCanClearCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var numberBox = sender as NumberBox;
            e.CanExecute = !numberBox.IsReadOnly && numberBox.Value != numberBox.Minimum;
        }

        #endregion Clear

        private static void InitializeCommand()
        {
            IncreaseCommand = new RoutedCommand("Increase", typeof(NumberBox));
            DecreaseCommand = new RoutedCommand("Decrease", typeof(NumberBox));
            ClearCommand = new RoutedCommand("Clear", typeof(NumberBox));

            CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
                new CommandBinding(IncreaseCommand, OnIncreaseCommand, OnCanIncreaseCommand));
            CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
                new CommandBinding(DecreaseCommand, OnDecreaseCommand, OnCanDecreaseCommand));
            CommandManager.RegisterClassCommandBinding(typeof(NumberBox),
                new CommandBinding(ClearCommand, OnClearCommand, OnCanClearCommand));
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
                _inputTextBox.Text = FormatValue(Value);
                _inputTextBox.IsReadOnly = IsReadOnly;
                _inputTextBox.PreviewTextInput -= InputTextBox_PreviewTextInput;
                _inputTextBox.PreviewTextInput += InputTextBox_PreviewTextInput;
                _inputTextBox.MouseWheel -= InputTextBox_MouseWheel;
                _inputTextBox.MouseWheel += InputTextBox_MouseWheel;
                _inputTextBox.TextChanged -= InputTextBox_TextChanged;
                _inputTextBox.TextChanged += InputTextBox_TextChanged;
                _inputTextBox.PreviewKeyDown -= InputTextBox_PreviewKeyDown;
                _inputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;

                // 添加粘贴事件处理
                DataObject.RemovePastingHandler(_inputTextBox, OnPasting);
                DataObject.AddPastingHandler(_inputTextBox, OnPasting);
            }

            _clearButton = GetTemplateChild(PART_ClearButton) as Button;
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsReadOnly)
            {
                // 在只读模式下，阻止上下键操作
                if (e.Key == Key.Up || e.Key == Key.Down)
                {
                    e.Handled = true;
                }
                return;
            }

            switch (e.Key)
            {
                case Key.Up:
                    if (IncreaseCommand.CanExecute(null, this))
                    {
                        IncreaseCommand.Execute(null, this);
                        e.Handled = true;
                    }
                    break;

                case Key.Down:
                    if (DecreaseCommand.CanExecute(null, this))
                    {
                        DecreaseCommand.Execute(null, this);
                        e.Handled = true;
                    }
                    break;
            }
        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            // 如果是只读模式，取消粘贴操作
            if (IsReadOnly)
            {
                e.CancelCommand();
                return;
            }

            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                var selectionStart = _inputTextBox.SelectionStart;
                var selectionLength = _inputTextBox.SelectionLength;
                var currentText = _inputTextBox.Text;

                // 模拟粘贴后的文本
                var newText = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, text);

                if (!IsValidNumericInput(newText))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void InputTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // 如果是只读模式，不处理滚轮事件
            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Current Step: {this.Step}");

            // 判断滚动方向，使用命令来增加或减少值
            if (e.Delta > 0)
            {
                // 向上滚动，增加值
                if (IncreaseCommand.CanExecute(null, this))
                {
                    IncreaseCommand.Execute(null, this);
                }
            }
            else
            {
                // 向下滚动，减少值
                if (DecreaseCommand.CanExecute(null, this))
                {
                    DecreaseCommand.Execute(null, this);
                }
            }

            // 将光标放置在输入框的末尾
            _inputTextBox.CaretIndex = _inputTextBox.Text.Length;

            // 标记事件已处理，防止冒泡
            e.Handled = true;
        }

        // 此方法预览输入文本事件
        private void InputTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // 如果是只读模式，阻止输入
            if (IsReadOnly)
            {
                e.Handled = true;
                return;
            }

            var selectionStart = _inputTextBox.SelectionStart;
            var selectionLength = _inputTextBox.SelectionLength;
            var fullText = _inputTextBox.Text;

            // 移除选中的文本并在正确位置插入新文本
            var newText = fullText.Remove(selectionStart, selectionLength).Insert(selectionStart, e.Text);

            e.Handled = !IsValidNumericInput(newText);
        }

        // 当输入框文本发生改变时，此方法会被调用
        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 如果是只读模式，不处理文本变化
            if (IsReadOnly)
            {
                return;
            }

            var text = _inputTextBox.Text;

            // 允许空文本或只有负号
            if (string.IsNullOrEmpty(text) || text == "-")
            {
                return;
            }

            if (double.TryParse(text, NumberStyle, CultureInfo.InvariantCulture, out var value))
            {
                value = Math.Round(value, DecimalPlaces);

                if (value < Minimum)
                {
                    value = Minimum;
                    _inputTextBox.Text = FormatValue(value);
                }
                else if (value > Maximum)
                {
                    value = Maximum;
                    _inputTextBox.Text = FormatValue(value);
                }
                Value = value;
            }
            else
            {
                // 如果解析失败，设置为最小值
                Value = Minimum;
                _inputTextBox.Text = FormatValue(Minimum);
            }
        }

        // 检查给定的文本是否是有效的数字输入
        private bool IsValidNumericInput(string text)
        {
            // 允许空文本或只有负号
            if (string.IsNullOrEmpty(text) || text == "-")
                return true;

            // 检查是否有多个小数点
            if (text.Count(c => c == '.') > 1)
                return false;

            // 检查是否有多个负号或负号不在开头
            if (text.Count(c => c == '-') > 1 || (text.Contains('-') && text.IndexOf('-') != 0))
                return false;

            if (text.Contains('.'))
            {
                var index = text.IndexOf('.');
                // 判断小数点后的位数是否小于等于允许的最大小数位数
                var decimalDigits = text.Length - index - 1;
                if (decimalDigits > DecimalPlaces)
                    return false;
            }

            // 尝试将文本转换为双精度浮点数
            return double.TryParse(text, NumberStyle, CultureInfo.InvariantCulture, out _);
        }

        private string FormatValue(double value)
        {
            // 根据 DecimalPlaces 格式化数字
            return value.ToString($"F{DecimalPlaces}", CultureInfo.InvariantCulture);
        }

        #endregion Override
    }
}