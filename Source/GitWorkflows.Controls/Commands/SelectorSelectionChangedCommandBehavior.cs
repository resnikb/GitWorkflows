using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Practices.Prism.Commands;

namespace GitWorkflows.Controls.Commands
{
    public class SelectorSelectionChangedCommandBehavior : CommandBehaviorBase<Selector>
    {
        public SelectorSelectionChangedCommandBehavior(Selector targetObject) : base(targetObject)
        {
            if (targetObject == null)
                throw new ArgumentNullException("targetObject");

            targetObject.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        { ExecuteCommand(); }
    }
}