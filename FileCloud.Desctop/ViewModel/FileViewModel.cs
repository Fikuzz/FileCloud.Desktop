using FileCloud.Desctop.Models;
using System;
using System.IO;
using System.Windows.Media;

namespace FileCloud.Desktop.ViewModels
{
    public class FileViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public string? PreviewPath { get; set; }
        public ImageSource? PreviewImage { get; set; }

        public FileViewModel(FileModel dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Path = dto.Path;
        }

        public bool HasImagePreview => PreviewImage != null;
    }
}
