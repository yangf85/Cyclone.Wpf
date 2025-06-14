using Cyclone.Wpf.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// 表示枚举项的包装类，用于UI绑定
    /// </summary>
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

        public override string ToString()
        {
            return IsUseAlias ? GetEnumDescription() : Enum.ToString();
        }
    }

    /// <summary>
    /// 枚举选择器控件，支持单选和多选(Flags)枚举
    /// </summary>
    [TemplatePart(Name = "PART_ItemsContainer", Type = typeof(ListBox))]
    public class EnumSelector : Control
    {
        private readonly string _radioButtonGroupName;
        private ListBox _listBox;
        private bool _isUpdatingSelection = false;

        static EnumSelector()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumSelector), new FrameworkPropertyMetadata(typeof(EnumSelector)));
        }

        public EnumSelector()
        {
            // 为每个实例生成唯一的 RadioButton 组名
            _radioButtonGroupName = $"EnumSelector_{Guid.NewGuid():N}";
        }

        #region 依赖属性

        #region Rows

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(nameof(Rows), typeof(int), typeof(EnumSelector), new PropertyMetadata(0));

        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        #endregion Rows

        #region Columns

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(string), typeof(EnumSelector), new PropertyMetadata(string.Empty));

        public string Columns
        {
            get => (string)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        #endregion Columns

        #region EnumType

        private ObservableCollection<EnumObject> _enums;

        public static readonly DependencyProperty EnumTypeProperty =
            DependencyProperty.Register(nameof(EnumType), typeof(Type), typeof(EnumSelector), new PropertyMetadata(OnEnumTypeChanged));

        public Type EnumType
        {
            get => (Type)GetValue(EnumTypeProperty);
            set => SetValue(EnumTypeProperty, value);
        }

        private static void OnEnumTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as EnumSelector;
            selector?.Dispatcher.BeginInvoke(new Action(() =>
            {
                selector.UpdateItemsSource();
                selector.UpdateHasFlags();
            }), DispatcherPriority.Loaded);
        }

        #endregion EnumType

        #region SelectedEnum

        public static readonly DependencyProperty SelectedEnumProperty =
            DependencyProperty.Register(nameof(SelectedEnum), typeof(Enum), typeof(EnumSelector),
                new FrameworkPropertyMetadata(default(Enum), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedEnumChanged));

        public Enum SelectedEnum
        {
            get => (Enum)GetValue(SelectedEnumProperty);
            set => SetValue(SelectedEnumProperty, value);
        }

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

        public static readonly DependencyProperty IsUseAliasProperty =
            DependencyProperty.Register(nameof(IsUseAlias), typeof(bool), typeof(EnumSelector), new PropertyMetadata(true, OnIsUseAliasChanged));

        public bool IsUseAlias
        {
            get => (bool)GetValue(IsUseAliasProperty);
            set => SetValue(IsUseAliasProperty, value);
        }

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

        #region HasFlags

        private static readonly DependencyPropertyKey HasFlagsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(HasFlags), typeof(bool), typeof(EnumSelector),
                new PropertyMetadata(false));

        public static readonly DependencyProperty HasFlagsProperty = HasFlagsPropertyKey.DependencyProperty;

        public bool HasFlags
        {
            get => (bool)GetValue(HasFlagsProperty);
            private set => SetValue(HasFlagsPropertyKey, value);
        }

        #endregion HasFlags

        #region RadioButtonGroupName

        public string RadioButtonGroupName
        {
            get => _radioButtonGroupName;
        }

        #endregion RadioButtonGroupName

        #endregion 依赖属性

        #region 重写方法

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // 先移除旧事件（如果有）
            if (_listBox != null)
            {
                _listBox.SelectionChanged -= ListBox_SelectionChanged;
            }

            _listBox = GetTemplateChild("PART_ItemsContainer") as ListBox;

            if (_listBox != null)
            {
                UpdateItemsSource();
                _listBox.SelectionChanged += ListBox_SelectionChanged;
            }
        }

        #endregion 重写方法

        #region 事件处理

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSelection || _listBox == null || _enums == null) { return; }

            _isUpdatingSelection = true;

            try
            {
                if (HasFlaggsAttribute())
                {
                    // 检查是否有新选中的项
                    foreach (EnumObject addedItem in e.AddedItems)
                    {
                        // 处理选中的组合值
                        HandleEnumItemSelection(addedItem, true);
                    }

                    // 检查是否有取消选中的项
                    foreach (EnumObject removedItem in e.RemovedItems)
                    {
                        // 处理取消选中的组合值
                        HandleEnumItemSelection(removedItem, false);
                    }

                    // 更新SelectedEnum属性
                    UpdateSelectedEnumFromSelections();
                }
                else
                {
                    // 处理普通枚举（RadioButton 模式）
                    // 对于 RadioButton，我们需要处理互斥逻辑
                    if (e.AddedItems.Count > 0)
                    {
                        var selectedItem = e.AddedItems[0] as EnumObject;
                        if (selectedItem != null)
                        {
                            // 确保只有一个项被选中
                            foreach (var item in _enums)
                            {
                                if (item != selectedItem)
                                {
                                    item.IsSelected = false;
                                }
                            }
                            selectedItem.IsSelected = true;
                            SelectedEnum = selectedItem.Enum;
                        }
                    }
                }
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }

        #endregion 事件处理

        #region 私有方法

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

                // 订阅每个 EnumObject 的 PropertyChanged 事件以处理 RadioButton 的选择
                foreach (var enumObj in _enums)
                {
                    enumObj.PropertyChanged += EnumObject_PropertyChanged;
                }

                _listBox.ItemsSource = _enums;

                // 更新初始选择
                if (SelectedEnum != null)
                {
                    UpdateSelectionFromSelectedEnum();
                }
            }
        }

        private void EnumObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(EnumObject.IsSelected) && !_isUpdatingSelection && !HasFlaggsAttribute())
            {
                _isUpdatingSelection = true;
                try
                {
                    var changedItem = sender as EnumObject;
                    if (changedItem != null && changedItem.IsSelected)
                    {
                        // 对于 RadioButton 模式，确保只有一个选中
                        foreach (var item in _enums)
                        {
                            if (item != changedItem)
                            {
                                item.IsSelected = false;
                            }
                        }
                        SelectedEnum = changedItem.Enum;
                    }
                }
                finally
                {
                    _isUpdatingSelection = false;
                }
            }
        }

        private void UpdateHasFlags()
        {
            HasFlags = HasFlaggsAttribute();
        }

        private void UpdateSelectionFromSelectedEnum()
        {
            if (_enums == null || SelectedEnum == null)
                return;

            if (HasFlaggsAttribute())
            {
                // 处理[Flags]枚举
                int selectedValue = Convert.ToInt32(SelectedEnum);

                // 创建枚举值关系映射
                var enumValueMap = CreateEnumValueMap();

                // 首先取消所有选择
                foreach (var item in _enums)
                {
                    item.IsSelected = false;
                }

                // 然后设置基本选择
                foreach (var item in _enums)
                {
                    int itemValue = Convert.ToInt32(item.Enum);
                    // 检查当前标志位是否被设置
                    if (itemValue != 0 && (selectedValue & itemValue) == itemValue)
                    {
                        item.IsSelected = true;
                    }
                }

                // 最后更新复合选项状态
                UpdateAllCompositeItems(enumValueMap);
            }
            else
            {
                // 处理普通枚举（RadioButton 模式）
                int selectedValue = Convert.ToInt32(SelectedEnum);
                foreach (var item in _enums)
                {
                    item.IsSelected = Convert.ToInt32(item.Enum) == selectedValue;
                }
            }
        }

        // 处理枚举项的选择或取消选择，包括处理复合关系
        private void HandleEnumItemSelection(EnumObject item, bool isSelected)
        {
            int itemValue = Convert.ToInt32(item.Enum);

            // 首先更新当前项
            item.IsSelected = isSelected;

            // 创建所有枚举值的映射表，用于分析组合关系
            var enumValueMap = CreateEnumValueMap();

            if (isSelected)
            {
                // 当选中一个项时，选中其所有子项（包含在该值中的所有基本标志）
                UpdateChildItemsSelection(itemValue, true, enumValueMap);
            }
            else
            {
                // 当取消选中一个项时，有两种情况处理：

                // 1. 如果是组合值，取消选中它的所有子项
                if (IsCompositeValue(itemValue))
                {
                    UpdateChildItemsSelection(itemValue, false, enumValueMap);
                }

                // 2. 总是取消选中包含它的所有复合项（父项）
                UpdateParentItemsSelection(itemValue, false, enumValueMap);
            }

            // 最后检查和更新所有复合项的状态
            UpdateAllCompositeItems(enumValueMap);
        }

        // 创建枚举值的组合关系映射
        private Dictionary<int, List<int>> CreateEnumValueMap()
        {
            var result = new Dictionary<int, List<int>>();

            // 获取所有枚举值
            var allValues = _enums.Select(e => Convert.ToInt32(e.Enum)).ToList();

            // 对每个值，查找其所有可能的子值组合
            foreach (int value in allValues)
            {
                result[value] = new List<int>();
                foreach (int otherValue in allValues)
                {
                    // 如果otherValue是value的非零子集，则添加到该value的子值列表中
                    if (otherValue != 0 && otherValue != value && (value & otherValue) == otherValue)
                    {
                        result[value].Add(otherValue);
                    }
                }
            }

            return result;
        }

        // 更新所有子项的选择状态
        private void UpdateChildItemsSelection(int value, bool isSelected, Dictionary<int, List<int>> enumValueMap)
        {
            if (!enumValueMap.ContainsKey(value))
                return;

            // 获取该值的所有子值
            var childValues = enumValueMap[value];

            foreach (int childValue in childValues)
            {
                // 找到对应的EnumObject并更新其选择状态
                var childItem = _enums.FirstOrDefault(e => Convert.ToInt32(e.Enum) == childValue);
                if (childItem != null)
                {
                    childItem.IsSelected = isSelected;
                }
            }
        }

        // 更新所有父项的选择状态
        private void UpdateParentItemsSelection(int value, bool isSelected, Dictionary<int, List<int>> enumValueMap)
        {
            if (isSelected)
                return; // 只有在取消选中时才需要更新父项

            // 找到所有包含该值的复合值（父项）
            foreach (var entry in enumValueMap)
            {
                if (entry.Value.Contains(value))
                {
                    // 找到对应的EnumObject并取消选中
                    var parentItem = _enums.FirstOrDefault(e => Convert.ToInt32(e.Enum) == entry.Key);
                    if (parentItem != null)
                    {
                        parentItem.IsSelected = false;
                    }
                }
            }
        }

        // 更新所有复合项的状态
        private void UpdateAllCompositeItems(Dictionary<int, List<int>> enumValueMap)
        {
            foreach (var entry in enumValueMap)
            {
                // 跳过没有子项的基本值
                if (entry.Value.Count == 0)
                    continue;

                var compositeItem = _enums.FirstOrDefault(e => Convert.ToInt32(e.Enum) == entry.Key);
                if (compositeItem != null)
                {
                    // 检查该复合值的所有子值是否都被选中
                    bool allChildrenSelected = true;
                    foreach (int childValue in entry.Value)
                    {
                        var childItem = _enums.FirstOrDefault(e => Convert.ToInt32(e.Enum) == childValue);
                        if (childItem == null || !childItem.IsSelected)
                        {
                            allChildrenSelected = false;
                            break;
                        }
                    }

                    // 只有当所有子值都被选中时，才选中复合值
                    compositeItem.IsSelected = allChildrenSelected;
                }
            }
        }

        // 根据当前选择更新SelectedEnum属性
        private void UpdateSelectedEnumFromSelections()
        {
            int result = 0;

            // 使用所有选中项计算结果
            foreach (var item in _enums)
            {
                if (item.IsSelected)
                {
                    int itemValue = Convert.ToInt32(item.Enum);
                    result |= itemValue;
                }
            }

            SelectedEnum = (Enum)Enum.ToObject(EnumType, result);
        }

        // 判断一个值是否是组合值（由多个基本值组合而成）
        private bool IsCompositeValue(int value)
        {
            if (value == 0)
                return false;

            var enumValueMap = CreateEnumValueMap();
            return enumValueMap.ContainsKey(value) && enumValueMap[value].Count > 0;
        }

        private bool HasFlaggsAttribute()
        {
            if (EnumType == null) { return false; }

            var attributes = EnumType.GetCustomAttribute<FlagsAttribute>();
            return attributes != null;
        }

        #endregion 私有方法
    }
}