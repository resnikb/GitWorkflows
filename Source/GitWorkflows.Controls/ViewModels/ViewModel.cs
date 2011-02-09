using System;
using System.Windows;
using System.Windows.Threading;
using GitWorkflows.Common;

namespace GitWorkflows.Controls.ViewModels
{
    public abstract class ViewModel : NotifyPropertyChanged
    {
        private static readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

        public static void ExecuteOnDispatcher(Action action)
        {
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.BeginInvoke(action);
        }

        protected override void ExecuteAction(Action action)
        { ExecuteOnDispatcher(action); }
    }
}