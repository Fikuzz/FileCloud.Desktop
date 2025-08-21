using FileCloud.Desktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.ViewModels
{
    public class FolderViewModel : ItemViewModel
    {
        public FolderViewModel(FolderModel dto) 
        {
            Id = dto.Id;
            Name = dto.Name;
        }
    }
}
