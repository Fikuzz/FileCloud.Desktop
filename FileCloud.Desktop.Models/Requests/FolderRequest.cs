using System.ComponentModel.DataAnnotations;

namespace FileCloud.Desktop.Models.Requests
{
    public record FolderRequest(
        [Required]
        string Name,
        Guid? parentId);
}
