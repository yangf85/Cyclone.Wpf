using System.Windows;

namespace Cyclone.Wpf.Controls;

public interface ITransition
{
    void Start(FrameworkElement element);
    void Stop(FrameworkElement element);
    TimeSpan Duration { get; }
}






