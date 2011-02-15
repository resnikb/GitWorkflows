using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;

namespace GitWorkflows.Controls.ViewModels
{
    public abstract class ViewModel : NotificationObject, IDynamicMetaObjectProvider
    {
        private sealed class Metadata
        {
            public Dictionary<string, CommandFactory> CommandFactories;
        }

        private static readonly ConcurrentDictionary<Type, Metadata> _cachedMetadata = new ConcurrentDictionary<Type, Metadata>();
        private readonly ConcurrentDictionary<string, object> _backingVariables = new ConcurrentDictionary<string, object>();

        protected ViewModel()
        {
            var metadata = _cachedMetadata.GetOrAdd(GetType(), CreateMetadata);
            CreateCommands(metadata);
        }

        private void CreateCommands(Metadata metadata)
        {
            foreach (var pair in metadata.CommandFactories)
                _backingVariables.TryAdd(pair.Key, pair.Value.Create(this));
        }

        protected T GetProperty<T>(Expression<Func<T>> expression, T defaultValue = default(T))
        {
            var propertyName = PropertySupport.ExtractPropertyName(expression);
            return GetProperty(propertyName, defaultValue);    
        }

        protected T GetProperty<T>(string name, T defaultValue = default(T))
        { return (T)_backingVariables.GetOrAdd(name, defaultValue); }

        protected bool SetProperty<T>(Expression<Func<T>> expression, T newValue)
        {
            var propertyName = PropertySupport.ExtractPropertyName(expression);
            
            object currentValue;
            if (_backingVariables.TryGetValue(propertyName, out currentValue) && Equals(currentValue, newValue))
                return false;

            _backingVariables.AddOrUpdate(propertyName, newValue, (n, v) => newValue);
            RaisePropertyChanged(propertyName);
            return true;
        }

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

        private static Metadata CreateMetadata(Type viewModelType)
        {
            var methods = viewModelType.GetMethods();
            var properties = viewModelType.GetProperties();

            var executeData = from m in methods
                              from attr in m.GetCustomAttributes(typeof(CommandExecuteAttribute), true)
                              from name in ((CommandExecuteAttribute)attr).Names
                              select new { CommandName=name, Execute=m };

            var canExecuteData = from m in methods.Cast<MemberInfo>().Concat(properties)
                                 from attr in m.GetCustomAttributes(typeof(CommandCanExecuteAttribute), true)
                                 from name in ((CommandCanExecuteAttribute)attr).Names
                                 select new { CommandName=name, CanExecute=m };

            var commandData = executeData.GroupJoin(
                canExecuteData, 
                e => e.CommandName, 
                ce => ce.CommandName, 
                (e, ceCollection) => new {e.CommandName, e.Execute, CanExecute = ceCollection.Select(ce => ce.CanExecute).SingleOrDefault()}
            );

            return new Metadata
            {
                CommandFactories = commandData.ToDictionary(d => d.CommandName, d => new CommandFactory(d.Execute, d.CanExecute))
            };
        }

        /// <summary>
        /// Returns the <see cref="T:System.Dynamic.DynamicMetaObject"/> responsible for binding operations performed on this object.
        /// </summary>
        /// <returns>
        /// The <see cref="T:System.Dynamic.DynamicMetaObject"/> to bind this object.
        /// </returns>
        /// <param name="parameter">The expression tree representation of the runtime value.</param>
        public DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new ViewModelMeta(parameter, this, _backingVariables);
        }

        private sealed class CommandFactory
        {
            private readonly Func<ViewModel, Action<object>> _executeBinder;
            private readonly Func<ViewModel, Func<object, bool>> _canExecuteBinder;

            public CommandFactory(MethodInfo executeMethod, MemberInfo canExecuteMember)
            {
                if (executeMethod == null)
                    throw new ArgumentNullException("executeMethod");

                _executeBinder = CreateExecuteBinder(executeMethod);
                if (canExecuteMember != null)
                {
                    if (canExecuteMember is PropertyInfo)
                        _canExecuteBinder = CreateCanExecuteBinder((PropertyInfo)canExecuteMember);
                    else
                        _canExecuteBinder = CreateCanExecuteBinder((MethodInfo)canExecuteMember);
                }    
            }

            public DelegateCommandBase Create(ViewModel viewModel)
            {
                if (_canExecuteBinder == null)
                    return new DelegateCommand<object>(_executeBinder(viewModel));

                return new DelegateCommand<object>(_executeBinder(viewModel), _canExecuteBinder(viewModel));
            }
            
            private static Func<ViewModel, Func<object, bool>> CreateCanExecuteBinder(PropertyInfo canExecuteProperty)
            {
                if (!typeof(bool).IsAssignableFrom(canExecuteProperty.PropertyType))
                {
                    throw new ArgumentException(
                        string.Format(
                            "Property {0}.{1} must be of bool type to be used as the CanExecute method for a command.", 
                            canExecuteProperty.DeclaringType.Name, 
                            canExecuteProperty.Name
                            ), 
                        "canExecuteMethod"
                    );
                }

                return vm => o => (bool)canExecuteProperty.GetValue(vm, null);
            }

            private static Func<ViewModel, Func<object, bool>> CreateCanExecuteBinder(MethodInfo canExecuteMethod)
            {
                var parameters = canExecuteMethod.GetParameters();
                if (parameters.Length > 1)
                {
                    throw new ArgumentException(
                        string.Format(
                            "Method {0}.{1} can only have zero or one parameters to be used as the CanExecute method for a command.", 
                            canExecuteMethod.DeclaringType.Name, 
                            canExecuteMethod.Name
                            ), 
                        "canExecuteMethod"
                    );
                }

                if (!typeof(bool).IsAssignableFrom(canExecuteMethod.ReturnType))
                {
                    throw new ArgumentException(
                        string.Format(
                            "Method {0}.{1} must return bool to be used as the CanExecute method for a command.", 
                            canExecuteMethod.DeclaringType.Name, 
                            canExecuteMethod.Name
                            ), 
                        "canExecuteMethod"
                    );
                }

                if (parameters.Length == 0)
                    return vm => o => (bool)canExecuteMethod.Invoke(vm, null);

                return vm => o => (bool)canExecuteMethod.Invoke(vm, new[]{o});
            }

            private static Func<ViewModel, Action<object>> CreateExecuteBinder(MethodInfo executeMethod)
            {
                var parameters = executeMethod.GetParameters();
                if (parameters.Length > 1)
                {
                    throw new ArgumentException(
                        string.Format(
                            "Method {0}.{1} can only have zero or one parameters to be used as the Execute method for a command.", 
                            executeMethod.DeclaringType.Name, 
                            executeMethod.Name
                            ), 
                        "executeMethod"
                    );
                }

                if (parameters.Length == 0)
                    return vm => o => executeMethod.Invoke(vm, null);

                return vm => o => executeMethod.Invoke(vm, new[]{o});
            }

        }
    }

    public class ViewModelMeta : DynamicMetaObject
    {
        private readonly ConcurrentDictionary<string, object> _backingVariables;

        public ViewModelMeta(Expression expression, ViewModel value, ConcurrentDictionary<string, object> backingVariables)
            : base(expression, BindingRestrictions.Empty, value)
        {
            _backingVariables = backingVariables;
        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            object value;
            if (!_backingVariables.TryGetValue(binder.Name, out value))
                return binder.FallbackGetMember(this);

            var expViewModel = Expression.Convert(Expression, typeof(ViewModel));
            var target = Expression.Call(expViewModel, "GetProperty", new[]{typeof(object)}, Expression.Constant(binder.Name), Expression.Constant(null));
            var restrictions = BindingRestrictions.GetInstanceRestriction(Expression, Value);

            return new DynamicMetaObject(target, restrictions);
        }

    }
}