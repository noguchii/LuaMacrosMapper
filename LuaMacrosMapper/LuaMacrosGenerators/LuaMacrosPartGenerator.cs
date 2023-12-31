﻿using LuaMacrosMapper.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace LuaMacrosMapper.LuaMacrosGenerators
{
    public abstract class LuaMacrosGeneratorPart
    {
        public List<LuaMacrosGeneratorPart> InlineGenerator { get; set; } = new List<LuaMacrosGeneratorPart>();

        public abstract string Generate();

        #region Generation Shortcuts
        protected LuaMacrosGeneratorPart? Root { get; set; }

        protected string _Is(bool flag, string code)
        {
            return flag ? code : "";
        }

        protected string _Inline(int indent = 2)
        {
            var indentString = "";
            for (var i = 0; i < indent; i++)
            {
                indentString += " ";
            }

            return string.Join("\n", InlineGenerator.SelectMany(g => g.Generate().Split("\n")).Select(o => indentString + o));
        }

        #endregion
    }

    public class CommandPart :  LuaMacrosGeneratorPart
    {
        public string Application { get; set; } = "";
        public string? Arguments { get; set; }

        public override string Generate()
        {
            var app = Application.Replace("\\", "\\\\");
            var args = (Arguments ?? "").Replace("\\", "\\\\");

            if (string.IsNullOrEmpty(args))
            {
                return $@"lmc_spawn('{app}')";
            }
            else
            {
                return $@"lmc_spawn('{app}', '{args}')";
            }
        }
    }

    public class CodePart : LuaMacrosGeneratorPart
    {
        public string Code { get; set; } = "";
        public override string Generate()
        {
            return Code;
        }
    }

    public class KeyInputPart : LuaMacrosGeneratorPart
    {
        public VirtualKey[]? Keys { get; set; }

        public override string Generate()
        {
            var code = "";
            if (Keys != null)
            {
                for (var i = 0; i < Keys.Length; i++)
                {
                    var downflags = (Keys[i].Flags.Value == 0 ? 0 : 1);
                    code += $"lmc_send_input({Keys[i].Key.Value}, 0, {downflags}) -- {Keys[i].Name.Value} down\n";
                }
            }
            code += $"{_Inline()}\n";
            if (Keys != null)
            {
                for (var i = Keys.Length - 1; i >= 0; i--)
                {
                    var upflags = (Keys[i].Flags.Value == 0 ? 2 : 3);
                    code += $"lmc_send_input({Keys[i].Key.Value}, 0, {upflags}) -- {Keys[i].Name.Value} up\n";
                }
            }
            return code;
        }
    }

    public class KeyMapPart : LuaMacrosGeneratorPart
    {
        public VirtualKey? VirtualKey { get; set; }
        public VirtualKey[]? Modifiers { get; set; }

        public override string Generate()
        {
            if (VirtualKey == null) return $"{_Inline()}\n";

            var upflags = VirtualKey.Flags.Value + 1; // convert to up flags
            var modifiers = "";
            var modifiersComment = "";
            if (Modifiers != null)
            {
                foreach (var m in Modifiers)
                {
                    modifiers += $" and pressed[{m.Key.Value}] == 1";
                    modifiersComment += $" + {m.Name.Value}";
                }
            }

            var code =
$@"if (button == {VirtualKey.Key.Value} and flags == {upflags}{modifiers}) then  -- {VirtualKey.Name.Value}{modifiersComment}
{_Inline()}
end";
            return code;
        }
    }

    public class DeviceHandlePart: LuaMacrosGeneratorPart
    {
        public string Device { get; set; } = "";
        public int DeviceIndex { get; set; } = 0;

        public override string Generate()
        {
            var code =
@$"lmc_device_set_name(""{Device}"", ""{Device}"");
pressed{DeviceIndex} = {{}}
lmc_set_handler(""{Device}"", function(button, direction, ts, flags)
  pressed{DeviceIndex}[button] = direction
  if direction == 1 then return end
  local pressed = pressed{DeviceIndex}
{_Inline()}
end)
";
            return code;
        }
    }

    public class LuaMacrosGenerator : LuaMacrosGeneratorPart
    {
        public bool IsMinimaize { get; set; }
        public string Name { get; set; } = "";

        public override string Generate()
        {
            var code =
$@"-- {Name} generated by LuaMacros Mapper
{_Is(IsMinimaize,
@"lmc.minimizeToTray = true
lmc_minimize()")}
{_Inline(0)}";
            return code;
        }
    }
}
