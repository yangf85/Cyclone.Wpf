using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cyclone.Wpf.Controls
{
    public class BreadCrumbBar : Selector
    {
        static BreadCrumbBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadCrumbBar), new FrameworkPropertyMetadata(typeof(BreadCrumbBar)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is BreadCrumbBarItem;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new BreadCrumbBarItem();
        }
    }
}