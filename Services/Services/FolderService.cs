using FileCloud.Desktop.Models;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Models.Requests;
using FileCloud.Desktop.Models.Responses;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace FileCloud.Desktop.Services
{
    public class FolderService
    {
        private readonly string _apiSubUrl = "/api/folder";

        private readonly HttpClient _client;
        private readonly ILogger<FolderService> _logger;
        private readonly FileService _fileService;

        public FolderService(IAppSettingsService settings, ILogger<FolderService> logger, FileService fileService)
        {
            _logger = logger;
            _fileService = fileService;
            _client = new HttpClient
            {
                BaseAddress = new Uri(settings.ApiBaseUrl)
            };
        }

        /// <summary>
        /// Получить папку по ID
        /// </summary>
        public async Task<FolderModel> GetFolderAsync(Guid id)
        {
            return await ServerStateService.ExecuteIfServerActive<FolderModel>(_logger, async () =>
            {
                var response = await _client.GetAsync(Path.Combine(_apiSubUrl, id.ToString()));
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при получении папки: {error}");
                }
                var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<FolderModel>>();
                if(apiResult.Error != null)
                {
                    _logger.LogError($"Error: {apiResult.Error}");
                    throw new HttpRequestException(apiResult.Error);
                }
                return apiResult.Response;
            });
        }
        /// <summary>
        /// Получить список папок внутри родительской папки
        /// </summary>
        public async Task<ContentResponse> GetFolderContentAsync(Guid id)
        {
            return await ServerStateService.ExecuteIfServerActive<ContentResponse>(_logger, async () =>
            {
                var response = await _client.GetAsync($"{_apiSubUrl}/{id}/childs");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при получении папок: {error}");
                }
                var apiResult = await response.Content.ReadFromJsonAsync<ApiResult<ContentResponse>>();
                if( apiResult.Error != null)
                {
                    _logger.LogError($"Error: {apiResult.Error}");
                    throw new HttpRequestException(apiResult.Error);
                }
                return apiResult.Response;
            }); 
        }

        /// <summary>
        /// Создать папку
        /// </summary>
        public async Task<Guid> CreateFolderAsync(string name, Guid parentId)
        {
            return await ServerStateService.ExecuteIfServerActive<Guid>(_logger, async () =>
            {
                var response = await _client.PostAsJsonAsync($"{_apiSubUrl}/create", new FolderRequest(name, parentId));
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при создании папки: {error}");
                }
                var id = await response.Content.ReadFromJsonAsync<Guid>();
                return id;
            });
        }

        /// <summary>
        /// Удалить папку
        /// </summary>
        public async Task<DeleteFolderResponse> DeleteFolderAsync(Guid folderId)
        {
            return await ServerStateService.ExecuteIfServerActive<DeleteFolderResponse>(_logger, async () =>
            {
                var response = await _client.DeleteAsync($"{_apiSubUrl}/delete/{folderId}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при удалении папки: {error}");
                }
                return (await response.Content.ReadFromJsonAsync<DeleteFolderResponse>())!;
            });
        }

        /// <summary>
        /// Переименовать папку
        /// </summary>
        public async Task<FolderModel> RenameFolderAsync(Guid folderId, string newName)
        {
            return await ServerStateService.ExecuteIfServerActive<FolderModel>(_logger, async () =>
            {
                var response = await _client.PutAsJsonAsync($"{_apiSubUrl}/rename/{folderId}", new RenameFolderRequest(newName));
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при переименовании папки: {error}");
                }

                return (await response.Content.ReadFromJsonAsync<FolderModel>())!;
            });
        }

        /// <summary>
        /// Переместить папку
        /// </summary>
        public async Task<FolderModel> MoveFolderAsync(Guid folderId, Guid newParentId)
        {
            return await ServerStateService.ExecuteIfServerActive<FolderModel>(_logger, async () =>
            {
                var response = await _client.PutAsJsonAsync($"{_apiSubUrl}/move/{folderId}", new MoveFolderRequest(newParentId));
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при перемещении папки: {error}");
                }

                return (await response.Content.ReadFromJsonAsync<FolderModel>())!;
            });
        }

        /// <summary>
        /// Загрузить папку на сервер каскадно
        /// </summary>
        public async Task UploadFolderRecursiveAsync(string folderPath, Guid parentFolderId)
        {
            // 1. Создаем папку на сервере
            var folderName = Path.GetFileName(folderPath);
            var serverFolderId = await CreateFolderAsync(folderName, parentFolderId);

            // 2. Загружаем все файлы в этой папке
            foreach (var file in Directory.GetFiles(folderPath))
            {
                await _fileService.UploadFileAsync(serverFolderId, file);
            }

            // 3. Рекурсивно обрабатываем все подпапки
            foreach (var subFolder in Directory.GetDirectories(folderPath))
            {
                await UploadFolderRecursiveAsync(subFolder, serverFolderId);
            }
        }
    }
}
