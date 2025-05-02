using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 自定义颜色面板控件
/// </summary>
//public class CustomColorPalette : Control
//{
//    private WrapPanel _colorsPanel;

//    static CustomColorPalette()
//    {
//        DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomColorPalette),
//            new FrameworkPropertyMetadata(typeof(CustomColorPalette)));
//    }

//    #region CustomColors

//    public ObservableCollection<Color> CustomColors
//    {
//        get => (ObservableCollection<Color>)GetValue(CustomColorsProperty);
//        set => SetValue(CustomColorsProperty, value);
//    }

//    public static readonly DependencyProperty CustomColorsProperty =
//        DependencyProperty.Register(nameof(CustomColors), typeof(ObservableCollection<Color>), typeof(CustomColorPalette),
//        new PropertyMetadata(new ObservableCollection<Color>(), OnCustomColorsChanged));

//    private static void OnCustomColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
//    {
//        if (d is CustomColorPalette palette)
//        {
//            palette.UpdateColorButtons();
//        }
//    }

//    #endregion CustomColors

//    #region SelectedColor

//    public Color SelectedColor
//    {
//        get => (Color)GetValue(SelectedColorProperty);
//        set => SetValue(SelectedColorProperty, value);
//    }

//    public static readonly DependencyProperty SelectedColorProperty =
//        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(CustomColorPalette),
//        new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

//    #endregion SelectedColor

//    #region MaxColors

//    public int MaxColors
//    {
//        get => (int)GetValue(MaxColorsProperty);
//        set => SetValue(MaxColorsProperty, value);
//    }

//    public static readonly DependencyProperty MaxColorsProperty =
//        DependencyProperty.Register(nameof(MaxColors), typeof(int), typeof(CustomColorPalette),
//        new PropertyMetadata(16));

//    #endregion MaxColors

//    #region Events

//    public event EventHandler<ColorSelectedEventArgs> ColorSelected;

//    #endregion Events

//    #region Commands

//    public ICommand SelectColorCommand
//    {
//        get => (ICommand)GetValue(SelectColorCommandProperty);
//        set => SetValue(SelectColorCommandProperty, value);
//    }

//    public static readonly DependencyProperty SelectColorCommandProperty =
//        DependencyProperty.Register(nameof(SelectColorCommand), typeof(ICommand), typeof(CustomColorPalette));

//    public ICommand AddColorCommand
//    {
//        get => (ICommand)GetValue(AddColorCommandProperty);
//        set => SetValue(AddColorCommandProperty, value);
//    }

//    public static readonly DependencyProperty AddColorCommandProperty =
//        DependencyProperty.Register(nameof(AddColorCommand), typeof(ICommand), typeof(CustomColorPalette));

//    public ICommand ClearColorsCommand
//    {
//        get => (ICommand)GetValue(ClearColorsCommandProperty);
//        set => SetValue(ClearColorsCommandProperty, value);
//    }

//    public static readonly DependencyProperty ClearColorsCommandProperty =
//        DependencyProperty.Register(nameof(ClearColorsCommand), typeof(ICommand), typeof(CustomColorPalette));

//    #endregion Commands

//    public CustomColorPalette()
//    {
//        CustomColors = new ObservableCollection<Color>();
//    }

//    public override void OnApplyTemplate()
//    {
//        base.OnApplyTemplate();

//        // 获取模板部件
//        _colorsPanel = GetTemplateChild("PART_ColorsPanel") as WrapPanel;

//        // 初始化命令
//        SelectColorCommand = new RelayCommand(param =>
//        {
//            if (param is Color color)
//            {
//                SelectedColor = color;
//                ColorSelected?.Invoke(this, new ColorSelectedEventArgs(color));
//            }
//        });

//        AddColorCommand = new RelayCommand(_ => AddColor(SelectedColor));

//        ClearColorsCommand = new RelayCommand(_ => ClearColors());

//        // 初始化颜色面板
//        UpdateColorButtons();
//    }

//    /// <summary>
//    /// 添加颜色到自定义调色板
//    /// </summary>
//    public void AddColor(Color color)
//    {
//        // 检查颜色是否已存在
//        if (!CustomColors.Contains(color))
//        {
//            // 如果已达到最大数量，移除最早添加的颜色
//            if (CustomColors.Count >= MaxColors)
//            {
//                CustomColors.RemoveAt(0);
//            }

//            // 添加新颜色
//            CustomColors.Add(color);

//            // 保存到设置
//            SaveCustomColors();
//        }
//    }

//    /// <summary>
//    /// 清空自定义颜色
//    /// </summary>
//    public void ClearColors()
//    {
//        CustomColors.Clear();
//        SaveCustomColors();
//    }

//    /// <summary>
//    /// 保存自定义颜色到设置
//    /// </summary>
//    private void SaveCustomColors()
//    {
//        // 在实际应用中，这里可以保存到配置文件或用户设置
//        // 示例实现只更新UI
//        UpdateColorButtons();
//    }

//    /// <summary>
//    /// 更新颜色按钮
//    /// </summary>
//    private void UpdateColorButtons()
//    {
//        if (_colorsPanel == null)
//            return;

//        // 清空现有按钮
//        _colorsPanel.Children.Clear();

//        // 添加自定义颜色按钮
//        foreach (Color color in CustomColors)
//        {
//            AddColorButton(_colorsPanel, color);
//        }

//        // 添加"添加"按钮
//        Button addButton = new Button
//        {
//            Content = "+",
//            Width = 25,
//            Height = 25,
//            Margin = new Thickness(2),
//            ToolTip = "添加当前颜色到自定义调色板"
//        };

//        addButton.Command = AddColorCommand;

//        _colorsPanel.Children.Add(addButton);
//    }

//    private void AddColorButton(Panel panel, Color color)
//    {
//        // 创建颜色按钮
//        Button button = new Button
//        {
//            Background = new SolidColorBrush(color),
//            BorderBrush = new SolidColorBrush(Colors.Gray),
//            BorderThickness = new Thickness(1),
//            Margin = new Thickness(2),
//            Width = 25,
//            Height = 25,
//        };

//        // 右键菜单（用于删除）
//        ContextMenu contextMenu = new ContextMenu();
//        MenuItem deleteItem = new MenuItem { Header = "删除" };
//        deleteItem.Click += (s, e) =>
//        {
//            CustomColors.Remove(color);
//            SaveCustomColors();
//        };

//        contextMenu.Items.Add(deleteItem);
//        button.ContextMenu = contextMenu;

//        // 绑定命令
//        button.Command = SelectColorCommand;
//        button.CommandParameter = color;

//        // 添加到面板
//        panel.Children.Add(button);
//    }
//}