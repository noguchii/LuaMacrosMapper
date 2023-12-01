using LuaMacrosMapper.Models;
using MessagePack;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LuaMacrosMapper.ViewModels
{
    [MessagePackObject]
    public class InputMacro : NotificationObject, IMacro, IMessagePackSerializationCallbackReceiver
    {
        [Key(0)]
        public ReactivePropertySlim<VirtualKey> VirtualKey { get; set; }
        [Key(1)]
        public ReactivePropertySlim<bool> WithShift { get; set; }
        [Key(2)]
        public ReactivePropertySlim<bool> WithCtrl { get; set; }
        [Key(3)]
        public ReactivePropertySlim<bool> WithAlt { get; set; }
        [Key(4)]
        public ReactivePropertySlim<bool> WithWin { get; set; }
        [Key(5)]
        public ReactivePropertySlim<bool> WithRShift { get; set; }
        [Key(6)]
        public ReactivePropertySlim<bool> WithRCtrl { get; set; }
        [Key(7)]
        public ReactivePropertySlim<bool> WithRAlt { get; set; }
        [Key(8)]
        public ReactivePropertySlim<bool> WithRWin { get; set; }
        [IgnoreMember]
        public IReactiveProperty<string> InlineText { get; private set; } = null!;
        [IgnoreMember]
        public ReactiveCommandSlim SetKeyCommand { get; }
        [Obsolete("Default constractor only use to deserialize.", true)]
        public InputMacro() : this(true) { }

        public InputMacro(bool deserializing = false)
        {
            VirtualKey = new ReactivePropertySlim<VirtualKey>(new VirtualKey(0, deserializing: deserializing));
            WithAlt = new ReactivePropertySlim<bool>(false);
            WithShift = new ReactivePropertySlim<bool>(false);
            WithCtrl = new ReactivePropertySlim<bool>(false);
            WithWin = new ReactivePropertySlim<bool>(false);
            WithRAlt = new ReactivePropertySlim<bool>(false);
            WithRShift = new ReactivePropertySlim<bool>(false);
            WithRCtrl = new ReactivePropertySlim<bool>(false);
            WithRWin = new ReactivePropertySlim<bool>(false);

            SetKeyCommand = new ReactiveCommandSlim();

            SetKeyCommand.Subscribe(() =>
            {
                VirtualKey.Value.Key.Value = CapturedVirtualKey.Instance.Key;
                VirtualKey.Value.Flags.Value = CapturedVirtualKey.Instance.Flags;
            });

            if (!deserializing)
            {
                DefineSubscribe();
            }
        }

        void DefineSubscribe()
        {
            InlineText = Observable.CombineLatest(
                VirtualKey.Value.Name,
                WithShift, WithCtrl, WithRAlt, WithWin,
                WithRShift, WithRCtrl, WithRAlt, WithRWin,
                (key, s, c, a, w, rs, rc, ra, rw) =>
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        return "[Unkown Key]";
                    }
                    var modifers = "";
                    if (s) modifers += "Shift+";
                    if (c) modifers += "Ctrl+";
                    if (a) modifers += "Alt+";
                    if (w) modifers += "Win+";
                    if (rs) modifers += "RShift+";
                    if (rc) modifers += "RCtrl+";
                    if (ra) modifers += "RAlt+";
                    if (rw) modifers += "RWin+";
                    return $@"{{{modifers}{key}}}";
                })
                .ToReactiveProperty(string.Empty)
                .AddTo(Disposable);
        }

        public void OnBeforeSerialize()
        {

        }

        public void OnAfterDeserialize()
        {
            DefineSubscribe();
        }
    }
}
