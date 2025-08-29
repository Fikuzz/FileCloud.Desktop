using FileCloud.Desktop.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.ViewModels.Factories
{
    public interface IFileViewModelFactory
    {
        FileViewModel Create(FileModel dto);
    }
}
