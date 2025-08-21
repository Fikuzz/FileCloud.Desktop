using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Helpers
{
    public class FileMappingManager
    {
        private static readonly string AppDataFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FileCloud.Desktop");

        private static readonly string MappingFile = Path.Combine(AppDataFolder, "file_mapping.json");

        private static List<FileMapping> _mappings = new();

        static FileMappingManager()
        {
            Load();
        }

        public static void AddOrUpdate(Guid id, string localPath)
        {
            var existing = _mappings.FirstOrDefault(m => m.Id == id);
            if (existing != null)
            {
                existing.LocalPath = localPath;
            }
            else
            {
                _mappings.Add(new FileMapping { Id = id, LocalPath = localPath });
            }
            Save();
        }

        public static string? GetLocalPath(Guid id)
        {
            return _mappings.FirstOrDefault(m => m.Id == id)?.LocalPath;
        }

        public static void Remove(Guid id)
        {
            _mappings.RemoveAll(m => m.Id == id);
            Save();
        }

        private static void Load()
        {
            if (!Directory.Exists(AppDataFolder))
                Directory.CreateDirectory(AppDataFolder);

            if (File.Exists(MappingFile))
            {
                var json = File.ReadAllText(MappingFile);
                _mappings = JsonSerializer.Deserialize<List<FileMapping>>(json) ?? new();
            }
        }

        private static void Save()
        {
            var json = JsonSerializer.Serialize(_mappings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(MappingFile, json);
        }

        private class FileMapping
        {
            public Guid Id { get; set; }
            public string LocalPath { get; set; } = "";
        }
    }
}
