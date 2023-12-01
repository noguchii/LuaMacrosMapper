using MessagePack;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuaMacrosMapper.ViewModels
{
    [Union(0, typeof(InputMacro))]
    [Union(1, typeof(CommandMacro))]
    [Union(2, typeof(CodeMacro))]
    public interface IMacro : INotifyPropertyChanged
    {
        IReactiveProperty<string> InlineText { get; }
    }
}
