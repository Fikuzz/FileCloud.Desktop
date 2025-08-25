using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.Services;
using FileCloud.Desktop.ViewModels.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private readonly PreviewHelper _previewHelper;
        private readonly IAppSettingsService _settings;
        private readonly IFileDialogService _dialogService;
        private readonly ILogger<MainViewModel> _logger;

        // ----------------------
        // Данные для UI
        // ----------------------
        public ObservableCollection<ItemViewModel> Items { get; } = new();
        public ObservableCollection<ItemViewModel> SelectedItem{ get; } = new();
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
        public MainViewModel(FileService fileService, FolderService folderService, SyncService syncService, PreviewHelper previewHelper, IAppSettingsService settings, IFileDialogService dialogService, ILogger<MainViewModel> logger)
        {
            _fileService = fileService;
            _folderService = folderService;
            _syncService = syncService;
            _settings = settings;
            _dialogService = dialogService;
            _previewHelper = previewHelper;
            _logger = logger;

            FolderPath.Add(_settings.RootFolderId);

            // Привязка команд к методам (RelayCommand или AsyncCommand)
            LoadFolderChildsCommand = new RelayCommand(async param => await GetFolderChilds(param as FolderViewModel));
            UploadFileCommand = new RelayCommand(async _ => await UploadFile());
            CreateFolderCommand = new RelayCommand(_ => CreateFolder());
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
        private async Task GetFolderChilds(FolderViewModel? folder)
        {
            Items.Clear();

            Guid folderId = folder != null ? folder.Id : _settings.RootFolderId;
            FolderPathSet(folderId);

            try
            {
                var childs = await _folderService.GetFolderContentAsync(FolderPath.Last());
                List<ItemViewModel> items = childs.Folders
                    .Select(f => new FolderViewModel(f, _folderService))
                    .Cast<ItemViewModel>()
                    .ToList();
                items.AddRange(childs.Files
                    .Select(f => new FileViewModel(f, _fileService))
                    .Cast<ItemViewModel>()
                    .ToList());

                foreach (var item in items)
                {
                    try
                    {
                        await _previewHelper.SetPreview(item);
                    }
                    catch (Exception ex){
                        _logger.LogError(ex.Message);
                    }
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }
        private async Task UploadFile()
        {
            var files = _dialogService.OpenFiles(string.Empty);
            if (files == null || files.Length == 0)
            {
                StatusMessage = "Файл не выбран";
                return;
            }

            try
            {
                foreach (var file in files)
                {
                    await _fileService.UploadFileAsync(FolderPath.Last(), file);
                }
            }
            catch(Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }
        private async Task CreateFolder()
        {
            var baseName = "new folder";
            var name = baseName;
            int i = 1;
            while(Items.Where(i => i.Name == name).Count() > 0)
            {
                name = baseName + i;
                i++;
            }
            var folder = new FolderViewModel(name, FolderPath.Last(), _folderService);
            await _previewHelper.SetPreview(folder);
            Items.Add(folder);
        }
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
        private void FolderPathSet(Guid id)
        {
            var folderIndex = FolderPath.IndexOf(id);
            if (folderIndex == -1)
            {
                FolderPath.Add(id);
            }
            else
            {
                while (FolderPath.Count > folderIndex + 1)
                {
                    FolderPath.RemoveAt(FolderPath.Count - 1);
                }
            }
        }

        // ----------------------
        // Методы для FileSyncService
        // ----------------------
        private void OnFileReceived(FileModel file) => throw new NotImplementedException();
        private void OnFileDeleted(Guid id) => throw new NotImplementedException();
        private void OnServerStateChanged(string description)
        {
            StatusMessage = description;
        }

        // ----------------------
        // INotifyPropertyChanged
        // ----------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}