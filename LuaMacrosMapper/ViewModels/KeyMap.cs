using LuaMacrosMapper.Models;
using MessagePack;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

namespace LuaMacrosMapper.ViewModels
{
    [MessagePackObject]
    public class KeyMap : NotificationObject, IMessagePackSerializationCallbackReceiver
    {
        [Key(0)]
        public ReactivePropertySlim<bool> IsActive { get; set; }
        [Key(1)]
        public ReactivePropertySlim<VirtualKey> VirtualKey { get; set; }
        [Key(2)]
        public ReactiveCollection<VirtualKey> Modifiers { get; set; }
        [Key(3)]
        public ReactiveCollection<IMacro> Macro { get; set; }

        [IgnoreMember]
        public ReactivePropertySlim<bool> IsSimulated { get; }
        [IgnoreMember]
        public ReactivePropertySlim<bool> IsEditing { get; }
        [IgnoreMember]
        public ReactivePropertySlim<bool> IsSelected { get; }
        [IgnoreMember]
        public ReadOnlyReactivePropertySlim<string> ModifiersText { get; private set; } = null!;
        [IgnoreMember]
        public ReadOnlyReactivePropertySlim<string> InlineText { get; private set; } = null!;
        [IgnoreMember]
        public ReactiveCommandSlim<KeyMap> ToggleEditCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim<VirtualKey> SetModifierCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim<VirtualKey> UnsetModifierCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim AddKeyMacroCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim AddCommandMacroCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim AddCodeMacroCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim<IMacro> RemoveMacroCommand { get; }

        [Obsolete("Default constractor only use to deserialize.", true)]
        public KeyMap() : this(deserializing: true)
        {
        }

        public KeyMap(bool deserializing)
        {
            IsActive = new ReactivePropertySlim<bool>(true);
            IsSimulated = new ReactivePropertySlim<bool>();
            IsEditing = new ReactivePropertySlim<bool>();
            IsSelected = new ReactivePropertySlim<bool>();
            VirtualKey = new ReactivePropertySlim<VirtualKey>(new VirtualKey(0, deserializing: deserializing));
            Modifiers = new ReactiveCollection<VirtualKey>();
            Macro = new ReactiveCollection<IMacro>();

            ToggleEditCommand = new ReactiveCommandSlim<KeyMap>();
            SetModifierCommand = new ReactiveCommandSlim<VirtualKey>();
            UnsetModifierCommand = new ReactiveCommandSlim<VirtualKey>();
            AddKeyMacroCommand = new ReactiveCommandSlim();
            AddCommandMacroCommand = new ReactiveCommandSlim();
            AddCodeMacroCommand = new ReactiveCommandSlim();
            RemoveMacroCommand = new ReactiveCommandSlim<IMacro>();

            ToggleEditCommand.Where(m => m != null).Subscribe(m =>
            {
                m.IsEditing.Value = !m.IsEditing.Value;
                if (m.IsEditing.Value)
                {
                    while (Modifiers.Count < 3)
                    {
                        Modifiers.Add(new VirtualKey(0));
                    }
                }
                else
                {
                    SortModifiers();
                }
            });

            SetModifierCommand.Where(vk => vk != null).Subscribe(vk =>
            {
                vk.Key.Value = CapturedVirtualKey.Instance.VirtualKey.Value.Key.Value;
                vk.Flags.Value = CapturedVirtualKey.Instance.VirtualKey.Value.Flags.Value;
            });

            UnsetModifierCommand.Where(vk => vk != null).Subscribe(vk =>
            {
                vk.Key.Value = 0;
                vk.Flags.Value = 0;
            });

            AddKeyMacroCommand.Subscribe(_ => Macro.Add(new InputMacro(deserializing: false)));
            AddCommandMacroCommand.Subscribe(_ => Macro.Add(new CommandMacro(deserializing :false)));
            AddCodeMacroCommand.Subscribe(_ => Macro.Add(new CodeMacro(deserializing:false)));
            RemoveMacroCommand.Subscribe(m => Macro.Remove(m));

            if (!deserializing)
            {
                DefineSubscribe();
            }
        }

        void DefineSubscribe()
        {
            ModifiersText = Modifiers.ObserveElementObservableProperty(x => x.Name)
                .Select(_ => Modifiers.Count == 0 ? "None" : string.Join(" + ", Modifiers.Where(m => m.Key.Value > 0).Select(m => m.Name.Value)))
                .ToReadOnlyReactivePropertySlim("None")
                .AddTo(Disposable);

            InlineText = Observable.CombineLatest(
                Macro.ObserveElementObservableProperty(x => x.InlineText),
                IsEditing,
                (_, _) => Macro.Count == 0 ? "None" : string.Join(" ", Macro.Select(m => m.InlineText.Value)))
                .ToReadOnlyReactivePropertySlim("None")
                .AddTo(Disposable);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            DefineSubscribe();
        }

        void SortModifiers()
        {
            var sorted = Modifiers
                .DistinctBy(m => m.Key.Value)
                .Where(x => x != null)
                .Where(k => k.Key.Value != VirtualKey.Value.Key.Value)
                .Select(k => (Key: k.Key.Value, Flags: k.Flags.Value, Device: k.Device.Value))
                .ToList();

            sorted.Sort((a, b) =>
            {
                if (a.Key <= 0 && b.Key <= 0) return 0;
                if (a.Key <= 0) return 1;
                if (b.Key <= 0) return -1;

                return a.Key.CompareTo(b.Key);
            });

            for (var i = 0; i < 3; i++)
            {
                if (i < sorted.Count)
                {
                    Modifiers[i].Key.Value = sorted[i].Key;
                    Modifiers[i].Flags.Value = sorted[i].Flags;
                    Modifiers[i].Device.Value = sorted[i].Device;
                }
                else
                {
                    Modifiers[i].Key.Value = 0;
                    Modifiers[i].Flags.Value = 0;
                    Modifiers[i].Device.Value = "";
                }
            }
        }
    }
}
