using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 颜色文本模式
    /// </summary>
    public enum ColorTextMode
    {
        HEX,
        RGB
    }

    /// <summary>
    /// 颜色文本输入框控件，支持HEX和RGB格式
    /// </summary>
    public class ColorTextBox : TextBox
    {
        #region Color

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Color), typeof(ColorTextBox),
                new FrameworkPropertyMetadata(Colors.Black,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnColorChanged));

        #endregion Color

        #region TextMode

        public ColorTextMode TextMode
        {
            get => (ColorTextMode)GetValue(TextModeProperty);
            set => SetValue(TextModeProperty, value);
        }

        public static readonly DependencyProperty TextModeProperty =
            DependencyProperty.Register(nameof(TextMode), typeof(ColorTextMode), typeof(ColorTextBox),
                new PropertyMetadata(ColorTextMode.HEX, OnTextModeChanged));

        #endregion TextMode

        #region IsInputValid

        public bool IsInputValid
        {
            get => (bool)GetValue(IsInputValidProperty);
            private set => SetValue(IsInputValidPropertyKey, value);
        }

        // 只读依赖属性键
        private static readonly DependencyPropertyKey IsInputValidPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(IsInputValid), typeof(bool), typeof(ColorTextBox),
                new PropertyMetadata(true));

        // 公开的只读依赖属性
        public static readonly DependencyProperty IsInputValidProperty =
            IsInputValidPropertyKey.DependencyProperty;

        #endregion IsInputValid

        #region 事件声明

        /// <summary>
        /// 当颜色文本输入有效时触发
        /// </summary>
        public static readonly RoutedEvent ColorTextValidatedEvent = EventManager.RegisterRoutedEvent(
            nameof(ColorTextValidated), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ColorTextBox));

        /// <summary>
        /// 颜色文本验证成功事件
        /// </summary>
        public event RoutedEventHandler ColorTextValidated
        {
            add { AddHandler(ColorTextValidatedEvent, value); }
            remove { RemoveHandler(ColorTextValidatedEvent, value); }
        }

        #endregion 事件声明

        #region 私有字段

        private bool _isUpdatingText;
        private bool _isUpdatingColor;

        #endregion 私有字段

        #region 构造函数

        static ColorTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorTextBox),
                new FrameworkPropertyMetadata(typeof(ColorTextBox)));
        }

        public ColorTextBox()
        {
            // 初始化设置
            Loaded += OnLoaded;
            TextChanged += ColorTextBox_TextChanged;
            PreviewTextInput += ColorTextBox_PreviewTextInput;
            PreviewKeyDown += ColorTextBox_PreviewKeyDown;
            DataObject.AddPastingHandler(this, OnPaste);
        }

        #endregion 构造函数

        #region 事件处理

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化文本
            UpdateTextFromColor();
        }

        // 文本变化处理
        private void ColorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isUpdatingText)
            {
                try
                {
                    _isUpdatingColor = true;
                    ValidateAndUpdateColor();
                }
                finally
                {
                    _isUpdatingColor = false;
                }
            }
        }

        // 按键处理 - 支持回车键确认
        private void ColorTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ValidateAndUpdateColor();

                // 如果有效，则触发验证事件
                if (IsInputValid)
                {
                    RaiseEvent(new RoutedEventArgs(ColorTextValidatedEvent, this));
                }

                // 失去焦点，以便显示其他控件
                Keyboard.ClearFocus();
            }
        }

        // 文本输入预览（验证字符）
        private void ColorTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (TextMode == ColorTextMode.HEX)
            {
                e.Handled = !ColorHelper.IsValidHexChar(e.Text);
            }
            else if (TextMode == ColorTextMode.RGB)
            {
                e.Handled = !ColorHelper.IsValidRgbChar(e.Text);
            }
        }

        // 粘贴处理
        private void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));

                // 如果粘贴的内容包含完整的颜色格式，自动切换模式
                if (ColorHelper.IsValidHexString(text) && TextMode != ColorTextMode.HEX)
                {
                    TextMode = ColorTextMode.HEX;
                }
                else if (ColorHelper.IsValidRgbString(text) && TextMode != ColorTextMode.RGB)
                {
                    TextMode = ColorTextMode.RGB;
                }
                // 验证当前模式下的合法性
                else if (TextMode == ColorTextMode.HEX && !ColorHelper.IsValidHexChar(text))
                {
                    e.CancelCommand();
                }
                else if (TextMode == ColorTextMode.RGB && !ColorHelper.IsValidRgbChar(text))
                {
                    e.CancelCommand();
                }
            }
        }

        // 颜色属性变化处理
        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (ColorTextBox)d;
            if (!textBox._isUpdatingColor)
            {
                try
                {
                    textBox._isUpdatingText = true;
                    textBox.UpdateTextFromColor();
                }
                finally
                {
                    textBox._isUpdatingText = false;
                }
            }
        }

        // 文本模式变化处理
        private static void OnTextModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (ColorTextBox)d;
            try
            {
                textBox._isUpdatingText = true;
                textBox.UpdateTextFromColor();
            }
            finally
            {
                textBox._isUpdatingText = false;
            }
        }

        #endregion 事件处理

        #region 私有辅助方法

        // 根据颜色更新文本
        private void UpdateTextFromColor()
        {
            if (TextMode == ColorTextMode.HEX)
            {
                Text = ColorHelper.ColorToHexString(Color);
            }
            else
            {
                Text = ColorHelper.ColorToRgbString(Color);
            }

            IsInputValid = true;
        }

        // 验证文本并更新颜色
        private void ValidateAndUpdateColor()
        {
            string text = Text;

            if (ColorHelper.TryParseColorText(text, TextMode, out Color parsedColor))
            {
                Color = parsedColor;
                IsInputValid = true;
            }
            else
            {
                IsInputValid = false;
            }
        }

        #endregion 私有辅助方法
    }
}