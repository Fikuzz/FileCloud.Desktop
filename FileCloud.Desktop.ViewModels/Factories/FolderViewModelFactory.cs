using FileCloud.Desktop.Models;
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
    public class FolderViewModelFactory : IFolderViewModelFactory
    {
        private readonly IServiceProvider _sp;

        public FolderViewModelFactory(IServiceProvider sp)
        {
            _sp = sp;
        }

        public FolderViewModel Create(FolderModel dto, bool isNew = false)
        {
            // достаём зависимости из DI
            var folderService = _sp.GetRequiredService<FolderService>();

            // возвращаем готовый VM
            return new FolderViewModel(dto, folderService, isNew);
        }
    }
}
