using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using LuaMacrosMapper.Helpers;

namespace LuaMacrosMapper.Behaviors
{
    public class RawInputBehavior : Behavior<Window>
    {
        RawInputInterop? rawInput;

        public static readonly DependencyProperty KeyUpProperty =
            DependencyProperty.Register(
                nameof(KeyUp),
                typeof(ICommand),
                typeof(RawInputBehavior),
                new FrameworkPropertyMetadata());

        public ICommand KeyUp
        {
            get => (ICommand)GetValue(KeyUpProperty);
            set => SetValue(KeyUpProperty, value);
        }

        public static readonly DependencyProperty KeyDownProperty =
            DependencyProperty.Register(
                nameof(KeyDown),
                typeof(ICommand),
                typeof(RawInputBehavior),
                new FrameworkPropertyMetadata());

        public ICommand KeyDown
        {
            get => (ICommand)GetValue(KeyDownProperty);
            set => SetValue(KeyDownProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= AssociatedObject_Loaded;

            if (rawInput != null)
            {
                rawInput.OnKeyDown -= RawInput_OnKeyDown;
                rawInput.OnKeyUp -= RawInput_OnKeyUp;
                rawInput.Dispose();
                rawInput = null;
            }
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            var window = sender as Window;
            if (window == null) return;

            rawInput = new RawInputInterop(window);
            rawInput.OnKeyDown += RawInput_OnKeyDown;
            rawInput.OnKeyUp += RawInput_OnKeyUp;
        }

        private void RawInput_OnKeyDown(int vk, int sc, int flag, string deviceName)
        {
            KeyDown?.Execute(new InputEventArgs(vk, sc, flag, deviceName));
        }
        private void RawInput_OnKeyUp(int vk, int sc, int flag, string deviceName)
        {
            KeyUp?.Execute(new InputEventArgs(vk, sc, flag, deviceName));
        }

        public class InputEventArgs
        {
            public InputEventArgs(int vk, int sc, int flag, string deviceName)
            {
                VirtualKeyCode = vk;
                ScanCode = sc;
                Flags = flag;
                DeviceName = deviceName;
            }

            public int VirtualKeyCode { get; private set; }
            public int ScanCode { get; private set; }
            public int Flags { get; private set; }
            public string DeviceName { get; private set; }
        }
    }
}
