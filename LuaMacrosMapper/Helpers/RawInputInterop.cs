using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Xaml.Behaviors;

namespace LuaMacrosMapper.Helpers
{
    public class RawInputInterop : IDisposable
    {
        private HwndSource? _hwndSource;
        private HwndSourceHook? _hwndSourceHook;
        private nint _hwnd;
        bool disposing;

        /// <summary>
        /// Arguments:
        /// int : virtual key
        /// int : scan code
        /// int : device flag
        /// string : device name
        /// </summary>
        public event Action<int, int, int, string>? OnKeyDown;
        /// <summary>
        /// Arguments:
        /// int : virtual key
        /// int : scan code
        /// int : device flag
        /// string : device name
        /// </summary>
        public event Action<int, int, int, string>? OnKeyUp;

        public RawInputInterop(Window window)
        {
            // RawInput登録
            nint handle = new WindowInteropHelper(window).Handle;
            RawInputDevice[] devices = new RawInputDevice[1];
            devices[0].UsagePage = 0x01;    // Generic Desktop Controls 0x01
            devices[0].Usage = 0x06;        // Keyboard 0x06
            devices[0].Flags = 0x0000200;   // RIDEV_NOHOTKEYS 0x00000200
            devices[0].Target = handle;
            RegisterRawInputDevices(devices, 1, Marshal.SizeOf(typeof(RawInputDevice)));

            // WndProcフック
            _hwndSource = PresentationSource.FromVisual(window) as HwndSource;
            if (_hwndSource == null)
                throw new InvalidOperationException();
            _hwndSourceHook = new HwndSourceHook(WndProc);
            _hwndSource.AddHook(_hwndSourceHook);
            _hwnd = handle;
        }

        private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            if (msg == (int)WindowMessages.WM_INPUT)
            {
                WMInput(hwnd, wParam, lParam);
            }
            return nint.Zero;
        }

        private string GetDeviceName(nint deviceHandle)
        {
            var deviceNameBuffer = new StringBuilder(4096 * 2);
            deviceNameBuffer.Clear();

            // 
            var bufferSizeRef = (uint)deviceNameBuffer.Capacity;
            var firstCall = GetRawInputDeviceInfo(deviceHandle, (uint)DeviceInfoTypes.RIDI_DEVICENAME, deviceNameBuffer, ref bufferSizeRef);
            var firstError = Marshal.GetLastWin32Error();
            var firtsDataSize = bufferSizeRef;
            var devName = deviceNameBuffer.ToString();

            // 
            var deviceInfo = new RID_DEVICE_INFO();
            var deviceInfoSize = Convert.ToUInt32(Marshal.SizeOf<RID_DEVICE_INFO>());
            deviceInfo.cbSize = (int)deviceInfoSize;
            bufferSizeRef = deviceInfoSize;
            var secondCall = GetRawInputDeviceInfo(deviceHandle, (uint)DeviceInfoTypes.RIDI_DEVICEINFO, ref deviceInfo, ref bufferSizeRef);
            var secondError = Marshal.GetLastWin32Error();
            string hidData = "ERROR: hid data retrieval failed";
            if (deviceInfo.dwType == 2)
            {
#pragma warning disable CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
                hidData = deviceInfo.hid.ToString();
#pragma warning restore CS8600 // Null リテラルまたは Null の可能性がある値を Null 非許容型に変換しています。
            }

            return deviceInfo.keyboard + devName;
        }

        private void WMInput(nint hwnd, nint wParam, nint lParam)
        {
            var handle = _hwnd;
            var headerSize = Marshal.SizeOf(typeof(RawInputHeader));
            var inputSize = Marshal.SizeOf(typeof(RawInput));
            GetRawInputData(lParam, 0x10000003, out RawInput input, ref inputSize, headerSize);
            var keyboard = input.Keyborad;

            switch ((WindowMessages)keyboard.Message)
            {
                case WindowMessages.WM_KEYDOWN:
                case WindowMessages.WM_SYSKEYDOWN:
                    var keyDownDeviceName = GetDeviceName(input.Header.Device);
                    OnKeyDown?.Invoke(keyboard.VKey, keyboard.MakeCode, keyboard.Flags, keyDownDeviceName);
                    break;
                case WindowMessages.WM_KEYUP:
                case WindowMessages.WM_SYSKEYUP:
                    var keyUpDeviceName = GetDeviceName(input.Header.Device);
                    OnKeyUp?.Invoke(keyboard.VKey, keyboard.MakeCode, keyboard.Flags, keyUpDeviceName);
                    break;
            }
        }

        public void Dispose()
        {
            if (disposing) return;
            disposing = true;

            if (_hwndSource == null || _hwndSourceHook == null) return;
            _hwndSource.RemoveHook(_hwndSourceHook);
        }

        #region RegisterRawInputDevices

        enum WindowMessages
        {
            WM_INPUT = 0xFF,
            WM_KEYDOWN = 0x100,
            WM_KEYUP = 0x101,
            WM_SYSKEYDOWN = 0x104,
            WM_SYSKEYUP = 0x105,
        }

        struct RawInputDevice
        {
            public short UsagePage;
            public short Usage;
            public int Flags;
            public nint Target;
        }
        struct RawInputHeader
        {
            public int Type;
            public int Size;
            public nint Device;
            public nint WParam;
        }
        struct RawInput
        {
            public RawInputHeader Header;
            public RawKeyboard Keyborad;
        }
        struct RawKeyboard
        {
            public short MakeCode;
            public short Flags;
            public short Reserved;
            public short VKey;
            public int Message;
            public long ExtrInformation;
        }

        [DllImport("user32.dll")]
        static extern int RegisterRawInputDevices(RawInputDevice[] devices, int number, int size);

        [DllImport("user32.dll")]
        static extern int GetRawInputData(nint rawInput, int command, out RawInput data, ref int size, int headerSize);

        #endregion

        #region GetRawInputDeviceInfo

        enum DeviceInfoTypes
        {
            RIDI_PREPARSEDDATA = 0x20000005,
            RIDI_DEVICENAME = 0x20000007,
            RIDI_DEVICEINFO = 0x2000000B
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RID_DEVICE_INFO_HID
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwVendorId;
            [MarshalAs(UnmanagedType.U4)]
            public int dwProductId;
            [MarshalAs(UnmanagedType.U4)]
            public int dwVersionNumber;
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsagePage;
            [MarshalAs(UnmanagedType.U2)]
            public ushort usUsage;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RID_DEVICE_INFO_KEYBOARD
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwType;
            [MarshalAs(UnmanagedType.U4)]
            public int dwSubType;
            [MarshalAs(UnmanagedType.U4)]
            public int dwKeyboardMode;
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfFunctionKeys;
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfIndicators;
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfKeysTotal;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RID_DEVICE_INFO_MOUSE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int dwId;
            [MarshalAs(UnmanagedType.U4)]
            public int dwNumberOfButtons;
            [MarshalAs(UnmanagedType.U4)]
            public int dwSampleRate;
            [MarshalAs(UnmanagedType.U4)]
            public int fHasHorizontalWheel;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct RID_DEVICE_INFO
        {
            [FieldOffset(0)]
            public int cbSize;
            [FieldOffset(4)]
            public int dwType;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_MOUSE mouse;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_KEYBOARD keyboard;
            [FieldOffset(8)]
            public RID_DEVICE_INFO_HID hid;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetRawInputDeviceInfo(nint hDevice, uint uiCommand, nint pData, ref uint pcbSize);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetRawInputDeviceInfoA")]
        static extern uint GetRawInputDeviceInfo(nint hDevice, uint uiCommand, ref RID_DEVICE_INFO pData, ref uint pcbSize);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetRawInputDeviceInfoA", CharSet = CharSet.Ansi)]
        static extern uint GetRawInputDeviceInfo(nint hDevice, uint uiCommand, StringBuilder pData, ref uint pcbSize);

        #endregion
    }
}