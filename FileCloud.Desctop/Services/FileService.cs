using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Windows.Media;

namespace FileCloud.Desktop.Services
{
    public class FileService
    {
        private readonly HttpClient _client;

        public bool ServerState { get; set; }
        public FileService(string apiBaseUrl)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl)
            };
        }
        private (bool isActive, string error) IsServerActive()
        {
            string message = string.Empty;
            if (!ServerState)
                message = "Сервер закрыт.";
            return (ServerState, message);
        }
        /// <summary>
        /// Получить список всех файлов.
        /// </summary>
        public async Task<(List<FileModel>? result, string? error)> GetFilesAsync()
        {
            var conect = IsServerActive();
            if (!conect.isActive)
                return (null, conect.error);

            return (await _client.GetFromJsonAsync<List<FileModel>>("/api/file"), string.Empty);
        }
        /// <summary>
        /// Получить файл оп id.
        /// </summary>
        public async Task<(FileModel? result, string? error)> GetFileByIdAsync(Guid id)
        {
            var conect = IsServerActive();
            if (!conect.isActive)
                return (null, conect.error);

            return (await _client.GetFromJsonAsync<FileModel>($"/api/file/{id}"), string.Empty);
        }
        /// <summary>
        /// Загрузить несколько файлов на сервер.
        /// </summary>
        public async Task<(string? result, string? error)> UploadFilesAsync(string path, IEnumerable<string> filePaths)
        {
            var conect = IsServerActive();
            if (!conect.isActive)
                return (null, conect.error);

            using var content = new MultipartFormDataContent();

            foreach (var filePath in filePaths)
            {
                var fileContent = new StreamContent(File.OpenRead(filePath));
                var fileName = Path.GetFileName(filePath);
                content.Add(fileContent, "files", fileName);
            }

            var response = await _client.PostAsync($"/api/file/stream-upload?path=uploads", content);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadAsStringAsync(), string.Empty);
        }
        /// <summary>
        /// Изменить файл.
        /// </summary>
        public async Task<(string? result, string? error)> UpdateFileAsync(Guid id, string newName, string newPath)
        {
            try
            {
                var conect = IsServerActive();
                if (!conect.isActive)
                    return (null, conect.error);
                var dto = new
                {
                    name = newName,
                    path = newPath
                };

                var json = JsonSerializer.Serialize(dto);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _client.PatchAsync($"/api/file/{id}", content);
                response.EnsureSuccessStatusCode();

                return (await response.Content.ReadAsStringAsync(), string.Empty);
            }
            catch(Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// Удалить файлы по списку идентификаторов.
        /// </summary>
        public async Task<(List<Guid>? result, string? error)> DeleteFilesAsync(List<Guid> fileIds)
        {
            var conect = IsServerActive();
            if (!conect.isActive)
                return (null, conect.error);

            var response = await _client.PostAsJsonAsync("/api/file/delete", fileIds);
            response.EnsureSuccessStatusCode();

            return (await response.Content.ReadFromJsonAsync<List<Guid>>(), string.Empty);
        }

        /// <summary>
        /// Получить превью изображения по идентификатору.
        /// </summary>
        public async Task<(ImageSource? result, string? error)> GetPreviewAsync(Guid id)
        {
            var conect = IsServerActive();
            if (!conect.isActive)
                return (null, conect.error);

            var response = await _client.GetAsync($"/api/file/preview/{id}");
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsByteArrayAsync();
            var image = ImageHelper.ByteArrayToImageSource(stream);

            return (image, string.Empty);
        }
        /// <summary>
        /// Получить файлы по идентификатору.
        /// </summary>
        public async Task<(string? result, string? error)> DownloadFileAsync(Guid id, string folder)
        {
            var conect = IsServerActive();
            if (!conect.isActive)
                return (null, conect.error);

            var response = await _client.GetAsync($"/api/file/download/{id}");
            if (response.IsSuccessStatusCode)
            {
                var contentDisposition = response.Content.Headers.ContentDisposition;
                var fileName = contentDisposition?.FileName?.Trim('"') ?? "new_file";

                var newName = ScriptHelper.Rename(fileName);

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var savePath = Path.Combine(folder, newName);

                await File.WriteAllBytesAsync(savePath, fileBytes);

                FileMappingManager.AddOrUpdate(id, savePath);
                return (newName, string.Empty);
            }
            else
            {
                return (null, "Не удалось получить файл!");
            }
        }
    }
}