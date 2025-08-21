using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Services.Configurations
{
    public interface IFileDialogService
    {
        string[]? OpenFiles(string filter);
    }
}
