using FileCloud.Desktop.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Models.Responses
{
    public class FolderContentResponse
    {
        public List<FileResponse> Files { get; set; } = new();
        public List<FolderResponse> Folders { get; set; } = new();
    }
}
