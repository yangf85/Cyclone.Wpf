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

public class SplitButtonItem : ButtonBase
{
    private SplitButton _root;

    protected override void OnClick()
    {
        base.OnClick();

        if (_root != null)
        {
            _root.SelectedIndex = _root.ItemContainerGenerator.IndexFromContainer(this);
            _root.ItemClickCommand?.Execute(_root.ItemClickCommandParameter);
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _root = ItemsControl.ItemsControlFromItemContainer(this) as SplitButton;
    }
}