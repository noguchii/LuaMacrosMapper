using LuaMacrosMapper.Models;
using MessagePack;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace LuaMacrosMapper.ViewModels
{
    [MessagePackObject]
    public class CodeMacro : NotificationObject, IMacro, IMessagePackSerializationCallbackReceiver
    {
        [Key(0)]
        public ReactivePropertySlim<string> Code { get; set; }
        [IgnoreMember]
        public IReactiveProperty<string> InlineText { get; } 

        [Obsolete("Default constractor only use to deserialize.", true)]
        public CodeMacro() : this(deserializing: true)
        { 
        }

        public CodeMacro(bool deserializing = false)
        {
            Code = new ReactivePropertySlim<string>();
            InlineText = new ReactivePropertySlim<string>("[Code]").ToReactiveProperty<string>().AddTo(Disposable);

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

    }
}
