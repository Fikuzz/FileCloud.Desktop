using FileCloud.Desktop.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Models.Models
{
    public record AuthSessionModel
    (
        Guid Id,
        string Login,
        string Email,
        string Token,
        FolderModel RootFolder);
}
