using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Helpers;
using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using FileCloud.Desktop.ViewModels.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace FileCloud.Desktop.ViewModels
{
    public class FileViewModel : ItemViewModel, IEditableItem
    {
        private readonly FileService _fileService;
        private readonly IFileSaveService _fileSaveService;
        private string _originalName;

        public long? Size { get; set; }
        public byte[]? PreviewImage { get; set; }
        public bool HasImagePreview => PreviewImage != null;

        public event EventHandler? DeleteLocalFile;

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand CommitEditCommand { get; }
        public ICommand CancelEditCommand { get; }

        public ICommand OpenLocalFileCommand { get; }

        public FileViewModel(FileModel dto, FileService fileService, IFileSaveService fileSaveService)
        {
            Id = dto.Id;
            Name = dto.Name;
            Size = dto.Size;

            _fileSaveService = fileSaveService;
            _fileService = fileService;

            DeleteCommand = new RelayCommand(async _ => await Delete());
            CommitEditCommand = new RelayCommand(async _ => await OnCommitEdit());
            RenameCommand = new RelayCommand(_ => BeginEdit());
            CancelEditCommand = new RelayCommand(_ => OnCancelEdit());
            SaveFileCommand = new RelayCommand(async _ => await Save());
            OpenLocalFileCommand = new RelayCommand(async _ => await OpenLocal());
        }

        private void BeginEdit()
        {
            IsEditing = true;
            _originalName = Name;
        }

        private async Task Delete()
        {
            if (Id != Guid.Empty)
            {
                try
                {
                    await _fileService.DeleteFileAsync(Id);
                }
                catch
                {
                    //придумать обработку ошибок
                }
                DeleteLocalFile?.Invoke(this, EventArgs.Empty);

            }
        }
        private async Task<string> Save()
        {
            try
            {
                var content = await _fileService.DownloadFileAsync(Id);
                return await _fileSaveService.SaveFileAsync(Id, Name, content);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "error";
            }
        }

        private async Task OnCommitEdit()
        {
            if (IsEditing == false)
            {
                return;
            }
            IsEditing = false;

            try
            {
                await _fileService.RenameFileAsync(Id, Name);
            }
            catch
            {
                Name = _originalName;
            }
        }

        private void OnCancelEdit()
        {
            IsEditing = false;

            Name = _originalName;
        }
        private async Task OpenLocal()
        {
            var localPath = FileMappingManager.GetLocalPath(Id);
            if(localPath == null)
            {
                localPath = await Save();
            }

            try
            {
                Process.Start(new ProcessStartInfo(localPath)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось открыть файл: {ex.Message}");
            }
        }

        public override async Task<string> DeleteAsync()
        {
            var responce = await _fileService.DeleteFileAsync(Id);
            return responce.Name;
        }

    }
}
