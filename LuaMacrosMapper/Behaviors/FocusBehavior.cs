using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LuaMacrosMapper.Behaviors
{
    public class FocusBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register(
                nameof(IsFocused),
                typeof(bool),
                typeof(FocusBehavior),
                new FrameworkPropertyMetadata(false, (sender, e) =>
                {
                    if (sender is UIElement element)
                    {
                        if ((bool)e.NewValue)
                        {
                            element.Focus();
                        }
                        else
                        {
                            element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                        }
                    }
                }));

        public bool IsFocused
        {
            get => (bool)GetValue(IsFocusedProperty);
            set => SetValue(IsFocusedProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.GotFocus += AssociatedObject_GotFocus;
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        }


        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.GotFocus -= AssociatedObject_GotFocus;
            AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            IsFocused = false;
        }
        private void AssociatedObject_GotFocus(object sender, RoutedEventArgs e)
        {
            IsFocused = true;
        }
    }
}
