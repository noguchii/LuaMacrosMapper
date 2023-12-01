using LuaMacrosMapper.Helpers;
using LuaMacrosMapper.LuaMacrosGenerators;
using LuaMacrosMapper.Models;
using MessagePack;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace LuaMacrosMapper.ViewModels
{
    [MessagePackObject]
    public class Mapping : NotificationObject, IMessagePackSerializationCallbackReceiver
    {
        [Key(0)]
        public ReactivePropertySlim<string> Name { get; set; }
        [Key(1)]
        public ReactiveCollection<KeyMap> KeyMaps { get; set; }
        [Key(2)]
        public ReactivePropertySlim<bool> IsMinimaizeOnRun { get; set; }
        [IgnoreMember]
        public ReactivePropertySlim<string> Filename { get; }
        [IgnoreMember]
        public ReactivePropertySlim<bool> IsEditingName { get; }
        [IgnoreMember]
        public ReactiveCommandSlim DoubleClickCommand { get; }
        [IgnoreMember]
        public AsyncReactiveCommand LostFocusCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim RemoveKeyMapCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim ToActiveCommand { get; }
        [IgnoreMember]
        public ReactiveCommandSlim ToInactiveCommand { get; }
        [IgnoreMember]
        public AsyncReactiveCommand SaveCommand { get; }

        [Obsolete("Default constractor only use to deserialize.", true)]
        public Mapping() : this(deserializing: true)
        {

        }

        public Mapping(bool deserializing)
        {
            Name = new ReactivePropertySlim<string>();
            KeyMaps = new ReactiveCollection<KeyMap>();
            Filename = new ReactivePropertySlim<string>();

            IsMinimaizeOnRun = new ReactivePropertySlim<bool>(true);
            IsEditingName = new ReactivePropertySlim<bool>();

            DoubleClickCommand = new ReactiveCommandSlim();
            LostFocusCommand = new AsyncReactiveCommand();
            RemoveKeyMapCommand = new ReactiveCommandSlim();
            ToActiveCommand = new ReactiveCommandSlim();
            ToInactiveCommand = new ReactiveCommandSlim();
            SaveCommand = new AsyncReactiveCommand();

            DoubleClickCommand.Subscribe(() => IsEditingName.Value = true).AddTo(Disposable);
            RemoveKeyMapCommand.Subscribe(() =>
            {
                var target = KeyMaps.Where(m => m.IsSelected.Value).ToList();
                foreach (var item in target)
                {
                    KeyMaps.Remove(item);
                    item.Dispose();
                }
            }).AddTo(Disposable);
            ToActiveCommand.Subscribe(() =>
            {
                foreach (var item in KeyMaps.Where(m => m.IsSelected.Value))
                {
                    item.IsActive.Value = true;
                }
            }).AddTo(Disposable);
            ToInactiveCommand.Subscribe(() =>
            {
                foreach (var item in KeyMaps.Where(m => m.IsSelected.Value))
                {
                    item.IsActive.Value = false;
                }
            }).AddTo(Disposable);
            LostFocusCommand.Subscribe(async () =>
            {
                try
                {
                    await Save();
                }
                catch (Exception)
                {
                    // failed save
                }

                IsEditingName.Value = false;
            }).AddTo(Disposable);

            SaveCommand.Subscribe(async () =>
            {
                try
                {
                    await Save();
                }
                catch
                {
                    // Failed
                }
            });

            if (!deserializing)
            {
                DefineSubscribe();
            }
        }

        void DefineSubscribe()
        {
        }
        public void OnBeforeSerialize()
        {
        }
        public void OnAfterDeserialize()
        {
            DefineSubscribe();
        }

        public void AddKeyMap(VirtualKey virtualKey)
        {
            var km = new KeyMap(false);
            km.VirtualKey.Value = virtualKey;

            KeyMaps.Add(km);
        }

        public async Task Save()
        {
            await MapperHelper.SerializeToJson(Filename.Value, this);
        }

        public string Generate()
        {
            var root = new LuaMacrosGenerator()
            {
                IsMinimaize = IsMinimaizeOnRun.Value,
                Name = Name.Value
            };

            var devices = KeyMaps.GroupBy(m => m.VirtualKey.Value.Device.Value).ToList();
            for (var i = 0; i < devices.Count; i++)
            {
                var device = devices[i];
                if (device.Count() == 0) continue;

                var handlerPart = new DeviceHandlePart()
                {
                    Device = device.First().VirtualKey.Value.Device.Value,
                    DeviceIndex = i
                };

                foreach (var map in device)
                {
                    var mapPart = new KeyMapPart()
                    {
                        VirtualKey = map.VirtualKey.Value.Clone(),
                        Modifiers = map.Modifiers.Where(m => m.Key.Value > 0).Select(m => m.Clone()).ToArray()
                    };

                    if (map.Macro == null) continue;
                    foreach (var macro in map.Macro)
                    {
                        LuaMacrosGeneratorPart? macroPart = null;

                        if (macro is InputMacro key)
                        {
                            var keys = new List<VirtualKey>();
                            if (key.WithRWin.Value) keys.Add(new VirtualKey(92));
                            if (key.WithWin.Value) keys.Add(new VirtualKey(91));
                            if (key.WithRAlt.Value) keys.Add(new VirtualKey(165));
                            if (key.WithAlt.Value) keys.Add(new VirtualKey(164));
                            if (key.WithRCtrl.Value) keys.Add(new VirtualKey(163));
                            if (key.WithCtrl.Value) keys.Add(new VirtualKey(162));
                            if (key.WithRShift.Value) keys.Add(new VirtualKey(161));
                            if (key.WithShift.Value) keys.Add(new VirtualKey(160));
                            keys.Add(key.VirtualKey.Value.Clone());

                            macroPart = new KeyInputPart()
                            {
                                Keys = keys.ToArray(),
                            };
                        }
                        else if (macro is CommandMacro cmd)
                        {
                            macroPart = new CommandPart()
                            {
                                Application = cmd.Application.Value,
                                Arguments = cmd.Arguments.Value,
                            };
                        }
                        else if (macro is CodeMacro code)
                        {
                            macroPart = new CodePart()
                            {
                                Code = code.Code.Value
                            };
                        }

                        if (macroPart == null) continue;
                        mapPart.InlineGenerator.Add(macroPart);
                    }

                    handlerPart.InlineGenerator.Add(mapPart);
                }

                root.InlineGenerator.Add(handlerPart);
            }

            return root.Generate();
        }

        public async Task<string> GenerateAndOutput()
        {
            var lua = Generate();
            var filename = Path.GetFileNameWithoutExtension(Filename.Value);
            var path = Path.Combine(MapperHelper.GeneratedMacrosDirectory, $"{filename}.lua");
            await MapperHelper.SaveFileAsync(path, lua);

            return path;
        }
    }
}
