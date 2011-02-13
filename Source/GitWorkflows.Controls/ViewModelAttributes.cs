using System;
using System.Diagnostics;

namespace GitWorkflows.Controls
{
    public abstract class NameContainerAttribute : Attribute
    {
        public string[] Names
        { get; private set; }

        protected NameContainerAttribute(params string[] names)
        {
            Debug.Assert(names != null && names.Length > 0);
            Names = names;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DependsOnPropertiesAttribute : NameContainerAttribute
    {
        public DependsOnPropertiesAttribute(params string[] propertyNames)
            : base(propertyNames)
        {}
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CommandExecuteAttribute : NameContainerAttribute
    {
        public CommandExecuteAttribute(params string[] commandNames)
            : base(commandNames)
        {}
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class CommandCanExecuteAttribute : NameContainerAttribute
    {
        public CommandCanExecuteAttribute(params string[] commandNames)
            : base(commandNames)
        {}
    }
}