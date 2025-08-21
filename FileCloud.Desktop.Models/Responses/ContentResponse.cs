using FileCloud.Desktop.Models.Models;
using FileCloud.Desktop.Models.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Models.Responses
{
    public class ContentResponse
    {
        public List<FileModel> Files { get; set; } = new();
        public List<FolderModel> Folders { get; set; } = new();
    }
}
