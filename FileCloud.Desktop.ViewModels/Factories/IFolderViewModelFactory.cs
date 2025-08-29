using FileCloud.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.ViewModels.Factories
{
    public interface IFolderViewModelFactory
    {
        FolderViewModel Create(FolderModel dto, bool isNew = false);
    }
}
