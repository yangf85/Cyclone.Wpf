using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Cyclone.Wpf.Controls;

public class CarouselItem : ListBoxItem
{
    static CarouselItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CarouselItem), new FrameworkPropertyMetadata(typeof(CarouselItem)));
    }

    public CarouselItem()
    {
    }
}