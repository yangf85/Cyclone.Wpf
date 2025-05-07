using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 颜色选择器控件，提供下拉式的颜色选择功能
    /// </summary>
    [TemplatePart(Name = "PART_ColorDisplay", Type = typeof(Border))]
    [TemplatePart(Name = "PART_DropDownButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_ColorSelector", Type = typeof(ColorSelector))]
    public class ColorPicker : Control, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged 实现

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged 实现

        #region 私有字段

        private Border _colorDisplay;
        private ToggleButton _dropDownButton;
        private Popup _popup;
        private ColorSelector _colorSelector;
        private bool _isUpdatingColor;

        #endregion 私有字段

        #region 构造函数

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker),
                new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {
            // 注册键盘事件
            KeyDown += ColorPicker_KeyDown;
        }

        #endregion 构造函数

        #region 依赖属性

        #region SelectedColor - 当前选中的颜色

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
                new FrameworkPropertyMetadata(Colors.Red,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedColorChanged));

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker picker)
            {
                picker.OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
            }
        }

        #endregion SelectedColor - 当前选中的颜色

        #region IsOpen - 下拉框是否打开

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(ColorPicker),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnIsOpenChanged));

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker picker)
            {
                picker.OnIsOpenChanged((bool)e.OldValue, (bool)e.NewValue);
            }
        }

        #endregion IsOpen - 下拉框是否打开

        #region DisplayColorText - 是否显示颜色文本

        public bool DisplayColorText
        {
            get => (bool)GetValue(DisplayColorTextProperty);
            set => SetValue(DisplayColorTextProperty, value);
        }

        public static readonly DependencyProperty DisplayColorTextProperty =
            DependencyProperty.Register(nameof(DisplayColorText), typeof(bool), typeof(ColorPicker),
                new PropertyMetadata(true));

        #endregion DisplayColorText - 是否显示颜色文本

        #region ColorTextFormat - 颜色文本格式

        public ColorTextMode ColorTextFormat
        {
            get => (ColorTextMode)GetValue(ColorTextFormatProperty);
            set => SetValue(ColorTextFormatProperty, value);
        }

        public static readonly DependencyProperty ColorTextFormatProperty =
            DependencyProperty.Register(nameof(ColorTextFormat), typeof(ColorTextMode), typeof(ColorPicker),
                new PropertyMetadata(ColorTextMode.HEX, OnColorTextFormatChanged));

        private static void OnColorTextFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorPicker picker)
            {
                picker.UpdateColorText();
            }
        }

        #endregion ColorTextFormat - 颜色文本格式

        #region ColorText - 颜色文本显示

        public string ColorText
        {
            get => (string)GetValue(ColorTextProperty);
            private set => SetValue(ColorTextPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ColorTextPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ColorText), typeof(string), typeof(ColorPicker),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ColorTextProperty = ColorTextPropertyKey.DependencyProperty;

        #endregion ColorText - 颜色文本显示

        #region PlacementTarget - 弹出框定位目标

        public UIElement PlacementTarget
        {
            get => (UIElement)GetValue(PlacementTargetProperty);
            set => SetValue(PlacementTargetProperty, value);
        }

        public static readonly DependencyProperty PlacementTargetProperty =
            DependencyProperty.Register(nameof(PlacementTarget), typeof(UIElement), typeof(ColorPicker),
                new PropertyMetadata(null));

        #endregion PlacementTarget - 弹出框定位目标

        #region Placement - 弹出框定位方式

        public PlacementMode Placement
        {
            get => (PlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(ColorPicker),
                new PropertyMetadata(PlacementMode.Bottom));

        #endregion Placement - 弹出框定位方式

        #endregion 依赖属性

        #region 路由事件

        public static readonly RoutedEvent SelectedColorChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectedColorChanged), RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(ColorPicker));

        public event RoutedEventHandler SelectedColorChanged
        {
            add { AddHandler(SelectedColorChangedEvent, value); }
            remove { RemoveHandler(SelectedColorChangedEvent, value); }
        }

        #endregion 路由事件

        #region 重写方法

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 解除旧控件事件
            UnregisterEvents();

            // 获取新控件
            _colorDisplay = GetTemplateChild("PART_ColorDisplay") as Border;
            _dropDownButton = GetTemplateChild("PART_DropDownButton") as ToggleButton;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _colorSelector = GetTemplateChild("PART_ColorSelector") as ColorSelector;

            // 配置Popup
            if (_popup != null)
            {
                // 设置StaysOpen为false，这样点击外部会自动关闭
                _popup.StaysOpen = false;
            }

            // 注册新控件事件
            RegisterEvents();

            // 更新显示
            UpdateColorText();
        }

        #endregion 重写方法

        #region 私有方法

        private void RegisterEvents()
        {
            if (_dropDownButton != null)
            {
                _dropDownButton.Click += DropDownButton_Click;
            }

            if (_popup != null)
            {
                _popup.Opened += Popup_Opened;
                _popup.Closed += Popup_Closed;
            }

            if (_colorSelector != null)
            {
                _colorSelector.ColorChanged += ColorSelector_ColorChanged;
                // 防止点击ColorSelector时Popup关闭
                _colorSelector.MouseDown += ColorSelector_MouseDown;
            }
        }

        private void UnregisterEvents()
        {
            if (_dropDownButton != null)
            {
                _dropDownButton.Click -= DropDownButton_Click;
            }

            if (_popup != null)
            {
                _popup.Opened -= Popup_Opened;
                _popup.Closed -= Popup_Closed;
            }

            if (_colorSelector != null)
            {
                _colorSelector.ColorChanged -= ColorSelector_ColorChanged;
                _colorSelector.MouseDown -= ColorSelector_MouseDown;
            }
        }

        // 更新颜色文本显示
        private void UpdateColorText()
        {
            if (_isUpdatingColor)
                return;

            try
            {
                _isUpdatingColor = true;

                string text = string.Empty;

                switch (ColorTextFormat)
                {
                    case ColorTextMode.HEX:
                        text = $"#{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
                        if (SelectedColor.A < 255)
                        {
                            text = $"#{SelectedColor.A:X2}{SelectedColor.R:X2}{SelectedColor.G:X2}{SelectedColor.B:X2}";
                        }
                        break;

                    case ColorTextMode.RGB:
                        if (SelectedColor.A < 255)
                        {
                            text = $"RGBA({SelectedColor.R}, {SelectedColor.G}, {SelectedColor.B}, {SelectedColor.A / 255.0:F2})";
                        }
                        else
                        {
                            text = $"RGB({SelectedColor.R}, {SelectedColor.G}, {SelectedColor.B})";
                        }
                        break;
                }

                ColorText = text;
            }
            finally
            {
                _isUpdatingColor = false;
            }
        }

        #endregion 私有方法

        #region 事件处理

        private void OnSelectedColorChanged(Color oldColor, Color newColor)
        {
            if (!_isUpdatingColor)
            {
                try
                {
                    _isUpdatingColor = true;

                    // 更新选择器颜色
                    if (_colorSelector != null)
                    {
                        _colorSelector.SelectedColor = newColor;
                    }

                    // 更新文本显示
                    UpdateColorText();

                    // 触发事件
                    RaiseEvent(new RoutedEventArgs(SelectedColorChangedEvent, this));
                }
                finally
                {
                    _isUpdatingColor = false;
                }
            }
        }

        private void OnIsOpenChanged(bool oldValue, bool newValue)
        {
            if (_popup != null)
            {
                _popup.IsOpen = newValue;
            }
        }

        private void DropDownButton_Click(object sender, RoutedEventArgs e)
        {
            IsOpen = !IsOpen;
        }

        private void Popup_Opened(object sender, EventArgs e)
        {
            if (_colorSelector != null)
            {
                // 确保颜色选择器显示当前颜色
                _colorSelector.SelectedColor = SelectedColor;

                // 获取焦点
                _colorSelector.Focus();
            }
        }

        private void Popup_Closed(object sender, EventArgs e)
        {
            // 更新下拉状态
            IsOpen = false;

            // 重新获取焦点
            Focus();
        }

        private void ColorSelector_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // 防止点击ColorSelector时Popup关闭
            e.Handled = true;
        }

        private void ColorSelector_ColorChanged(object sender, RoutedEventArgs e)
        {
            if (_colorSelector != null && !_isUpdatingColor)
            {
                SelectedColor = _colorSelector.SelectedColor;
            }
        }

        private void ColorPicker_KeyDown(object sender, KeyEventArgs e)
        {
            // 处理键盘事件
            switch (e.Key)
            {
                case Key.Escape:
                    if (IsOpen)
                    {
                        IsOpen = false;
                        e.Handled = true;
                    }
                    break;

                case Key.Enter:
                case Key.Space:
                    if (!IsOpen)
                    {
                        IsOpen = true;
                        e.Handled = true;
                    }
                    break;

                case Key.Down:
                    if (!IsOpen)
                    {
                        IsOpen = true;
                        e.Handled = true;
                    }
                    break;

                case Key.Tab:
                    if (IsOpen)
                    {
                        IsOpen = false;
                    }
                    break;
            }
        }

        #endregion 事件处理
    }
}