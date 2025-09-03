using FileCloud.Desktop.Commands;
using FileCloud.Desktop.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.ViewModels;
using FileCloud.Desktop.ViewModels.Interfaces;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Xml.Linq;

namespace FileCloud.Desktop.ViewModels
{
    public class FolderViewModel : ItemViewModel, IEditableItem
    {
        private readonly FolderService _folderService;
        private string _originalName;

        public ICommand RenameCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CommitEditCommand { get; }
        public ICommand CancelEditCommand { get; }

        public bool IsNew { get; private set; }

        public event EventHandler? DeleteLocalFolder;
        public FolderViewModel(FolderModel dto, FolderService folderService, bool isNew)
        {
            _folderService = folderService;

            RenameCommand = new RelayCommand(_ => BeginEdit());
            CommitEditCommand = new RelayCommand(async _ => await OnCommitEdit());
            CancelEditCommand = new RelayCommand(_ => OnCancelEdit());
            DeleteCommand = new RelayCommand(async _ => await Delete());

            Id = dto.Id;
            Name = dto.Name;
            FolderId = dto.ParentId;

            if(isNew)
            {
                IsNew = true;
                BeginEdit();
            }
        }

        private void BeginEdit()
        {
            IsEditing = true;
            if (!IsNew)
                _originalName = Name;
        }

        private async Task Delete()
        {
            if (Id != Guid.Empty)
            {
                try
                {
                    await DeleteAsync();
                }
                catch
                {
                    //придумать обработку ошибок
                }

            }
        }

        private async Task OnCommitEdit()
        {
            if(IsEditing == false)
            {
                return;
            }
            IsEditing = false;

            if (IsNew)
            {
                try
                {
                    var folderId = await _folderService.CreateFolderAsync(Name, FolderId);
                    Id = folderId;
                    IsNew = false;
                }
                catch
                {
                    DeleteLocalFolder?.Invoke(this, EventArgs.Empty);
                }
            }
            else
            {
                try
                {
                    await _folderService.RenameFolderAsync(Id, Name);
                }
                catch
                {
                    Name = _originalName;
                }
            }
        }

        private void OnCancelEdit()
        {
            IsEditing = false;

            if (IsNew)
            {
                DeleteLocalFolder?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Name = _originalName;
            }
        }

        public override async Task<string> DeleteAsync()
        {
            var response = await _folderService.DeleteFolderAsync(Id);
            return response.Name;
        }
    }
}