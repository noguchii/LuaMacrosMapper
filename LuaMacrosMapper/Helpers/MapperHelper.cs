using MessagePack.ReactivePropertyExtension;
using MessagePack.Resolvers;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaMacrosMapper.Helpers
{
    public static class MapperHelper
    {
        public static string ApplicationName => "LuaMacroMapper";
        public static string CompanyName => "noguchii.net";
        public static string WorkingDirectory => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), CompanyName, ApplicationName);
        public static string MappingsDirectory => Path.Combine(WorkingDirectory, "maps");
        public static string BackupDirectory => Path.Combine(WorkingDirectory, "backup");
        public static string GeneratedMacrosDirectory => Path.Combine(WorkingDirectory, "macros");

        private static IFormatterResolver? _Resolver;
        public static IFormatterResolver Resolver
        {
            get
            {
                if (_Resolver == null)
                {
                    _Resolver = CompositeResolver.Create(
                        ReactivePropertyResolver.Instance, 
                        ContractlessStandardResolver.Instance);
                }
                return _Resolver;
            }
            private set { _Resolver = value; }
        }

        public static async Task SerializeToJson<T>(string path, T obj)
        {
            var options = MessagePackSerializerOptions.Standard.WithResolver(Resolver);
            var json = MessagePackSerializer.SerializeToJson(obj, options);

            await SaveFileAsync(path, json);
        }

        public static async Task<T> DeerializeFromJson<T>(string path)
        {
            var options = MessagePackSerializerOptions.Standard.WithResolver(Resolver);
            var mapBytes = MessagePackSerializer.ConvertFromJson(await File.ReadAllTextAsync(path), options);

            return MessagePackSerializer.Deserialize<T>(mapBytes, options);
        }

        public static async Task SaveFileAsync(string path, string content)
        {
            var directory = Path.GetDirectoryName(path);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            await File.WriteAllTextAsync(path, content);
        }
    }
}
