using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Cyclone.Wpf.Helpers;

public static class ElementHelper
{
    public static T? TryFindVisualParent<T>(this DependencyObject child) where T : DependencyObject
    {
        while (true)
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            switch (parentObject)
            {
                case null:
                    return null;

                case T parent:
                    return parent;

                default:
                    child = parentObject;
                    continue;
            }
        }
    }

    public static T? TryFindLogicalParent<T>(this DependencyObject child) where T : DependencyObject
    {
        while (true)
        {
            var parentObject = LogicalTreeHelper.GetParent(child);
            if (parentObject == null)
                return null;

            if (parentObject is T parent)
            {
                return parent;
            }

            child = parentObject;
        }
    }

}