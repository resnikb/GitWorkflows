using System;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.ViewModel;

namespace GitWorkflows.Controls.ViewModels
{
    public abstract class ViewModel : NotificationObject
    {
        protected bool SetProperty<T>(ref T backingVariable, T newValue, Expression<Func<T>> expression)
        {
            if ( Equals(backingVariable, newValue) )
                return false;

            backingVariable = newValue;
            RaisePropertyChanged(expression);
            return true;
        }

#pragma warning disable 1911
        protected override void RaisePropertyChanged(string propertyName)
        { UIDispatcher.Schedule(() => base.RaisePropertyChanged(propertyName)); }
#pragma warning restore 1911
    }
}