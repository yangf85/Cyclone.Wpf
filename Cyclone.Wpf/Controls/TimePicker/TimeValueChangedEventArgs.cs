using System;

namespace Cyclone.Wpf.Controls
{
    public class TimeValueChangedEventArgs : EventArgs
    {
        public int Value { get; }

        public TimeValueChangedEventArgs(int value)
        {
            Value = value;
        }
    }
}