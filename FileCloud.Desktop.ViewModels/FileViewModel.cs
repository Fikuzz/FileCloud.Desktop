using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.ViewModels.Interfaces;
using System;
using System.IO;
using System.Windows.Input;

namespace FileCloud.Desktop.ViewModels
{
    public class FileViewModel : ItemViewModel, IEditableItem
    {
        public long? Size { get; set; }
        public byte[]? PreviewImage { get; set; }
        public FileViewModel(FileModel dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Size = dto.Size;
        }

        public bool HasImagePreview => PreviewImage != null;

        public ICommand CommitEditCommand => throw new NotImplementedException();

        public ICommand CancelEditCommand => throw new NotImplementedException();
    }
}
