using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FileCloud.Desktop.Services
{
    public class FileService
    {
        private readonly HttpClient _client;

        public FileService(string apiBaseUrl)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl)
            };
        }

        /// <summary>
        /// Получить список всех файлов.
        /// </summary>
        public async Task<List<FileModel>> GetFilesAsync()
        {
            return await _client.GetFromJsonAsync<List<FileModel>>("/api/file");
        }

        /// <summary>
        /// Загрузить несколько файлов на сервер.
        /// </summary>
        public async Task<string> UploadFilesAsync(string path, IEnumerable<string> filePaths)
        {
            using var content = new MultipartFormDataContent();

            foreach (var filePath in filePaths)
            {
                var fileContent = new StreamContent(File.OpenRead(filePath));
                var fileName = Path.GetFileName(filePath);
                content.Add(fileContent, "files", fileName);
            }

            var response = await _client.PostAsync($"/api/file/stream-upload?path=uploads", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Удалить файлы по списку идентификаторов.
        /// </summary>
        public async Task<List<Guid>> DeleteFilesAsync(List<Guid> fileIds)
        {
            var response = await _client.PostAsJsonAsync("/api/file/delete", fileIds);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<Guid>>();
        }

        /// <summary>
        /// Получить превью изображения по идентификатору.
        /// </summary>
        public async Task<ImageSource> GetPreviewAsync(Guid id)
        {
            var response = await _client.GetAsync($"/api/file/preview/{id}");
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsByteArrayAsync();
            var image = ImageHelper.ByteArrayToImageSource(stream);

            return image;
        }
        /// <summary>
        /// Получить файлы по идентификатору.
        /// </summary>
        public async Task DownloadFilesAsync(IEnumerable<Guid> ids, string folder)
        {
            foreach(var id in ids)
            {
                var response = await _client.GetAsync($"/api/file/download/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var contentDisposition = response.Content.Headers.ContentDisposition;
                    var fileName = contentDisposition?.FileName?.Trim('"') ?? "new_file";
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    var savePath = Path.Combine(folder, fileName);
                    
                    await File.WriteAllBytesAsync(savePath, fileBytes);
                }
                else
                {
                    Console.WriteLine("error: Не удалось получить файл!");
                }
            }
        }
    }
}