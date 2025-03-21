using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 【枚举类型数据源转换扩展】
/// 功能：将枚举类型转换为可直接绑定到ItemsControl的可观察集合
/// 特性：
///   - 支持常规枚举类型和Nullable<Enum>类型
///   - 自动处理可空枚举类型，首项添加null值
///   - 内置类型验证确保输入为合法枚举类型
///
/// 使用示例：
/// <code>
/// <!-- XAML中使用 -->
/// xmlns:ext="clr-namespace:YourNamespace"
///
/// <ComboBox ItemsSource="{ext:EnumToItemsSource EnumType={x:Type local:WeekDays}}"/>
///
/// <!-- 支持可空枚举 -->
/// <ComboBox ItemsSource="{ext:EnumToItemsSource EnumType={x:Type local:Status?}}"/>
/// </code>
///
/// 典型应用场景：
/// 1. 动态生成枚举选项的下拉菜单
/// 2. 创建可空枚举类型的选项集合
/// 3. 需要数据绑定的枚举类型选择器
///
/// 注意事项：
/// - 必须通过EnumType属性指定合法枚举类型
/// - 可空枚举会自动生成包含null的集合
/// - 结果集合适用于ItemsControl及其派生控件
/// </remarks
public class EnumToItemsSourceExtension : MarkupExtension
{
    private Type _enumType;

    public Type EnumType
    {
        get { return _enumType; }
        set
        {
            if (value != _enumType)
            {
                if (null != value)
                {
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;
                    if (!enumType.IsEnum)
                    {
                        throw new ArgumentException("Type must bu for an Enum");
                    }
                }

                _enumType = value;
            }
        }
    }

    public EnumToItemsSourceExtension()
    {
    }

    public EnumToItemsSourceExtension(Type enumType)
    {
        EnumType = enumType;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (null == _enumType)
        {
            throw new InvalidOperationException("The EnumType must be specified.");
        }

        var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
        var enumValues = Enum.GetValues(actualEnumType);

        if (actualEnumType == _enumType)
        {
            return enumValues;
        }

        var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
        enumValues.CopyTo(tempArray, 1);

        return tempArray;
    }
}