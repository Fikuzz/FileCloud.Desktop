using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Services.Services
{
    public class FileSaveService : IFileSaveService
    {
        private readonly IAppSettingsService _settings;
        public FileSaveService(IAppSettingsService settings)
        {
            _settings = settings;
        }

        public async Task SaveFileAsync(Guid id, string fileName, byte[] content, string? path = null)
        {
            var directory = path != null ? path : _settings.DownloadPath;
            path = Path.Combine(directory, fileName);
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if(File.Exists(path))
            {
                Console.WriteLine("file already exist");
                return;
            }
            await File.WriteAllBytesAsync(path, content);
            FileMappingManager.AddOrUpdate(id, path);
        }
    }
}
