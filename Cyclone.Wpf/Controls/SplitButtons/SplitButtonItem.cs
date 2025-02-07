using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Controls;

public class SplitButtonItem :ButtonBase
{
    private SplitButton SplitButton
    {
        get
        {
            return ItemsControl.ItemsControlFromItemContainer(this) as SplitButton;
        }
    }

    public SplitButtonItem()
    {
        PreviewMouseLeftButtonUp += SplitButtonItem_PreviewMouseLeftButtonUp;
    }

    private void SplitButtonItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (SplitButton != null)
        {
            SplitButton.OnItemClick(this, this);
            SplitButton.IsDropDownOpen = false;
        }
    }
}
