using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaMacrosMapper.Models
{
    public class CapturedVirtualKey : NotificationObject
    {
        public static CapturedVirtualKey Instance { get; } = new CapturedVirtualKey();

        public int Key { get => VirtualKey.Value.Key.Value; }
        public int Flags { get => VirtualKey.Value.Flags.Value; }
        public string Device { get => VirtualKey.Value.Device.Value; }

        public ReactivePropertySlim<VirtualKey> VirtualKey { get; } = new ReactivePropertySlim<VirtualKey>(new VirtualKey(0, deserializing: false));
    }
}
