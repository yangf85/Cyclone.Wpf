using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Cyclone.UI.Controls
{
    public class HintBoxItem : ComboBoxItem
    {
        private HintBox _ParentHintBox => ItemsControl.ItemsControlFromItemContainer(this) as HintBox;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            e.Handled = true;
            _ParentHintBox?.NotifyHintBoxItemMouseLeftButtonDown(this);
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            e.Handled = true;
            _ParentHintBox?.NotifyHintBoxItemMouseEnter(this);
            base.OnMouseEnter(e);
        }

        internal void SetHighlight(bool highlight)
        {
            IsHighlighted = highlight;
        }
    }
}