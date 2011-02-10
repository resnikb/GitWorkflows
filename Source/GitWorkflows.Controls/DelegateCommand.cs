using System;
using System.Diagnostics;
using System.Windows.Input;

namespace GitWorkflows.Controls
{
    public class DelegateCommand<T> : ICommand
    {
        private static readonly EventArgs _emptyEventArgs = new EventArgs();

        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public event EventHandler CanExecuteChanged;

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        { return _canExecute == null || _canExecute((T)parameter); }

        public void Execute(object parameter)
        {
            Debug.Assert(CanExecute(parameter));
            _execute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
                handler(this, _emptyEventArgs);
        }
    }
}