using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Services;
using FileCloud.Desktop.Services.Configurations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.ViewModels.Factories
{
    public class FileViewModelFactory : IFileViewModelFactory
    {
        private readonly IServiceProvider _sp;

        public FileViewModelFactory(IServiceProvider sp)
        {
            _sp = sp;
        }

        public FileViewModel Create(FileModel dto)
        {
            // достаём зависимости из DI
            var saveService = _sp.GetRequiredService<IFileSaveService>();
            var fileService = _sp.GetRequiredService<FileService>();

            // возвращаем готовый VM
            return new FileViewModel(dto, fileService, saveService);
        }
    }
}
