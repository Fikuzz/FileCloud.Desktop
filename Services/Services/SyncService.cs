using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.ServerMessages;
using Microsoft.AspNetCore.SignalR.Client;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FileCloud.Desktop.Services.Services;

public  class SyncService
{
    private  Guid _currentWatchedFolderId;
    private readonly HubConnection _connection;
    private readonly MessageBus _bus;

    public ServerIsActiveMessage LastServerState { get; private set; }

    public SyncService(IAppSettingsService settings, MessageBus bus)
    {
        _bus = bus;
        _connection = new HubConnectionBuilder()
            .WithUrl($"{settings.ApiBaseUrl}/fileHub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<FileModel>("FileLoaded", async (file) =>
        {
            await _bus.Publish(new FileUploadedMessage(file));
        });

        _connection.On<Guid>("FileDeleted", async (fileId) =>
        {
            await _bus.Publish(new ItemDeletedMessage(fileId));
        });

        _connection.On<Guid>("FolderCreated", async (folderId) =>
        {
            await _bus.Publish(new FolderCreatedMessage(folderId));
        });

        _connection.On<Guid>("FolderDeleted", async (folderId) =>
        {
            await _bus.Publish(new ItemDeletedMessage(folderId));
        });

        _connection.On<ItemRenamedMessage>("FileRenamed", async (response) =>
        {
            await _bus.Publish(response);
        });

        _connection.On<ItemRenamedMessage>("FolderRenamed", async (response) =>
        {
            await _bus.Publish(response);
        });

        _connection.Closed += async (error) =>
        {
            await OnServerStateChange(ServerStatus.Offline, error?.Message);
        };

        _connection.Reconnecting += async (error) =>
        {
            await OnServerStateChange(ServerStatus.Connecting, error?.Message);
        };

        _connection.Reconnected += async (connectionId) =>
        {
            await OnServerStateChange(ServerStatus.Online, "Сервер доступен");

            if (_currentWatchedFolderId != Guid.Empty)
            {
                await _connection.InvokeAsync("JoinFolderGroup", _currentWatchedFolderId);
            }
        };
    }

    public async Task StartMonitoringAsync(int intervalMs = 5000)
    {
        while (true)
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                    await _connection.InvokeAsync("Ping");
                    await OnServerStateChange(ServerStatus.Online, "Сервер доступен");

                    // Если до подключения уже был известен путь (например, из настроек или главного окна),
                    // присоединяемся к соответствующей группе.
                    if (_currentWatchedFolderId != Guid.Empty)
                    {
                        await _connection.InvokeAsync("JoinFolderGroup", _currentWatchedFolderId);
                    }
                }
                return;
            }
            catch
            {
                await OnServerStateChange(ServerStatus.Unknown, "Не удалось подключиться к серверу");
            }
            await Task.Delay(intervalMs);
        }
    }

    /// <summary>
    /// Переключает отслеживание на новую папку. Отписывается от старой группы, подписывается на новую.
    /// </summary>
    /// <param name="folderId">Guid папки для отслеживания</param>
    public async Task SwitchWatchedFolderAsync(Guid folderId)
    {
        // Если пытаемся переключиться на ту же папку, ничего не делаем
        if (_currentWatchedFolderId == folderId)
        {
            return;
        }

        // Выходим из группы старой папки, если она была
        if (_currentWatchedFolderId != Guid.Empty)
        {
            try
            {
                await _connection.InvokeAsync("LeaveFolderGroup", _currentWatchedFolderId);
            }
            catch (Exception ex)
            {
                // Логируем ошибку, но не прерываем выполнение. Возможно, соединение разорвано.
                Console.WriteLine($"Could not leave group: {ex.Message}");
            }
        }

        // Входим в группу новой папки
        try
        {
            await _connection.InvokeAsync("JoinFolderGroup", folderId);
            // Обновляем текущий Guid только в случае успешного присоединения
            _currentWatchedFolderId = folderId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not join group '{folderId}': {ex.Message}");
        }
    }

    private async Task OnServerStateChange(ServerStatus status, string? message)
    {
        ServerStateService.SetServerState(status == ServerStatus.Online ? true : false);
        LastServerState = new ServerIsActiveMessage(status, message);
        await _bus.Publish(LastServerState);
    }
}