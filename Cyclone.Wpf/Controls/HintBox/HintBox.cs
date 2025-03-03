using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

namespace Cyclone.UI.Controls
{
    [TemplatePart(Name = PART_ClearTextButton, Type = typeof(System.Windows.Controls.Button))]
    [TemplatePart(Name = PART_InputTextBox, Type = typeof(System.Windows.Controls.TextBox))]
    [TemplatePart(Name = PART_DisplayPopup, Type = typeof(System.Windows.Controls.Primitives.Popup))]
    public class HintBox : Selector
    {
        private const string PART_ClearTextButton = nameof(PART_ClearTextButton);

        private const string PART_DisplayPopup = nameof(PART_DisplayPopup);

        private const string PART_InputTextBox = nameof(PART_InputTextBox);

        private System.Windows.Controls.Primitives.Popup _displayPopup;

        private System.Windows.Controls.TextBox _inputTextBox;

        public System.Windows.Controls.Button _clearTextButton;

        static HintBox()
        {
            InitializeCommands();
        }

        #region Text

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(HintBox), new FrameworkPropertyMetadata(default(string)));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        #endregion Text

        #region IsIgnoreCase

        public static readonly DependencyProperty IsIgnoreCaseProperty =
            DependencyProperty.Register(nameof(IsIgnoreCase), typeof(bool), typeof(HintBox), new PropertyMetadata(true));

        public bool IsIgnoreCase
        {
            get => (bool)GetValue(IsIgnoreCaseProperty);
            set => SetValue(IsIgnoreCaseProperty, value);
        }

        #endregion IsIgnoreCase

        #region IsOpenPopup

        public static readonly DependencyProperty IsOpenPopupProperty =
            DependencyProperty.Register(nameof(IsOpenPopup), typeof(bool), typeof(HintBox), new PropertyMetadata(default(bool), OnIsOpenPopupChanged));

        public bool IsOpenPopup
        {
            get => (bool)GetValue(IsOpenPopupProperty);
            set => SetValue(IsOpenPopupProperty, value);
        }

        private static void OnIsOpenPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        #endregion IsOpenPopup

        #region ClearTextCommand

        public static RoutedCommand ClearTextCommand { get; private set; }

        private static void OnCanClearTextCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            var searchBox = (HintBox)sender;

            if (searchBox._inputTextBox != null)
            {
                e.CanExecute = !string.IsNullOrEmpty(searchBox._inputTextBox.Text);
            }
        }

        private static void OnClearTextCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var searchBox = (HintBox)sender;
            if (searchBox._inputTextBox != null)
            {
                searchBox._inputTextBox.Clear();
                searchBox._inputTextBox.Focus();
            }
        }

        #endregion ClearTextCommand

        private static void InitializeCommands()
        {
            ClearTextCommand = new RoutedCommand("ClearText", typeof(HintBox));

            CommandManager.RegisterClassCommandBinding(typeof(HintBox),
               new CommandBinding(ClearTextCommand, OnClearTextCommand, OnCanClearTextCommand));
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //过滤项目，当默认的过滤器为null时候，按照类型的ToString方法来过滤
            if (Items != null)
            {
                if (Items.Filter == null || Items.Filter == FilterPredicate)
                {
                    Items.Filter = FilterPredicate;
                }
            }
            IsOpenPopup = true;
            _inputTextBox?.Focus();
            RaiseEvent(new RoutedEventArgs(TextChangedEvent, this));
        }

        private void SetText(string text)
        {
            if (_inputTextBox != null)
            {
                _inputTextBox.Text = text;
                _inputTextBox.CaretIndex = _inputTextBox.Text.Length;
            }
        }

        internal void NotifyHintBoxItemMouseEnter(HintBoxItem hintBoxItem)
        {
            for (int i = 0; i < ItemContainerGenerator.Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is HintBoxItem item)
                {
                    item.SetHighlight(false);
                }
            }
            hintBoxItem.SetHighlight(true);
        }

        internal void NotifyHintBoxItemMouseLeftButtonDown(HintBoxItem hintBoxItem)
        {
            SelectedItem = hintBoxItem.Content;
            SetText(SelectedItem.ToString());
            IsOpenPopup = false;
        }

        #region Override

        private int _highlightIndex = -1;

        private int _HighlightIndex
        {
            get => _highlightIndex;

            set
            {
                if (value <= 0)
                {
                    value = 0;
                }
                if (value >= Items.Count)
                {
                    value = Items.Count - 1;
                }
                _highlightIndex = value;
            }
        }

        private bool FilterPredicate(object item)
        {
            if (string.IsNullOrEmpty(Text) || item == null)
            {
                return true;
            }
            if (IsIgnoreCase)
            {
                return item.ToString().ToUpper().Contains(Text.ToUpper());
            }
            else
            {
                return item.ToString().Contains(Text);
            }
        }

        private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Items.Count == 0) return;

            switch (e.Key)
            {
                case Key.Down:
                case Key.Tab:
                    _HighlightIndex += 1;
                    MoveSelectionTo(_HighlightIndex);
                    e.Handled = true;
                    break;

                case Key.Up:
                    _HighlightIndex -= 1;
                    MoveSelectionTo(_HighlightIndex);
                    e.Handled = true;
                    break;

                case Key.Enter:
                    SelectHighlightedItem();
                    break;
            }
        }

        private void MoveSelectionTo(int position)
        {
            for (int i = 0; i < ItemContainerGenerator.Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is HintBoxItem item)
                {
                    item.SetHighlight(false);
                }
            }

            if (ItemContainerGenerator.ContainerFromIndex(position) is HintBoxItem current)
            {
                current.SetHighlight(true);
            };
        }

        private void SelectHighlightedItem()
        {
            for (int i = 0; i < ItemContainerGenerator.Items.Count; i++)
            {
                if (ItemContainerGenerator.ContainerFromIndex(i) is HintBoxItem item)
                {
                    item.IsSelected = false;
                    if (item.IsHighlighted)
                    {
                        item.IsSelected = true;
                        NotifyHintBoxItemMouseLeftButtonDown(item);
                        return;
                    }
                }
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new HintBoxItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is HintBoxItem;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _inputTextBox = GetTemplateChild(PART_InputTextBox) as System.Windows.Controls.TextBox;
            if (_inputTextBox != null)
            {
                _inputTextBox.TextChanged -= InputTextBox_TextChanged;
                _inputTextBox.TextChanged += InputTextBox_TextChanged;
                _inputTextBox.PreviewKeyDown -= InputTextBox_PreviewKeyDown;
                _inputTextBox.PreviewKeyDown += InputTextBox_PreviewKeyDown;
            }
            _displayPopup = GetTemplateChild(PART_DisplayPopup) as System.Windows.Controls.Primitives.Popup;
        }

        #endregion Override

        #region TextChanged

        public static readonly RoutedEvent TextChangedEvent = EventManager.RegisterRoutedEvent("TextChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HintBox));

        public event RoutedEventHandler TextChanged
        {
            add { AddHandler(TextChangedEvent, value); }
            remove { RemoveHandler(TextChangedEvent, value); }
        }

        #endregion TextChanged
    }
}