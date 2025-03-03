using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Cyclone.Wpf.Helpers;

/// <summary>
/// 泛型枚举特性值转换器，用于将枚举值转换为特性的属性值
/// </summary>
/// <typeparam name="TAttribute">要获取的特性类型</typeparam>
public class EnumAttributeTypeConverter<TAttribute> : EnumConverter where TAttribute : Attribute
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    public EnumAttributeTypeConverter(Type type) : base(type)
    {
    }

    /// <summary>
    /// 枚举字段所使用的特性类型
    /// </summary>
    public TAttribute AttributeType { get; set; }

    /// <summary>
    /// 获取转换器实际使用的特性类型
    /// </summary>
    /// <returns></returns>
    private Type FindTypeConvetrAttributeType()
    {
        var attr = EnumType.GetCustomAttribute<TypeConverterAttribute>();
        var name = attr.ConverterTypeName;
        var property = Type.GetType(name, true).GetProperty(nameof(AttributeType));
        return property.PropertyType;
    }

    private string GetAttributeValueFromEnum(Enum @enum)
    {
        var type = FindTypeConvetrAttributeType();
        if (type == null)
        {
            return @enum.ToString();
        }

        var fieldInfo = EnumType.GetField(@enum.ToString());

        var attr = fieldInfo.GetCustomAttribute(type);
        if (attr == null)
        {
            return @enum.ToString();
        }

        var properties = attr.GetType().GetProperties();

        var list = new List<string>();
        foreach (var property in properties)
        {
            if (property.Name == "TypeId")
            {
                continue;
            }
            list.Add(property.GetValue(attr).ToString());
        }

        return string.Join(" ", list); ;
    }

    private Enum GetEnumFromAttributeValue(string attributeValue)
    {
        var enumValues = Enum.GetValues(EnumType);
        var attributeValues = attributeValue.Split(' ');
        foreach (Enum enumValue in enumValues)
        {
            var str = GetAttributeValueFromEnum(enumValue);
            var enumAttributeValues = str.Split(' ');
            if (enumAttributeValues.SequenceEqual(attributeValues))
            {
                return enumValue;
            }
        }
        return null;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="culture"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
        //var str = value.ToString();
        //var enumValue = GetEnumFromAttributeValue(str);
        return value;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="context"></param>
    /// <param name="culture"></param>
    /// <param name="value"></param>
    /// <param name="destinationType"></param>
    /// <returns></returns>
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    {
        var str = GetAttributeValueFromEnum((Enum)value);
        return str;
    }
}