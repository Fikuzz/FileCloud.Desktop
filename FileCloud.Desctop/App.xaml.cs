using FileCloud.Desktop;
using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.Services;
using FileCloud.Desktop.View.Helpers;
using FileCloud.Desktop.ViewModels;
using FileCloud.Desktop.ViewModels.Factories;
using FileCloud.Desktop.ViewModels.Helpers;
using FileCloud.Desktop.ViewModels.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace FileCloud.Desktop.View;
public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; }
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();

        var syncService = ServiceProvider.GetRequiredService<SyncService>();
        _ = syncService.StartMonitoringAsync();

        // Запуск главного окна через DI
        var login = ServiceProvider.GetRequiredService<Login>();
        login.Show();
        FileMappingManager.Cleanup();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Logger
        services.AddLogging(config =>
        {
            // Можно добавить разные провайдеры, например консоль/файл/Null
            config.SetMinimumLevel(LogLevel.Information);
        });

        // Конфигурация (appsettings.json)
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory) // папка с exe
            .AddJsonFile("appsettings.json", optional: false)
            .Build();
        services.AddSingleton<IConfiguration>(config);

        services.AddSingleton<IServiceProvider>(provider => provider);
        // Сервисы
        services.AddSingleton<IAppSettingsService, AppSettingsService>();
        services.AddSingleton<IDialogService, FileDialogService>();
        services.AddSingleton<IFileSaveService, FileSaveService>();
        services.AddSingleton<SyncService>();
        services.AddSingleton<FileService>();
        services.AddSingleton<FolderService>();
        services.AddSingleton<ILoginService, LoginService>();

        services.AddSingleton<PreviewHelper>();
        services.AddSingleton<MessageBus>();
        services.AddSingleton<IUiDispatcher>(new WpfDispatcher(Current.Dispatcher));

        // ViewModels
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<LoginViewModel>();

        services.AddSingleton<ILoginViewModelFactory, LoginViewModelFactory>();
        services.AddSingleton<IMainViewModelFactory, MainViewModelFactory>();
        services.AddTransient<FileViewModel>();
        services.AddTransient<IFileViewModelFactory,  FileViewModelFactory>();
        services.AddTransient<FolderViewModel>();
        services.AddTransient<IFolderViewModelFactory, FolderViewModelFactory>();

        // Windows/Views
        services.AddSingleton<MainWindow>();
        services.AddSingleton<Login>();
    }
}