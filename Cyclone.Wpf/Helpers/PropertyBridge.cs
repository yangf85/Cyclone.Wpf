using System.ComponentModel;
using System.Windows;

namespace Cyclone.Wpf.Helpers
{
    /// <summary>
    /// 【属性桥接器】
    /// 解决 WPF 中复杂双向数据传递场景，通过附加属性机制实现任意控件与数据源之间的属性值同步。
    /// 特别适用于标准绑定机制难以实现的场景，支持自定义转换和类型适配。
    ///
    /// ▶ 典型使用场景：
    ///   1. 自定义控件需要将用户输入反向更新到特定数据模型
    ///   2. 需要在运行时动态建立属性间关联
    ///   3. 跨控件层级的数据传递
    ///   4. 需要应用转换器的双向数据流
    ///
    /// ▶ XAML 使用示例：
    /// <TextBox Text="{Binding MyText}"
    ///          local:PropertyBridge.Source="{Binding ViewModel}"
    ///          local:PropertyBridge.SourceProperty="TextProperty"
    ///          local:PropertyBridge.TargetProperty="{Binding Text, RelativeSource={RelativeSource Self}}" />
    ///
    /// ▶ 注意事项：
    ///   - Source 必须是一个有效的数据对象引用
    ///   - SourceProperty 必须是 Source 对象上存在的可写属性名
    ///   - 支持转换器，会自动应用绑定中定义的转换器进行类型转换
    /// </summary>
    public static class PropertyBridge
    {
        #region Source

        /// <summary>
        /// 获取源数据对象
        /// </summary>
        public static object GetSource(DependencyObject obj) => obj.GetValue(SourceProperty);

        /// <summary>
        /// 设置源数据对象，即数据的来源对象
        /// </summary>
        public static void SetSource(DependencyObject obj, object value) => obj.SetValue(SourceProperty, value);

        /// <summary>
        /// 源数据对象依赖属性
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.RegisterAttached("Source", typeof(object), typeof(PropertyBridge), new PropertyMetadata(null));

        #endregion Source

        #region SourceProperty

        /// <summary>
        /// 获取源属性名
        /// </summary>
        public static string GetSourceProperty(DependencyObject obj) => (string)obj.GetValue(SourcePropertyProperty);

        /// <summary>
        /// 设置源属性名，即源对象上的属性名称
        /// </summary>
        public static void SetSourceProperty(DependencyObject obj, string value) => obj.SetValue(SourcePropertyProperty, value);

        /// <summary>
        /// 源属性名依赖属性
        /// </summary>
        public static readonly DependencyProperty SourcePropertyProperty =
            DependencyProperty.RegisterAttached("SourceProperty", typeof(string), typeof(PropertyBridge), new PropertyMetadata(null));

        #endregion SourceProperty

        #region TargetProperty

        /// <summary>
        /// 获取目标属性值
        /// </summary>
        public static object GetTargetProperty(DependencyObject obj) => obj.GetValue(TargetPropertyProperty);

        /// <summary>
        /// 设置目标属性值，从这个属性的变化会触发值向源属性的回传
        /// </summary>
        public static void SetTargetProperty(DependencyObject obj, object value) => obj.SetValue(TargetPropertyProperty, value);

        /// <summary>
        /// 目标属性依赖属性
        /// </summary>
        public static readonly DependencyProperty TargetPropertyProperty =
            DependencyProperty.RegisterAttached("TargetProperty", typeof(object), typeof(PropertyBridge), new PropertyMetadata(OnTargetPropertyChanged));

        #endregion TargetProperty

        #region BindingInitialized

        /// <summary>
        /// 获取绑定是否已初始化标志
        /// </summary>
        public static bool GetBindingProxyInitialized(DependencyObject obj) => (bool)obj.GetValue(BindingProxyInitializedProperty);

        /// <summary>
        /// 设置绑定是否已初始化标志
        /// </summary>
        public static void SetBindingProxyInitialized(DependencyObject obj, bool value) => obj.SetValue(BindingProxyInitializedProperty, value);

        /// <summary>
        /// 绑定初始化状态依赖属性
        /// </summary>
        public static readonly DependencyProperty BindingProxyInitializedProperty =
            DependencyProperty.RegisterAttached("BindingProxyInitialized", typeof(bool), typeof(PropertyBridge), new PropertyMetadata(false));

        #endregion BindingInitialized

        /// <summary>
        /// 当目标属性值变化时，将值回传到源对象的源属性
        /// </summary>
        private static void OnTargetPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var initialized = GetBindingProxyInitialized(target);
            if (!initialized)
            {
                // 首次调用时仅初始化标志，不进行值回传
                // 这是因为此时绑定流尚未完全建立
                SetBindingProxyInitialized(target, true);
                return;
            }

            // 获取目标元素，确保是 FrameworkElement
            var frameworkElement = target as FrameworkElement;
            if (frameworkElement == null) return;

            // 获取目标属性的描述信息
            var dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromName(
                TargetPropertyProperty.Name, typeof(PropertyBridge), target.GetType());
            if (dependencyPropertyDescriptor == null) return;

            // 获取目标属性的绑定表达式
            var dependencyProperty = dependencyPropertyDescriptor.DependencyProperty;
            var bindingExpression = frameworkElement.GetBindingExpression(dependencyProperty);

            if (bindingExpression?.ResolvedSource == null || bindingExpression.ResolvedSourcePropertyName == null) return;

            // 获取源对象和源属性名
            var source = GetSource(target);
            var sourceProperty = GetSourceProperty(target);

            if (source == null || sourceProperty == null) return;

            // 获取源属性的反射信息
            var propertyInfo = source.GetType().GetProperty(sourceProperty);
            if (propertyInfo == null) return;

            // 获取当前绑定到目标属性的值
            var targetValue = frameworkElement.GetValue(bindingExpression.TargetProperty);
            var typedVal = targetValue;

            // 如果有转换器，则应用转换器的反向转换
            var converter = bindingExpression.ParentBinding.Converter;
            if (converter != null)
                typedVal = converter.ConvertBack(typedVal, propertyInfo.PropertyType,
                    bindingExpression.ParentBinding.ConverterParameter,
                    bindingExpression.ParentBinding.ConverterCulture);

            // 通过反射将转换后的值设置到源对象的属性上
            propertyInfo.SetValue(source, typedVal);
        }
    }
}