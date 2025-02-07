using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Markup.Primitives;
using System.Windows.Media;

namespace Cyclone.Wpf.Converters;

public abstract class BasicConverter : MarkupExtension, IValueConverter
{
    public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

    public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }
}

public sealed class FuncValueConverter<TIn, TOut> : IValueConverter
{
    private readonly Func<TIn?, TOut?> _convert;

    private readonly Func<TOut?, TIn?> _convertBack;

    public FuncValueConverter(Func<TIn?, TOut?> convert, Func<TOut?, TIn?> convertBack = null)
    {
        _convert = convert ?? throw new ArgumentNullException(nameof(convert));
        _convertBack = convertBack;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return DependencyProperty.UnsetValue;
        }

        var typeConverter = TypeDescriptor.GetConverter(typeof(TIn));

        if (value is not TIn obj)
        {
            if (typeConverter.CanConvertFrom(value.GetType()))
            {
                obj = (TIn)typeConverter.ConvertFrom(value);
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        return _convert(obj);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TOut targetValue)
        {
            return _convertBack(targetValue);
        }
        return default(TIn);
    }
}

public class FuncValueConverter<TIn, TParam, TOut> : IValueConverter
{
    private readonly Func<TIn?, TParam?, TOut> _convert;

    private readonly Func<TOut, TParam, TIn> _convertBack;

    public FuncValueConverter(Func<TIn?, TParam?, TOut> convert, Func<TOut, TParam, TIn> convertBack = null)
    {
        _convert = convert ?? throw new ArgumentNullException(nameof(convert));
        _convertBack = convertBack;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null)
        {
            return DependencyProperty.UnsetValue;
        }
        if (parameter is null)
        {
            return DependencyProperty.UnsetValue;
        }
        if (parameter is TParam param)
        {
            var typeConverter = TypeDescriptor.GetConverter(typeof(TIn));

            if (value is not TIn obj)
            {
                if (typeConverter.CanConvertFrom(value.GetType()))
                {
                    obj = (TIn)typeConverter.ConvertFrom(value);
                }
                else
                {
                    return DependencyProperty.UnsetValue;
                }
            }

            return _convert(obj, param);
        }
        return DependencyProperty.UnsetValue;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is TOut targetValue)
        {
            if (parameter is TParam param)
            {
                return _convertBack(targetValue, param);
            }
            return DependencyProperty.UnsetValue;
        }
        return default(TIn);
    }
}