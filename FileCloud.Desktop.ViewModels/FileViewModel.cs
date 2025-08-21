using FileCloud.Desktop.Models.Models;
using System;
using System.IO;

namespace FileCloud.Desktop.ViewModels
{
    public class FileViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? PreviewPath { get; set; }
        public byte[]? PreviewImage { get; set; }

        public FileViewModel(FileModel dto)
        {
            Id = dto.Id;
            Name = dto.Name;
        }

        public bool HasImagePreview => PreviewImage != null;
    }
}
