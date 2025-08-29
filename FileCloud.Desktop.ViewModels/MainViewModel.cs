using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.ServerMessages;
using FileCloud.Desktop.Services.Services;
using FileCloud.Desktop.ViewModels.Factories;
using FileCloud.Desktop.ViewModels.Helpers;
using FileCloud.Desktop.ViewModels.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace FileCloud.Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ----------------------
        // Сервисы через DI
        // ----------------------
        private readonly IFileViewModelFactory _fileVmFactory;
        private readonly IFolderViewModelFactory _folderVmFactory;

        private readonly IUiDispatcher _dispatcher;
        private readonly FileService _fileService;
        private readonly FolderService _folderService;
        private readonly SyncService _syncService;
        private readonly PreviewHelper _previewHelper;
        private readonly MessageBus _bus;
        private readonly IAppSettingsService _settings;
        private readonly IFileDialogService _dialogService;
        private readonly ILogger<MainViewModel> _logger;

        // ----------------------
        // Данные для UI
        // ----------------------
        public ObservableCollection<ItemViewModel> Items { get; } = new();
        public ObservableCollection<ItemViewModel> SelectedItem{ get; } = new();
        public ObservableCollection<FolderViewModel> FolderPath { get; } = new(); // навигация по папкам

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
        public ICommand SaveFilesCommand { get; }

        private readonly FolderModel _rootFolder;

        // ----------------------
        // Конструктор
        // ----------------------
        public MainViewModel(FileService fileService, FolderService folderService, SyncService syncService, PreviewHelper previewHelper, MessageBus bus, IAppSettingsService settings, IFileDialogService dialogService, ILogger<MainViewModel> logger, IUiDispatcher dispatcher, IFileViewModelFactory fileViewModelFactory, IFolderViewModelFactory folderViewModelFactory)
        {
            _fileVmFactory = fileViewModelFactory;
            _folderVmFactory = folderViewModelFactory;

            _dispatcher = dispatcher;
            _fileService = fileService;
            _folderService = folderService;
            _syncService = syncService;
            _bus = bus;
            _settings = settings;
            _dialogService = dialogService;
            _previewHelper = previewHelper;
            _logger = logger;

            _rootFolder = new Models.FolderModel(_settings.RootFolderId, "Root", Guid.Empty);
            var baseFolderVM = _folderVmFactory.Create(_rootFolder);
            FolderPath.Add(baseFolderVM);

            // Привязка команд к методам (RelayCommand или AsyncCommand)
            LoadFolderChildsCommand = new RelayCommand(async param => await GetFolderChilds(param as FolderViewModel));
            UploadFileCommand = new RelayCommand(async _ => await UploadFile());
            CreateFolderCommand = new RelayCommand(async _ => await CreateFolder());
            SaveFilesCommand = new RelayCommand(_ => SaveFiles());

            // Подписки на события SyncService
            _bus.Subscribe<FileUploadedMessage>(async msg => await AddFile(msg));
            _bus.Subscribe<ItemDeletedMessage>(msg => DeleteItem(msg));
            _bus.Subscribe<ServerIsActiveMessage>(msg => OnServerStateChanged(msg));

            _syncService.StartMonitoringAsync();
        }

        // ----------------------
        // Асинхронные методы для работы с сервером
        // ----------------------
        private async Task GetFolderChilds(FolderViewModel? folder)
        {
            Items.Clear();

            if (folder == null)
            {
                FolderPathSet(FolderPathGetLast);
            }
            else
            {
                FolderPathSet(folder);
            }

            try
            {;
                var childs = await _folderService.GetFolderContentAsync(FolderPathGetLastId);
                List<ItemViewModel> items = childs.Folders
                    .Select(f => _folderVmFactory.Create(f))
                    .Cast<ItemViewModel>()
                    .ToList();
                items.AddRange(childs.Files
                    .Select(f => _fileVmFactory.Create(f))
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
                    await _fileService.UploadFileAsync(FolderPathGetLastId, file);
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
            var folder = _folderVmFactory.Create(new FolderModel(Guid.Empty, name, FolderPathGetLastId), true);
            await _previewHelper.SetPreview(folder);
            Items.Add(folder);
        }
        private void SaveFiles()
        {
            foreach(FileViewModel file in SelectedItem)
            {
                file.SaveFileCommand.Execute(this);
            }
        }

        // ----------------------
        // Методы для локальной работы с файлами
        // ----------------------
        private Task GetPreview(FileViewModel file) => throw new NotImplementedException();
        private Task OpenFile(FileViewModel file) => throw new NotImplementedException();
        private void FolderPathSet(FolderViewModel vm)
        {
            var folder = FolderPath.SingleOrDefault(f => f.Id == vm.Id);
            if (folder == null)
            {
                FolderPath.Add(vm);
            }
            else
            {
                int index = FolderPath.IndexOf(folder);
                while (FolderPath.Count > index + 1)
                {
                    FolderPath.RemoveAt(FolderPath.Count - 1);
                }
            }
        }
        private Guid FolderPathGetLastId =>
            FolderPath.Last().Id;
        private FolderViewModel FolderPathGetLast =>
            FolderPath.Last();

        // ----------------------
        // Методы для FileSyncService
        // ----------------------
        private async Task AddFile(FileUploadedMessage msg)
        {
            if (msg.Model.FolderId == FolderPathGetLastId)
            {
                var fileVM = _fileVmFactory.Create(msg.Model);
                await _previewHelper.SetPreview(fileVM);
                _dispatcher.BeginInvoke(() => Items.Add(fileVM));
            }
        }
        private void DeleteItem(ItemDeletedMessage msg)
        {
            var deletedFile = Items.Where(i => i.Id == msg.Id).First();
            if (deletedFile != null)
            {
                _dispatcher.BeginInvoke(() => Items.Remove(deletedFile));
            }
        }
        private void OnServerStateChanged(ServerIsActiveMessage message)
        {
            if (message.IsActive)
            {
                _dispatcher.BeginInvoke(async () => await GetFolderChilds(null));
            }
            _dispatcher.BeginInvoke(() => StatusMessage = message.Message);
        }

        // ----------------------
        // INotifyPropertyChanged
        // ----------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}