using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace FileCloud.Desktop.View.Behaviors
{
    public class FocusBehavior
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject obj) =>
            (bool)obj.GetValue(IsFocusedProperty);

        public static void SetIsFocused(DependencyObject obj, bool value) =>
            obj.SetValue(IsFocusedProperty, value);

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb && (bool)e.NewValue)
            {
                tb.Dispatcher.BeginInvoke(
                   DispatcherPriority.Input,
                   new Action(() =>
                   {
                       tb.Focus();
                       tb.SelectAll();
                       Keyboard.Focus(tb);
                   }));
            }
        }
    }
}
