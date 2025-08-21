using FileCloud.Desktop.Services.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class AppSettingsService : IAppSettingsService
{
    private readonly ILogger<AppSettingsService> _logger;
    private readonly IConfiguration _config;

    public AppSettingsService(IConfiguration config, ILogger<AppSettingsService> logger)
    {
        _config = config;
        _logger = logger;
        _logger.LogInformation("AppSettingsService создан");
    }
    public string AppBasePath => Environment.CurrentDirectory;
    public string ApiBaseUrl => _config["ServerSettings:ApiBaseUrl"];

    public string DownloadPath => _config["DownloadSettings:DownloadPath"]
        ?? Path.Combine(Environment.CurrentDirectory, "Downloads");

    public Guid RootFolderId => Guid.Parse(_config["SystemFolders:RootFolderId"]);
}