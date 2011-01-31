using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Threading;
using GitWorkflows.Package.Extensions;

namespace GitWorkflows.Package.ViewModels
{
    abstract class ViewModel : INotifyPropertyChanged
    {
        private static readonly Dispatcher _dispatcher = Application.Current.Dispatcher;
        private static readonly Dictionary<string, PropertyChangedEventArgs> _propertyChangedArgsCache = new Dictionary<string, PropertyChangedEventArgs>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T backingVariable, T newValue, params Expression<Func<object>>[] expressions)
        {
            if ( Equals(backingVariable, newValue) )
                return false;

            backingVariable = newValue;
            RaisePropertyChanged(expressions);
            return true;
        }

        protected void RaisePropertyChanged(params string[] names)
        {
            var handler = PropertyChanged;
            if (handler == null) 
                return;

            #if DEBUG
                var invalidNames = names.Except(GetType().GetProperties().Select(p => p.Name));
                if (invalidNames.Any())
                {
                    throw new InvalidOperationException(
                        string.Format(
                            "The following properties are not found or not public in class {0}: {1}"
                            , GetType().Name
                            , string.Join(", ", invalidNames)
                        )
                    );
                }
            #endif

            ExecuteOnDispatcher( 
                () => names.ForEach(
                        name => 
                        {
                            PropertyChangedEventArgs args;
                            if (!_propertyChangedArgsCache.TryGetValue(name, out args))
                            {
                                args = new PropertyChangedEventArgs(name);
                                _propertyChangedArgsCache.Add(name, args);
                            }
                            
                            handler(this, args);
                        } 
                )
            );
        }

        protected void RaisePropertyChanged(params Expression<Func<object>>[] expressions)
        {
            if (PropertyChanged != null)
            {
                var propertyNames = expressions.Select(GetPropertyName).ToArray();
                RaisePropertyChanged(propertyNames);
            }
        }

        protected void RaiseAllPropertiesChanged()
        {
            if (PropertyChanged != null)
                RaisePropertyChanged(GetType().GetProperties().Select(p => p.Name).ToArray());
        }

        protected static void ExecuteOnDispatcher(Action action)
        {
            if (_dispatcher.CheckAccess())
                action();
            else
                _dispatcher.BeginInvoke(action);
        }

        private static string GetPropertyName(Expression<Func<object>> expression)
        {
            var member = expression.Body as MemberExpression;
            if (member == null)
                member = (MemberExpression)((UnaryExpression) expression.Body).Operand;

            Debug.Assert(member != null);
            return member.Member.Name;
        }
    }
}