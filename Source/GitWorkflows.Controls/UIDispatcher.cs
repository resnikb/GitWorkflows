using System;
using System.Windows;
using System.Windows.Threading;

namespace GitWorkflows.Controls
{
    public static class UIDispatcher
    {
        private static readonly Dispatcher _dispatcher = Application.Current.Dispatcher;

        public static void Schedule(Action action)
        {
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.BeginInvoke(action);
        }

        public static void Invoke(Action action)
        {
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.Invoke(action);
        }

        public static void BeginInvoke(Action action)
        { _dispatcher.BeginInvoke(action); }
    }
}