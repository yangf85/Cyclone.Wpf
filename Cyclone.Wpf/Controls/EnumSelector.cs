using Cyclone.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

internal class EnumObject : NotificationObject
{
    public EnumObject(Enum @enum, bool isUseAlias = true)
    {
        Enum = @enum;

        IsUseAlias = isUseAlias;
    }

    public bool IsSelected
    {
        get => field;
        set => Set(ref field, value);
    }

    public Enum Enum
    {
        get => field;
        set => Set(ref field, value);
    }

    public bool IsUseAlias
    {
        get => field;
        set => Set(ref field, value);
    }

    public override string ToString()
    {
        return IsUseAlias ? GetEnumDescription() : Enum.ToString();
    }

    private string GetEnumDescription()
    {
        // 获取枚举值的字段信息
        FieldInfo fieldInfo = Enum.GetType().GetField(Enum.ToString());

        if (fieldInfo == null)
            return Enum.ToString();

        // 如果找到Description特性，返回描述内容，否则返回枚举名称
        return fieldInfo.GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute attr
            ? attr.Description
            : Enum.ToString();
    }
}

[TemplatePart(Name = "PART_ItemsContainer", Type = typeof(ListBox))]
public class EnumSelector : Control
{
    private ListBox _listBox;
    private bool _isUpdatingSelection = false;

    static EnumSelector()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumSelector), new FrameworkPropertyMetadata(typeof(EnumSelector)));
    }

    #region Rows

    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    public static readonly DependencyProperty RowsProperty =
        DependencyProperty.Register(nameof(Rows), typeof(int), typeof(EnumSelector), new PropertyMetadata(default(int)));

    #endregion Rows

    #region Columns

    public string Columns
    {
        get => (string)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(string), typeof(EnumSelector), new PropertyMetadata(default(string)));

    #endregion Columns

    #region EnumType

    private ObservableCollection<EnumObject> _enums;

    public Type EnumType
    {
        get => (Type)GetValue(EnumTypeProperty);
        set => SetValue(EnumTypeProperty, value);
    }

    public static readonly DependencyProperty EnumTypeProperty =
        DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumSelector), new PropertyMetadata(OnEnumTypeChanged));

    private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = d as EnumSelector;
        selector?.Dispatcher.BeginInvoke(new Action(() =>
        {
            selector.UpdateItemsSource();
        }), DispatcherPriority.Loaded);
    }

    #endregion EnumType

    #region SelectedEnum

    public Enum SelectedEnum
    {
        get => (Enum)GetValue(SelectedEnumProperty);
        set => SetValue(SelectedEnumProperty, value);
    }

    public static readonly DependencyProperty SelectedEnumProperty =
        DependencyProperty.Register(nameof(SelectedEnum), typeof(Enum), typeof(EnumSelector),
            new FrameworkPropertyMetadata(default(Enum), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedEnumChanged));

    private static void OnSelectedEnumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = d as EnumSelector;
        if (selector != null && !selector._isUpdatingSelection)
        {
            selector._isUpdatingSelection = true;
            selector.UpdateSelectionFromSelectedEnum();
            selector._isUpdatingSelection = false;
        }
    }

    #endregion SelectedEnum

    #region IsUseAlias

    public bool IsUseAlias
    {
        get => (bool)GetValue(IsUseAliasProperty);
        set => SetValue(IsUseAliasProperty, value);
    }

    public static readonly DependencyProperty IsUseAliasProperty =
        DependencyProperty.Register(nameof(IsUseAlias), typeof(bool), typeof(EnumSelector), new PropertyMetadata(true, OnIsUseAliasChanged));

    private static void OnIsUseAliasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = d as EnumSelector;
        if (selector?._enums != null)
        {
            foreach (var item in selector._enums)
            {
                item.IsUseAlias = (bool)e.NewValue;
            }
        }
    }

    #endregion IsUseAlias

    #region Override

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _listBox = GetTemplateChild("PART_ItemsContainer") as ListBox;

        if (_listBox != null)
        {
            UpdateItemsSource();
            _listBox.SelectionChanged += ListBox_SelectionChanged;
        }
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingSelection || _listBox == null || _enums == null)
            return;

        _isUpdatingSelection = true;

        if (HasFlaggsAttribute())
        {
            // 处理[Flags]枚举，多选模式
            int result = 0;
            foreach (var item in _enums.Where(i => i.IsSelected))
            {
                result |= Convert.ToInt32(item.Enum);
            }
            SelectedEnum = (Enum)Enum.ToObject(EnumType, result);
        }
        else
        {
            // 处理普通枚举，单选模式
            var selectedItem = _enums.FirstOrDefault(i => i.IsSelected);
            if (selectedItem != null)
            {
                SelectedEnum = selectedItem.Enum;
            }
        }

        _isUpdatingSelection = false;
    }

    #endregion Override

    #region Private

    private void UpdateItemsSource()
    {
        if (_listBox != null && EnumType != null)
        {
            if (HasFlaggsAttribute())
            {
                _listBox.SelectionMode = SelectionMode.Multiple;
            }
            else
            {
                _listBox.SelectionMode = SelectionMode.Single;
            }

            var values = Enum.GetValues(EnumType).Cast<Enum>().Select(i => new EnumObject(i, IsUseAlias));
            _enums = new ObservableCollection<EnumObject>(values);
            _listBox.ItemsSource = _enums;

            for (int i = 0; i < _enums.Count; i++)
            {
                var item = _listBox.ItemContainerGenerator.ContainerFromIndex(i) as ListBoxItem;
                if (item != null)
                {
                    var binding = new Binding("IsSelected");
                    binding.Source = _enums[i];
                    binding.Mode = BindingMode.TwoWay;
                    BindingOperations.SetBinding(item, ListBoxItem.IsSelectedProperty, binding);
                }
            }

            // 更新初始选择
            if (SelectedEnum != null)
            {
                UpdateSelectionFromSelectedEnum();
            }
        }
    }

    private void UpdateSelectionFromSelectedEnum()
    {
        if (_enums == null || SelectedEnum == null)
            return;

        if (HasFlaggsAttribute())
        {
            // 处理[Flags]枚举
            int selectedValue = Convert.ToInt32(SelectedEnum);
            foreach (var item in _enums)
            {
                int itemValue = Convert.ToInt32(item.Enum);
                // 检查当前标志位是否被设置
                item.IsSelected = (selectedValue & itemValue) == itemValue && itemValue != 0;
            }
        }
        else
        {
            // 处理普通枚举
            int selectedValue = Convert.ToInt32(SelectedEnum);
            foreach (var item in _enums)
            {
                item.IsSelected = Convert.ToInt32(item.Enum) == selectedValue;
            }
        }
    }

    private bool HasFlaggsAttribute()
    {
        if (EnumType == null) { return false; }

        var attributes = EnumType.GetCustomAttribute<FlagsAttribute>();
        return attributes != null;
    }

    #endregion Private
}