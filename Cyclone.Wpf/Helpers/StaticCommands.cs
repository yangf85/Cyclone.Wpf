using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Cyclone.Wpf.Helpers;

public class StaticCommands
{
    public ICommand ToggleSelectAll { get; } = new ToggleSelectAllImpl();

    private class ToggleSelectAllImpl : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (parameter is Selector selector)
            {
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}