// IconTemplateSelector.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls;

/// <summary>
/// 根据内容类型选择合适的模板的选择器
/// </summary>
public class IconTemplateSelector : DataTemplateSelector
{
    /// <summary>
    /// 路径图标模板
    /// </summary>
    public DataTemplate PathTemplate { get; set; }

    /// <summary>
    /// 图片图标模板
    /// </summary>
    public DataTemplate ImageTemplate { get; set; }

    /// <summary>
    /// 字体图标模板
    /// </summary>
    public DataTemplate FontTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item == null)
        {
            return null;
        }

        // 根据内容类型选择模板
        if (item is Geometry)
        {
            return PathTemplate;
        }
        else if (item is ImageSource)
        {
            return ImageTemplate;
        }
        else if (item is string)
        {
            return FontTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}