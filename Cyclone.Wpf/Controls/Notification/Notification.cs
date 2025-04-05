using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// Defines the position where notifications will appear
/// </summary>
public enum NotificationPosition
{
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

/// <summary>
/// Defines the type of notification
/// </summary>
public enum NotificationType
{
    Information,
    Success,
    Warning,
    Error
}

/// <summary>
/// Main notification service for displaying toast notifications
/// Works seamlessly with both regular WPF windows and window handles
/// </summary>
public class NotificationService
{
    #region Private Fields

    private readonly List<NotificationWindow> _activeNotifications = new List<NotificationWindow>();
    private readonly object _lockObject = new object();
    private NotificationPosition _position = NotificationPosition.BottomRight;
    private TimeSpan _displayTime = TimeSpan.FromSeconds(5);
    private int _maxNotifications = 5;
    private int _offsetX = 20;
    private int _offsetY = 20;
    private int _spacing = 10;
    private IntPtr _windowHandle;
    private Window _windowReference;

    #endregion Private Fields

    #region Public Properties

    /// <summary>
    /// Gets or sets the position where notifications will appear
    /// </summary>
    public NotificationPosition Position
    {
        get => _position;
        set => _position = value;
    }

    /// <summary>
    /// Gets or sets how long notifications will be displayed
    /// </summary>
    public TimeSpan DisplayTime
    {
        get => _displayTime;
        set => _displayTime = value;
    }

    /// <summary>
    /// Gets or sets the maximum number of notifications to display at once
    /// </summary>
    public int MaxNotifications
    {
        get => _maxNotifications;
        set => _maxNotifications = value;
    }

    /// <summary>
    /// Gets or sets the X offset from the edge of the screen
    /// </summary>
    public int OffsetX
    {
        get => _offsetX;
        set => _offsetX = value;
    }

    /// <summary>
    /// Gets or sets the Y offset from the edge of the screen
    /// </summary>
    public int OffsetY
    {
        get => _offsetY;
        set => _offsetY = value;
    }

    /// <summary>
    /// Gets or sets the spacing between notifications
    /// </summary>
    public int Spacing
    {
        get => _spacing;
        set => _spacing = value;
    }

    #endregion Public Properties

    #region Constructors

    /// <summary>
    /// Creates a notification service that positions relative to the given WPF window
    /// </summary>
    /// <param name="window">The WPF window to position notifications relative to</param>
    public NotificationService(Window window)
    {
        if (window == null)
            throw new ArgumentNullException(nameof(window));

        _windowReference = window;

        // Get the window handle if the window is already initialized
        if (PresentationSource.FromVisual(window) is HwndSource)
            _windowHandle = new WindowInteropHelper(window).Handle;

        // Attach to SourceInitialized to get the handle if the window isn't initialized yet
        window.SourceInitialized += (s, e) =>
        {
            _windowHandle = new WindowInteropHelper(window).Handle;
        };
    }

    /// <summary>
    /// Creates a notification service that positions relative to the given window handle
    /// </summary>
    /// <param name="windowHandle">The window handle to position notifications relative to</param>
    public NotificationService(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            throw new ArgumentException("Window handle cannot be zero.", nameof(windowHandle));

        _windowHandle = windowHandle;
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Shows a notification with the specified message
    /// </summary>
    /// <param name="message">The message to display</param>
    /// <param name="type">The type of notification</param>
    public void Show(string message, NotificationType type = NotificationType.Information)
    {
        Show(null, message, type);
    }

    /// <summary>
    /// Shows a notification with the specified title and message
    /// </summary>
    /// <param name="title">The title to display</param>
    /// <param name="message">The message to display</param>
    /// <param name="type">The type of notification</param>
    public void Show(string title, string message, NotificationType type = NotificationType.Information)
    {
        // Ensure we're on the UI thread
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => Show(title, message, type));
            return;
        }

        // Check if we have too many notifications already
        lock (_lockObject)
        {
            if (_activeNotifications.Count >= _maxNotifications)
            {
                // Remove the oldest notification
                if (_activeNotifications.Count > 0)
                {
                    var oldestNotification = _activeNotifications[0];
                    _activeNotifications.RemoveAt(0);
                    oldestNotification.Close();
                }
            }

            // Create new notification
            var notification = new NotificationWindow
            {
                Title = title,
                Message = message,
                NotificationType = type
            };

            // Set the owner appropriately based on what we have
            if (_windowHandle != IntPtr.Zero)
            {
                notification.SetOwnerHandle(_windowHandle);
            }
            else if (_windowReference != null)
            {
                notification.Owner = _windowReference;
            }

            // Add notification to active list
            _activeNotifications.Add(notification);

            // Setup notification events
            notification.Closed += (s, e) =>
            {
                lock (_lockObject)
                {
                    _activeNotifications.Remove(notification);
                    RepositionNotifications();
                }
            };

            // Position and show the notification
            PositionNotification(notification);
            notification.Show();

            // Setup auto-close timer
            var timer = new DispatcherTimer
            {
                Interval = _displayTime
            };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                notification.StartCloseAnimation();
            };
            timer.Start();
        }
    }

    /// <summary>
    /// Closes all active notifications
    /// </summary>
    public void CloseAll()
    {
        lock (_lockObject)
        {
            foreach (var notification in _activeNotifications.ToArray())
            {
                notification.Close();
            }
            _activeNotifications.Clear();
        }
    }

    #endregion Public Methods

    #region Private Methods

    // Positions a new notification based on current settings
    private void PositionNotification(NotificationWindow notification)
    {
        RECT windowRect = new RECT();

        // Get the position and size of the window
        if (_windowHandle != IntPtr.Zero)
        {
            GetWindowRect(_windowHandle, ref windowRect);
        }
        else if (_windowReference != null)
        {
            // Fallback to using the WPF window
            windowRect.left = (int)_windowReference.Left;
            windowRect.top = (int)_windowReference.Top;
            windowRect.right = (int)(_windowReference.Left + _windowReference.ActualWidth);
            windowRect.bottom = (int)(_windowReference.Top + _windowReference.ActualHeight);
        }
        else
        {
            // Get the working area of the primary screen
            var workingArea = SystemParameters.WorkArea;
            windowRect.left = (int)workingArea.Left;
            windowRect.top = (int)workingArea.Top;
            windowRect.right = (int)workingArea.Right;
            windowRect.bottom = (int)workingArea.Bottom;
        }

        // Show the notification to calculate its actual size
        notification.Opacity = 0;
        notification.Show();
        notification.UpdateLayout();

        double notificationWidth = notification.ActualWidth;
        double notificationHeight = notification.ActualHeight;
        notification.Hide();
        notification.Opacity = 1;

        // Calculate position based on settings
        double left, top;

        switch (_position)
        {
            case NotificationPosition.TopLeft:
                left = windowRect.left + _offsetX;
                top = windowRect.top + _offsetY + GetNotificationOffset(notification);
                break;

            case NotificationPosition.TopRight:
                left = windowRect.right - notificationWidth - _offsetX;
                top = windowRect.top + _offsetY + GetNotificationOffset(notification);
                break;

            case NotificationPosition.BottomLeft:
                left = windowRect.left + _offsetX;
                top = windowRect.bottom - notificationHeight - _offsetY - GetNotificationOffset(notification);
                break;

            case NotificationPosition.BottomRight:
            default:
                left = windowRect.right - notificationWidth - _offsetX;
                top = windowRect.bottom - notificationHeight - _offsetY - GetNotificationOffset(notification);
                break;
        }

        notification.Left = left;
        notification.Top = top;
        notification.StartEntryAnimation();
    }

    // Calculates the offset for a notification based on existing notifications
    private double GetNotificationOffset(NotificationWindow notification)
    {
        int index = _activeNotifications.IndexOf(notification);
        if (index <= 0) return 0;

        double offset = 0;
        for (int i = 0; i < index; i++)
        {
            if (i < _activeNotifications.Count)
            {
                offset += _activeNotifications[i].ActualHeight + _spacing;
            }
        }

        return offset;
    }

    // Repositions all active notifications after one is closed
    private void RepositionNotifications()
    {
        for (int i = 0; i < _activeNotifications.Count; i++)
        {
            var notification = _activeNotifications[i];
            var targetTop = 0.0;

            // Recalculate the target top position based on position setting
            if (_position == NotificationPosition.TopLeft || _position == NotificationPosition.TopRight)
            {
                RECT windowRect = new RECT();
                if (_windowHandle != IntPtr.Zero)
                {
                    GetWindowRect(_windowHandle, ref windowRect);
                    targetTop = windowRect.top + _offsetY + GetNotificationOffset(notification);
                }
                else if (_windowReference != null)
                {
                    targetTop = _windowReference.Top + _offsetY + GetNotificationOffset(notification);
                }
                else
                {
                    targetTop = SystemParameters.WorkArea.Top + _offsetY + GetNotificationOffset(notification);
                }
            }
            else // Bottom positions
            {
                RECT windowRect = new RECT();
                if (_windowHandle != IntPtr.Zero)
                {
                    GetWindowRect(_windowHandle, ref windowRect);
                    targetTop = windowRect.bottom - notification.ActualHeight - _offsetY - GetNotificationOffset(notification);
                }
                else if (_windowReference != null)
                {
                    targetTop = _windowReference.Top + _windowReference.ActualHeight - notification.ActualHeight - _offsetY - GetNotificationOffset(notification);
                }
                else
                {
                    targetTop = SystemParameters.WorkArea.Bottom - notification.ActualHeight - _offsetY - GetNotificationOffset(notification);
                }
            }

            // Animate to new position
            notification.AnimateToPosition(targetTop);
        }
    }

    #endregion Private Methods

    #region Native Methods

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    #endregion Native Methods
}

/// <summary>
/// Notification window for displaying toast messages
/// </summary>
public class NotificationWindow : Window
{
    #region Private Fields

    private TextBlock _titleTextBlock;
    private TextBlock _messageTextBlock;
    private Button _closeButton;
    private Border _iconBorder;
    private TextBlock _iconText;
    private HwndSource _hwndSource;

    #endregion Private Fields

    #region Public Properties

    /// <summary>
    /// Gets or sets the notification message
    /// </summary>
    public string Message
    {
        get => _messageTextBlock?.Text ?? string.Empty;
        set
        {
            if (_messageTextBlock != null)
                _messageTextBlock.Text = value;
        }
    }

    /// <summary>
    /// Gets or sets the notification type
    /// </summary>
    public NotificationType NotificationType { get; set; }

    #endregion Public Properties

    #region Constructors

    /// <summary>
    /// Creates a new notification window
    /// </summary>
    public NotificationWindow()
    {
        // Configure window style
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Topmost = true;
        ShowInTaskbar = false;
        SizeToContent = SizeToContent.WidthAndHeight;
        ResizeMode = ResizeMode.NoResize;
        Background = Brushes.Transparent;
        MaxWidth = 400;

        // Create content
        CreateContent();

        // Initialize HwndSource
        SourceInitialized += (s, e) =>
        {
            _hwndSource = PresentationSource.FromVisual(this) as HwndSource;
        };
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Sets the window owner using a handle instead of a WPF window reference
    /// </summary>
    /// <param name="ownerHandle">Handle to the owner window</param>
    public void SetOwnerHandle(IntPtr ownerHandle)
    {
        if (ownerHandle == IntPtr.Zero)
            return;

        // If the window is not yet initialized, we need to wait
        if (!IsInitialized)
        {
            SourceInitialized += (s, e) => SetOwnerHandleCore(ownerHandle);
        }
        else
        {
            SetOwnerHandleCore(ownerHandle);
        }
    }

    /// <summary>
    /// Starts the entry animation for the notification
    /// </summary>
    public void StartEntryAnimation()
    {
        // Create and start the animation
        DoubleAnimation fadeInAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromMilliseconds(300)
        };

        this.BeginAnimation(OpacityProperty, fadeInAnimation);
    }

    /// <summary>
    /// Starts the close animation for the notification
    /// </summary>
    public void StartCloseAnimation()
    {
        // Create and start the animation
        DoubleAnimation fadeOutAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromMilliseconds(300)
        };
        fadeOutAnimation.Completed += (s, e) => Close();

        this.BeginAnimation(OpacityProperty, fadeOutAnimation);
    }

    /// <summary>
    /// Animates the notification to a new vertical position
    /// </summary>
    /// <param name="targetTop">The target top position</param>
    public void AnimateToPosition(double targetTop)
    {
        // Create and start the animation
        DoubleAnimation topAnimation = new DoubleAnimation
        {
            To = targetTop,
            Duration = TimeSpan.FromMilliseconds(200),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        this.BeginAnimation(TopProperty, topAnimation);
    }

    #endregion Public Methods

    #region Private Methods

    // Core implementation to set the owner handle
    private void SetOwnerHandleCore(IntPtr ownerHandle)
    {
        if (_hwndSource == null || _hwndSource.Handle == IntPtr.Zero)
            return;

        // Set the owner using native Win32 API
        SetWindowLongPtr(_hwndSource.Handle, GWLP_HWNDPARENT, ownerHandle);
    }

    // Creates the UI elements for the notification
    private void CreateContent()
    {
        // Create main container
        Grid mainGrid = new Grid
        {
            Margin = new Thickness(10)
        };
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // Create outer border with styling
        Border outerBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(245, 245, 245)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(12),
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                ShadowDepth = 3,
                BlurRadius = 5,
                Opacity = 0.3
            }
        };

        // Create icon
        _iconBorder = new Border
        {
            Width = 40,
            Height = 40,
            CornerRadius = new CornerRadius(20),
            Margin = new Thickness(0, 0, 12, 0),
        };

        _iconText = new TextBlock
        {
            FontFamily = new FontFamily("Segoe UI Symbol"),
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.White
        };

        _iconBorder.Child = _iconText;
        Grid.SetRowSpan(_iconBorder, 2);
        mainGrid.Children.Add(_iconBorder);

        // Create title text block
        _titleTextBlock = new TextBlock
        {
            FontWeight = FontWeights.Bold,
            FontSize = 14,
            Margin = new Thickness(0, 0, 0, 4),
            TextWrapping = TextWrapping.Wrap
        };
        Grid.SetColumn(_titleTextBlock, 1);
        mainGrid.Children.Add(_titleTextBlock);

        // Create message text block
        _messageTextBlock = new TextBlock
        {
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            Margin = new Thickness(0)
        };
        Grid.SetColumn(_messageTextBlock, 1);
        Grid.SetRow(_messageTextBlock, 1);
        mainGrid.Children.Add(_messageTextBlock);

        // Create close button
        _closeButton = new Button
        {
            Content = "✕",
            FontFamily = new FontFamily("Segoe UI Symbol"),
            Padding = new Thickness(5, 0, 5, 0),
            BorderThickness = new Thickness(0),
            Background = Brushes.Transparent,
            Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            Cursor = Cursors.Hand
        };
        _closeButton.Click += (s, e) => StartCloseAnimation();
        Grid.SetColumn(_closeButton, 2);
        mainGrid.Children.Add(_closeButton);

        // Add main grid to outer border
        outerBorder.Child = mainGrid;

        // Set the content
        Content = outerBorder;

        // Add mouse events for the entire window
        MouseLeftButtonDown += (s, e) => StartCloseAnimation();
        MouseEnter += (s, e) =>
        {
            // Add visual feedback on hover
            outerBorder.Background = new SolidColorBrush(Color.FromRgb(250, 250, 250));
        };
        MouseLeave += (s, e) =>
        {
            // Restore original background on leave
            outerBorder.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
        };
    }

    // Updates visual appearance based on notification type
    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        // Set appearance based on notification type
        switch (NotificationType)
        {
            case NotificationType.Information:
                _iconBorder.Background = new SolidColorBrush(Color.FromRgb(88, 150, 236));  // Blue
                _iconText.Text = "ℹ";
                break;

            case NotificationType.Success:
                _iconBorder.Background = new SolidColorBrush(Color.FromRgb(79, 186, 111));  // Green
                _iconText.Text = "✓";
                break;

            case NotificationType.Warning:
                _iconBorder.Background = new SolidColorBrush(Color.FromRgb(246, 187, 66));  // Orange
                _iconText.Text = "⚠";
                break;

            case NotificationType.Error:
                _iconBorder.Background = new SolidColorBrush(Color.FromRgb(232, 86, 86));   // Red
                _iconText.Text = "✕";
                break;
        }

        if (string.IsNullOrEmpty(Title))
        {
            _titleTextBlock.Visibility = Visibility.Collapsed;
        }
    }

    #endregion Private Methods

    #region Native Methods

    // Constants
    private const int GWLP_HWNDPARENT = -8;

    // PInvoke methods for setting window owner
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    // 64-bit compatible version for SetWindowLongPtr
    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    #endregion Native Methods
}