using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace FileCloud.Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ----------------------
        // Сервисы через DI
        // ----------------------
        private readonly FileService _fileService;
        private readonly FolderService _folderService;
        private readonly SyncService _syncService;
        private readonly IAppSettingsService _settings;

        // ----------------------
        // Данные для UI
        // ----------------------
        public ObservableCollection<FileViewModel> Files { get; } = new();
        public ObservableCollection<FileViewModel> SelectedFiles { get; } = new();
        public List<Guid> FolderPath { get; } = new(); // навигация по папкам

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        // ----------------------
        // Команды для UI
        // ----------------------
        public ICommand LoadFolderChildsCommand { get; }
        public ICommand UploadFileCommand { get; }
        public ICommand CreateFolderCommand { get; }
        public ICommand DeleteFileCommand { get; }
        public ICommand DeleteFolderCommand { get; }
        public ICommand RenameFileCommand { get; }
        public ICommand RenameFolderCommand { get; }
        public ICommand MoveFileCommand { get; }
        public ICommand MoveFolderCommand { get; }
        public ICommand DownloadFileCommand { get; }
        public ICommand DownloadPreviewCommand { get; }
        public ICommand OpenFileCommand { get; }

        // ----------------------
        // Конструктор
        // ----------------------
        public MainViewModel(FileService fileService, FolderService folderService, SyncService syncService, IAppSettingsService settings)
        {
            _fileService = fileService;
            _folderService = folderService;
            _syncService = syncService;
            _settings = settings;

            FolderPath.Add(_settings.RootFolderId);

            // Привязка команд к методам (RelayCommand или AsyncCommand)
            LoadFolderChildsCommand = new RelayCommand(async _ => await GetFolderChilds());
            UploadFileCommand = new RelayCommand(async _ => await UploadFile());
            CreateFolderCommand = new RelayCommand(async _ => await CreateFolder());
            DeleteFileCommand = new RelayCommand(async _ => await DeleteFile());
            DeleteFolderCommand = new RelayCommand(async _ => await DeleteFolder());
            RenameFileCommand = new RelayCommand(async _ => await RenameFile());
            RenameFolderCommand = new RelayCommand(async _ => await RenameFolder());
            MoveFileCommand = new RelayCommand(async _ => await MoveFile());
            MoveFolderCommand = new RelayCommand(async _ => await MoveFolder());
            DownloadFileCommand = new RelayCommand(async _ => await DownloadFile());
            DownloadPreviewCommand = new RelayCommand(async _ => await DownloadPreview());
            OpenFileCommand = new RelayCommand(async param =>
            {
                if (param is FileViewModel file)
                    await OpenFile(file);
            });

            // Подписка на события FileSyncService
            _syncService.FileReceived += OnFileReceived;
            _syncService.FileDeleted += OnFileDeleted;
            _syncService.ServerState += OnServerStateChanged;
            _syncService.StartMonitoringAsync();
        }

        // ----------------------
        // Асинхронные методы для работы с сервером
        // ----------------------
        private async Task GetFolderChilds()
        {
            var childs = await _folderService.
        }
        private Task UploadFile() => throw new NotImplementedException();
        private Task CreateFolder() => throw new NotImplementedException();
        private Task DeleteFile() => throw new NotImplementedException();
        private Task DeleteFolder() => throw new NotImplementedException();
        private Task RenameFile() => throw new NotImplementedException();
        private Task RenameFolder() => throw new NotImplementedException();
        private Task MoveFile() => throw new NotImplementedException();
        private Task MoveFolder() => throw new NotImplementedException();
        private Task DownloadFile() => throw new NotImplementedException();
        private Task DownloadPreview() => throw new NotImplementedException();

        // ----------------------
        // Методы для локальной работы с файлами
        // ----------------------
        private Task GetPreview(FileViewModel file) => throw new NotImplementedException();
        private Task OpenFile(FileViewModel file) => throw new NotImplementedException();

        // ----------------------
        // Методы для FileSyncService
        // ----------------------
        private void OnFileReceived(FileModel file) => throw new NotImplementedException();
        private void OnFileDeleted(Guid id) => throw new NotImplementedException();
        private void OnServerStateChanged(bool isActive, string description) => throw new NotImplementedException();

        // ----------------------
        // INotifyPropertyChanged
        // ----------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}