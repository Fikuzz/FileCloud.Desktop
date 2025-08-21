using FileCloud.Desktop.Models;
using FileCloud.Desktop.Models.Models;
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

        public FolderService(IAppSettingsService settings, ILogger<FolderService> logger)
        {
            _logger = logger;
            _client = new HttpClient
            {
                BaseAddress = new Uri(settings.ApiBaseUrl)
            };
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
        public async Task<FolderModel> CreateFolderAsync(string name, Guid parentId)
        {
            return await ServerStateService.ExecuteIfServerActive<FolderModel>(_logger, async () =>
            {
                var response = await _client.PostAsJsonAsync($"/create", new { Name = name, ParentId = parentId });
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при создании папки: {error}");
                }

                return (await response.Content.ReadFromJsonAsync<FolderModel>())!;
            });
        }

        /// <summary>
        /// Удалить папку
        /// </summary>
        public async Task DeleteFolderAsync(Guid folderId)
        {
            await ServerStateService.ExecuteIfServerActive<Task>(_logger, async () =>
            {
                var response = await _client.DeleteAsync($"/delete/{folderId}");
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при удалении папки: {error}");
                }
                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Переименовать папку
        /// </summary>
        public async Task<FolderModel> RenameFolderAsync(Guid folderId, string newName)
        {
            return await ServerStateService.ExecuteIfServerActive<FolderModel>(_logger, async () =>
            {
                var response = await _client.PutAsJsonAsync($"/rename", new { Id = folderId, NewName = newName });
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
                var response = await _client.PutAsJsonAsync($"/move", new { Id = folderId, NewParentId = newParentId });
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError(error);
                    throw new HttpRequestException($"Ошибка при перемещении папки: {error}");
                }

                return (await response.Content.ReadFromJsonAsync<FolderModel>())!;
            });
        }
    }
}
