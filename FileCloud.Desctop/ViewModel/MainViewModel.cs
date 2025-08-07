using FileCloud.Desktop.Models;
using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FileCloud.Desktop.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly FileService _fileService;
        public ObservableCollection<FileViewModel> Files { get; } = new();

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoadFilesCommand { get; }
        public ICommand UploadFilesCommand { get; }
        public ICommand SaveFilesCommand { get; }
        public ICommand DeleteFilesCommand { get; }

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
        }

        private async Task LoadFiles()
        {
            try
            {
                StatusMessage = "Загрузка файлов...";
                Files.Clear();

                var result = await _fileService.GetFilesAsync();

                foreach (var file in result)
                {
                    var fileView = new FileViewModel(file);

                    string ext = System.IO.Path.GetExtension(file.Name).ToLower();

                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".mp4" || ext == ".mov" || ext == ".mkv")
                    {
                        // Загружаем превью с сервера
                        var imageData = await _fileService.GetPreviewAsync(file.Id);

                        if (imageData != null)
                        {
                            fileView.PreviewImage = imageData;
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
            var result = await _fileService.UploadFilesAsync("uploads", dialog.FileNames);
            StatusMessage = result;
            await LoadFiles();
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
                var dialog = new OpenFolderDialog();
                if (dialog.ShowDialog() != true)
                    return;

                StatusMessage = "Сохранение файла...";
                await _fileService.DownloadFilesAsync(ids, dialog.FolderName);
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
                var result = await _fileService.DeleteFilesAsync(ids); // новый метод
                StatusMessage = $"Удалено файлов: {result.Count}";

                foreach (var deletedId in result)
                {
                    var file = Files.FirstOrDefault(f => f.Id == deletedId);
                    if (file != null)
                        Files.Remove(file);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка при удалении: {ex.Message}";
            }
        }
        private string GetIconPathForExtension(string ext)
        {
            var icoPath = $"/Assets/Icons/{ext}.png";

            return File.Exists(icoPath) ? icoPath : "/Assets/Icons/default.png";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}