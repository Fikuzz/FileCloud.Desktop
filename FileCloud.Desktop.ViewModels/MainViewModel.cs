using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.Services.ServerMessages;
using FileCloud.Desktop.Services.Services;
using FileCloud.Desktop.ViewModels.Factories;
using FileCloud.Desktop.ViewModels.Helpers;
using FileCloud.Desktop.ViewModels.Interfaces;
using Microsoft.Extensions.Logging;
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
        private readonly IFileViewModelFactory _fileVmFactory;
        private readonly IFolderViewModelFactory _folderVmFactory;

        private readonly IUiDispatcher _dispatcher;
        private readonly FileService _fileService;
        private readonly FolderService _folderService;
        private readonly SyncService _syncService;
        private readonly PreviewHelper _previewHelper;
        private readonly MessageBus _bus;
        private readonly IAppSettingsService _settings;
        private readonly IDialogService _dialogService;
        private readonly ILogger<MainViewModel> _logger;

        // ----------------------
        // Данные для UI
        // ----------------------
        public ObservableCollection<ItemViewModel> Items { get; } = new();
        public ObservableCollection<ItemViewModel> SelectedItems { get; } = new();
        public ObservableCollection<FolderViewModel> FolderPath { get; } = new(); // навигация по папкам

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    // Можно запускать поиск сразу при вводе
                    ApplySearchFilter();
                }
            }
        }
        private string _searchQuery;

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private ServerStatus _serverStatus;
        public ServerStatus ServerStatus
        {
            get => _serverStatus;
            set { _serverStatus = value; OnPropertyChanged(); }
        }

        private string _serverStatusMessage;
        public string ServerStatusMessage
        {
            get => _serverStatusMessage;
            set { _serverStatusMessage = value; OnPropertyChanged(); }
        }
        private bool _isLeftPanelMinimized;
        public bool IsLeftPanelMinimized
        {
            get => _isLeftPanelMinimized;
            set { _isLeftPanelMinimized = value; OnPropertyChanged(); }
        }

        // ----------------------
        // Команды для UI
        // ----------------------
        public ICommand LoadFolderChildsCommand { get; }
        public ICommand UploadFileCommand { get; }
        public ICommand CreateFolderCommand { get; }
        public ICommand SaveFilesCommand { get; }
        public ICommand DeleteItemsCommand { get; }
        public ICommand ToggleLeftPanelCommand { get; }

        private readonly FolderModel _rootFolder;

        // ----------------------
        // Конструктор
        // ----------------------
        public MainViewModel(FileService fileService, FolderService folderService, SyncService syncService, PreviewHelper previewHelper, MessageBus bus, IAppSettingsService settings, IDialogService dialogService, ILogger<MainViewModel> logger, IUiDispatcher dispatcher, IFileViewModelFactory fileViewModelFactory, IFolderViewModelFactory folderViewModelFactory)
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
            DeleteItemsCommand = new RelayCommand(async _ => await DeleteItemsAsync());
            SaveFilesCommand = new RelayCommand(_ => SaveFiles());
            ToggleLeftPanelCommand = new RelayCommand(_ =>
            {
                IsLeftPanelMinimized = !IsLeftPanelMinimized;
            });

            // Подписки на события SyncService
            _bus.Subscribe<FileUploadedMessage>(async msg => await AddFile(msg));
            _bus.Subscribe<ItemDeletedMessage>(msg => DeleteItem(msg));
            _bus.Subscribe<ServerIsActiveMessage>(msg => OnServerStateChanged(msg));
            _bus.Subscribe<FolderCreatedMessage>(msg => LoadFolder(msg));

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
            var folder = _folderVmFactory.Create(new FolderModel(Guid.Empty, name, FolderPathGetLastId), (s, e) => DeleteItem(new ItemDeletedMessage(Guid.Empty)));
            await _previewHelper.SetPreview(folder);
            Items.Add(folder);
        }
        private void SaveFiles()
        {
            var files = SelectedItems.OfType<FileViewModel>();
            if(files.Count() == 0)
            {
                StatusMessage = "Выберите хотя бы 1 файл";
                return;
            }

            foreach (FileViewModel file in files)
            {
                file.SaveFileCommand.Execute(this);
            }
        }
        private async Task DeleteItemsAsync()
        {
            if (SelectedItems.Count == 0)
            {
                StatusMessage = "Выберите хотя бы 1 файл";
                return;
            }

            var dialogResult = _dialogService.SendOkCancelMessage($"Вы действительно хотите удалить {SelectedItems.Count} файл(-ов)?", "Подтвердитe удаление");
            if(dialogResult == true)
            {
                var deleteResult = new List<string>();

                var deletedItems = SelectedItems.ToList();
                foreach(var item in deletedItems)
                {
                    deleteResult.Add(await item.DeleteAsync());
                }
                StatusMessage = $"Удалено {deleteResult.Count} файл(-ов):";
            }
        }

        // ----------------------
        // Методы для локальной работы с файлами
        // ----------------------
        private void ApplySearchFilter()
        {
            foreach (var item in Items)
            {
                item.IsVisible = string.IsNullOrWhiteSpace(SearchQuery)
                                 || item.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
            }
        }
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
            if (Items.Count == 0)
                return;
            var deletedFile = Items.Where(i => i.Id == msg.Id).First();
            if (deletedFile != null)
            {
                _dispatcher.BeginInvoke(() => Items.Remove(deletedFile));
            }
        }
        private void OnServerStateChanged(ServerIsActiveMessage message)
        {
            switch(message.Status)
            {
                case ServerStatus.Online:
                    _dispatcher.BeginInvoke(async () => await GetFolderChilds(null));
                    break;
            }

            _dispatcher.BeginInvoke(() => ServerStatus = message.Status);
            _dispatcher.BeginInvoke(() => ServerStatusMessage = message.Message);
        }
        private async Task LoadFolder(FolderCreatedMessage msg)
        {

            var newFolder = await _folderService.GetFolderAsync(msg.id);

            var folder = Items.FirstOrDefault(f => f.Id == newFolder.Id || f.Name == newFolder.Name);
            if (folder != null)
                return;

            var folderVM = _folderVmFactory.Create(newFolder);
            await _previewHelper.SetPreview(folderVM);
            _dispatcher.BeginInvoke(() => Items.Add(folderVM));
        }
        
        // ----------------------
        // Drag & Drop Методы
        // ----------------------
        public async Task HandleDroppedFiles(string[] filePaths)
        {
            foreach (var filePath in filePaths)
            {
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Загружаем файл на сервер
                        await _fileService.UploadFileAsync(FolderPathGetLastId, filePath);
                    }
                    catch(Exception ex)
                    {
                        StatusMessage = ex.Message;
                    }
                }
                else if (Directory.Exists(filePath))
                {
                    try
                    {
                        // Загружаем папку на сервер (рекурсивно)
                        await _folderService.UploadFolderRecursiveAsync(filePath, FolderPathGetLastId);
                    }
                    catch (Exception ex)
                    {
                        StatusMessage = ex.Message;
                    }
                }
            }
        }
        public IEnumerable<ItemViewModel> GetSelectedItems() =>
            SelectedItems;

        // ----------------------
        // INotifyPropertyChanged
        // ----------------------
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}