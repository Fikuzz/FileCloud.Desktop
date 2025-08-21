using FileCloud.Desktop.Models.Models;

namespace FileCloud.Desktop.Models.Response
{
    public class FileResponse
    {
        FileModel? File { get; set; }
        string? Error { get; set; }
    }
}
