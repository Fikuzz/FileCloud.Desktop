using FileCloud.Desktop;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();

        // Запуск главного окна через DI
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logger
        services.AddLogging(config =>
        {
            // Можно добавить разные провайдеры, например консоль/файл/Null
            // config.AddConsole(); // если не нужен, оставляем закомментированным
            config.SetMinimumLevel(LogLevel.Information);
        });

        // Конфигурация (appsettings.json)
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // папка с exe
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        services.AddSingleton(config);

        // Сервисы
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<FileService>();
        services.AddSingleton<FolderService>();

        // ViewModels
        services.AddSingleton<MainViewModel>();

        // Windows/Views
        services.AddSingleton<MainWindow>();
    }
}