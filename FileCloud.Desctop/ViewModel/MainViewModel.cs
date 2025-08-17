using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FileCloud.Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private FileSyncService _fileSyncService;
        private readonly FileService _fileService;
        private string? _statusMessage;
        public ObservableCollection<FileViewModel> Files { get; } = new();
        public string? StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoadFilesCommand { get; }
        public ICommand UploadFilesCommand { get; }
        public ICommand SaveFilesCommand { get; }
        public ICommand DeleteFilesCommand { get; }
        public ICommand FileOpenCommand { get; }

        public ObservableCollection<FileViewModel> SelectedFiles { get; set; }
        public MainViewModel()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            string apiBaseUrl = config["ServerSettings:ApiBaseUrl"];
            _fileService = new FileService(apiBaseUrl);

            SelectedFiles = new ObservableCollection<FileViewModel>();

            LoadFilesCommand = new RelayCommand(async _ => await LoadFiles());
            UploadFilesCommand = new RelayCommand(async _ => await UploadFiles());
            SaveFilesCommand = new RelayCommand(async _ => await SaveFiles());
            DeleteFilesCommand = new RelayCommand(async _ => await DeleteFiles());
            FileOpenCommand = new RelayCommand(async param =>
            {
                if(param is FileViewModel file)
                {
                    await OpenLocalFile(file);
                }
            });

            _fileSyncService = new FileSyncService(apiBaseUrl);
            _fileSyncService.FileReceived += OnFileReceived;
            _fileSyncService.ServerState += ServerStateChanged;
            _fileSyncService.FileDeleted += OnFileDeleted;
            _fileSyncService.StartMonitoringAsync();
        }
        private void OnFileReceived(FileModel file)
        {
            // Update your ObservableCollection< FileModel >
            App.Current.Dispatcher.Invoke(async() =>
            {
                var fileView = new FileViewModel(file);

                string ext = System.IO.Path.GetExtension(file.Name).ToLower();

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".mp4" || ext == ".mov" || ext == ".mkv")
                {
                    // Загружаем превью с сервера
                    var (imageData, error) = await _fileService.GetPreviewAsync(file.Id);

                    if (imageData != null)
                    {
                        fileView.PreviewImage = imageData;
                    }
                    else
                    {
                        StatusMessage = error;
                    }
                }
                else
                {
                    // Устанавливаем путь к локальной иконке по расширению
                    fileView.PreviewPath = GetIconPathForExtension(ext);
                }

                Files.Add(fileView);
            });
        }
        private void OnFileDeleted(Guid id)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                var file = Files.First(f => f.Id == id);
                Files.Remove(file);
            });
        }
        private void ServerStateChanged(bool isActive, string description)
        {
            App.Current.Dispatcher.Invoke(async () =>
            {
                _fileService.ServerState = isActive;
                StatusMessage = description;

                if (isActive)
                    await LoadFiles();
            });
        }
        private async Task LoadFiles()
        {
            try
            {
                StatusMessage = "Загрузка файлов...";
                Files.Clear();

                var (result, error) = await _fileService.GetFilesAsync();
                if(result == null)
                {
                    StatusMessage = error;
                    return;
                }
                foreach (var file in result)
                {
                    var fileView = new FileViewModel(file);

                    string ext = System.IO.Path.GetExtension(file.Name).ToLower();

                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".mp4" || ext == ".mov" || ext == ".mkv")
                    {
                        // Загружаем превью с сервера
                        var (imageData, errorImage) = await _fileService.GetPreviewAsync(file.Id);

                        if (imageData != null)
                        {
                            fileView.PreviewImage = imageData;
                        }
                        else
                        {
                            StatusMessage = errorImage;
                        }
                    }
                    else
                    {
                        // Устанавливаем путь к локальной иконке по расширению
                        fileView.PreviewPath = GetIconPathForExtension(ext);
                    }

                    Files.Add(fileView);
                }

                StatusMessage = "Файлы загружены.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
            }
        }
        private async Task UploadFiles()
        {
            var dialog = new OpenFileDialog { Multiselect = true };
            if (dialog.ShowDialog() != true)
                return;

            StatusMessage = "Загрузка файлов...";
            var (result, error) = await _fileService.UploadFilesAsync("uploads", dialog.FileNames);
            StatusMessage = error == string.Empty ? result : error;
            //await LoadFiles();
        }
        private async Task SaveFiles()
        {
            if (!SelectedFiles.Any())
            {
                StatusMessage = "Выберите хотя бы один файл.";
                return;
            }
            try
            {
                var ids = SelectedFiles.Select(f => f.Id).ToList();
                var folderPath = DownloadSettings.DownloadPath;

                StatusMessage = "Сохранение файла...";
                foreach (var id in ids)
                {
                    var (result, error) = await _fileService.DownloadFileAsync(id, folderPath);
                    if (error != string.Empty)
                    {
                        StatusMessage = error;
                        return;
                    }

                }
                StatusMessage = "Файлы сохранены.";
            }
            catch
            {
                StatusMessage = "Не удалось скачать файлы.";
            }
        }
        private async Task DeleteFiles()
        {
            if (!SelectedFiles.Any())
            {
                StatusMessage = "Выберите хотя бы один файл.";
                return;
            }

            try
            {
                var ids = SelectedFiles.Select(f => f.Id).ToList();
                var (result, error) = await _fileService.DeleteFilesAsync(ids); // новый метод
                if (result == null)
                {
                    StatusMessage = error;
                    return;
                }
                else
                {
                    StatusMessage = $"Удалено файлов: {result.Count}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при удалении: {ex.Message}";
            }
        }
        private async Task OpenLocalFile(FileViewModel file)
        {
            var path = FileMappingManager.GetLocalPath(file.Id);

            if (path == null || !File.Exists(path))
            {
                var folderPath = DownloadSettings.DownloadPath;
                var (newName, error) = await _fileService.DownloadFileAsync(file.Id, folderPath);
                if(newName == null)
                {
                    StatusMessage = error;
                    return;
                }
                path = Path.Combine(folderPath, newName);
            }

            if (File.Exists(path))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true // нужно для открытия через ассоциацию в Windows
                });
            }
        }
        private string GetIconPathForExtension(string ext)
        {
            var iconsPath = Path.Combine(Environment.CurrentDirectory, "Assets\\Icons");
            var ico = Path.Combine(iconsPath, $"{ext}.png");

            return File.Exists(ico) ? ico : Path.Combine(iconsPath, "default.png");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}