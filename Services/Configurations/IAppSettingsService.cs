using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Desktop.Services.Configurations
{
    public interface IAppSettingsService
    {
        string AppBasePath { get; }
        string ApiBaseUrl { get; }
        string DownloadPath { get; }
        Guid RootFolderId { get; }
    }
}
