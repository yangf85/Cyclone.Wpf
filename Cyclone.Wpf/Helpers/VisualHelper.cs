using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;

namespace Cyclone.Wpf.Helpers;

public static class VisualHelper
{
    public static T TryFindParent<T>(this DependencyObject child) where T : DependencyObject
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
}