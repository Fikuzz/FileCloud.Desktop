using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Models.Responses;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using FileCloud.Desktop.Models.Requests;
using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Services.Services;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Models;

namespace FileCloud.Desktop.Services
{
    public class FileService
    {
        private readonly string _apiSubUrl = "/api/file";

        private readonly HttpClient _client;
        private readonly ILogger<FileService> _logger;
        public FileService(IAppSettingsService settings, ILogger<FileService> logger)
        {
            _logger = logger;
            _client = new HttpClient
            {
                BaseAddress = new Uri(settings.ApiBaseUrl)
            };
        }
        /// <summary>
        /// Получить список всех файлов.
        /// </summary>
        public async Task<List<FileModel>> GetFilesAsync()
        {
            return await ServerStateService.ExecuteIfServerActive<List<FileModel>>(_logger, async () =>
            {
                var result = await _client.GetFromJsonAsync<List<ApiResult<FileModel>>>("");

                if (result == null || result.Count == 0)
                    return new List<FileModel>();

                // Логирование ошибок
                foreach (var r in result.Where(r => r.Error != null))
                    _logger.LogError("Ошибка при получении файла: {Error}", r.Error);

                return result
                    .Where(r => r.Error == null)
                    .Select(r => r.Response!)
                    .ToList();
            });
        }
        /// <summary>
        /// Загрузить несколько файлов на сервер.
        /// </summary>
        public async Task<FileModel> UploadFileAsync(Guid folderId, string filePath)
        {
            return await ServerStateService.ExecuteIfServerActive<FileModel>(_logger, async () =>
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogError("Файл не найден");
                    throw new FileNotFoundException($"Файл по пути {filePath} не найден");
                }

                using var content = new MultipartFormDataContent();
                FileUploadRequest uploadRequest = new FileUploadRequest()
                {
                    Name = Path.GetFileName(filePath),
                    Stream = File.OpenRead(filePath),
                    FolderId = folderId
                };

                var fileContent = new StreamContent(uploadRequest.Stream);
                content.Add(new StringContent(uploadRequest.FolderId.ToString()), "folderId");
                content.Add(fileContent, "file", uploadRequest.Name);

                var response = await _client.PostAsync($"{_apiSubUrl}/stream-upload", content);

                if (!response.IsSuccessStatusCode)
                {
                    var serverError = await response.Content.ReadAsStringAsync();
                    _logger.LogError(serverError);
                    throw new HttpRequestException($"Ошибка сервера: {serverError}");
                }

                // Считываем результат сервера
                var serverResult = await response.Content.ReadFromJsonAsync<ApiResult<FileModel>>();
                if(serverResult.Error != null)
                {
                    _logger.LogError($"Error: {serverResult.Error}");
                    throw new HttpRequestException(serverResult.Error);
                }
                return serverResult.Response;
            });
        }
        /// <summary>
        /// Удалить файл по идентификатору.
        /// </summary>
        public async Task<DeleteFileResponse> DeleteFileAsync(Guid fileId)
        {
            return await ServerStateService.ExecuteIfServerActive<DeleteFileResponse>(_logger, async () =>
            {
                var response = await _client.DeleteAsync($"{_apiSubUrl}/delete/{fileId}");
                if (!response.IsSuccessStatusCode)
                {
                    var serverError = await response.Content.ReadAsStringAsync();
                    _logger.LogError(serverError);
                    throw new HttpRequestException($"Ошибка сервера: {serverError}");
                }

                // Считываем результат сервера
                var serverResult = await response.Content.ReadFromJsonAsync<ApiResult<DeleteFileResponse>>();
                if(serverResult.Error != null)
                {
                    _logger.LogError($"Error: {serverResult.Error}");
                    throw new HttpRequestException(serverResult.Error);
                }
                return serverResult.Response;
            });
        }

        /// <summary>
        /// Получить превью изображения по идентификатору.
        /// </summary>
        public async Task<byte[]> GetPreviewAsync(Guid id)
        {
            return await ServerStateService.ExecuteIfServerActive<byte[]>(_logger, async () =>
            {
                var response = await _client.GetAsync($"{_apiSubUrl}/preview/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var serverError = await response.Content.ReadAsStringAsync();
                    _logger.LogError(serverError);
                    throw new HttpRequestException($"Ошибка сервера: {serverError}");
                }

                var bytes = await response.Content.ReadAsByteArrayAsync();

                return bytes;
            });
        }
        /// <summary>
        /// Загрузить файл по идентификатору.
        /// </summary>
        public async Task<byte[]> DownloadFileAsync(Guid id)
        {
            return await ServerStateService.ExecuteIfServerActive<byte[]>(_logger, async () =>
            {
                var response = await _client.GetAsync($"{_apiSubUrl}/download/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var serverError = await response.Content.ReadAsStringAsync();
                    _logger.LogError(serverError);
                    throw new HttpRequestException($"Ошибка сервера: {serverError}");
                }

                var contentDisposition = response.Content.Headers.ContentDisposition;
                if (response.Content.Headers.ContentDisposition == null)
                    _logger.LogWarning("Content-Disposition отсутствует, используется имя по умолчанию");

                var fileName = contentDisposition?.FileName?.Trim('"') ?? "new_file";

                var newName = ScriptHelper.Rename(fileName);

                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                return fileBytes;
            });
        }
        /// <summary>
        /// Переименование файла
        /// </summary>
        public async Task<string> RenameFileAsync(Guid id, string newName)
        {
            return await ServerStateService.ExecuteIfServerActive<string>(_logger, async () =>
            {
                var request = new { newName = newName };
                var response = await _client.PutAsJsonAsync($"{_apiSubUrl}/rename/{id}", request);
                if (!response.IsSuccessStatusCode)
                {
                    var serverError = await response.Content.ReadAsStringAsync();
                    _logger.LogError(serverError);
                    throw new HttpRequestException($"Ошибка сервера: {serverError}");
                }

                var serverResult = await response.Content.ReadAsStringAsync();
                return serverResult;
            });
        }
        /// <summary>
        /// Переимещение файла
        /// </summary>
        public async Task<string> MoveFileAsync(Guid id, Guid newFolder)
        {
            return await ServerStateService.ExecuteIfServerActive<string>(_logger, async () =>
            {
                var request = new { newFolder = newFolder };
                var response = await _client.PutAsJsonAsync($"{_apiSubUrl}/move/{id}", request);
                if (!response.IsSuccessStatusCode)
                {
                    var serverError = await response.Content.ReadAsStringAsync();
                    _logger.LogError(serverError);
                    throw new HttpRequestException($"Ошибка сервера: {serverError}");
                }

                var serverResult = await response.Content.ReadAsStringAsync();
                return serverResult;
            });
        }
    }
}