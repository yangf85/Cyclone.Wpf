using System.Windows;
using System.Windows.Controls;

namespace Cyclone.Wpf.Controls
{
    /// <summary>
    /// HintBox 的项容器，继承自 ComboBoxItem
    /// </summary>
    public class HintBoxItem : ComboBoxItem
    {
        static HintBoxItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HintBoxItem),
                new FrameworkPropertyMetadata(typeof(HintBoxItem)));
        }

        public HintBoxItem()
        {
        }

        // 如果需要自定义行为，可以重写相应方法
        protected override void OnSelected(RoutedEventArgs e)
        {
            base.OnSelected(e);

            // 选中时关闭下拉框
            if (Parent is HintBox hintBox)
            {
                hintBox.IsDropDownOpen = false;
            }
        }
    }
}