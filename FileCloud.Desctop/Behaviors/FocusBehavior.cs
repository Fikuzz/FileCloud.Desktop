using System.Text.RegularExpressions;
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
                       Match match = Regex.Match(tb.Text, @"^(.+)\.");
                       tb.Focus();
                       if (match.Success)
                       {
                           tb.Select(
                               match.Index,
                               match.Length - 1);
                       }
                       else
                       {
                           tb.SelectAll();
                       }
                       Keyboard.Focus(tb);
                   }));
            }
        }
    }
}
