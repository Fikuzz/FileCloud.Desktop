using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services.Configurations;
using Microsoft.AspNetCore.SignalR.Client;

namespace FileCloud.Desktop.Services.Services;
public class SyncService
{
    private readonly HubConnection _connection;

    public event Action<FileModel> FileReceived;
    public event Action<Guid> FileDeleted;
    public event Action<string> ServerState;

    public SyncService(IAppSettingsService settings)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"{settings.ApiBaseUrl}/fileHub")
            .WithAutomaticReconnect()
            .Build();
    }

    private void Start()
    {
        _connection.On<string>("FileLoaded", async (fileId) =>
        {
            //var guid = Guid.Parse(fileId);
            //var apiResult = await _fileService.GetFileByIdAsync(guid);
            //if (fileModel != null)
            //    FileReceived?.Invoke(fileModel);
        });

        _connection.On<string>("FileDeleted", (fileId) =>
        {
            //var guid = Guid.Parse(fileId);
            //FileDeleted?.Invoke(guid);
        });

        _connection.Closed += (error) =>
        {
            //_fileService.ServerState = false;
            //ServerState?.Invoke(false, "Соединение закрыто. Переподключение...");
            return Task.CompletedTask;
        };

        _connection.Reconnecting += (error) =>
        {
            //_fileService.ServerState = false;
            //ServerState?.Invoke(false, "Пытаюсь переподключиться...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            //_fileService.ServerState = true;
            //ServerState?.Invoke(true, "Переподключился!");
            return Task.CompletedTask;
        };
    }

    public async Task StartMonitoringAsync(int intervalMs = 5000)
    {
        while (true)
        {
            try
            {
                await _connection.StartAsync();
                await _connection.InvokeAsync("Ping");
                ServerStateService.SetServerState(true);
                ServerState?.Invoke("Сервер доступен");
                Start();
                return;
            }
            catch
            {
                ServerStateService.SetServerState(false);
                ServerState?.Invoke("Сервер не отвечает");
            }

            await Task.Delay(intervalMs);
        }
    }
}
