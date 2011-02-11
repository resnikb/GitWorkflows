using System;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.ViewModel;

namespace GitWorkflows.Common
{
    public class NotifyPropertyChanged : NotificationObject
    {
        protected bool SetProperty<T>(ref T backingVariable, T newValue, Expression<Func<T>> expression)
        {
            if ( Equals(backingVariable, newValue) )
                return false;

            backingVariable = newValue;
            RaisePropertyChanged(expression);
            return true;
        }
    }
}