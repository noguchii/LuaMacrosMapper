using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace LuaMacrosMapper.Helpers
{
    public static class VirtualKeyHelper
    {
        public static string GetName(int virtualKey, int flags = 0)
        {
            string name = string.Empty;
            if (virtualKey == 0) return "";

            if (flags == 2)
            {
                name = virtualKey switch
                {
                    8 => "BackSpace",
                    9 => "Tab",
                    12 => "Clear",
                    13 => "NumPad Enter",
                    16 => "RShift",
                    17 => "RCtrl",
                    18 => "RAlt",
                    19 => "Pause",
                    20 => "Toggle CapsLock",
                    21 => "Hangle",
                    22 => "IME On",
                    23 => "Junja",
                    24 => "Final",
                    25 => "Hanja / Kanji / HankakuZenkaku",
                    26 => "IME Off",
                    27 => "Escape",
                    28 => "Convert",
                    29 => "NonConvert",
                    30 => "Accept",
                    31 => "ModeChange",
                    32 => "Space",
                    33 => "PageUp",
                    34 => "PageDown",
                    35 => "End",
                    36 => "Home",
                    37 => "Left",
                    38 => "Up",
                    39 => "Right",
                    40 => "Down",
                    41 => "Select",
                    42 => "Print",
                    43 => "Execute",
                    44 => "PrintScreen",
                    45 => "Insert",
                    46 => "Delete",
                    47 => "Help",
                    91 => "LWin",
                    92 => "RWin",
                    93 => "Menu",
                    94 => "Power",
                    95 => "Sleep",
                    106 => "NumPad *",
                    107 => "NumPad +",
                    108 => "NumPad Enter",
                    109 => "NumPad -",
                    110 => "NumPad .",
                    111 => "NumPad /",
                    144 => "NumLock",
                    145 => "ScrollLock",
                    160 => "LShift",
                    161 => "RShift",
                    162 => "LCtrl",
                    163 => "RCtrl",
                    164 => "LAlt",
                    165 => "RAlt",
                    166 => "Browser Back",
                    167 => "Browser Forward",
                    168 => "Browser Refresh",
                    169 => "Browser Stop",
                    170 => "Browser Search",
                    171 => "Browser Favorites",
                    172 => "Browser Home",
                    173 => "Volume Mute",
                    174 => "Volume Down",
                    175 => "Volume Up",
                    176 => "Media NextTrack",
                    177 => "Media PrevTrack",
                    178 => "Media Stop",
                    179 => "Media Play/Pause",
                    180 => "Launch Mail",
                    181 => "Launch MediaSelect",
                    182 => "Launch App1",
                    183 => "Launch App2",
                    227 => "ICO Help",
                    228 => "ICO 00",
                    229 => "IME Process",
                    230 => "ICO Clear",
                    233 => "Reset",
                    234 => "Jump",
                    235 => "PA1",
                    236 => "PA2",
                    237 => "PA3",
                    238 => "WSCTRL",
                    239 => "CUSEL",
                    240 => "CapsLock",
                    241 => ">Katakana",
                    242 => ">Hiragana",
                    243 => ">Hankaku",
                    244 => ">Zenkaku",
                    255 => "Pause / ScreenCapture",
                    _ => string.Empty,
                };
            }
            if (!string.IsNullOrEmpty(name)) return name;

            name = virtualKey switch
            {
                8 => "BackSpace",
                9 => "Tab",
                12 => "NumPad Clear",
                13 => "Enter",
                16 => "Shift",
                17 => "Ctrl",
                18 => "Alt",
                19 => "Pause",
                20 => "Toggle CapsLock",
                21 => "Hangle",
                22 => "IME On",
                23 => "Junja",
                24 => "Final",
                25 => "Hanja / Kanji / HankakuZenkaku",
                26 => "IME Off",
                27 => "Escape",
                28 => "Convert",
                29 => "NonConvert",
                30 => "Accept",
                31 => "ModeChange",
                32 => "Space",
                33 => "NumPad PageUp",
                34 => "NumPad PageDown",
                35 => "NumPad End",
                36 => "NumPad Home",
                37 => "NumPad Left",
                38 => "NumPad Up",
                39 => "NumPad Right",
                40 => "NumPad Down",
                41 => "Select",
                42 => "Print",
                43 => "Execute",
                44 => "PrintScreen",
                45 => "NumPad Insert",
                46 => "NumPad Delete",
                47 => "Help",
                91 => "LWin",
                92 => "RWin",
                93 => "Menu",
                94 => "Power",
                95 => "Sleep",
                106 => "NumPad *",
                107 => "NumPad +",
                108 => "NumPad Enter",
                109 => "NumPad -",
                110 => "NumPad .",
                111 => "NumPad /",
                144 => "NumLock",
                145 => "ScrollLock",
                160 => "LShift",
                161 => "RShift",
                162 => "LCtrl",
                163 => "RCtrl",
                164 => "LAlt",
                165 => "RAlt",
                166 => "Browser Back",
                167 => "Browser Forward",
                168 => "Browser Refresh",
                169 => "Browser Stop",
                170 => "Browser Search",
                171 => "Browser Favorites",
                172 => "Browser Home",
                173 => "Volume Mute",
                174 => "Volume Down",
                175 => "Volume Up",
                176 => "Media NextTrack",
                177 => "Media PrevTrack",
                178 => "Media Stop",
                179 => "Media Play/Pause",
                180 => "Launch Mail",
                181 => "Launch MediaSelect",
                182 => "Launch App1",
                183 => "Launch App2",
                227 => "ICO Help",
                228 => "ICO 00",
                229 => "IME Process",
                230 => "ICO Clear",
                233 => "Reset",
                234 => "Jump",
                235 => "PA1",
                236 => "PA2",
                237 => "PA3",
                238 => "WSCTRL",
                239 => "CUSEL",
                240 => "CapsLock",
                241 => ">Katakana",
                242 => ">Hiragana",
                243 => ">Hankaku",
                244 => ">Zenkaku",
                255 => "Pause",
                _ => string.Empty,
            };
            if (!string.IsNullOrEmpty(name)) return name;

            // NumPad 0 ~ 9
            if (96 <= virtualKey && virtualKey <= 105)
            {
                return $"NumPad {virtualKey - 96}";
            }
            // F1 ~ F24
            if (112 <= virtualKey && virtualKey <= 135)
            {
                return $"F{virtualKey - 111}";
            }

            var charCode = MapVirtualKey(virtualKey, 2);
            if (charCode != 0)
            {
                return Convert.ToChar(charCode).ToString();
            }

            return string.Empty;
        }

        [DllImport("user32.dll", EntryPoint = "MapVirtualKeyA")]
        public extern static int MapVirtualKey(int wCode, int wMapType);
    }

    public class LuaMacroSpecialKeys
    {
        public class LuaKeyData
        {
            public int VirtualKey { get; private set; }
            public string Name { get; private set; } = "";
            public string ModiferName { get; private set; } = "";

            public static LuaKeyData Create(int virtualKey, string name, string modfierName = "")
            {
                return new()
                {
                    VirtualKey = virtualKey,
                    Name = name,
                    ModiferName = modfierName
                };
            }
        }

        private static List<LuaKeyData>? _DataSource;

        public static ReadOnlyCollection<LuaKeyData>? _Data;
        public static ReadOnlyCollection<LuaKeyData> Data
        {
            get
            {
                if (_Data == null)
                    _Data = InitilizeData();
                return new ReadOnlyCollection<LuaKeyData>(_Data);
            }
        }

        private static ReadOnlyCollection<LuaKeyData> InitilizeData()
        {
            var data = new List<LuaKeyData> {
                LuaKeyData.Create(8, "BACKSPACE"),
                LuaKeyData.Create(19 ,"BREAK"),
                LuaKeyData.Create(20 ,"CAPSLOCK"),
                LuaKeyData.Create(12 ,"CLEAR"),
                LuaKeyData.Create(46 ,"DELETE"),
                LuaKeyData.Create(40 ,"DOWN"),
                LuaKeyData.Create(35 ,"END"),
                LuaKeyData.Create(13, "ENTER"),
                LuaKeyData.Create(27, "ESCAPE"),
                LuaKeyData.Create(112, "F1"),
                LuaKeyData.Create(113, "F2"),
                LuaKeyData.Create(114, "F3"),
                LuaKeyData.Create(115, "F4"),
                LuaKeyData.Create(116, "F5"),
                LuaKeyData.Create(117, "F6"),
                LuaKeyData.Create(118, "F7"),
                LuaKeyData.Create(119, "F8"),
                LuaKeyData.Create(120, "F9"),
                LuaKeyData.Create(121, "F10"),
                LuaKeyData.Create(122, "F11"),
                LuaKeyData.Create(123, "F12"),
                LuaKeyData.Create(124, "F13"),
                LuaKeyData.Create(125, "F14"),
                LuaKeyData.Create(126, "F15"),
                LuaKeyData.Create(127, "F16"),
                LuaKeyData.Create(128, "F17"),
                LuaKeyData.Create(129, "F18"),
                LuaKeyData.Create(130, "F19"),
                LuaKeyData.Create(131, "F20"),
                LuaKeyData.Create(132, "F21"),
                LuaKeyData.Create(133, "F22"),
                LuaKeyData.Create(134, "F23"),
                LuaKeyData.Create(135, "F24"),
                LuaKeyData.Create(47,"HELP"),
                LuaKeyData.Create(36, "HOME"),
                LuaKeyData.Create(45, "INS"),
                LuaKeyData.Create(37, "LEFT"),
                LuaKeyData.Create(96, "NUM0"),
                LuaKeyData.Create(97, "NUM1"),
                LuaKeyData.Create(98, "NUM2"),
                LuaKeyData.Create(99, "NUM3"),
                LuaKeyData.Create(100, "NUM4"),
                LuaKeyData.Create(101, "NUM5"),
                LuaKeyData.Create(102, "NUM6"),
                LuaKeyData.Create(103, "NUM7"),
                LuaKeyData.Create(104, "NUM8"),
                LuaKeyData.Create(105, "NUM9"),
                LuaKeyData.Create(110, "NUMDECIMAL"),
                LuaKeyData.Create(111, "NUMDIVIDE"),
                LuaKeyData.Create(144, "NUMLOCK"),
                LuaKeyData.Create(109, "NUMMINUS"),
                LuaKeyData.Create(106, "NUMMULTIPLY"),
                LuaKeyData.Create(107, "NUMPLUS"),
                LuaKeyData.Create(34, "PGDN"),
                LuaKeyData.Create(33, "PGUP"),
                LuaKeyData.Create(44, "PRTSC"),
                LuaKeyData.Create(39, "RIGHT"),
                LuaKeyData.Create(145, "SCROLLLOCK"),
                LuaKeyData.Create(9, "TAB", "&"),
                LuaKeyData.Create(16, "Shift", "+"),
                LuaKeyData.Create(160, "LSHIFT", "+<"),
                LuaKeyData.Create(161, "RSHIFT", "+>"),
                LuaKeyData.Create(17, "Ctrl", "^"),
                LuaKeyData.Create(162, "LCTRL", "^<"),
                LuaKeyData.Create(163, "RCTRL", "^>"),
                LuaKeyData.Create(18, "Alt", "%"),
                LuaKeyData.Create(164, "LALT", "%<"),
                LuaKeyData.Create(165, "RALT", "%>"),
                LuaKeyData.Create(91, "LWin", "#"),
                LuaKeyData.Create(92, "RWin", "#>"),
            };
            
            _DataSource = data;
            return new ReadOnlyCollection<LuaKeyData>(data);
        }
        
        public static string? GetName(int virtualKey)
        {
            var data = Data.FirstOrDefault(d => d.VirtualKey == virtualKey);
            if (data == null) return null;
            return data.Name;
        }
        public static string? GetModiferName(int virtualKey)
        {
            var data = Data.FirstOrDefault(d => d.VirtualKey == virtualKey);
            if (data == null) return null;
            return data.Name;
        }
    }
}
