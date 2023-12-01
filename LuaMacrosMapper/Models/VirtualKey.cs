using LuaMacrosMapper.Helpers;
using MessagePack;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace LuaMacrosMapper.Models
{
    [MessagePackObject]
    public class VirtualKey : NotificationObject, IMessagePackSerializationCallbackReceiver
    {
        [Key(0)]
        public ReactivePropertySlim<int> Key { get; set; }
        [Key(1)]
        public ReactivePropertySlim<int> Flags { get; set; }
        [Key(2)]
        public ReactivePropertySlim<string> Device { get; set; }
        [IgnoreMember]
        public ReadOnlyReactivePropertySlim<string> Name { get; private set; } = null!;
        [Obsolete("Default constractor only use to deserialize.", true)]
        public VirtualKey() : this(0, deserializing: true)
        {
        }
        public VirtualKey(int key, int flags = 0, string device = "", bool deserializing = false)
        {
            Key = new ReactivePropertySlim<int>(key);
            Flags = new ReactivePropertySlim<int>(flags);
            Device = new ReactivePropertySlim<string>(device);

            if (!deserializing)
            {
                DefineSubscribe();
            }
        }

        private void DefineSubscribe()
        {
            Name = Observable.CombineLatest(Key, Flags, VirtualKeyHelper.GetName)
                .ToReadOnlyReactivePropertySlim(string.Empty)
                .AddTo(Disposable);
        }
        public void OnBeforeSerialize()
        {

        }
        public void OnAfterDeserialize()
        {
            DefineSubscribe();
        }

        public bool EqualsValue(VirtualKey target)
        {
            return Key.Value == target.Key.Value && Flags.Value == target.Flags.Value && Device.Value == target.Device.Value;
        }

        public bool EqualsValue(int key, int flags, string? device = null)
        {
            if (device == null)
            {
                return Key.Value == key && Flags.Value == flags;
            }
            else
            {
                return Key.Value == key && Flags.Value == flags && Device.Value == device;
            }
        }

        public VirtualKey Clone()
        {
            return new VirtualKey(Key.Value, Flags.Value, Device.Value);
        }
    }
}
