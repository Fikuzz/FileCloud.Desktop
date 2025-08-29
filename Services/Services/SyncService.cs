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

        _connection.On<Guid>("FileDeleted", async (fileId) =>
        {
            await _bus.Publish(new ItemDeletedMessage(fileId));
        });

        _connection.On<Guid>("FolderDeleted", async (folderId) =>
        {
            await _bus.Publish(new ItemDeletedMessage(folderId));
        });

        _connection.Closed += async (error) =>
        {
            ServerStateService.SetServerState(false);
            await _bus.Publish(new ServerIsActiveMessage(false, error?.Message));
        };

        _connection.Reconnecting += async (error) =>
        {
            ServerStateService.SetServerState(false);
            await _bus.Publish(new ServerIsActiveMessage(false, error?.Message));
        };

        _connection.Reconnected += async (connectionId) =>
        {
            ServerStateService.SetServerState(true);
            await _bus.Publish(new ServerIsActiveMessage(true, "Сервер доступен"));
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
                await _bus.Publish(new ServerIsActiveMessage(true, "Сервер доступен"));
                Start();
                return;
            }
            catch
            {
                ServerStateService.SetServerState(false);
                await _bus.Publish(new ServerIsActiveMessage(true, "Сервер доступен"));
            }

            await Task.Delay(intervalMs);
        }
    }
}
