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
/// 自定义的标记扩展类，用于转换枚举类型到绑定数据源，继承自MarkupExtension
/// 如果枚举对象未实现TypeConverter,那么会显示枚举的DescriptionAttribute,对应的类型将变为字符串
/// </summary>
[MarkupExtensionReturnType(typeof(IEnumerable))]
public class EnumItemsSourceExtension : MarkupExtension
{
    public EnumItemsSourceExtension()
    {
    }

    private Type _enumType;

    /// <summary>
    /// 枚举类型
    /// </summary>
    public Type EnumType
    {
        get { return _enumType; }

        set
        {
            if (value != _enumType)
            {
                if (null != value)
                {
                    // 获取传入值的基础类型，如果是可空类型则获取其基础类型，否则返回传入值本身
                    var enumType = Nullable.GetUnderlyingType(value) ?? value;
                    if (!enumType.IsEnum)
                    {
                        throw new ArgumentException("Type must be for an Enum");
                    }
                }

                _enumType = value;
            }
        }
    }

    /// <summary>
    /// 查看枚举类型是否定义有类型转换器
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    private bool HasEnumAttributeTypeConverter(Type enumType)
    {
        var typeConverterAttr = enumType.GetCustomAttribute<TypeConverterAttribute>();
        if (typeConverterAttr == null)
        {
            return false;
        }

        var baseType = typeof(EnumAttributeTypeConverter<>);

        var converterType = Type.GetType(typeConverterAttr.ConverterTypeName).GetGenericTypeDefinition();

        return baseType == converterType;
    }

    private IEnumerable GetEnumDescriptionAttribute(Type enumType)
    {
        var values = Enum.GetValues(enumType);

        foreach (var value in values)
        {
            var attr = value.GetType().GetField(value.ToString()).GetCustomAttribute<DescriptionAttribute>();
            yield return attr?.Description ?? value;
        }
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        //如果枚举类型为空，则抛出异常
        if (null == _enumType)
        {
            throw new InvalidOperationException("The EnumType must be specified.");
        }

        if (HasEnumAttributeTypeConverter(EnumType))
        {
            return Enum.GetValues(EnumType);
        }
        else
        {
            return GetEnumDescriptionAttribute(EnumType);
        }
    }
}