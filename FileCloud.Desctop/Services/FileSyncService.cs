using FileCloud.Desktop.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.ViewModels;
using Microsoft.AspNetCore.SignalR.Client;

public class FileSyncService
{
    private readonly FileService _fileService;
    private readonly HubConnection _connection;

    public event Action<FileModel> FileReceived;

    public FileSyncService(string baseUrl)
    {
        _fileService = new FileService(baseUrl);
        _connection = new HubConnectionBuilder()
            .WithUrl($"{baseUrl}/fileHub")
            .WithAutomaticReconnect()
            .Build();
    }

    public async Task StartAsync()
    {
        _connection.On<string>("FileLoaded", async (fileId) =>
        {
            var guid = Guid.Parse(fileId);
            var fileModel = await _fileService.GetFileByIdAsync(guid);
            FileReceived?.Invoke(fileModel);
        });

        _connection.Closed += async (error) =>
        {
            await Task.Delay(2000);
            await _connection.StartAsync();
        };

        await _connection.StartAsync();
    }
}
