using FileCloud.Desktop.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;

public class FileSyncService
{
    private readonly FileService _fileService;
    private readonly HubConnection _connection;

    public event Action<FileModel> FileReceived;
    public event Action<Guid> FileDeleted;
    public event Action<bool, string> ServerState;

    public FileSyncService(string baseUrl)
    {
        _fileService = new FileService(baseUrl);
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/fileHub")
            .WithAutomaticReconnect()
            .Build();
    }

    private void Start()
    {
        _connection.On<string>("FileLoaded", async (fileId) =>
        {
            var guid = Guid.Parse(fileId);
            var (fileModel, error) = await _fileService.GetFileByIdAsync(guid);
            if(fileModel != null)
                FileReceived?.Invoke(fileModel);
        });

        _connection.On<string>("FileDeleted", (fileId) =>
        {
            var guid = Guid.Parse(fileId);
            FileDeleted?.Invoke(guid);
        });

        _connection.Closed += (error) =>
        {
            _fileService.ServerState = false;
            ServerState?.Invoke(false, "Соединение закрыто. Переподключение...");
            return Task.CompletedTask;
        };

        _connection.Reconnecting += (error) =>
        {
            _fileService.ServerState = false;
            ServerState?.Invoke(false, "Пытаюсь переподключиться...");
            return Task.CompletedTask;
        };

        _connection.Reconnected += (connectionId) =>
        {
            _fileService.ServerState = true;
            ServerState?.Invoke(true, "Переподключился!");
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
                _fileService.ServerState = true;
                ServerState?.Invoke(true, "Сервер доступен");
                Start();
                return;
            }
            catch
            {
                _fileService.ServerState = false;
                ServerState?.Invoke(false, "Сервер не отвечает");
            }

            await Task.Delay(intervalMs);
        }
    }
}
