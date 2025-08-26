using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.ServerMessages;
using Microsoft.AspNetCore.SignalR.Client;
using System.Text.Json;

namespace FileCloud.Desktop.Services.Services;
public class SyncService
{
    private readonly HubConnection _connection;
    private readonly MessageBus _bus;
    public event Action<string> ServerState;

    public SyncService(IAppSettingsService settings, MessageBus bus)
    {
        _bus = bus;
        _connection = new HubConnectionBuilder()
            .WithUrl($"{settings.ApiBaseUrl}/fileHub")
            .WithAutomaticReconnect()
            .Build();
    }

    private void Start()
    {
        _connection.On<FileModel>("FileLoaded", async (file) =>
        {
            if(file != null)
                await _bus.Publish(new FileUploadedMessage(file));
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
