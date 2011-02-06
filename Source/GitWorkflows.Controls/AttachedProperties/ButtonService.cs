using System.Windows;
using System.Windows.Controls;

namespace GitWorkflows.Controls.AttachedProperties
{
    public static class ButtonService
    {
        public static readonly DependencyProperty ResultProperty = DependencyProperty.RegisterAttached(
            "Result",
            typeof(bool?),
            typeof(ButtonService),
            new PropertyMetadata(null, OnResultChanged)
        );

        public static bool? GetResult(Button button)
        { return (bool?)button.GetValue(ResultProperty); }

        public static void SetResult(Button button, bool? value)
        { button.SetValue(ResultProperty, value); }

        private static void OnResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var button = (Button)d;
            button.Click -= OnButtonClick;

            if (e.NewValue is bool? && ((bool?)e.NewValue).HasValue)
                button.Click += OnButtonClick;
        }

        private static void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var result = GetResult(button);
            if (!result.HasValue)
                return;

            var window = Window.GetWindow(button);
            if (window != null)
                window.DialogResult = result.Value;
        }
    }
}