using LuaMacrosMapper.Helpers;
using LuaMacrosMapper.Models;
using MessagePack;
using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LuaMacrosMapper.ViewModels
{
    [MessagePackObject]
    public class CommandMacro : NotificationObject, IMacro, IMessagePackSerializationCallbackReceiver
    {
        [Key(1)]
        public ReactivePropertySlim<string> Application { get; set; }
        [Key(2)]
        public ReactivePropertySlim<string> Arguments { get; set; }
        [IgnoreMember]
        public IReactiveProperty<string> InlineText { get; private set; } = null!;
        [IgnoreMember]
        public ReadOnlyReactivePropertySlim<string> ApplicationName { get; private set; } = null!;
        [IgnoreMember]
        public ReadOnlyReactivePropertySlim<bool> IsVisiblePlaceHolder { get; private set; } = null!;
        [IgnoreMember]
        public ReadOnlyReactivePropertySlim<string> SelectAppButtonText { get; private set; } = null!;
        [IgnoreMember]
        public ReactiveCommandSlim SelectAppCommand { get; }
        
        [Obsolete("Default constractor only use to deserialize.", true)]
        public CommandMacro() : this(deserializing: true)
        {

        }
        public CommandMacro(bool deserializing = false)
        {
            Application = new ReactivePropertySlim<string>();
            Arguments = new ReactivePropertySlim<string>();
            SelectAppCommand = new ReactiveCommandSlim();

            SelectAppCommand.Subscribe(OnSelectApp);

            if (!deserializing)
            {
                DefineSubscribe();
            }
        }

        void DefineSubscribe()
        {
            ApplicationName = Application.Select(app => !string.IsNullOrEmpty(app) && Path.Exists(app) ? Path.GetFileNameWithoutExtension(app) :  string.Empty)
                .ToReadOnlyReactivePropertySlim(string.Empty).AddTo(Disposable);

            SelectAppButtonText = ApplicationName.Select(n => string.IsNullOrEmpty(n) ? "Select App" : n)
                .ToReadOnlyReactivePropertySlim(string.Empty).AddTo(Disposable);

            InlineText = ApplicationName.Select(app => string.IsNullOrEmpty(app) ? "[Unkonwn Command]" : $"{{{app}}}")
                .ToReactiveProperty(string.Empty).AddTo(Disposable);

            IsVisiblePlaceHolder = Arguments.Select(string.IsNullOrEmpty)
                .ToReadOnlyReactivePropertySlim(false).AddTo(Disposable);
        }
        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            DefineSubscribe();
        }

        private void OnSelectApp()
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            try
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
            }
            catch { }
            dialog.Filter = "Exe files (*.exe)|*.exe|Bat files (*.bat)|*.bat|Shortcut files (*.lnk)|*.lnk";

            if (dialog.ShowDialog() == true)
            {
                var app = dialog.FileName;
                Application.Value = app;
            }
        }
    }
}
