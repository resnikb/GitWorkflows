using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Practices.Prism.ViewModel;

namespace GitWorkflows.Controls.ViewModels
{
    public abstract class ViewModel : NotificationObject
    {
        private sealed class Metadata
        {
            public Dictionary<string, string[]> Dependencies;
        }

        private static readonly ConcurrentDictionary<Type, Metadata> _cachedMetadata = new ConcurrentDictionary<Type, Metadata>();

        private readonly ConcurrentDictionary<string, object> _backingVariables = new ConcurrentDictionary<string, object>();
        private readonly Metadata _metadata;

        protected ViewModel()
        {
            _metadata = _cachedMetadata.GetOrAdd(GetType(), CreateMetadata);
        }

        protected T GetProperty<T>(Expression<Func<T>> expression, T defaultValue = default(T))
        {
            var propertyName = PropertySupport.ExtractPropertyName(expression);
            return (T)_backingVariables.GetOrAdd(propertyName, defaultValue);    
        }

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
        {
            string[] dependencies;
            if (!_metadata.Dependencies.TryGetValue(propertyName, out dependencies))
            { 
                UIDispatcher.Schedule(() => base.RaisePropertyChanged(propertyName));
                return;
            }

            UIDispatcher.Schedule(
                () =>
                {
                    base.RaisePropertyChanged(propertyName);
                    foreach (var name in dependencies)
                        base.RaisePropertyChanged(name);
                }
            );
        }
#pragma warning restore 1911

        private static Metadata CreateMetadata(Type viewModelType)
        {
            return new Metadata
            {
                Dependencies = GetDependencies(viewModelType)
            };    
        }

        private static Dictionary<string, string[]> GetDependencies(Type viewModelType)
        {
            var result = new Dictionary<string, string[]>();
            var graph = new Dictionary<string, HashSet<string>>();

            foreach (var property in viewModelType.GetProperties())
            {
                var attrs = property.GetCustomAttributes(typeof(DependsOnPropertiesAttribute), true);
                if (attrs.Length == 0)
                    continue;

                foreach (var name in attrs.Cast<DependsOnPropertiesAttribute>().SelectMany(a => a.Names))
                {
                    HashSet<string> deps;
                    if (!graph.TryGetValue(name, out deps))
                    {
                        deps = new HashSet<string>();
                        graph.Add(name, deps);
                    }

                    deps.Add(property.Name);
                }
            }

            while (graph.Count > 0)
                Resolve(graph.First().Key, graph, result);

            return result;
        }

        private static void Resolve(string name, Dictionary<string, HashSet<string>> graph, Dictionary<string, string[]> resolved)
        {
            HashSet<string> unresolvedNames;
            if (!graph.TryGetValue(name, out unresolvedNames))
                return;

            foreach (var unresolvedName in unresolvedNames.ToArray())
            {
                Resolve(unresolvedName, graph, resolved);

                string[] resolvedNames;
                if (resolved.TryGetValue(name, out resolvedNames))
                    unresolvedNames.UnionWith(resolvedNames);
            }

            if (unresolvedNames.Count > 0)
                resolved.Add(name, unresolvedNames.ToArray());

            graph.Remove(name);
        }
    }
}