using System.Runtime.InteropServices;
using System.Windows.Markup;
using System.Windows;

// 将 ComVisible 设置为 false 会使此程序集中的类型对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，请将该类型的 ComVisible
// 属性设置为 true。

// 主题特定资源词典所处位置(未在页面中找到资源时候才使用,或者应用程序资源字典中找到时使用
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: ComVisible(true)]

// 如果此项目向 COM 公开，则下列 GUID 用于 typelib 的 ID。

[assembly: Guid("f69d36bb-0a6a-4019-813b-20c6759d9398")]


//添加统一的网址命名空间
[assembly: XmlnsPrefix("https://www.cyclone/wpf", "cy")]
[assembly: XmlnsDefinition("https://www.cyclone/wpf", "Cyclone.Wpf")]
//[assembly: XmlnsDefinition("https://www.cyclone/wpf", "Cyclone.Wpf.Resources")]
//[assembly: XmlnsDefinition("https://www.cyclone/wpf", "Cyclone.Wpf.Styles")]
[assembly: XmlnsDefinition("https://www.cyclone/wpf", "Cyclone.Wpf.Controls")]
[assembly: XmlnsDefinition("https://www.cyclone/wpf", "Cyclone.Wpf.Helpers")]
[assembly: XmlnsDefinition("https://www.cyclone/wpf", "Cyclone.Wpf.Converters")]