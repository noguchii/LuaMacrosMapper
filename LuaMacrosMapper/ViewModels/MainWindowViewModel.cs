using LuaMacrosMapper.Behaviors;
using LuaMacrosMapper.Helpers;
using LuaMacrosMapper.Models;
using MessagePack;
using MessagePack.ReactivePropertyExtension;
using MessagePack.Resolvers;
using Microsoft.Xaml.Behaviors.Core;
using Reactive.Bindings;
using Reactive.Bindings.Disposables;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace LuaMacrosMapper.ViewModels
{
    public class MainWindowViewModel : NotificationObject
    {
        public ReactiveCollection<Mapping> Mappings { get; }
        public ReactivePropertySlim<Mapping?> SelectedMapping { get; }
        public ReactivePropertySlim<VirtualKey> CapturedKey { get; }
        public ReactiveCollection<VirtualKeyMenuItem> SpecialKeyMenuItems { get; }
        public ReactiveCommandSlim<RawInputBehavior.InputEventArgs> RawKeyDownCommand { get; }
        public ReactiveCommandSlim<RawInputBehavior.InputEventArgs> RawKeyUpCommand { get; }
        public ReactiveCommandSlim NewMappingCommand { get; }
        public ReactiveCommandSlim DeleteMappingCommand { get; }
        public ReactiveCommandSlim AddKeyMapCommand { get; }
        public AsyncReactiveCommand GenerateCommand { get; }
        public AsyncReactiveCommand LoadedWindowCommand { get; }
        public ReactiveCommandSlim CopyCapturedDeviceCommand { get; }
        public ReactiveCommandSlim UnsetCapturedDeviceCommand { get; }
        public ReactiveCommandSlim ToggleCapturedFlagCommand { get; }
        public ReactiveCommandSlim<VirtualKeyMenuItem> SetSpecialKeyCommand { get; }

        public ReactivePropertySlim<bool> IsWindowInitilized { get; }
        public ReadOnlyReactivePropertySlim<bool> IsMappingSelected { get; }

        Regex DeviceNameRegex = new Regex(@"#([\da-fA-F]\&[\da-fA-F]+)&0&0000#");
        readonly string Undefined = "Undefined";

        public MainWindowViewModel()
        {
            IsWindowInitilized = new ReactivePropertySlim<bool>(false);
            CapturedKey = CapturedVirtualKey.Instance.VirtualKey
                .ToReactivePropertySlimAsSynchronized(x => x.Value).AddTo(Disposable);
            Mappings = new ReactiveCollection<Mapping>();
            SelectedMapping = new ReactivePropertySlim<Mapping?>();
            SpecialKeyMenuItems = new ReactiveCollection<VirtualKeyMenuItem>();
            BuildSpecialKeyMenuItems();

            IsMappingSelected = SelectedMapping.CombineLatest(IsWindowInitilized, (m, i) => m != null && i)
                .ToReadOnlyReactivePropertySlim().AddTo(Disposable);

            LoadedWindowCommand = new AsyncReactiveCommand().AddTo(Disposable);
            RawKeyDownCommand = new ReactiveCommandSlim<RawInputBehavior.InputEventArgs>(IsWindowInitilized);
            RawKeyUpCommand = new ReactiveCommandSlim<RawInputBehavior.InputEventArgs>(IsWindowInitilized);
            NewMappingCommand = new ReactiveCommandSlim(IsWindowInitilized);
            DeleteMappingCommand = new ReactiveCommandSlim(IsMappingSelected);
            AddKeyMapCommand = new ReactiveCommandSlim(IsMappingSelected).AddTo(Disposable);
            GenerateCommand = new AsyncReactiveCommand(IsMappingSelected).AddTo(Disposable);
            CopyCapturedDeviceCommand = new ReactiveCommandSlim(IsWindowInitilized).AddTo(Disposable);
            UnsetCapturedDeviceCommand = new ReactiveCommandSlim(IsWindowInitilized).AddTo(Disposable);
            ToggleCapturedFlagCommand = new ReactiveCommandSlim(IsWindowInitilized).AddTo(Disposable);
            SetSpecialKeyCommand = new ReactiveCommandSlim<VirtualKeyMenuItem>(IsMappingSelected).AddTo(Disposable);

            LoadedWindowCommand.Subscribe(OnWindowLoadedAsync).AddTo(Disposable);
            RawKeyDownCommand.Subscribe(OnRawKeyDown).AddTo(Disposable);
            RawKeyUpCommand.Subscribe(OnRawKeyUp).AddTo(Disposable);
            NewMappingCommand.Subscribe(OnNewMapping).AddTo(Disposable);
            DeleteMappingCommand.Subscribe(OnDeleteMapping).AddTo(Disposable);
            AddKeyMapCommand.Subscribe(OnAddKeyMap).AddTo(Disposable);
            GenerateCommand.Subscribe(OnGenerate).AddTo(Disposable);
            CopyCapturedDeviceCommand.Subscribe(OnCopyDevice).AddTo(Disposable);
            SetSpecialKeyCommand.Subscribe(OnSetSpecialKey).AddTo(Disposable);
            ToggleCapturedFlagCommand.Subscribe(OnToggleCapturedFlag).AddTo(Disposable);
            UnsetCapturedDeviceCommand.Subscribe(OnUnsetCapturedDevice).AddTo(Disposable);

            SelectedMapping.Subscribe(mapping =>
            {
                if (mapping == null) return;
                foreach (var map in mapping.KeyMaps)
                {
                    map.IsSimulated.Value = false;
                }
            }).AddTo(Disposable);
        }


        #region Initilize Methods
        public async Task OnWindowLoadedAsync()
        {
            if (IsWindowInitilized.Value) return;
            try
            {
                await LoadMappingsAsync();

                if (Mappings.Count > 0)
                {
                    SelectedMapping.Value = Mappings.FirstOrDefault();
                }
            }
            finally
            {
                IsWindowInitilized.Value = true;
            }
        }

        async Task LoadMappingsAsync()
        {
            try
            {
                var files = Directory.GetFiles(MapperHelper.MappingsDirectory, "*.luamapping");
                foreach (var path in files)
                {
                    try
                    {
                        var map = await MapperHelper.DeerializeFromJson<Mapping>(path);
                        map.Filename.Value = path;
                        if (map == null) throw new InvalidDataException();

                        Mappings.Add(map);
                    }
                    catch (Exception ex)
                    {
                        Debugger.Log(1, "Serialize Error", ex.ToString());
                    }
                }
            }
            catch
            {
            }
        }

        void BuildSpecialKeyMenuItems()
        {
            var Item = new Func<int, int, VirtualKeyMenuItem>((key, flags) =>
            {
                var vk = new VirtualKey(key, flags, deserializing: false);
                return new VirtualKeyMenuItem(vk.Name.Value, vk);
            });

            var mediaCategory = new VirtualKeyMenuItem()
            {
                Header = "Media",
                Items = Enumerable.Range(166, 18).Select(index => Item(index, 2)).ToList()
            };
            mediaCategory.Items.AddRange(new List<int>() { 94, 95 }.Select(index => Item (index, 2)));

            var funcCategory = new VirtualKeyMenuItem()
            {
                Header = "Function",
                Items = Enumerable.Range(124, 12).Select(index => Item(index, 0)).ToList()
            };

            var specialKeys = new List<VirtualKeyMenuItem>
            {
                Item(22, 0),
                Item(26, 0),
                Item(44, 2),
                mediaCategory,
                funcCategory
            };

            SpecialKeyMenuItems.AddRangeOnScheduler(specialKeys);
        }

        #endregion

        #region Captured Key Methods
        private void OnToggleCapturedFlag()
        {
            var current = CapturedKey.Value.Flags.Value;
            CapturedKey.Value.Flags.Value = current == 0 ? 2 : 0;
        }

        private void OnUnsetCapturedDevice()
        {
            CapturedKey.Value.Device.Value = Undefined;
        }

        private void OnSetSpecialKey(VirtualKeyMenuItem menuItem)
        {
            if (menuItem == null || menuItem.VirtualKey == null) return;

            CapturedKey.Value.Key.Value = menuItem.VirtualKey.Key.Value;
            CapturedKey.Value.Flags.Value = menuItem.VirtualKey.Flags.Value;
        }

        void OnRawKeyDown(RawInputBehavior.InputEventArgs e)
        {
            // Key Capture
            var vk = ConvertRawInput(e);
            CapturedKey.Value.Key.Value = vk.Key;
            CapturedKey.Value.Flags.Value = vk.Flags;
            CapturedKey.Value.Device.Value = vk.Device;

            Simulate(vk.Key, vk.Flags, vk.Device, true);
        }

        void OnRawKeyUp(RawInputBehavior.InputEventArgs e)
        {
            var vk = ConvertRawInput(e);
            // 0: down, 1: up, 2: down ext, 3: up ext
            Simulate(vk.Key, vk.Flags - 1, vk.Device, false);
        }

        (int Key, int Flags, string Device) ConvertRawInput(RawInputBehavior.InputEventArgs e)
        {
            var key = e.VirtualKeyCode;
            var flags = e.Flags;
            var match = DeviceNameRegex.Match(e.DeviceName);
            string device = Undefined;
            if (match.Success)
            {
                device = match.Groups[1].Value;
            }

            return (key, flags, device);
        }
        void OnCopyDevice()
        {
            var device = CapturedKey.Value.Device.Value;
            if (device == null) return;

            Clipboard.SetDataObject(device, true);
            MessageBox.Show($@"Copy device name to clipboard", "LuaMacros Mapper");
        }

        void Simulate(int key, int flags, string device, bool isDown)
        {
            var mapping = SelectedMapping.Value;
            if (mapping == null) return;

            foreach (var map in mapping.KeyMaps
                .Where(k => k.IsSimulated.Value != isDown && k.VirtualKey.Value.EqualsValue(key, flags, device)))
            {
                map.IsSimulated.Value = isDown;
            }
        }

        #endregion

        #region Mapping Methods

        async void OnNewMapping()
        {
            var mapping = new Mapping(deserializing: false);
            mapping.Name.Value = "New Mapping";
            
            var directory = MapperHelper.MappingsDirectory;
            string newPath = string.Empty;

            while (true)
            {
                newPath = Path.Combine(directory, $"map{DateTime.Now.ToString("yyyyMMddHHmmssf")}.luamapping");
                if (!File.Exists(newPath))
                {
                    mapping.Filename.Value = newPath;
                    await SaveMappingAsync(mapping);
                    break;
                }
                await Task.Delay(100);
            }

            Mappings.Add(mapping);

            SelectedMapping.Value = mapping;
        }

        void OnDeleteMapping()
        {
            var mapping = SelectedMapping.Value;
            if (mapping != null)
            {
                try
                {
                    var index = Mappings.IndexOf(mapping);
                    if (Mappings.Remove(mapping))
                    {
                        var nextSelect = Mappings.Skip(Math.Max(index - 1, 0)).FirstOrDefault();
                        SelectedMapping.Value = nextSelect;

                        if (Path.Exists(mapping.Filename.Value))
                        {
                            File.Delete(mapping.Filename.Value);
                        }
                    }
                }
                finally
                {
                    mapping.Dispose();
                }
            }
        }

        void OnAddKeyMap()
        {
            if (CapturedKey.Value.Device.Value == Undefined)
            {
                MessageBox.Show($@"""{Undefined}"" device can't add. Media keys can't map because these don't have device name.", "LuaMacros Mapper : Error");
                return;
            }

            if (CapturedKey.Value.Key.Value <= 0) return;

            SelectedMapping.Value?.AddKeyMap(CapturedKey.Value.Clone());
        }

        async Task SaveMappingAsync(Mapping map)
        {
            try
            {
                var path = map.Filename.Value;
                await MapperHelper.SerializeToJson(path, map);
            }
            catch (MessagePackSerializationException ex)
            {
                Debugger.Log(1, "Serialize Error", ex.ToString());
            }
        }

        async Task OnGenerate()
        {
            var mapping = SelectedMapping.Value;
            if (mapping == null) return;

            try
            {
                await mapping.GenerateAndOutput();
            }
            catch
            {
                MessageBox.Show("Failed to generate", "LuaMacros Mapper : Error");
            }

            try
            {
                Process.Start("explorer.exe", @$"""{MapperHelper.GeneratedMacrosDirectory}""");
            }
            catch { }
        }

        #endregion
    }
}
