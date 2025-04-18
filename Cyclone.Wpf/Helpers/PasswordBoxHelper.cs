using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Cyclone.Wpf.Helpers
{
    /// <summary>
    /// 提供PasswordBox的扩展功能
    /// </summary>
    public static class PasswordBoxHelper
    {
        #region Watermark

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty));

        public static string GetWatermark(DependencyObject obj) => (string)obj.GetValue(WatermarkProperty);

        public static void SetWatermark(DependencyObject obj, string value) => obj.SetValue(WatermarkProperty, value);

        #endregion Watermark

        #region Password

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, OnPasswordChanged)
                {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        private static readonly DependencyProperty IsUpdatingProperty =
            DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // 避免无限循环
                if ((bool)passwordBox.GetValue(IsUpdatingProperty))
                    return;

                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

                if (!GetShowPassword(passwordBox))
                {
                    string password = (string)e.NewValue;
                    if (passwordBox.Password != password)
                        passwordBox.Password = password ?? string.Empty;
                }

                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                passwordBox.SetValue(IsUpdatingProperty, true);
                SetPassword(passwordBox, passwordBox.Password);
                passwordBox.SetValue(IsUpdatingProperty, false);
            }
        }

        public static string GetPassword(DependencyObject obj) => (string)obj.GetValue(PasswordProperty);

        public static void SetPassword(DependencyObject obj, string value) => obj.SetValue(PasswordProperty, value);

        #endregion Password

        #region HasClearButton

        public static readonly DependencyProperty HasClearButtonProperty =
            DependencyProperty.RegisterAttached("HasClearButton", typeof(bool), typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        public static bool GetHasClearButton(DependencyObject obj) => (bool)obj.GetValue(HasClearButtonProperty);

        public static void SetHasClearButton(DependencyObject obj, bool value) => obj.SetValue(HasClearButtonProperty, value);

        #endregion HasClearButton

        #region ClearCommand

        private static readonly RoutedCommand clearCommand = new RoutedCommand("ClearPassword", typeof(PasswordBoxHelper));

        public static RoutedCommand ClearCommand => clearCommand;

        static PasswordBoxHelper()
        {
            CommandManager.RegisterClassCommandBinding(
                typeof(PasswordBox),
                new CommandBinding(clearCommand, ExecuteClearCommand));
        }

        private static void ExecuteClearCommand(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                passwordBox.Clear();
                SetPassword(passwordBox, string.Empty);
            }
        }

        #endregion ClearCommand

        #region ShowPassword

        public static readonly DependencyProperty ShowPasswordProperty =
            DependencyProperty.RegisterAttached("ShowPassword", typeof(bool), typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnShowPasswordChanged));

        public static bool GetShowPassword(DependencyObject obj) => (bool)obj.GetValue(ShowPasswordProperty);

        public static void SetShowPassword(DependencyObject obj, bool value) => obj.SetValue(ShowPasswordProperty, value);

        private static void OnShowPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                bool showPassword = (bool)e.NewValue;

                if (showPassword)
                {
                    // 当显示为明文时，确保Password附加属性包含最新密码
                    SetPassword(passwordBox, passwordBox.Password);

                    // 添加失焦事件
                    passwordBox.LostFocus -= PasswordBox_LostFocus;
                    passwordBox.LostFocus += PasswordBox_LostFocus;
                }
                else
                {
                    // 隐藏密码时移除事件
                    passwordBox.LostFocus -= PasswordBox_LostFocus;
                }
            }
        }

        private static void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                // 失焦时隐藏密码
                SetShowPassword(passwordBox, false);
            }
        }

        #endregion ShowPassword

        #region HasPasswordVisibilityToggle

        public static readonly DependencyProperty HasPasswordVisibilityToggleProperty =
            DependencyProperty.RegisterAttached("HasPasswordVisibilityToggle", typeof(bool), typeof(PasswordBoxHelper),
                new PropertyMetadata(false));

        public static bool GetHasPasswordVisibilityToggle(DependencyObject obj) =>
            (bool)obj.GetValue(HasPasswordVisibilityToggleProperty);

        public static void SetHasPasswordVisibilityToggle(DependencyObject obj, bool value) =>
            obj.SetValue(HasPasswordVisibilityToggleProperty, value);

        #endregion HasPasswordVisibilityToggle
    }
}