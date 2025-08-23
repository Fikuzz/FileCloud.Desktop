using FileCloud.Desktop.ViewModels;
using FileCloud.Desktop.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FileCloud.Desktop.View.Behaviors
{
    public class EditableTextBehavior
    {
        public static readonly DependencyProperty EnableCommitCancelProperty =
        DependencyProperty.RegisterAttached(
            "EnableCommitCancel",
            typeof(bool),
            typeof(EditableTextBehavior),
            new PropertyMetadata(false, OnChanged));

        public static void SetEnableCommitCancel(DependencyObject obj, bool value)
            => obj.SetValue(EnableCommitCancelProperty, value);

        public static bool GetEnableCommitCancel(DependencyObject obj)
            => (bool)obj.GetValue(EnableCommitCancelProperty);

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox tb)
            {
                if ((bool)e.NewValue)
                    tb.KeyDown += OnKeyDown;
                else
                    tb.KeyDown -= OnKeyDown;
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is IEditableItem vm)
            {
                if (e.Key == Key.Enter)
                {
                    if (vm.CommitEditCommand?.CanExecute(null) == true)
                        vm.CommitEditCommand.Execute(null);

                    e.Handled = true;
                }
                else if (e.Key == Key.Escape)
                {
                    if (vm.CancelEditCommand?.CanExecute(null) == true)
                        vm.CancelEditCommand.Execute(null);

                    e.Handled = true;
                }
            }
        }
    }
}
