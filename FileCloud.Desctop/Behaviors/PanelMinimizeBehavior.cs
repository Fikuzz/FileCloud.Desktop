using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FileCloud.Desktop.View.Behaviors
{
    public class PanelMinimizeBehavior : Behavior<Border>
    {
        public static readonly DependencyProperty IsMinimizedProperty =
            DependencyProperty.Register("IsMinimized", typeof(bool), typeof(PanelMinimizeBehavior),
                new PropertyMetadata(false, OnIsMinimizedChanged));

        public bool IsMinimized
        {
            get => (bool)GetValue(IsMinimizedProperty);
            set => SetValue(IsMinimizedProperty, value);
        }

        private static void OnIsMinimizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (PanelMinimizeBehavior)d;
            behavior.UpdatePanelState();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            UpdatePanelState();
        }

        private void UpdatePanelState()
        {
            if (AssociatedObject == null) return;

            var animation = new DoubleAnimation
            {
                To = IsMinimized ? 70 : 200,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
            };

            AssociatedObject.Padding = IsMinimized ? new Thickness(0, 8, 8, 8) : new Thickness(8);
            AssociatedObject.BeginAnimation(FrameworkElement.WidthProperty, animation);
        }
    }
}
