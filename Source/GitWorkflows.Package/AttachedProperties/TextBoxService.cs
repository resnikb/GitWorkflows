using System;
using System.Windows;
using System.Windows.Controls;

namespace GitWorkflows.Package.AttachedProperties
{
    static class TextBoxService
    {
        public static readonly DependencyProperty SelectOnFocusProperty = DependencyProperty.RegisterAttached(
            "SelectOnFocus",
            typeof(bool),
            typeof(TextBoxService),
            new PropertyMetadata(false, OnSelectOnFocusChanged)
        );

        public static bool GetSelectOnFocus(TextBox textBox)
        { return (bool)textBox.GetValue(SelectOnFocusProperty); }

        public static void SetSelectOnFocus(TextBox textBox, bool value)
        { textBox.SetValue(SelectOnFocusProperty, value); }

        private static void OnSelectOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textbox = (TextBox)d;
            textbox.GotFocus -= OnTextBoxFocused;

            if (e.NewValue is bool && (bool)e.NewValue)
                textbox.GotFocus += OnTextBoxFocused;
        }

        private static void OnTextBoxFocused(object sender, RoutedEventArgs e)
        {
            var textbox = (TextBox)sender;
            textbox.Dispatcher.BeginInvoke(new Action(textbox.SelectAll));
        }
    }
}