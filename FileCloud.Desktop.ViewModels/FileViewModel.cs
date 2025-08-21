using FileCloud.Desktop.Models.Models;
using System;
using System.IO;

namespace FileCloud.Desktop.ViewModels
{
    public class FileViewModel : ItemViewModel
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
    }
}
