using LuaMacrosMapper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaMacrosMapper.ViewModels
{
    public class VirtualKeyMenuItem : NotificationObject
    {
        public string? Header { get; set; }
        public VirtualKey? VirtualKey { get; set; }
        public List<VirtualKeyMenuItem>? Items { get; set; }

        public VirtualKeyMenuItem() {  }

        public VirtualKeyMenuItem(string? header, VirtualKey? virtualKey = null, List<VirtualKeyMenuItem>? items = null)
        {
            Header = header;
            VirtualKey = virtualKey;
            Items = items;
        }
    }
}
