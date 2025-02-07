using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace Cyclone.Wpf.Controls;

public class RadioButtonGroup : Selector
{
    protected override bool IsItemItsOwnContainerOverride(object item)
    {
        return item is RadioButton;
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
        return new RadioButton() { GroupName = nameof(RadioButtonGroup) };
    }

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);

        if (element is RadioButton radioButton)
        {
            if (ItemsSource == null)
            {
                //当没有设置数据源的时候RadioButton的的DataContext就是Content,
                //但是按钮无法使用这个SelectedItem作为命令参数
                radioButton.DataContext = radioButton.Content;
            }
            else
            {
                radioButton.DataContext = item;
            }
        }
    }

    protected override void OnSelectionChanged(SelectionChangedEventArgs e)
    {
        base.OnSelectionChanged(e);

        if (e.AddedItems.Count > 0)
        {
            var item = e.AddedItems[0];
            if (item is RadioButton radioButton)
            {
                radioButton.IsChecked = true;
            }
            else
            {
                if (ItemContainerGenerator.ContainerFromItem(item) is RadioButton container)
                {
                    container.IsChecked = true;
                }
            }

           
        }
        
    }

  


}
