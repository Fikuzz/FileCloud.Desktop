using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Models.Requests
{
    public record MoveFolderRequest(
        Guid FolderId);
}
