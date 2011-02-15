using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GitWorkflows.Controls.Commands
{
    public static class SelectionChanged
    {
        private static readonly DependencyProperty SelectionChangedCommandBehaviorProperty = DependencyProperty.RegisterAttached(
            "SelectionChangedCommandBehavior",
            typeof(SelectorSelectionChangedCommandBehavior),
            typeof(SelectionChanged),
            null
        );

        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached(
            "Command",
            typeof(ICommand),
            typeof(SelectionChanged),
            new PropertyMetadata(OnCommandChanged)
        );

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached(
            "CommandParameter",
            typeof(object),
            typeof(SelectionChanged),
            new PropertyMetadata(OnCommandParameterChanged)
        );

        public static ICommand GetCommand(Selector selector)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            return selector.GetValue(CommandProperty) as ICommand;
        }

        public static void SetCommand(Selector selector, ICommand command)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            selector.SetValue(CommandProperty, command);            
        }

        public static object GetCommandParameter(Selector selector)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            return selector.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(Selector selector, object parameter)
        {
            if (selector == null)
                throw new ArgumentNullException("selector");

            selector.SetValue(CommandParameterProperty, parameter);            
        }

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as Selector;
            if (selector != null)
                GetOrCreateBehavior(selector).Command = e.NewValue as ICommand;
        }

        private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = d as Selector;
            if (selector != null)
                GetOrCreateBehavior(selector).CommandParameter = e.NewValue;
        }

        private static SelectorSelectionChangedCommandBehavior GetOrCreateBehavior(Selector selector)
        {
            var behavior = selector.GetValue(SelectionChangedCommandBehaviorProperty) as SelectorSelectionChangedCommandBehavior;
            if (behavior == null)
            {
                behavior = new SelectorSelectionChangedCommandBehavior(selector);
                selector.SetValue(SelectionChangedCommandBehaviorProperty, behavior);
            }

            return behavior;
        }
    }
}